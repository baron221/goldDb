import store from '@/store';

function checkPermission(el, binding) {
  const { value } = binding;

  if (value && value instanceof Array) {
    if (value.length > 0) {
      const roles = store.user().roles;
      const hasPermission = roles.some(role => value.includes(role));

      if (!hasPermission) {
        if (el.parentNode) {
          el.parentNode.removeChild(el);
        }
      }
    }
  } else if (typeof value === 'string') {
    const route = store.permission().routes.find(r => r.name === window.location.hash.split('?')[0].split('/').pop()); 

    const permissionStore = store.permission();

    const currentPath = window.location.hash.replace('#', '').split('?')[0];

    let permissions = null;
    for (const key in permissionStore.permissions) {
      if (currentPath.includes(key)) {
        permissions = permissionStore.permissions[key];
      }
    }

    if (permissions) {
      const map = {
        'search': 'canSearch',
        'create': 'canCreate',
        'delete': 'canDelete',
        'save': 'canSave',
        'print': 'canPrint'
      };
      const field = map[value] || value;
      if (!permissions[field]) {
        if (el.parentNode) {
          el.parentNode.removeChild(el);
        }
      }
    }
  } else {
    throw new Error(`need roles or action! Like v-permission="['admin']" or v-permission="'create'"`);
  }
}

export default {
  mounted(el, binding) {
    checkPermission(el, binding);
  },
  updated(el, binding) {
    checkPermission(el, binding);
  }
};
