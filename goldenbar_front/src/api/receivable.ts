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
