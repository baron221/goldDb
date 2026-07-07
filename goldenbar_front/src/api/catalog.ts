import request from '@/utils/request';

export function getCatalogs(params: any) {
  return request({
    url: '/catalogs',
    method: 'get',
    params
  });
}

export function getCatalog(id: number) {
  return request({
    url: `/catalogs/${id}`,
    method: 'get'
  });
}

export function createCatalog(data: any) {
  return request({
    url: '/catalogs',
    method: 'post',
    data
  });
}

export function updateCatalog(id: number, data: any) {
  return request({
    url: `/catalogs/${id}`,
    method: 'put',
    data
  });
}

export function deleteCatalog(id: number) {
  return request({
    url: `/catalogs/${id}`,
    method: 'delete'
  });
}
