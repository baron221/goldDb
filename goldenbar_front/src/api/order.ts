import request from '@/utils/request';

export function getMyOrders(params?: any) {
  return request({
    url: '/order',
    method: 'get',
    params
  });
}

export function createOrder(data: any) {
  return request({
    url: '/order',
    method: 'post',
    data
  });
}

export function getAllOrders(params: any) {
  return request({
    url: '/order/all',
    method: 'get',
    params
  });
}

export function updateOrderStatus(id: number, data: any) {
  return request({
    url: `/order/${id}/status`,
    method: 'put',
    data
  });
}

export function deleteOrder(id: number) {
  return request({
    url: `/order/${id}`,
    method: 'delete'
  });
}

export function getOrderHistory(id: number) {
  return request({
    url: `/order/${id}/history`,
    method: 'get'
  });
}

export function saveOrderStatement(id: number, data: any) {
  return request({
    url: `/order/${id}/statement`,
    method: 'post',
    data,
    timeout: 0 
  });
}

export function getOrderStatement(id: number) {
  return request({
    url: `/order/${id}/statement`,
    method: 'get',
    timeout: 0
  });
}

export function getOrderStatementPdf(id: number) {
  return request({
    url: `/order/${id}/statement/pdf`,
    method: 'get',
    timeout: 0 
  });
}

export function getSettlementSummary(params: any) {
  return request({
    url: '/order/settlement-summary',
    method: 'get',
    params
  });
}
