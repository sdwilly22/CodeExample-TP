<template>
	<panel :hideHeading="true">
		<transfer-request-list class="p-l-10 p-r-10"></transfer-request-list>
	</panel>
</template> 

<script>
    const Panel = () => import("@/Shared/VueComponents/Page/Panel.vue")
    const TransferRequestList = () => import("./TransferRequestList.vue")

    export default {
        components: {
            'panel': Panel,
            'transfer-request-list': TransferRequestList,
        },
    }
</script>