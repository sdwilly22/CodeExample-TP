<template>
    <div>
        <div class="page-header-wrapper">
            <h1 class="page-header">
                Transfer Requests
                <sup rel="tooltip" title="Transfer Request Page Tour" class="cursor-pointer"><span class="fas fa-paw fa-sm" id="pageTour" aria-label="Click here to begin transfer request page tour" tabindex="0"></span></sup>
            </h1>
            <ol class="breadcrumb pull-right">
                <li><a href="/">Home</a></li>
                <li class="active">Transfer Requests</li>
            </ol>
        </div>

        <transfer-request-list-panel></transfer-request-list-panel>
    </div>
</template>

<script> 
    const TransferRequestListPanel = () => import("@/Awards/BrowseTransferRequests/components/TransferRequestListPanel.vue")

    export default {
        components: {
            'transfer-request-list-panel': TransferRequestListPanel,
        },
    }
</script>