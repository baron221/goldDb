import request from '@/utils/request';

export function getContacts(params: any) {
  return request({
    url: '/admin/contacts',
    method: 'get',
    params
  });
}

export function processContact(id: number, data: { isProcessed: boolean, adminMemo?: string }) {
  return request({
    url: `/admin/contacts/${id}/process`,
    method: 'put',
    data
  });
}

export function deleteContact(id: number) {
  return request({
    url: `/admin/contacts/${id}`,
    method: 'delete'
  });
}
