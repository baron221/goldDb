import request from '@/utils/request';

export function getUserSummaries(params: any) {
  return request({
    url: '/receivables/summaries',
    method: 'get',
    params
  });
}

export function getLogisticsReceivableSummary() {
  return request({
    url: '/receivables/logistics-summary',
    method: 'get'
  });
}

export function getReceivableOrderHistory(params: any) {
  return request({
    url: '/receivables/order-history',
    method: 'get',
    params
  });
}

export function getReceivableOrderHistorySummary(params: any) {
  return request({
    url: '/receivables/order-history/summary',
    method: 'get',
    params
  });
}

export function getCompletedReceivableOrders(params: any) {
  return request({
    url: '/receivables/order-history/completed',
    method: 'get',
    params
  });
}

export function getReceivableOverdueSummary(params?: any) {
  return request({
    url: '/receivables/overdue-summary',
    method: 'get',
    params
  });
}

export function getReceivableChargeApplications(chargeId: number) {
  return request({
    url: `/receivables/${chargeId}/charge-applications`,
    method: 'get'
  });
}

export function getReceivables(params: any) {
  return request({
    url: '/receivables',
    method: 'get',
    params
  });
}

export function processDeposit(data: any) {
  return request({
    url: '/receivables/deposit',
    method: 'post',
    data
  });
}

export function getPuritySummary(userId: number) {
  return request({
    url: '/receivables/purity-summary',
    method: 'get',
    params: { userId }
  });
}

export function getLedgerBefore(id: number) {
  return request({
    url: `/receivables/${id}/ledger-before`,
    method: 'get'
  });
}

export function getUserSummaryById(userId: number) {
  return request({
    url: `/receivables/user-summary/${userId}`,
    method: 'get'
  });
}

export function getReceivableChargeSummary(orderIds: number[]) {
  return request({
    url: '/receivables/order-charge-summary',
    method: 'post',
    data: { orderIds }
  });
}

export function updateReceivable(id: number, data: any) {
  return request({
    url: `/receivables/${id}`,
    method: 'put',
    data
  });
}

export function cancelReceivable(id: number) {
  return request({
    url: `/receivables/${id}/cancel`,
    method: 'post'
  });
}
