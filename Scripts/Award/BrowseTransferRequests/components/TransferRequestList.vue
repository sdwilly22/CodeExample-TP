<template>
    <div>
        <div class="d-flex m-b-10">
            <div class="m-l-auto">
                <div>
                    <span id="searchLabel" class="hidden">Search</span>
                    <input type="text" class="form-control form-control-sm f-s-12 table-search" placeholder="Search" v-model.trim="filters.searchText" aria-labelledby="searchLabel" />
                </div>
            </div>
        </div>
        <transfer-requests-filters v-model="filters"></transfer-requests-filters>
        <br>
        <data-table :headers="headers" :items="filteredTransferRequests" :is-loading="isLoading" :searchText="filters.searchText" :init-pagination="pagination" showCount countType="Transfer Requests" item-key="RequestID">
            <template v-slot:row="props">
                <td class="text-center">
                    <div class="no-wrap">
                        {{ formatDateTime(props.item.RequestTime, fmtDateTime) }}
                    </div>
                </td>
                <td class="text-left">
                    <div class="no-wrap">
                        <a :href="'/Award/' + props.item.AwardNum">{{ props.item.AwardNum }}</a> ({{props.item.AwardVersionNum}})
                        <span v-if="props.item.IsBuyerDifferent || props.item.IsCODifferent" class="fas fa-asterisk pull-right fa-sm m-l-5 m-t-3" rel="tooltip" title="This BEAR record differs from data in PRISM">
                        </span>
                    </div>
                </td>

                <td class="text-left">{{ props.item.CustomerName }}</td>

                <td class="text-center">
                    <div class="no-wrap">
                        {{ formatDateTime(props.item.PopEndTime, fmtDate) }}
                    </div>
                </td>

                <td class="text-center">
                    <span :data-tooltip="isActionRequired(props.item) ? false : 'top'" :aria-label="getStatusToolTip(props.item)">
                       {{ getStatusDisplayText(props.item) }}
                    </span>
                </td>

                <td class="text-left">
                    <span v-if="props.item.IsContractOfficerTransferRequest" class="updating-table-text" data-tooltip="top" :aria-label="'Current CO: ' + props.item.OrigContractOfficerName">
                        <i class="fas fa-reply-all fa-flip-horizontal" :aria-label="'Transfer Pending to ' + props.item.ContractOfficerName + ' From ' + props.item.OrigContractOfficerName"></i> {{ props.item.ContractOfficerName }}
                    </span>
                    <span v-else>
                        {{ props.item.ContractOfficerName }}
                    </span>
                </td>
                <td class="text-left">
                    <span v-if="props.item.IsBuyerTransferRequest" class="updating-table-text" data-tooltip="top" :aria-label="'Current Buyer: ' + props.item.OrigBuyerName">
                        <i class="fas fa-reply-all fa-flip-horizontal" :aria-label="'Transfer Pending to ' + props.item.BuyerName + ' From ' + props.item.OrigBuyerName"></i> {{ props.item.BuyerName }}
                    </span>
                    <span v-else>
                        {{ props.item.BuyerName }}
                    </span>
                </td>
                <td class="text-center action f-s-15 v-align-middle">
                    <div class="no-wrap">
                        <span data-tooltip="top"
                              :aria-label="getNonConcurButtonToolTip(props.item)">
                            <button class="btn btn-sm action-icon nonconcur-icon"
                                    v-on:click="openNonconcurModal(props.item)"
                                    :disabled="!isActionRequired(props.item)"
                                    :aria-label="getNonConcurButtonToolTip(props.item)">
                                <i class="fas fa-times fa-inverse fa-lg"></i>
                            </button>
                        </span>
                        <span data-tooltip="top"
                              :aria-label="getConcurButtonToolTip(props.item)">
                            <button class="btn btn-sm action-icon concur-icon"
                                    v-on:click="openConcurModal(props.item)"
                                    :disabled="!isActionRequired(props.item)"
                                    :aria-label="getConcurButtonToolTip(props.item)">
                                <i class="fas fa-check fa-inverse fa-lg"></i>
                            </button>
                        </span>
                    </div>
                </td>
            </template>
            <template v-slot:rowDetails="props">
                <div class="p-t-15 p-b-15">
                    <div class="row m-0">
                        <div class="col-xs-6">
                            <div class="row">
                                <div class="col-xs-5 text-right">
                                    <span class="expand-row-label">Award Vendor</span>
                                </div>
                                <div class="col-xs-7">
                                    <span>{{ props.item.VendorName }}</span>
                                </div>
                            </div>

                            <div class="row">
                                <div class="col-xs-5 text-right">
                                    <span class="expand-row-label">Award Description</span>
                                </div>
                                <div class="col-xs-7">
                                    <span>{{ props.item.AwardDescription }}</span>
                                </div>
                            </div>

                            <div class="row">
                                <div class="col-xs-5 text-right">
                                    <span class="expand-row-label">PoP</span>
                                </div>
                                <div class="col-xs-7">
                                    <span>{{ formatDateTime(props.item.PopStartTime, fmtDate) }} through {{ formatDateTime(props.item.PopEndTime, fmtDate) }}</span>
                                </div>
                            </div>

                            <div class="row">
                                <div class="col-xs-5 text-right">
                                    <span class="expand-row-label">Requested By</span>
                                </div>
                                <div class="col-xs-7">
                                    <span>{{ props.item.RequestorFullName }}</span>
                                </div>
                            </div>

                            <div class="row">
                                <div class="col-xs-5 text-right">
                                    <span class="expand-row-label">Justification</span>
                                </div>
                                <div class="col-xs-7">
                                    <span>{{ props.item.Justification }}</span>
                                </div>
                            </div>
                        </div>
                        <div class="col-xs-5">
                            <div class="row">
                                <div class="col-xs-5 text-right">
                                    <span class="expand-row-label">PRISM CO</span>
                                </div>
                                <div class="col-xs-7">
                                    <span>{{ props.item.PRISMContractOfficerName }}</span>
                                </div>
                            </div>

                            <div class="row">
                                <div class="col-xs-5 text-right">
                                    <span class="expand-row-label">PRISM Buyer</span>
                                </div>
                                <div class="col-xs-7">
                                    <span>{{ props.item.PRISMBuyerName }}</span>
                                </div>
                            </div>
                        </div>
                        <div class="clearfix"></div>
                    </div>
                </div>
            </template>
        </data-table>
        <b-modal ref="request-modal" size="lg" :title="'Transfer for ' + transferRequestForm.AwardNum" class="request-modal">
            <div>
                <form-panel-section-item name="Decision">
                    <div class="transfer-modal-text" v-if="transferRequestForm.IsApproving">
                        Concur
                        <span class="fa-stack f-s-12 concur-icon">
                            <i class="fas fa-square fa-stack-2x"></i>
                            <i class="fas fa-check fa-stack-1x fa-inverse"></i>
                        </span>
                    </div>
                    <div class="transfer-modal-text" v-else>
                        Nonconcur
                        <span class="fa-stack f-s-12 nonconcur-icon">
                            <i class="fas fa-square fa-stack-2x"></i>
                            <i class="fas fa-times fa-stack-1x fa-inverse"></i>
                        </span>
                    </div>
                </form-panel-section-item>
                <form-panel-section-item :name="transferRequestModal.changeType + ' Change'">
                    <div class="transfer-modal-text">
                        <span>{{ transferRequestModal.previousUserFullName }}</span>
                        <span class="fas fa-reply-all fa-flip-horizontal m-l-10 p-t-3"></span>
                        <span class="m-l-10">{{ transferRequestModal.newUserFullName }}</span>
                    </div>
                </form-panel-section-item>
                <form-panel-section-item name="Justification">
                    <div class="transfer-modal-text">{{ transferRequestModal.requestJustification }}</div>
                </form-panel-section-item>
                <form-panel-section-item :name="transferRequestForm.IsApproving ? 'Approval Comments' : 'Justification for Rejection'" :required="!transferRequestForm.IsApproving">
                    <textarea rows="4" maxlength="500" v-model.trim="transferRequestForm.DecisionJustification" :class="[{'has-error' : transferRequestResponseFormErrors.DecisionJustification}, 'form-control', 'form-control-sm', 'f-s-12']"></textarea>
                    <template v-slot:customErrorMessage>
                        {{ transferRequestResponseFormErrors.DecisionJustification }}
                    </template>
                </form-panel-section-item>
            </div> <!-- Modal Content End -->
            <template #modal-footer>
                <div v-show="canOverrideTransferEmails">
                    <input id="chkBox" type="checkbox" v-model="transferRequestForm.NotifyUsersOfTransfer" />
                    <label for="chkBox" class="label-pad">Send Notifications</label>
                </div>
                <button type="button" @click="dismissTransferModal" class="btn btn-default m-t-0 m-b-0">Close</button>
                <button type="button" @click="updateTransferRequest" class="btn btn-primary m-t-0 m-b-0">Submit</button>
            </template>
        </b-modal>
    </div>
