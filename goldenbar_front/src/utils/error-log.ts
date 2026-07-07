import store from '@/store';
import { isString, isArray } from '@/utils/validate';
import settings from '@/settings';
import { nextTick } from 'vue';
import type { App } from 'vue';

const { errorLog: needErrorLog } = settings;

function checkNeed() {
  const env = import.meta.env.VITE_ENV || '';
  if (isString(needErrorLog)) {
    return env === needErrorLog;
  }
  if (isArray(needErrorLog)) {
    return needErrorLog.includes(env);
  }
  return false;
}

export function checkEnableLogs(app: App) {
  if (checkNeed()) {
    app.config.errorHandler = function(err, instance, info) {

      nextTick(() => {
        store.errorLog().addErrorLog({
          err,
          instance,
          info,
          url: window.location.href
        });
        console.error(err, info);
      });
    };
  }
}
