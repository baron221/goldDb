import request from '@/utils/request';

export function integratedSearch(query: string | object) {
  return request({
    url: '/search',
    method: 'get',
    params: typeof query === 'string' ? { q: query } : query
  });
}
