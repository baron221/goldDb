import request from '@/utils/request';

export function getNotices(params?: any) {
  return request({
    url: '/notices',
    method: 'get',
    params
  });
}

export function getNotice(id: number) {
  return request({
    url: `/notices/${id}`,
    method: 'get'
  });
}

export function createNotice(data: any) {
  return request({
    url: '/notices',
    method: 'post',
    data
  });
}

export function updateNotice(data: any) {
  return request({
    url: '/notices',
    method: 'put',
    data
  });
}

export function deleteNotice(id: number) {
  return request({
    url: `/notices/${id}`,
    method: 'delete'
  });
}
