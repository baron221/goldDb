import permissionStore from '@/store/modules/permission';
import router from '@/router';

export default function checkPermission(permissionType: any) {
  const store = permissionStore();
  const route = router.currentRoute.value;

  if (!route) return true;

  const key = route.name || route.path;
  const permissions = store.permissions[key];

  if (!permissions) {
    return true;
  }

  return !!permissions[permissionType];
}

export function hasActionPermission(action: any) {
  const store = permissionStore();
  const route = router.currentRoute.value;

  if (!route) return true;

  const key = route.name || route.path;
  const permissions = store.permissions[key];

  if (!permissions) return true;

  const map = {
    'search': 'canSearch',
    'create': 'canCreate',
    'delete': 'canDelete',
    'save': 'canSave',
    'print': 'canPrint'
  };

  const field = map[action] || action;
  return !!permissions[field];
}
