import request from '@/utils/request';

export function getProductSets(params: any) {
  return request({
    url: '/product-sets',
    method: 'get',
    params
  });
}

export function getAdminProductSets(params: any) {
  return request({
    url: '/product-sets/admin',
    method: 'get',
    params
  });
}

export function getProductSet(id: number) {
  return request({
    url: `/product-sets/${id}`,
    method: 'get'
  });
}

export function createProductSet(data: any) {
  return request({
    url: '/product-sets',
    method: 'post',
    data
  });
}

export function updateProductSet(id: number, data: any) {
  return request({
    url: `/product-sets/${id}`,
    method: 'put',
    data
  });
}

export function deleteProductSet(id: number) {
  return request({
    url: `/product-sets/${id}`,
    method: 'delete'
  });
}
