import request from '@/utils/request';

export function uploadFile(formData: FormData) {
  return request({
    url: '/file/upload',
    method: 'post',
    data: formData,
    headers: {
      'Content-Type': 'multipart/form-data'
    }
  });
}

export function uploadImage(formData: FormData) {
  return request({
    url: '/file/upload-image',
    method: 'post',
    data: formData,
    headers: {
      'Content-Type': 'multipart/form-data'
    }
  });
}
