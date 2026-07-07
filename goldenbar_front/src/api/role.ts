import request from '@/utils/request';

export function getRoles() {
  return request({
    url: '/role',
    method: 'get'
  });
}

export function getRoutes() {
  return request({
    url: '/user/menus',
    method: 'get'
  });
}

export function addRole(data: any) {
  return request({
    url: '/role',
    method: 'post',
    data
  });
}

export function updateRole(id: number | string, data: any) {
  return request({
    url: `/role/${id}`,
    method: 'put',
    data
  });
}

export function deleteRole(id: number | string) {
  return request({
    url: `/role/${id}`,
    method: 'delete'
  });
}

export function getRoleMenus(roleKey: any) {
  return request({
    url: `/role/${roleKey}/menus`,
    method: 'get'
  });
}

export function assignMenus(data: any) {
  return request({
    url: '/role/assign-menus',
    method: 'post',
    data
  });
}

export function getRoleUsers(roleKey: any) {
  return request({
    url: `/role/${roleKey}/users`,
    method: 'get'
  });
}

export function assignUsers(data: any) {
  return request({
    url: '/role/assign-users',
    method: 'post',
    data
  });
}
