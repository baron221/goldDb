import request from '@/utils/request';

export function getMyFavorites() {
  return request({
    url: '/favorite',
    method: 'get'
  });
}

export function addFavorite(data: { productId?: number; productSetId?: number }) {
  return request({
    url: '/favorite',
    method: 'post',
    data
  });
}

export function removeFavorite(id: number) {
  return request({
    url: `/favorite/${id}`,
    method: 'delete'
  });
}

export function removeFavoriteByProduct(params: { productId?: number; productSetId?: number }) {
  return request({
    url: '/favorite/product',
    method: 'delete',
    params
  });
}
