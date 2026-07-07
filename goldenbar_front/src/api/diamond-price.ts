import request from '@/utils/request';

export function getDiamondPrices(params: any) {
  return request({
    url: '/diamond-prices',
    method: 'get',
    params
  });
}

export function getLatestDiamondPrices(priceType: string) {
  return request({
    url: `/diamond-prices/latest/${priceType}`,
    method: 'get'
  });
}

export function createDiamondPrice(data: any) {
  return request({
    url: '/diamond-prices',
    method: 'post',
    data
  });
}

export function updateDiamondPrice(id: number, data: any) {
  return request({
    url: `/diamond-prices/${id}`,
    method: 'put',
    data
  });
}

export function deleteDiamondPrice(id: number) {
  return request({
    url: `/diamond-prices/${id}`,
    method: 'delete'
  });
}
