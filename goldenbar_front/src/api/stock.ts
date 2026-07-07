import request from '@/utils/request';

export function fetchStocks(query: any) {
  return request({
    url: '/stocks',
    method: 'get',
    params: query
  });
}

export function fetchStockSummary(query: any) {
  return request({
    url: '/stocks/summary',
    method: 'get',
    params: query
  });
}

export function getStockDetail(id: number) {
  return request({
    url: `/stocks/${id}`,
    method: 'get'
  });
}

export function createStock(data: any) {
  return request({
    url: '/stocks',
    method: 'post',
    data
  });
}

export function updateStock(id: number, data: any) {
  return request({
    url: `/stocks/${id}`,
    method: 'put',
    data
  });
}

export function deleteStock(id: number) {
  return request({
    url: `/stocks/${id}`,
    method: 'delete'
  });
}

export function deleteStocksBulk(data: any) {
  return request({
    url: '/stocks/delete-bulk',
    method: 'post',
    data
  });
}

export function rentStocks(data: any) {
  return request({
    url: '/stocks/rent',
    method: 'post',
    data
  });
}

export function returnStocks(ids: number[]) {
  return request({
    url: '/stocks/return',
    method: 'post',
    data: ids
  });
}

export function rentByBarcode(data: any) {
  return request({
    url: '/stocks/rent-barcode',
    method: 'post',
    data
  });
}

export function returnByBarcode(data: any) {
  return request({
    url: '/stocks/return-barcode',
    method: 'post',
    data
  });
}

export function bulkUpdateStock(data: any[]) {
  return request({
    url: '/stocks/bulk-update',
    method: 'post',
    data
  });
}

export function updateStockPhotos(id: number, attachments: any[], mainAttachmentId: number | null) {
  return request({
    url: `/stocks/${id}/photos`,
    method: 'post',
    data: { attachments, mainAttachmentId }
  });
}

export function exhaustStockItem(data: { orderItemId: number, stockId: number, memo?: string }) {
  return request({
    url: '/stocks/exhaust-item',
    method: 'post',
    data
  });
}
