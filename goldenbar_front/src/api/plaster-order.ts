import request from '@/utils/request';

export function getPlasterOrders(params: any) {
  return request({
    url: '/plaster-orders',
    method: 'get',
    params
  });
}

export function getPlasterOrder(id: number) {
  return request({
    url: `/plaster-orders/${id}`,
    method: 'get'
  });
}

export function createPlasterOrder(data: any) {
  return request({
    url: '/plaster-orders',
    method: 'post',
    data
  });
}

export function updatePlasterOrder(id: number, data: any) {
  return request({
    url: `/plaster-orders/${id}`,
    method: 'put',
    data
  });
}

export function deletePlasterOrder(id: number) {
  return request({
    url: `/plaster-orders/${id}`,
    method: 'delete'
  });
}
