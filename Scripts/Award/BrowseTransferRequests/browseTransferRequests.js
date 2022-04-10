import Vue from 'vue'
import axios from 'axios'
import { store } from '../../Shared/Data/Store'
import { BootstrapVue } from 'bootstrap-vue'

// Install BootstrapVue
Vue.use(BootstrapVue)

Vue.prototype.$http = axios;
window.Vue = Vue;

import TransferRequests from './view/BrowseTransferRequests.vue'; 

new Vue({
    store,
    render: h => h(TransferRequests)
}).$mount('#browseTransferRequestsWrapper');