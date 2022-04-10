using AutoMapper;
using BEAR.BusinessLogic.Awards;
using BEAR.BusinessLogic.Awards.DTO;
using BEAR.Web.Models.Awards;
using BEAR.Web.Models.Shared;
using BEAR.Web.Filters;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using System.Net;
using System.Data.SqlClient;
using BEAR.DAL.UserAssignments;
using BEAR.DAL.Users;

namespace BEAR.Web.Controllers.API
{
    [Authorize]
    public class AwardController : ApiController
    {
        private static ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly object ApprovalLock = new object();

        private IAwardService _awards;
        private IMapper _mapper;

        public AwardController(IAwardService awardService, IMapper mapper)
        {
            _awards = awardService;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("api/awards")]
        public async Task<IHttpActionResult> GetAwards()
        {
            log.Debug("GetAwards: Entering");

            try
            {
                var input = new GetAwardsDTOInput
                {
                    Username = ((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.Sid).Value,
                    IsGroupLead = User.IsInRole("GroupLead")
                };

                var awards = _mapper.Map<List<BrowseAwardRecord>>(await _awards.GetAwards(input));

                log.Debug("GetAwards: Exiting");
                return Ok(awards);
            }
            catch (Exception ex)
            {
                log.Error("GetAwards: Error on GetAwards ", ex);
                log.Debug("GetAwards: Exiting");
                return BadRequest();
            }
        }

        [HttpGet]
        [Route("api/awards/{awardNumber}")]
        public async Task<IHttpActionResult> GetAward(string awardNumber)
        {
            log.Debug("GetAward: Entering");

            try
            {
                AwardDetailsDTOInput awardDTO = new AwardDetailsDTOInput() { 
                        AwardNumber = awardNumber,
                        Username = ((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.Sid).Value,
                        IsGroupLead = User.IsInRole("GroupLead")
                    };

                var award = _mapper.Map<AwardDetails>(await _awards.GetAwardDetails(awardDTO));

                log.Debug("GetAward: Exiting");
                return Ok(award);
            }
            catch (Exception ex)
            {
                log.Error("GetAward: Error on GetAwardDetails ", ex);
                log.Debug("GetAward: Exiting");
                return BadRequest("Error retrieving Award");
            }
        }

        [HttpPut]
        [Route("api/awards/{awardNumber}")]
        public async Task<IHttpActionResult> UpdateAward(InlineAwardForm form)
        {
            log.Debug("UpdateAward: Entering");

            if (!ModelState.IsValid)
            {
                log.Error("UpdateAward: Failed Validation");
                log.Debug("UpdateAward: Exiting");
                return BadRequest(ModelState);
            }

            try
            {
                var formDTO = _mapper.Map<InlineAwardForm, EditAwardDTOInput>(form);
                formDTO.Username = ((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.Sid).Value;
                formDTO.IsGroupLead = User.IsInRole("GroupLead");
                formDTO.CanEditAwards = User.IsInRole("CanEditAwards");

                await _awards.UpdateAward(formDTO);

                log.Debug("UpdateAward: Exiting");
                return Ok();
            }
            catch (Exception ex)
            {
                log.Error("UpdateAward: Error updating Award", ex);
                log.Debug("UpdateAward: Exiting");
                return BadRequest("Error updating Award");
            }
        }

        [HttpPost]
        [Route("api/awards/comments")]
        public async Task<IHttpActionResult> AddAwardComment(CommentForm comment)
        {
            log.Debug("AddAwardComment: Entering");

            if (ModelState.IsValid)
            {
                var awardComment = new AwardCommentDTOInput()
                    {
                        AwardNumber = comment.ID,
                        CommenterUsername = ((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.Sid).Value,
                        CommentText = comment.CommentText,
                        IsGroupLead = User.IsInRole("GroupLead"),
                        CanAddAwardComments = User.IsInRole("CanAddAwardComments"),
                        CommenterFullName = ((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.Name).Value,
                    };

                try
                {
                    var newAwardComment = _mapper.Map<AwardCommentDTOOutput, AwardComment>(await _awards.AddAwardComment(awardComment));

                    log.Debug("AddAwardComment: Exiting");
                    return Ok(new
                    {
                        responseText = "Comment Added Successfully",
                        time = newAwardComment.CommentTime,
                        user = newAwardComment.CommenterFullName
                    });
                }
                catch (Exception ex)
                {
                    log.Error("AddAwardComment: Error adding comment", ex);
                    log.Debug("AddAwardComment: Exiting due to error");

                    return BadRequest("Error while adding comment to database");
                }
            }
            else
            {
                var responseMessage = ModelState.Values.SelectMany(v => v.Errors).Count() > 1
                    ? "Error Adding Comment"
                    : ModelState.Values.FirstOrDefault()?.Errors.FirstOrDefault()?.ErrorMessage;

                return BadRequest(responseMessage);
            }
        }

        [HttpGet]
        [Route("api/awards/{awardNum}/comments")]
        public async Task<IHttpActionResult> GetAwardComments(string awardNum)
        {
            log.Debug("GetAwardComments: Entering");

            try
            {
                List<AwardComment> comments = _mapper.Map<List<AwardComment>>(await _awards.GetAwardComments(awardNum));

                log.Debug("GetAwardComments: Exiting");
                return Ok(comments);
            }
            catch(Exception ex)
            {
                log.Error("GetAwardComments: Error getting comments", ex);
                log.Debug("GetAwardComments: Exiting");
                return BadRequest("Error retrieving award comments");
            }
        }

        [HttpGet]
        [Route("api/awards/formatSubcategories")]
        public async Task<IHttpActionResult> GetFormatSubcategories()
        {

            log.Debug("GetFormatSubcategories: Entering");

            try
            {
                var formatSubcategories = _mapper.Map<List<TreeNode>>(await _awards.GetContractFileFormatSubcategories());

                log.Debug("GetFormatSubcategories: Exiting");
                return Ok(formatSubcategories);
            }
            catch (Exception ex)
            {
                log.Error("GetFormatSubcategories: Error on GetFormatSubcategories ", ex);
                log.Debug("GetFormatSubcategories: Exiting");
                return BadRequest();
            }
        }

        [HttpGet]
        [Route("api/awards/contractFileStatuses")]
        public async Task<IHttpActionResult> GetContractFileStatuses()
        {
            log.Debug("GetContractFileStatuses: Entering");

            try
            {
                var contractFileFormatStatuses = _mapper.Map<List<ContractFileFormatStatusDTOOutput>, List<DropdownItem>>(await _awards.GetContractFileFormatStatuses());

                log.Debug("GetContractFileStatuses: Exiting");
                return Ok(contractFileFormatStatuses);
            }
            catch (Exception ex)
            {
                log.Error("GetContractFileStatuses: Error getting available statuses", ex);
                log.Debug("GetContractFileStatuses: Exiting");
                return BadRequest();
            }
        }

        [HttpPut]
        [Route("api/awards/{awardNumber}/contractFileStatus")]
        public async Task<IHttpActionResult> UpdateContractFileStatus(ContractFileStatusForm form)
        {
            log.Debug("UpdateContractFileStatus: Entering");

            if (!ModelState.IsValid)
            {
                log.Error("UpdateContractFileStatus: Failed Validation");
                log.Debug("UpdateContractFileStatus: Exiting");
                return BadRequest(ModelState);
            }
            
            try
            {
                if(!String.IsNullOrWhiteSpace(form.CommentText)) {
                    // Add award comment
                    var awardComment = new AwardCommentDTOInput()
                    {
                        AwardNumber = form.AwardNumber,
                        CommenterUsername = ((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.Sid).Value,
                        CommentText = form.CommentText,
                        IsGroupLead = User.IsInRole("GroupLead"),
                        CanAddAwardComments = User.IsInRole("CanAddAwardComments"),
                        CommenterFullName = ((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.Name).Value,
                    };
                    var newAwardComment = _mapper.Map<AwardCommentDTOOutput, AwardComment>(await _awards.AddAwardComment(awardComment));
                }

                // Update contract file status
                var formDTO = _mapper.Map<ContractFileStatusForm, UpdateContractFileStatusDTOInput>(form);
                formDTO.Username = ((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.Sid).Value;
                formDTO.IsGroupLead = User.IsInRole("GroupLead");
                formDTO.CanEditAwards = User.IsInRole("CanEditAwards");

                await _awards.UpdateContractFileStatus(formDTO);

                log.Debug("UpdateContractFileStatus: Exiting");
                return Ok();
            }
            catch (Exception ex)
            {
                log.Error("UpdateContractFileStatus: Error updating Contract File Status", ex);
                log.Debug("UpdateContractFileStatus: Exiting");
                return BadRequest("Error updating Contract File Status");
            }
        }

        [HttpGet]
        [Route("api/awards/{awardNum}/transfers")]
        public async Task<IHttpActionResult> GetAwardTransferHistory(string awardNum)
        {
            log.Debug("GetAwardTransferHistory: Entering");

            try
            {
                List<AwardTransferRecord> history = _mapper.Map<List<AwardTransferDTOOutput>, List<AwardTransferRecord>>(await _awards.GetAwardTransferHistory(awardNum));

                log.Debug("GetAwardTransferHistory: Exiting");
                return Ok(history);
            }
            catch(Exception ex)
            {
                log.Error("GetAwardTransferHistory: Error getting transfer history", ex);
                log.Debug("GetAwardTransferHistory: Exiting");
                return BadRequest("Error retrieving transfer history");
            }
        }

        [HttpGet]
        [Route("api/awards/GetModAssociatedAwards")]
        public async Task<IHttpActionResult> GetModAssociatedAwards(long modId)
        {
            log.Debug("GetModAssociatedAwards: Entering");

            try
            {
                List<AwardVersion> output = _mapper.Map<List<AwardVersion>>(await _awards.GetAwardsForMod(modId));

                log.Debug("GetModAssociatedAwards: Exiting");
                return Json(new { success = true, data = output.ToArray() });
            }
            catch(Exception ex)
            {
                log.Error("GetModAssociatedAwards: Error getting associated awards", ex);
                log.Debug("GetModAssociatedAwards: Exiting");
                return BadRequest("Error retrieving associated awards");
            }
        }

        [HttpGet]
        [Route("api/awards/GetModAvailableAwards")]
        public async Task<IHttpActionResult> GetModAvailableAwards(long modId)
        {
            log.Debug("GetModAvailableAwards: Entering");

            try{
                List<AwardVersion> output = _mapper.Map<List<AwardVersion>>(await _awards.GetAvailableAwardsForMod(modId));

                log.Debug("GetModAvailableAwards: Exiting");
                return Json(new { success = true, data = output.ToArray() });
            }
            catch(Exception ex)
            {
                log.Error("GetModAvailableAwards: Error getting available awards", ex);
                log.Debug("GetModAvailableAwards: Exiting");
                return BadRequest("Error retrieving available awards");
            }
        }

        [HttpGet]
        [Route("api/awards/{awardID}/summary")]
        public async Task<IHttpActionResult> GetAwardSummary(string awardID)
        {
            log.Debug("GetAwardSummary: Entering");

            try
            {
                var summary = _mapper.Map<AwardSummaryDTOOutput, AwardSummary>(await _awards.GetAwardSummary(awardID));

                log.Debug("GetAwardSummary: Exiting");
                return Ok(summary);
            }
            catch (Exception ex)
            {
                log.Error("GetAwardSummary: Error on GetAwardSummary ", ex);
                log.Debug("GetAwardSummary: Exiting");
                return BadRequest("Error retrieving Award Summary");
            }
        }

        [HttpPost]
        [AllowAPIAccess(Roles = "GroupLead,CanTransferAllAwards")]
        [Route("api/award/{awardNum}/transfer")]
        public IHttpActionResult TransferAward(AwardTransferForm form)
        {
            log.Debug("TransferAward: Entering");

            var transfer = _mapper.Map<AwardTransferDTOInput>(form);
            transfer.UserEmail = ((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.Email).Value;
            transfer.UserFullName = ((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.Name).Value;
            transfer.Username = ((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.Sid).Value;
            transfer.CanTransferAllAwards = User.IsInRole("CanTransferAllAwards");
            transfer.CanOverrideTransferEmails = User.IsInRole("CanOverrideTransferEmails");

            try
            {
                _awards.TransferAward(transfer);

                log.Debug("TransferAward: Exiting");
                return Ok();
            }
            catch (SqlException se)
            {
                if (se.Procedure.Contains("dbmail"))
                {
                    log.Error("TransferAward: Transfer completed but Email did not send. - ", se);
                    return Ok();
                }
                log.Error("TransferAward: Error making transfer", se);
                return BadRequest("Error on SubmitCloseout");
            }
            catch (Exception e)
            {
                log.Error("TransferAward: Error making transfer", e);
                return BadRequest("Error transferring award");
            }
        }

        [HttpGet]
        [Route("api/awards/customer/{customerID}")]
        public async Task<IHttpActionResult> GetCustomerAwards(string customerID)
        {
            log.Debug("GetCustomerAwards: Entering");
            try
            {
                List<CustomerAward> custAwards = _mapper.Map<List<CustomerAward>>(await _awards.GetCustomerAwards(customerID));

                log.Debug("GetCustomerAwards: Exiting");
                return Ok(custAwards);
            }
            catch (Exception ex)
            {
                log.Error("GetCustomerAwards: Error on GetAwardsByCustomer ", ex);
                log.Debug("GetCustomerAwards: Exiting");

                return BadRequest();
            }
        }

        [HttpGet]
        [Route("api/awards/transferRequests")]
        public async Task<IHttpActionResult> GetTransferRequests()
        {
            log.Debug("GetTransferRequests: Entering");
            try
            {
                List<PendingAwardTransfer> pendingTransfers;
                if (User.IsInRole("CanApproveAllAwardTransfers"))
                {
                    pendingTransfers = _mapper.Map<List<PendingAwardTransfer>>(await _awards.GetAllPendingTransfers());
                } 
                else
                {
                    pendingTransfers = _mapper.Map<List<PendingAwardTransferDTOOutput>, List<PendingAwardTransfer>>(
                        await _awards.GetMyPendingTransfers(((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.Sid).Value));
                }
                log.Debug("GetTransferRequests: Exiting");
                return Ok(pendingTransfers);

            }
            catch (Exception ex)
            {
                log.Error("GetTransferRequests: Error retrieving pending transfers ", ex);
                log.Debug("GetTransferRequests: Exiting");

                return BadRequest();
            }
        }

        [HttpPut]
        [AllowAPIAccess(Roles = "GroupLead,CanApproveAllAwardTransfers")]
        [Route("api/awards/transferRequests/approveDenyRequest")]
        public IHttpActionResult ApproveDenyRequest(PendingAwardTransferDecision form)
        {
            log.Debug("ApproveDenyRequest: Entering");

            // Get group lead username
            var approveUsername = ((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.Sid).Value;

            try
            {
                // Map to Award Transfer Approval DTO 
                var awardTransferApproval = _mapper.Map<AwardTransferApprovalDTOInput>(form);
                awardTransferApproval.ApproveUser = approveUsername;
                awardTransferApproval.IsAdminTransfer = User.IsInRole("CanApproveAllAwardTransfers");
                awardTransferApproval.CanOverrideTransferEmails = User.IsInRole("CanOverrideTransferEmails");

                lock (ApprovalLock)
                {
                    _awards.SetAwardTransferApproval(awardTransferApproval);
                }
            }
            catch (SqlException se)
            {
                if (se.Procedure.Contains("dbmail"))
                {
                    log.Error("ApproveDenyRequest: Decision completed but Email did not send. - ", se);
                    return Ok();
                }
                log.Error("ApproveDenyRequest: Error updating request ", se);
                return BadRequest("Error on SubmitCloseout");
            }
            catch (Exception ex)
            {
                log.Error("ApproveDenyRequest: Error updating request ", ex);

                return BadRequest();
            }

            log.Debug("ApproveDenyRequest: Exiting");
            return Ok(form.AwardNum + " was updated successfully");
        }

        [Route("api/awards/statuses")]
        public async Task<IHttpActionResult> GetAwardStatuses()
        {
            log.Debug("GetAwardStatuses: Entering");

            try
            {
                var results = await _awards.GetAwardStatuses();
                var statuses = _mapper.Map<List<AwardStatusDTOOutput>, List<KeyValuePair<string, string>>>(results);

                log.Debug("GetAwardStatuses: Exiting");
                return Ok(statuses);
            }
            catch (Exception ex)
            {
                log.Error("GetAwardStatuses: Error on GetAwardStatuses", ex);
                log.Debug("GetAwardStatuses: Exiting");
                return BadRequest("Error retrieving Award Statuses");
            }
        }

        [Route("api/awards/transferRequestsStatuses")]
        public async Task<IHttpActionResult> GetOwlTransferStatuses()
        {
            log.Debug("GetOwlTransferStatuses: Entering");

            try
            {
                var results = await _awards.GetOwlTransferStatuses();
                var statuses = _mapper.Map<List<OwlTransferStatusDTOOutput>, List<KeyValuePair<string, string>>>(results);

                log.Debug("GetOwlTransferStatuses: Exiting");
                return Ok(statuses);
            }
            catch (Exception ex)
            {
                log.Error("GetOwlTransferStatuses: Error on GetOwlTransferStatuses", ex);
                log.Debug("GetOwlTransferStatuses: Exiting");
                return BadRequest("Error retrieving Award Statuses");
            }
        }

        [HttpGet]
        [Route("api/awards/GetProjectAssociatedAwards")]
        public async Task<IHttpActionResult> GetProjectAssociatedAwards(string projectNum)
        {
            log.Debug("GetProjectAssociatedAwards: Entering");

            try
            {
                var awards = _mapper.Map<List<PrismAwardsForPreAwardDTOOutput>, List<AssociatedAward>>(await _awards.GetAwardsForPreAward(projectNum));

                log.Debug("GetProjectAssociatedAwards: Exiting");
                return Ok(awards);
            } 
            catch (Exception ex)
            {
                log.Error("GetProjectAssociatedAwards: Error on GetAwardsForPreAward ", ex);
                log.Debug("GetProjectAssociatedAwards: Exiting");
                return BadRequest("Error retrieving Associated Awards");
            }
        }

        [HttpGet]
        [Route("api/awards/GetPreAwardAvailableAwards")]
        public async Task<IHttpActionResult> GetPreAwardAvailableAwards(string preAwardNum)
        {
            log.Debug("GetPreAwardAvailableAwards: Entering");

            try
            {
                var dtoInput = new PrismAwardsAvailableForPreAwardDTOInput()
                {
                    PreAwardNumber = preAwardNum,
                    Username = ((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.Sid).Value,
                    IsGroupLead = User.IsInRole("GroupLead")
                };

                var awards =  _mapper.Map<List<PrismAwardsAvailableForPreAwardDTOOutput>, List<AvailableAwardAssociation>>(await _awards.GetAvailableAwardsForPreAward(dtoInput));

                log.Debug("GetPreAwardAvailableAwards: Exiting");
                return Ok(awards);
            }
            catch(Exception ex)
            {
                log.Error("GetPreAwardAvailableAwards: Error retrieving awards", ex);
                log.Debug("GetPreAwardAvailableAwards: Exiting");
                return BadRequest("Error retrieving award data");
            }
        }

        [HttpPut]
        [Route("api/awards/AssociateAwardToProject/{preAwardNum}/{awardNum}")]
        public async Task<IHttpActionResult> AssociateProjectToAward(string preAwardNum, string awardNum)
        {
            log.Debug("AssociateProjectToAward: Entering");
            try
            {
                await _awards.AssociateAwardToPreAward(new AssociateAwardToPreAwardDTOInput()
                {
                    AwardNumber = awardNum,
                    PreAwardNumber = preAwardNum
                });

                log.Debug("AssociateProjectToAward: Exiting");
                return Json(new { success = true });
            }
            catch (Exception e)
            {
                log.Error("AssociateProjectToAward: Error updating Association", e);
                log.Debug("AssociateProjectToAward: Exiting");
                return BadRequest();
            }
        }

        [HttpGet]
        [Route("api/awards/types")]
        public async Task<IHttpActionResult> GetAwardTypes()
        {
            log.Debug("GetAwardTypes: Entering");

            try
            {
                List<KeyValuePair<string, string>> awardTypes = _mapper.Map<List<AwardTypeDTOOutput>, List<KeyValuePair<string, string>>>(await _awards.GetAwardTypes());

                log.Debug("GetAwardTypes: Exiting");
                return Ok(awardTypes);
            }
            catch (Exception ex)
            {
                log.Error("GetAwardTypes: Error getting available award types", ex);
                log.Debug("GetAwardTypes: Exiting");
                return BadRequest();
            }
        }

        [HttpGet]
        [Route("api/awards/preawardtypes")]
        public async Task<IHttpActionResult> GetPreAwardTypes()
        {
            log.Debug("GetPreAwardTypes: Entering");

            try
            {
                List<KeyValuePair<string, string>> preAwardTypes = _mapper.Map<List<AwardTypeDTOOutput>, List<KeyValuePair<string, string>>>(await _awards.GetPreAwardTypes());

                log.Debug("GetPreAwardTypes: Exiting");
                return Ok(preAwardTypes);
            }
            catch (Exception ex)
            {
                log.Error("GetPreAwardTypes: Error getting available pre-award types", ex);
                log.Debug("GetPreAwardTypes: Exiting");
                return BadRequest();
            }
        }

        [HttpGet]
        [Route("api/awards/closeout/dropdowns")]
        public async Task<IHttpActionResult> GetCloseoutFormDropdowns()
        {
            log.Debug("GetCloseoutFormDropdowns: Entering");

            try
            {
                CloseoutFormDropdowns dropdowns = new CloseoutFormDropdowns()
                {
                    PricingTypes =  _mapper.Map<List<PricingTypeDTOOutput>, List<KeyValuePair<string, string>>>(await _awards.GetAwardPricingTypes()),
                    CPARS = _mapper.Map<List<CloseoutCPARSTypeDTOOutput>, List<KeyValuePair<string, string>>>(await _awards.GetCloseoutCPARSTypes()),
                    Invoices = _mapper.Map<List<CloseoutInvoiceTypeDTOOutput>, List<KeyValuePair<string, string>>>(await _awards.GetCloseoutInvoiceTypes()),
                    ContractFileStatuses = _mapper.Map<List<CloseoutContractFileStatusDTOOutput>, List<DropdownItem>>(await _awards.GetCloseoutContractFileStatuses())
                };

                log.Debug("GetCloseoutFormDropdowns: Exiting");
                return Ok(dropdowns);
            }
            catch (Exception ex)
            {
                log.Error("GetCloseoutFormDropdowns: Error getting available closeout form dropdowns", ex);
                log.Debug("GetCloseoutFormDropdowns: Exiting");
                return BadRequest();
            }
        }

        [HttpGet]
        [Route("api/awards/{awardNum}/closeout")]
        public async Task<IHttpActionResult> LoadCloseout(string awardNum)
        {
            log.Debug("GetCloseoutFormDropdowns: Entering");
            try
            {

                var input = new CloseoutFormDTOInput
                {
                    AwardNumber = awardNum,
                    Username = ((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.Sid).Value,
                    IsGroupLead = User.IsInRole("GroupLead"),
                    IsAdmin = User.IsInRole("CanEditSubmitCloseout")
                };

                var closeoutForm = _mapper.Map<CloseoutForm>(await _awards.GetCloseoutForm(input));
                log.Debug("GetCloseoutFormDropdowns: Exiting");
                return Ok(closeoutForm);
            }
            catch(AccessViolationException ex)
            {
                string username = ((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.Sid).Value;
                log.Error("LoadCloseout: " + ex);
                return StatusCode(HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                log.Error("LoadCloseout: Error on load - ", ex);
                return BadRequest("Error on load");
            }
        }

        [HttpPut]
        [Route("api/awards/closeout")]
        public async Task<IHttpActionResult> SaveCloseout(CloseoutForm form)
        {
            log.Debug("SaveCloseout: Entering");

            try
            {
                CloseoutDTOInput input = _mapper.Map<CloseoutDTOInput>(form);
                input.Username =  ((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.Sid).Value;;
                input.IsGroupLead = User.IsInRole("GroupLead");
                input.IsAdmin = User.IsInRole("CanEditSubmitCloseout");

                //add closeout to database
                await _awards.SaveCloseout(input);

                log.Debug("SaveCloseout: Exiting");
                return Ok();

            }
            catch (Exception ex)
            {
                log.Error("SaveCloseout: Error on save - ", ex);
                return BadRequest("Error on SaveCloseout");
            }
        }

        [HttpPost]
        [Route("api/awards/closeout")]
        public async Task<IHttpActionResult> SubmitCloseout(CloseoutForm form)
        {
            log.Debug("SubmitCloseout: Entering");

            try
            {
                if (ModelState.IsValid)
                {
                    CloseoutDTOInput input = _mapper.Map<CloseoutDTOInput>(form);
                    input.Username = ((ClaimsIdentity) User.Identity).FindFirst(ClaimTypes.Sid).Value;
                    input.UserEmail = ((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.Email).Value;;
                    input.IsGroupLead = User.IsInRole("GroupLead");
                    input.IsAdmin = User.IsInRole("CanEditSubmitCloseout");

                    //add closeout to database and submit
                    await _awards.SubmitCloseout(input);

                    log.Debug("SubmitCloseout: Exiting");
                    return Ok();
                }
                else
                {
                    log.Debug("SubmitCloseout: Error on SubmitCloseout - Model state wasn't valid");
                    return BadRequest("Model state invalid");
                }
            }
            catch (SqlException se)
            {
                if (se.Procedure.Contains("dbmail"))
                {
                    log.Error("SubmitCloseout: Closeout Saved but Email did not send. - ", se);
                    return Ok();
                }
                log.Error("SubmitCloseout: Error on SubmitCloseout - ", se);
                return BadRequest("Error on SubmitCloseout");
            }
            catch (Exception ex)
            {
                log.Error("SubmitCloseout: Error on SubmitCloseout - ", ex);
                return BadRequest("Error on SubmitCloseout");
            }
        }

        [HttpPatch]
        [Route("api/awards/{awardNum}/closeout/lock")]
        public async Task<IHttpActionResult> UnlockCloseout(string awardNum)
        {
            log.Debug("UnlockCloseout: Entering");

            try
            {
                UnlockCloseoutDTOInput input = new UnlockCloseoutDTOInput()
                {
                        
                    HasUnlockCloseoutPermission = User.IsInRole("CanUnlockCloseout"),
                    Username = ((ClaimsIdentity) User.Identity).FindFirst(ClaimTypes.Sid).Value,
                    AwardNumber = awardNum
                };

                //unlock closeout
                await _awards.UnlockCloseout(input);

                log.Debug("UnlockCloseout: Exiting");
                return Ok();
            }
            catch (Exception ex)
            {
                log.Error("UnlockCloseout: Error on UnlockCloseout - ", ex);
                return BadRequest("Error on UnlockCloseout");
            }
        }

        [AllowAPIAccess(Roles = "CanBulkEditFundingHold")]
        [HttpPatch]
        [Route("api/awards/fundingHold")]
        public async Task<IHttpActionResult> BulkFundingHoldChange(BulkFundingHoldForm form)
        {
            log.Debug("BulkFundingHoldChange: Entering");

            try
            {
                BulkFundingHoldDTOInput input = new BulkFundingHoldDTOInput()
                {
                    Username = ((ClaimsIdentity) User.Identity).FindFirst(ClaimTypes.Sid).Value,
                    AwardNumbers = form.awardNumbers
                };

                var awards =  _mapper.Map<List<FundingHoldAward>>(await _awards.BulkUpdateFundingHoldFlag(input));

                log.Debug("BulkFundingHoldChange: Exiting");
                return Ok(awards);
            }
            catch (Exception ex)
            {
                log.Error("BulkFundingHoldChange: Error on BulkFundingHoldChange - ", ex);
                return BadRequest("Error on BulkFundingHoldChange");
            }
        }

        [AllowAPIAccess(Roles = "CanEditFundingHold")]
        [HttpPatch]
        [Route("api/awards/{awardNumber}/fundingHold")]
        public async Task<IHttpActionResult> UpdateAwardFundingHold(FundingHoldForm form)
        {
            log.Debug("UpdateAwardFundingHold: Entering");

            try
            {
                AwardFundingHoldDTOInput input = _mapper.Map<AwardFundingHoldDTOInput>(form);
                input.Username = ((ClaimsIdentity) User.Identity).FindFirst(ClaimTypes.Sid).Value;

                await _awards.UpdateFundingHoldFlag(input);

                log.Debug("UpdateAwardFundingHold: Exiting");
                return Ok();
            }
            catch (Exception ex)
            {
                log.Error("UpdateAwardFundingHold: Error updating Award Funding Hold", ex);
                log.Debug("UpdateAwardFundingHold: Exiting");
                return BadRequest("Error updating Award Funding Hold");
            }
        }

        [AllowAPIAccess(Roles = "Admin")]
        [HttpGet]
        [Route("api/awards/owner/{username}")]
        public async Task<IHttpActionResult> GetAwardsByUser(string username)
        {
            log.Debug("GetAwardsByUser: Entering");

            try
            {
                var awards = _mapper.Map<List<ProfileAward>>(await _awards.GetAwardsByUser(username));

                log.Debug("GetAwardsByUser: Exiting");
                return Ok(awards);
            }
            catch (Exception ex)
            {
                log.Error("GetAwardsByUser: Error getting awards for user " +username, ex);
                log.Debug("GetAwardsByUser: Exiting");
                return BadRequest("Error getting awards");
            }
        }
    }
}
