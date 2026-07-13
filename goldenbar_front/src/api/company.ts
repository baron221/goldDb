import request from '@/utils/request';
import type { Company, CompanyCreateRequest } from '@/types/company';

export function getCompanies(params: any) {
  return request({
    url: '/company',
    method: 'get',
    params
  });
}

export function getCompany(id: number) {
  return request({
    url: `/company/${id}`,
    method: 'get'
  });
}

export function createCompany(data: CompanyCreateRequest) {
  return request({
    url: '/company',
    method: 'post',
    data
  });
}

export function updateCompany(id: number, data: Partial<CompanyCreateRequest>) {
  return request({
    url: `/company/${id}`,
    method: 'put',
    data
  });
}

export function deleteCompany(id: number) {
  return request({
    url: `/company/${id}`,
    method: 'delete'
  });
}

export function getCompanyUsers(id: number) {
  return request({
    url: `/company/${id}/users`,
    method: 'get'
  });
}

export function addUserToCompany(id: number, userId: number) {
  return request({
    url: `/company/${id}/users/${userId}`,
    method: 'post'
  });
}

export function removeUserFromCompany(id: number, userId: number) {
  return request({
    url: `/company/${id}/users/${userId}`,
    method: 'delete'
  });
}

export function getAvailableUsers() {
  return request({
    url: '/company/available-users',
    method: 'get'
  });
}

export function getRetailersByCenter(centerId: number) {
  return request({
    url: `/company/logistics/${centerId}/retailers`,
    method: 'get'
  });
}

export function getUnassignedRetailers() {
  return request({
    url: '/company/retailers/unassigned',
    method: 'get'
  });
}

export function assignRetailersToCenter(centerId: number, retailerIds: number[]) {
  return request({
    url: `/company/logistics/${centerId}/assign`,
    method: 'post',
    data: retailerIds
  });
}

export function getManufacturersByCenter(centerId: number) {
  return request({
    url: `/company/logistics/${centerId}/manufacturers`,
    method: 'get'
  });
}

export function getUnassignedManufacturers(centerId: number) {
  return request({
    url: `/company/logistics/${centerId}/manufacturers/unassigned`,
    method: 'get'
  });
}

export function assignManufacturersToCenter(centerId: number, manufacturerIds: number[]) {
  return request({
    url: `/company/logistics/${centerId}/assign-manufacturers`,
    method: 'post',
    data: manufacturerIds
  });
}

export function getLogisticsCentersByManufacturer(manufacturerId: number) {
  return request({
    url: `/company/manufacturers/${manufacturerId}/logistics`,
    method: 'get'
  });
}

export function getUnassignedLogisticsCentersForManufacturer(manufacturerId: number) {
  return request({
    url: `/company/manufacturers/${manufacturerId}/logistics/unassigned`,
    method: 'get'
  });
}

export function assignLogisticsCentersToManufacturer(manufacturerId: number, logisticsIds: number[]) {
  return request({
    url: `/company/manufacturers/${manufacturerId}/assign-logistics`,
    method: 'post',
    data: logisticsIds
  });
}
