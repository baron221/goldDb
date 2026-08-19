import request from '@/utils/request';

export function getCompanySummaries(params: any) {
  return request({
    url: '/payables/summaries',
    method: 'get',
    params
  });
}

export function getPayables(params: any) {
  return request({
    url: '/payables',
    method: 'get',
    params
  });
}

export function getPayableSummaryForOrder(orderId: number) {
  return request({
    url: `/payables/summary-by-order/${orderId}`,
    method: 'get'
  });
}

export function getPayableOrderHistory(params: any) {
  return request({
    url: '/payables/order-history',
    method: 'get',
    params
  });
}

export function getPayableOrderHistorySummary(params: any) {
  return request({
    url: '/payables/order-history/summary',
    method: 'get',
    params
  });
}

export function getCompletedPayableOrders(params: any) {
  return request({
    url: '/payables/order-history/completed',
    method: 'get',
    params
  });
}

export function getChargeApplications(chargeId: number) {
  return request({
    url: `/payables/charges/${chargeId}/applications`,
    method: 'get'
  });
}

export function processPayment(data: any) {
  return request({
    url: '/payables/payment',
    method: 'post',
    data
  });
}

export function updatePayable(id: number, data: any) {
  return request({
    url: `/payables/${id}`,
    method: 'put',
    data
  });
}

export function cancelPayable(id: number) {
  return request({
    url: `/payables/${id}/cancel`,
    method: 'post'
  });
}

export function deletePayable(id: number) {
  return request({
    url: `/payables/${id}`,
    method: 'delete'
  });
}

export function getOrderChargeSummary(orderIds: number[]) {
  return request({
    url: '/payables/order-charge-summary',
    method: 'post',
    data: { orderIds }
  });
}

export function getPaymentApplications(paymentId: number) {
  return request({
    url: `/payables/${paymentId}/applications`,
    method: 'get'
  });
}

export function updatePaymentApplication(applicationId: number, data: any) {
  return request({
    url: `/payables/applications/${applicationId}`,
    method: 'put',
    data
  });
}
