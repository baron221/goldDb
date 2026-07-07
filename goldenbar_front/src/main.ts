import { createApp } from 'vue';

import ElementPlus from 'element-plus';
import 'element-plus/dist/index.css';
import 'element-plus/theme-chalk/dark/css-vars.css';
import '@/styles/tailwind.css';
import App from './App.vue';
import router from './router';
import { setupStore } from './store';

import '@/styles/index.scss';
import SvgIcon from './icons'; 
import './permission'; 
import vPermission from './directive/permission/index'; 
import { checkEnableLogs } from './utils/error-log'; 
import CommonSelect from './components/CommonSelect/index.vue';
import CompanySearch from './components/CompanySearch/index.vue';
import i18n from './lang';
import '@fortawesome/fontawesome-free/css/all.css';

const app = createApp(App);
setupStore(app);
app.use(router);
app.use(i18n);
app.use(ElementPlus, {
  zIndex: 3000
});

app.component('svg-icon', SvgIcon);
app.component('common-select', CommonSelect);
app.component('company-search', CompanySearch);
app.directive('permission', vPermission);
checkEnableLogs(app);

app.mount('#app');
