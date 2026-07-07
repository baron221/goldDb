import request from '@/utils/request';

export function login(data: any) {
  return request({
    url: '/user/login',
    method: 'post',
    data
  });
}

export function getInfo(token: string) {
  return request({
    url: '/user/info',
    method: 'get',
    params: { token }
  });
}

export function logout() {
  return request({
    url: '/user/logout',
    method: 'post'
  });
}

export function getMenus() {
  return request({
    url: '/user/menus',
    method: 'get'
  });
}

export function getUsers(params: any) {
  return request({
    url: '/user',
    method: 'get',
    params
  });
}

export function getUserDetail(id: number | string) {
  return request({
    url: `/user/${id}`,
    method: 'get'
  });
}

export function createUser(data: any) {
  return request({
    url: '/user',
    method: 'post',
    data
  });
}

export function updateUser(id: number | string, data: any) {
  return request({
    url: `/user/${id}`,
    method: 'put',
    data
  });
}

export function deleteUser(id: number | string) {
  return request({
    url: `/user/${id}`,
    method: 'delete'
  });
}

export function register(data: any) {
  return request({
    url: '/user/register',
    method: 'post',
    headers: {
      'Content-Type': data instanceof FormData ? 'multipart/form-data' : 'application/json'
    },
    data
  });
}

export function findId(data: any) {
  return request({
    url: '/user/find-id',
    method: 'post',
    data
  });
}

export function requestResetPassword(data: any) {
  return request({
    url: '/user/request-reset-password',
    method: 'post',
    data
  });
}

export function resetPassword(data: any) {
  return request({
    url: '/user/reset-password',
    method: 'post',
    data
  });
}

export function updateUserMenuSetting(data: any) {
  return request({
    url: '/user/menu-settings',
    method: 'post',
    data
  });
}

export function getUserMenuSettings() {
  return request({
    url: '/user/menu-settings',
    method: 'get'
  });
}

export function updateUserMenuSettingSort(data: any) {
  return request({
    url: '/user/menu-settings/sort',
    method: 'post',
    data
  });
}

export function getDevUsers() {
  return request({
    url: '/user/dev-users',
    method: 'get'
  });
}

export function getUserPersonalSettings() {
  return request({
    url: '/user/personal-settings',
    method: 'get'
  });
}

export function updateUserPersonalSettings(data: any) {
  return request({
    url: '/user/personal-settings',
    method: 'post',
    data
  });
}
