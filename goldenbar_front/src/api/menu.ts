import request from '@/utils/request';

export function getMenus(raw: boolean = false) {
  return request({
    url: '/user/menus',
    method: 'get',
    params: { raw }
  });
}

export function saveMenu(data: any) {
  return request({
    url: '/user/menus',
    method: 'post',
    data
  });
}

export function batchUpdateMenus(data: any) {
  return request({
    url: '/user/menus/batch',
    method: 'post',
    data
  });
}

export function deleteMenu(id: number | string) {
  return request({
    url: `/user/menus/${id}`,
    method: 'delete'
  });
}
