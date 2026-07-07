import request from '@/utils/request';
import type { Customer, CustomerCreateRequest } from '@/types/customer';

export function getCustomers(params: any) {
  return request({
    url: '/customer',
    method: 'get',
    params
  });
}

export function getCustomer(id: number) {
  return request({
    url: `/customer/${id}`,
    method: 'get'
  });
}

export function createCustomer(data: CustomerCreateRequest) {
  return request({
    url: '/customer',
    method: 'post',
    data
  });
}

export function updateCustomer(id: number, data: Partial<CustomerCreateRequest>) {
  return request({
    url: `/customer/${id}`,
    method: 'put',
    data
  });
}

export function deleteCustomer(id: number) {
  return request({
    url: `/customer/${id}`,
    method: 'delete'
  });
}
