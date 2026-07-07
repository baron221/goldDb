import request from '@/utils/request';

export function getMenuFavorites() {
  return request({
    url: '/user/menu-favorites',
    method: 'get'
  });
}

export function getMenuFavoritesDetail() {
  return request({
    url: '/user/menu-favorites/detail',
    method: 'get'
  });
}

export function updateMenuFavoriteSort(data: any) {
  return request({
    url: '/user/menu-favorites/sort',
    method: 'post',
    data
  });
}

export function toggleMenuFavorite(menuId: any) {
  return request({
    url: `/user/menu-favorites/toggle/${menuId}`,
    method: 'post'
  });
}

export function addMenuFavorite(menuId: any) {
  return request({
    url: `/user/menu-favorites/${menuId}`,
    method: 'post'
  });
}

export function removeMenuFavorite(menuId: any) {
  return request({
    url: `/user/menu-favorites/${menuId}`,
    method: 'delete'
  });
}
