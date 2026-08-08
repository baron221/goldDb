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
