import { createI18n } from 'vue-i18n';
import en from './en';
import ko from './ko';

const messages = {
  en,
  ko
};

const i18n = createI18n({
  legacy: false, 
  locale: localStorage.getItem('lang') || 'ko', 
  fallbackLocale: 'en',
  messages
});

export default i18n;
