<template>
    <div>
        <div class="d-flex">
            <div class="filter-wrapper">
                <div class="d-flex">
                    <status-filter v-model="filter.OwlStatusCode"></status-filter>
                </div>
            </div>
        </div>
    </div>
</template>

<script>
    import { mapGetters } from 'vuex'

    const StatusFilter = () => import("@/Awards/_Common/AwardFilters/TransferRequestsStatusFilter.vue")

    export default {
        components: {
            'status-filter': StatusFilter,
        },
        props: {
        },
        data() {
            return {
                filter: {
                    OwlStatusCode:'A'
                }
            }
        },
        created: function () {
            this.updateFilter();
        },

        methods: {
            updateFilter: function () {
                this.$emit('input', this.filter);
            }
        },
    }

</script>