</template>


<script>
    import { mapGetters } from 'vuex'
    import moment from 'moment'

    const DataTable = () => import("@/Shared/VueComponents/Table/ExpandableDataTable.vue")
    const FormPanelSectionItem = () => import("@/Shared/VueComponents/Form/FormPanelSectionItem.vue")
    import { BModal } from 'bootstrap-vue'
    const Filters = () => import("./TransferRequestListFilters.vue")

    export default {
        components: {
            'data-table': DataTable,
            'b-modal': BModal,
            'form-panel-section-item': FormPanelSectionItem,
            'transfer-requests-filters' : Filters,
        },
        data() {
            return {
                fmtDateTime: 'YYYY-MM-DD h:mm A',
                fmtDate: 'YYYY-MM-DD',
                isLoading: true,
                filters: {},
                pagination: {
                    page: 1,
                    sortBy: 'RequestTime',
                    descending: true,
                    rowsPerPage: 10
                },
                headers: [
                    {
                        text: 'Req. Date',
                        align: 'center',
                        sortable: true,
                        value: 'RequestTime',
                    },
                    {
                        text: 'Award #',
                        align: 'center',
                        sortable: true,
                        value: 'AwardNum'
                    },
                    {
                        text: 'Customer',
                        align: 'center',
                        sortable: true,
                        value: 'CustomerName'
                    },
                    {
                        text: 'PoP End',
                        align: 'center',
                        sortable: true,
                        value: 'PopEndTime'
                    },
                    {
                        text: 'Status',
                        align: 'center',
                        sortable: true,
                        value: 'OwlStatusCode'
                    },
                    {
                        text: 'CO',
                        align: 'center',
                        sortable: true,
                        value: 'ContractOfficerName'
                    },
                    {
                        text: 'Buyer',
                        align: 'center',
                        sortable: true,
                        value: 'BuyerName'
                    },
                    {
                        text: 'Action',
                        align: 'center',
                        sortable: false,
                        classes: 'action-dropdown-identifier',
                        value: '',
                    },
                ],
                transferRequests: [],
                transferRequestModal: {
                    changeType: '',
                    previousUserFullName: '',
                    newUserFullName: '',
                    requestJustification: '',
                },
                transferRequestForm: {
                    IsApproving: false,
                    AwardNum: '',
                    IsBuyerChange: false,
                    DecisionJustification: '',
                    NotifyUsersOfTransfer: false,
                },
                transferRequestResponseFormErrors: {}
            }
        },
        computed: {
            ...mapGetters([
                'canOverrideTransferEmails'
            ]),
            filteredTransferRequests: function () {
                return this.transferRequests.filter(transferRequests => {
                    return (this.filters.OwlStatusCode === '' || this.filters.OwlStatusCode === transferRequests.OwlStatusCode)
                });
            },
        },
        created: function () {
            this.getTransferRequests();
        },

        methods: {
            getTransferRequests: function () {
                this.$http.get('/api/awards/transferRequests')
                    .then((response) => {
                        this.transferRequests = response.data;
                    }).catch((error) => {
                        console.log("getTransferRequests: error");
                        console.log(error);
                    }).then(() => {
                        // always executed
                        this.isLoading = false;
                    });
            },

            isActionRequired: function (item) {
                return item.OwlStatusCode === 'A';
            },

            getStatusToolTip: function (awardObject) {
                if (awardObject.OwlStatusCode === 'A') {
                    return "";
                }
                else if (awardObject.OwlStatusCode === 'S') {
                    return "No action required. Superseded by the most recent released modification.";
                }
                else if (awardObject.OwlStatusCode === 'C' || awardObject.OwlStatusCode === 'N') {
                    const actionText = (awardObject.OwlStatusCode === 'C') ? 'Concurred' : 'Non Concurred';
                    const concurDisplayDate = awardObject.ConcurTime ? moment(awardObject.ConcurTime).format(this.fmtDate) : null;

                    if (awardObject.ConcurUserName && awardObject.ConcurTime) {
                        //Name and Date
                        return actionText + " by " + awardObject.ConcurUserName + " on " + concurDisplayDate;
                    }
                    else if (awardObject.ConcurTime) {
                        //Date and No Name
                        return actionText + " by CO or Buyer on " + concurDisplayDate;
                    }
                    else if (awardObject.ConcurUserName) {
                        //Name and No Date
                        return actionText + " by " + awardObject.ConcurUserName;
                    }
                    else {
                        // No Name and No Date
                        return actionText;
                    }
                }
                else {
                    return "";
                }
            },

            formatDateTime: function (dateTime, formatString) {
                return moment(dateTime).format(formatString);
            },

            getStatusDisplayText: function (awardObject) {
                if (awardObject.OwlStatusCode === 'A') {
                    return "Action Required";
                }
                if (awardObject.OwlStatusCode === 'C') {
                    return "Concurred";
                }
                if (awardObject.OwlStatusCode === 'N') {
                    return "Non Concurred";
                }
                if (awardObject.OwlStatusCode === 'S') {
                    return "Superseded";
                }
                else {
                    return "No Status";
                }
            },

            getNonConcurButtonToolTip: function (awardObject) {
                if (this.isActionRequired(awardObject)) {
                    return "Non Concur";
                }
                else {
                    return "No Action Required";
                }

            },

            getConcurButtonToolTip: function (awardObject) {
                if (this.isActionRequired(awardObject)) {
                    return "Concur";
                }
                else {
                    return "No Action Required";
                }
            },

            openModal: function (awardObject) {
                // Set Transfer Request Form Data
                this.transferRequestForm.AwardNum = awardObject.AwardNum;
                this.transferRequestForm.IsBuyerChange = awardObject.IsBuyerTransferRequest;
                this.transferRequestForm.DecisionJustification = '';

                // Set Modal Display Data
                this.transferRequestModal.changeType = awardObject.IsBuyerTransferRequest ? 'Buyer' : awardObject.IsContractOfficerTransferRequest ? 'CO' : '';
                this.transferRequestModal.previousUserFullName = awardObject.IsBuyerTransferRequest ? awardObject.OrigBuyerName : awardObject.IsContractOfficerTransferRequest ? awardObject.OrigContractOfficerName : '';
                this.transferRequestModal.newUserFullName = awardObject.IsBuyerTransferRequest ? awardObject.BuyerName : awardObject.IsContractOfficerTransferRequest ? awardObject.ContractOfficerName : '';
                this.transferRequestModal.requestJustification = awardObject.Justification;
                // Reset Transfer Request Form Errors
                this.transferRequestResponseFormErrors = {};

                // Show Modal
                this.$refs['request-modal'].show();
            },

            dismissTransferModal: function () {
                // Clear Transfer Request Form Data
                this.transferRequestForm.AwardNum = '';
                this.transferRequestForm.DecisionJustification = '';

                // Clear Modal Display Data
                this.transferRequestModal.changeType = '';
                this.transferRequestModal.previousUserFullName = '';
                this.transferRequestModal.newUserFullName = '';
                this.transferRequestModal.requestJustification = '';

                // Clear Transfer Request Form Errors
                this.transferRequestResponseFormErrors = {};

                // Hide Modal
                this.closeModal();
            },

            closeModal: function () {
                this.$refs['request-modal'].hide();
            },

            openConcurModal: function (awardObject) {
                if (awardObject.OwlStatusCode !== 'A') {
                    return false;
                }

                this.transferRequestForm.IsApproving = true;

                this.openModal(awardObject);
            },

            openNonconcurModal: function (awardObject) {
                if (awardObject.OwlStatusCode !== 'A') {
                    return false;
                }
                this.transferRequestForm.IsApproving = false;

                this.openModal(awardObject);
            },

            updateTransferRequest: function () {
                if (this.validateTransferRequest()) {
                    //close modal
                    this.closeModal();
                    this.submitTransferRequestResponse();
                }
            },

            validateTransferRequest: function () {
                this.transferRequestResponseFormErrors = {};

                if (!this.transferRequestForm.IsApproving && this.transferRequestForm.DecisionJustification.length <= 0) {
                    Vue.set(this.transferRequestResponseFormErrors, 'DecisionJustification', 'A Justification is required for a Nonconcur Decision');
                }

                return Object.keys(this.transferRequestResponseFormErrors).length === 0;
            },

            submitTransferRequestResponse: function () {

                this.$http.put('/api/awards/transferRequests/approveDenyRequest', this.transferRequestForm)
                    .then((response) => {
                        window.location.replace(`/Workload/TransferRequests?successMsg=${this.transferRequestForm.AwardNum}%20updated%20successfully`);
                    }).catch((error) => {
                        ShowErrorMessage("Error On Update");
                        console.log("submitTransferRequestResponse: error");
                        console.log(error);
                        this.dismissTransferModal();
                    });
            }
        }
    }
</script>

<style scoped>
    @import '/Content/custom-vue-bootstrap.css';

    .transfer-modal-text {
        padding-top: 7px;
    }

    .updating-table-text {
        padding-top: 7px;
        color: #2E6CA3;
    }

    .has-error {
        border-color: #f40600 !important;
    }

    .table-search {
        align-self: flex-end;
        min-width: 225px;
        height: 30px;
        padding: 5px;
    }

    .no-wrap {
        min-width: max-content;
    }

    .nonconcur-icon.action-icon {
        background-color: firebrick;
    }

    .concur-icon.action-icon {
        background-color: darkgreen;
        padding-left: 8px;
        padding-right: 7px;
    }

    .nonconcur-icon .fa-square {
        color: firebrick;
    }

    .concur-icon .fa-square {
        color: darkgreen;
    }

    .action-icon:disabled {
        background-color: gray;
    }

    .transfer-modal-text > .fa-stack {
        margin-top: -4px;
    }

    .label-pad {
    float: right;
    margin: 3px 0px;
    padding-right: 310px;
    padding-left: 5px;
}
</style>