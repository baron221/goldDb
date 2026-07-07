export function parseTime(time: any, cFormat: any) {
  if (arguments.length === 0 || !time) {
    return null;
  }
  const format = cFormat || '{y}-{m}-{d} {h}:{i}:{s}';
  let date;
  if (typeof time === 'object') {
    date = time;
  } else {
    if ((typeof time === 'string')) {
      if ((/^[0-9]+$/.test(time))) {

        time = parseInt(time);
      } else {

        if (typeof time === 'string' && !time.includes('T')) {
          time = time.replace(new RegExp(/-/gm), '/');
        }
      }
    }

    if ((typeof time === 'number') && (time.toString().length === 10)) {
      time = time * 1000;
    }
    date = new Date(time);
  }
  const formatObj = {
    y: date.getFullYear(),
    m: date.getMonth() + 1,
    d: date.getDate(),
    h: date.getHours(),
    i: date.getMinutes(),
    s: date.getSeconds(),
    a: date.getDay()
  };
  const time_str = format.replace(/{([ymdhisa])+}/g, (result, key) => {
    const value = formatObj[key];

    if (key === 'a') { return ['일', '월', '화', '수', '목', '금', '토'][value]; }
    return value.toString().padStart(2, '0');
  });
  return time_str;
}

export function formatTime(time: any, option: any) {
  if (('' + time).length === 10) {
    time = parseInt(time) * 1000;
  } else {
    time = +time;
  }
  const d = new Date(time);
  const now = Date.now();

  const diff = (now - d.getTime()) / 1000;

  if (diff < 30) {
    return '방금 전';
  } else if (diff < 3600) {

    return Math.ceil(diff / 60) + '분 전';
  } else if (diff < 3600 * 24) {
    return Math.ceil(diff / 3600) + '시간 전';
  } else if (diff < 3600 * 24 * 2) {
    return '1일 전';
  }
  if (option) {
    return parseTime(time, option);
  } else {
    return (
      d.getMonth() +
      1 +
      '월 ' +
      d.getDate() +
      '일 ' +
      d.getHours() +
      '시 ' +
      d.getMinutes() +
      '분'
    );
  }
}

export function getQueryObject(url: string) {
  url = url == null ? window.location.href : url;
  const search = url.substring(url.lastIndexOf('?') + 1);
  const obj = {};
  const reg = /([^?&=]+)=([^?&=]*)/g;
  search.replace(reg, (rs, $1, $2) => {
    const name = decodeURIComponent($1);
    let val = decodeURIComponent($2);
    val = String(val);
    obj[name] = val;
    return rs;
  });
  return obj;
}

export function byteLength(str: any) {
  let s = str.length;
  for (let i = str.length - 1; i >= 0; i--) {
    const code = str.charCodeAt(i);
    if (code > 0x7f && code <= 0x7ff) s++;
    else if (code > 0x7ff && code <= 0xffff) s += 2;
    if (code >= 0xDC00 && code <= 0xDFFF) i--;
  }
  return s;
}

export function cleanArray(actual: any) {
  const newArray = [];
  for (let i = 0; i < actual.length; i++) {
    if (actual[i]) {
      newArray.push(actual[i]);
    }
  }
  return newArray;
}

export function param(json: any) {
  if (!json) return '';
  return cleanArray(
    Object.keys(json).map(key => {
      if (json[key] === undefined) return '';
      return encodeURIComponent(key) + '=' + encodeURIComponent(json[key]);
    })
  ).join('&');
}

export function param2Obj(url: string) {
  const search = decodeURIComponent(url.split('?')[1]).replace(/\+/g, ' ');
  if (!search) {
    return {};
  }
  const obj = {};
  const searchArr = search.split('&');
  searchArr.forEach(v => {
    const index = v.indexOf('=');
    if (index !== -1) {
      const name = v.substring(0, index);
      const val = v.substring(index + 1, v.length);
      obj[name] = val;
    }
  });
  return obj;
}

export function html2Text(val: any) {
  const div = document.createElement('div');
  div.innerHTML = val;
  return div.textContent || div.innerText;
}

export function objectMerge(target: any, source: any) {
  if (typeof target !== 'object') {
    target = {};
  }
  if (Array.isArray(source)) {
    return source.slice();
  }
  Object.keys(source).forEach(property => {
    const sourceProperty = source[property];
    if (typeof sourceProperty === 'object') {
      target[property] = objectMerge(target[property], sourceProperty);
    } else {
      target[property] = sourceProperty;
    }
  });
  return target;
}

export function toggleClass(element: any, className: any) {
  if (!element || !className) {
    return;
  }
  let classString = element.className;
  const nameIndex = classString.indexOf(className);
  if (nameIndex === -1) {
    classString += '' + className;
  } else {
    classString =
      classString.substr(0, nameIndex) +
      classString.substr(nameIndex + className.length);
  }
  element.className = classString;
}

export function getTime(type: any) {
  if (type === 'start') {
    return new Date().getTime() - 3600 * 1000 * 24 * 90;
  } else {
    return new Date(new Date().toDateString());
  }
}

export function debounce(func: any, wait: any, immediate: any) {
  let timeout, args, context, timestamp, result;

  const later = function() {
    const last = +new Date() - timestamp;

    if (last < wait && last > 0) {
      timeout = setTimeout(later, wait - last);
    } else {
      timeout = null;
      if (!immediate) {
        result = func.apply(context, args);
        if (!timeout) context = args = null;
      }
    }
  };

  return function(this: any, ...args: any) {

    context = this;
    timestamp = +new Date();
    const callNow = immediate && !timeout;
    if (!timeout) timeout = setTimeout(later, wait);
    if (callNow) {
      result = func.apply(context, args);
      context = args = null;
    }

    return result;
  };
}

export function deepClone(source: any) {
  if (!source && typeof source !== 'object') {
    throw new Error('error arguments deepClone');
  }
  const targetObj = source.constructor === Array ? [] : {};
  Object.keys(source).forEach(keys => {
    if (source[keys] && typeof source[keys] === 'object') {
      targetObj[keys] = deepClone(source[keys]);
    } else {
      targetObj[keys] = source[keys];
    }
  });
  return targetObj;
}

export function uniqueArr(arr: any) {
  return Array.from(new Set(arr));
}

export function createUniqueString() {
  const timestamp = +new Date() + '';
  const randomNum = parseInt((1 + Math.random()) * 65536 + '') + '';
  return (+(randomNum + timestamp)).toString(32);
}

export function hasClass(ele: any, cls: any) {
  return !!ele.className.match(new RegExp('(\\s|^)' + cls + '(\\s|$)'));
}

export function addClass(ele: any, cls: any) {
  if (!hasClass(ele, cls)) ele.className += ' ' + cls;
}

export function removeClass(ele: any, cls: any) {
  if (hasClass(ele, cls)) {
    const reg = new RegExp('(\\s|^)' + cls + '(\\s|$)');
    ele.className = ele.className.replace(reg, ' ');
  }
}

export function getThumbnailUrl(url: string, version = 'thumb') {
  if (!url) return undefined;
  if (url.includes(`/${version}/`)) return url;

  const lastSlashIndex = url.lastIndexOf('/');
  if (lastSlashIndex === -1) return url;

  const path = url.substring(0, lastSlashIndex);
  const fileName = url.substring(lastSlashIndex + 1);

  return `${path}/${version}/${fileName}`;
}
