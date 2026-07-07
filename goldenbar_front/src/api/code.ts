import request from '@/utils/request';

export function getCodes() {
  return request({
    url: '/code',
    method: 'get'
  });
}

export function getCodesByGroup(groupCode: any) {
  return request({
    url: `/code/group/${groupCode}`,
    method: 'get'
  });
}

export function getCodesByParentId(parentId: any) {
  return request({
    url: `/code/parent/${parentId}`,
    method: 'get'
  });
}

export function saveCode(data: any) {
  return request({
    url: '/code',
    method: 'post',
    data
  });
}

export function deleteCode(id: number | string) {
  return request({
    url: `/code/${id}`,
    method: 'delete'
  });
}
