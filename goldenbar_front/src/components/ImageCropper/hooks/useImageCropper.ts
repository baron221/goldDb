import { ref, reactive, computed, watch, onMounted, onUnmounted } from 'vue';
import request from '@/utils/request';
import language from '../utils/language.js';
import mimes from '../utils/mimes.js';
import data2blob from '../utils/data2blob.js';

export function useImageCropper(props: any, emit: any, canvasRef: any) {
  const allowImgFormat = ['jpg', 'png'];
  const tempImgFormat = allowImgFormat.indexOf(props.imgFormatRaw) === -1 ? 'jpg' : props.imgFormatRaw;
  const lang = language[props.langType] ? language[props.langType] : language['en'];
  const mime = mimes[tempImgFormat];
  if (props.langExt) {
    Object.assign(lang, props.langExt);
  }

  let isSupported = true;
  if (typeof FormData !== 'function') {
    isSupported = false;
  }

  const step = ref(1);
  const loading = ref(0);
  const progress = ref(0);
  const hasError = ref(false);
  const errorMsg = ref('');
  const ratio = computed(() => props.width / props.height);
  const sourceImg = ref<HTMLImageElement | null>(null);
  const sourceImgUrl = ref('');
  const createImgUrl = ref('');

  const isSupportTouch = ref(document.hasOwnProperty('ontouchstart'));

  const sourceImgMouseDown = reactive({
    on: false,
    mX: 0,
    mY: 0,
    x: 0,
    y: 0
  });

  const previewContainer = reactive({
    width: 100,
    height: 100
  });

  const sourceImgContainer = reactive({
    width: 240,
    height: 184
  });

  const scale = reactive({
    zoomAddOn: false,
    zoomSubOn: false,
    range: 1,
    rotateLeft: false,
    rotateRight: false,
    degree: 0,
    x: 0,
    y: 0,
    width: 0,
    height: 0,
    maxWidth: 0,
    maxHeight: 0,
    minWidth: 0,
    minHeight: 0,
    naturalWidth: 0,
    naturalHeight: 0
  });

  const progressStyle = computed(() => {
    return {
      width: progress.value + '%'
    };
  });

  const sourceImgMasking = computed(() => {
    const sic = sourceImgContainer;
    const sicRatio = sic.width / sic.height;
    let x = 0;
    let y = 0;
    let w = sic.width;
    let h = sic.height;
    let s = 1;
    if (ratio.value < sicRatio) {
      s = sic.height / props.height;
      w = sic.height * ratio.value;
      x = (sic.width - w) / 2;
    }
    if (ratio.value > sicRatio) {
      s = sic.width / props.width;
      h = sic.width / ratio.value;
      y = (sic.height - h) / 2;
    }
    return {
      scale: s,
      x,
      y,
      width: w,
      height: h
    };
  });

  const sourceImgStyle = computed(() => {
    const top = scale.y + sourceImgMasking.value.y + 'px';
    const left = scale.x + sourceImgMasking.value.x + 'px';
    return {
      top,
      left,
      width: scale.width + 'px',
      height: scale.height + 'px',
      transform: 'rotate(' + scale.degree + 'deg)',
      '-ms-transform': 'rotate(' + scale.degree + 'deg)',
      '-moz-transform': 'rotate(' + scale.degree + 'deg)',
      '-webkit-transform': 'rotate(' + scale.degree + 'deg)',
      '-o-transform': 'rotate(' + scale.degree + 'deg)'
    };
  });

  const sourceImgShadeStyle = computed(() => {
    const sic = sourceImgContainer;
    const sim = sourceImgMasking.value;
    const w = sim.width === sic.width ? sim.width : (sic.width - sim.width) / 2;
    const h = sim.height === sic.height ? sim.height : (sic.height - sim.height) / 2;
    return {
      width: w + 'px',
      height: h + 'px'
    };
  });

  const previewStyle = computed(() => {
    const pc = previewContainer;
    let w = pc.width;
    let h = pc.height;
    const pcRatio = w / h;
    if (ratio.value < pcRatio) {
      w = pc.height * ratio.value;
    }
    if (ratio.value > pcRatio) {
      h = pc.width / ratio.value;
    }
    return {
      width: w + 'px',
      height: h + 'px'
    };
  });

  const reset = () => {
    loading.value = 0;
    hasError.value = false;
    errorMsg.value = '';
    progress.value = 0;
  };

  const off = () => {
    setTimeout(() => {
      emit('input', false);
      emit('close');
      if (step.value === 3 && loading.value === 2) {
        setStep(1);
      }
    }, 200);
  };

  const setStep = (no: number) => {
    setTimeout(() => {
      step.value = no;
    }, 200);
  };

  const checkFile = (file: File) => {
    if (file.type.indexOf('image') === -1) {
      hasError.value = true;
      errorMsg.value = lang.error.onlyImg;
      return false;
    }
    if (file.size / 1024 > props.maxSize) {
      hasError.value = true;
      errorMsg.value = lang.error.outOfSize + props.maxSize + 'kb';
      return false;
    }
    return true;
  };

  const onFileSelected = (file: File) => {
    reset();
    if (checkFile(file)) {
      setSourceImg(file);
    }
  };

  const setSourceImg = (file: File) => {
    const fr = new FileReader();
    fr.onload = () => {
      sourceImgUrl.value = fr.result as string;
      startCrop();
    };
    fr.readAsDataURL(file);
  };

  const createImg = (e?: number) => {
    const canvas = canvasRef.value;
    if (!canvas || !sourceImg.value) return;
    const ctx = canvas.getContext('2d');
    if (e) {
      sourceImgMouseDown.on = false;
    }
    canvas.width = props.width;
    canvas.height = props.height;
    ctx.clearRect(0, 0, props.width, props.height);
    ctx.fillStyle = '#fff';
    ctx.fillRect(0, 0, props.width, props.height);
    ctx.translate(props.width * 0.5, props.height * 0.5);
    ctx.rotate((Math.PI * scale.degree) / 180);
    ctx.translate(-props.width * 0.5, -props.height * 0.5);
    ctx.drawImage(
      sourceImg.value,
      scale.x / sourceImgMasking.value.scale,
      scale.y / sourceImgMasking.value.scale,
      scale.width / sourceImgMasking.value.scale,
      scale.height / sourceImgMasking.value.scale
    );
    createImgUrl.value = canvas.toDataURL(mime);
  };

  const startCrop = () => {
    const sim = sourceImgMasking.value;
    const img = new Image();
    img.src = sourceImgUrl.value;
    img.onload = () => {
      const nWidth = img.naturalWidth;
      const nHeight = img.naturalHeight;
      const nRatio = nWidth / nHeight;
      let w = sim.width;
      let h = sim.height;
      let x = 0;
      let y = 0;
      if (nWidth < props.width || nHeight < props.height) {
        hasError.value = true;
        errorMsg.value = lang.error.lowestPx + props.width + '*' + props.height;
        return false;
      }
      if (ratio.value > nRatio) {
        h = w / nRatio;
        y = (sim.height - h) / 2;
      }
      if (ratio.value < nRatio) {
        w = h * nRatio;
        x = (sim.width - w) / 2;
      }
      scale.range = 0;
      scale.x = x;
      scale.y = y;
      scale.width = w;
      scale.height = h;
      scale.degree = 0;
      scale.minWidth = w;
      scale.minHeight = h;
      scale.maxWidth = nWidth * sim.scale;
      scale.maxHeight = nHeight * sim.scale;
      scale.naturalWidth = nWidth;
      scale.naturalHeight = nHeight;
      sourceImg.value = img;
      createImg();
      setStep(2);
    };
  };

  const imgStartMove = (e: any) => {
    e.preventDefault();
    if (isSupportTouch.value && !e.targetTouches) {
      return false;
    }
    const et = e.targetTouches ? e.targetTouches[0] : e;
    sourceImgMouseDown.mX = et.screenX;
    sourceImgMouseDown.mY = et.screenY;
    sourceImgMouseDown.x = scale.x;
    sourceImgMouseDown.y = scale.y;
    sourceImgMouseDown.on = true;
  };

  const imgMove = (e: any) => {
    e.preventDefault();
    if (isSupportTouch.value && !e.targetTouches) {
      return false;
    }
    const et = e.targetTouches ? e.targetTouches[0] : e;
    const sim = sourceImgMasking.value;
    const nX = et.screenX;
    const nY = et.screenY;
    const dX = nX - sourceImgMouseDown.mX;
    const dY = nY - sourceImgMouseDown.mY;
    let rX = sourceImgMouseDown.x + dX;
    let rY = sourceImgMouseDown.y + dY;
    if (!sourceImgMouseDown.on) return;
    if (rX > 0) {
      rX = 0;
    }
    if (rY > 0) {
      rY = 0;
    }
    if (rX < sim.width - scale.width) {
      rX = sim.width - scale.width;
    }
    if (rY < sim.height - scale.height) {
      rY = sim.height - scale.height;
    }
    scale.x = rX;
    scale.y = rY;
  };

  const startRotateRight = () => {
    scale.rotateRight = true;
    const rotate = () => {
      if (scale.rotateRight) {
        const degree = ++scale.degree;
        createImg(degree);
        setTimeout(() => {
          rotate();
        }, 60);
      }
    };
    rotate();
  };

  const startRotateLeft = () => {
    scale.rotateLeft = true;
    const rotate = () => {
      if (scale.rotateLeft) {
        const degree = --scale.degree;
        createImg(degree);
        setTimeout(() => {
          rotate();
        }, 60);
      }
    };
    rotate();
  };

  const endRotate = () => {
    scale.rotateLeft = false;
    scale.rotateRight = false;
  };

  const zoomImg = (newRange: number) => {
    const sim = sourceImgMasking.value;
    const {
      maxWidth,
      maxHeight,
      minWidth,
      minHeight,
      width,
      height,
      x,
      y
    } = scale;
    const sWidth = sim.width;
    const sHeight = sim.height;
    const nWidth = minWidth + ((maxWidth - minWidth) * newRange) / 100;
    const nHeight = minHeight + ((maxHeight - minHeight) * newRange) / 100;
    let nX = sWidth / 2 - (nWidth / width) * (sWidth / 2 - x);
    let nY = sHeight / 2 - (nHeight / height) * (sHeight / 2 - y);
    if (nX > 0) {
      nX = 0;
    }
    if (nY > 0) {
      nY = 0;
    }
    if (nX < sWidth - nWidth) {
      nX = sWidth - nWidth;
    }
    if (nY < sHeight - nHeight) {
      nY = sHeight - nHeight;
    }
    scale.x = nX;
    scale.y = nY;
    scale.width = nWidth;
    scale.height = nHeight;
    scale.range = newRange;
    setTimeout(() => {
      if (scale.range === newRange) {
        createImg();
      }
    }, 300);
  };

  const startZoomAdd = () => {
    scale.zoomAddOn = true;
    const zoom = () => {
      if (scale.zoomAddOn) {
        const range = scale.range >= 100 ? 100 : ++scale.range;
        zoomImg(range);
        setTimeout(() => {
          zoom();
        }, 60);
      }
    };
    zoom();
  };

  const endZoomAdd = () => {
    scale.zoomAddOn = false;
  };

  const startZoomSub = () => {
    scale.zoomSubOn = true;
    const zoom = () => {
      if (scale.zoomSubOn) {
        const range = scale.range <= 0 ? 0 : --scale.range;
        zoomImg(range);
        setTimeout(() => {
          zoom();
        }, 60);
      }
    };
    zoom();
  };

  const endZoomSub = () => {
    scale.zoomSubOn = false;
  };

  const zoomChange = (e: any) => {
    zoomImg(Number(e.target.value));
  };

  const upload = () => {
    const fmData = new FormData();
    fmData.append(
      props.field,
      data2blob(createImgUrl.value, mime),
      props.field + '.' + imgFormat.value
    );
    if (typeof props.params === 'object' && props.params) {
      Object.keys(props.params).forEach(k => {
        fmData.append(k, props.params[k]);
      });
    }
    reset();
    loading.value = 1;
    setStep(3);
    request({
      url: props.url,
      method: 'post',
      data: fmData
    })
      .then(resData => {
        loading.value = 2;
        emit('crop-upload-success', resData.data);
      })
      .catch(err => {
        if (props.value) {
          loading.value = 3;
          hasError.value = true;
          errorMsg.value = lang.fail;
          emit('crop-upload-fail', err, props.field, props.ki);
        }
      });
  };

  const prepareUpload = () => {
    emit('crop-success', createImgUrl.value, props.field, props.ki);
    if (typeof props.url === 'string' && props.url) {
      upload();
    } else {
      off();
    }
  };

  const closeHandler = (e: KeyboardEvent) => {
    if (props.value && (e.key === 'Escape' || e.keyCode === 27)) {
      off();
    }
  };

  const imgFormat = computed(() => tempImgFormat);

  watch(() => props.value, (newValue) => {
    if (newValue && loading.value !== 1) {
      reset();
    }
  });

  onMounted(() => {
    document.addEventListener('keyup', closeHandler);
  });

  onUnmounted(() => {
    document.removeEventListener('keyup', closeHandler);
  });

  return {
    lang,
    isSupported,
    step,
    loading,
    hasError,
    errorMsg,
    sourceImgUrl,
    createImgUrl,
    scale,
    progressStyle,
    sourceImgStyle,
    sourceImgShadeStyle,
    previewStyle,
    off,
    setStep,
    onFileSelected,
    imgStartMove,
    imgMove,
    createImg,
    zoomChange,
    startZoomSub,
    endZoomSub,
    startZoomAdd,
    endZoomAdd,
    startRotateLeft,
    startRotateRight,
    endRotate,
    prepareUpload
  };
}
