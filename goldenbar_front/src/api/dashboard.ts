import request from '@/utils/request';

export function getStats() {
  return request({
    url: '/dashboard/stats',
    method: 'get'
  });
}

export function getRetailStats() {
  return request({
    url: '/dashboard/retail-stats',
    method: 'get'
  });
}

export function getCompanyStats() {
  return request({
    url: '/dashboard/company-stats',
    method: 'get'
  });
}

export function getDailyCompanyStats() {
  return request({
    url: '/dashboard/daily-company-stats',
    method: 'get'
  });
}

export function getFactoryStats() {
  return request({
    url: '/dashboard/factory-stats',
    method: 'get'
  });
}

export function getAdminStats() {
  return request({
    url: '/dashboard/admin-stats',
    method: 'get'
  });
}

export function getPartnerRetailerStats() {
  return request({
    url: '/dashboard/partner-retailers',
    method: 'get'
  });
}
