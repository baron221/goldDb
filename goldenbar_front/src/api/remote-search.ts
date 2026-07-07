import request from '@/utils/request';

export function searchUser(name: string) {
  return request({
    url: '/search/user',
    method: 'get',
    params: { name }
  });
}

export function transactionList(query: any) {
  return request({
    url: '/transaction/list',
    method: 'get',
    params: query
  });
}
