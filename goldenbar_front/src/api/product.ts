import request from '@/utils/request';

export function getProducts(params: any) {
  return request({
    url: '/products',
    method: 'get',
    params
  });
}

export function getAdminProducts(params: any) {
  return request({
    url: '/products/admin',
    method: 'get',
    params
  });
}

export function getProduct(id: number) {
  return request({
    url: `/products/${id}`,
    method: 'get'
  });
}

export function createProduct(data: any) {
  return request({
    url: '/products',
    method: 'post',
    data
  });
}

export function updateProduct(id: number, data: any) {
  return request({
    url: `/products/${id}`,
    method: 'put',
    data
  });
}

export function deleteProduct(id: number) {
  return request({
    url: `/products/${id}`,
    method: 'delete'
  });
}
