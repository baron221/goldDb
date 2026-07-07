import request from '@/utils/request';

export function getGoldPrices() {
  return request({
    url: '/gold-prices',
    method: 'get'
  });
}

export function getLatestGoldPrice() {
  return request({
    url: '/gold-prices/latest',
    method: 'get'
  });
}

export function createGoldPrice(data: any) {
  return request({
    url: '/gold-prices',
    method: 'post',
    data
  });
}

export function updateGoldPrice(id: number, data: any) {
  return request({
    url: `/gold-prices/${id}`,
    method: 'put',
    data
  });
}

export function deleteGoldPrice(id: number) {
  return request({
    url: `/gold-prices/${id}`,
    method: 'delete'
  });
}
