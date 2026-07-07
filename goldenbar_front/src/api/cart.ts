import request from '@/utils/request';

export function getCart() {
  return request({
    url: '/cart',
    method: 'get'
  });
}

export function addToCart(data: any) {
  return request({
    url: '/cart',
    method: 'post',
    data
  });
}

export function updateCartQuantity(id: number, quantity: number) {
  return request({
    url: `/cart/${id}/quantity`,
    method: 'put',
    data: { quantity }
  });
}

export function updateCartPrice(id: number, factoryPrice: number | null, laborCost: number | null) {
  return request({
    url: `/cart/${id}/price`,
    method: 'put',
    data: { factoryPrice, laborCost }
  });
}

export function removeFromCart(id: number) {
  return request({
    url: `/cart/${id}`,
    method: 'delete'
  });
}

export function clearCart() {
  return request({
    url: '/cart',
    method: 'delete'
  });
}
