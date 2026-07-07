import axios, { type AxiosResponse, type InternalAxiosRequestConfig } from 'axios';
import store from '@/store';
import { getToken } from '@/utils/auth';
import i18n from '@/lang';

const { t } = i18n.global;

const service = axios.create({
  baseURL: import.meta.env.VITE_BASE_API, 
  timeout: 60000 
});

const activeErrorMessages = new Set<string>();

function showErrorMsg(msg: string) {
  if (activeErrorMessages.has(msg)) {
    return;
  }
  activeErrorMessages.add(msg);
  ElMessage({
    message: msg,
    type: 'error',
    duration: 5 * 1000,
    onClose: () => {
      activeErrorMessages.delete(msg);
    }
  });
}

let isMessageBoxOpen = false;

function showUnauthorizedBox(msg: string) {
  if (isMessageBoxOpen) return;
  isMessageBoxOpen = true;

  ElMessageBox.confirm(msg, t('login.title'), {
    confirmButtonText: t('login.button'),
    cancelButtonText: t('notice.close'),
    type: 'warning'
  }).then(() => {
    store.user().resetToken();
    location.reload();
  }).finally(() => {
    isMessageBoxOpen = false;
  });
}

service.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {

    if (store.user().token && config.headers) {
      config.headers['Authorization'] = 'Bearer ' + getToken();
    }

    if (config.params) {
      const cleanedParams: Record<string, any> = {};
      Object.keys(config.params).forEach(key => {
        const value = config.params[key];
        if (value !== '' && value !== null && value !== undefined) {
          cleanedParams[key] = value;
        }
      });
      config.params = cleanedParams;
    }
    return config;
  },
  (error: any) => {

    return Promise.reject(error);
  }
);

service.interceptors.response.use(

  (response: AxiosResponse) => {
    const res = response.data;

    if (res.code !== 20000) {
      const message = res.message || t('apiErrors.unknown');

      if (res.code === 50008 || res.code === 50012 || res.code === 50014) {
        showUnauthorizedBox(t('apiErrors.unauthorized'));
      } else {

        showErrorMsg(message);
      }
      return Promise.reject(new Error(message));
    } else {
      return res;
    }
  },
  (error: any) => {

    let message = t('apiErrors.unknown');
    let isUnauthorized = false;

    if (error.response) {
      const status = error.response.status;
      switch (status) {
        case 400:
          message = t('apiErrors.badRequest');
          break;
        case 401:
          message = t('apiErrors.unauthorized');
          isUnauthorized = true;
          break;
        case 403:
          message = t('apiErrors.forbidden');
          break;
        case 404:
          message = t('apiErrors.notFound');
          break;
        case 500:
          message = t('apiErrors.serverError');
          break;
        default:
          message = error.message || t('apiErrors.unknown');
      }
    } else if (error.request) {

      message = t('apiErrors.networkError');
    } else {

      message = error.message || t('apiErrors.unknown');
    }

    if (isUnauthorized) {

      if (error.config && error.config.url && !error.config.url.includes('/auth/login')) {
        showUnauthorizedBox(message);
      } else {
        showErrorMsg(message);
      }
    } else {
      showErrorMsg(message);
    }
    return Promise.reject(error);
  }
);

export default service;
