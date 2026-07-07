import router from './router';
import userStore from './store/modules/user';
import permissionStore from './store/modules/permission';
import NProgress from 'nprogress'; 
import 'nprogress/nprogress.css'; 
import { getToken } from '@/utils/auth'; 
import getPageTitle from '@/utils/get-page-title';

NProgress.configure({ showSpinner: false }); 

const whiteList = ['/', '/home', '/about', '/marketplace', '/contact', '/terms', '/privacy', '/login', '/auth-redirect', '/register', '/find-id', '/find-password', '/reset-password']; 

function getFlatPaths(routes: any[], parentPath = ''): string[] {
  const paths: string[] = [];
  routes.forEach(route => {
    let fullPath = route.path;
    if (parentPath && !route.path.startsWith('/')) {
      fullPath = parentPath + (parentPath.endsWith('/') ? '' : '/') + route.path;
    } else if (parentPath && route.path.startsWith('/')) {
      fullPath = route.path;
    }
    paths.push(fullPath);
    if (route.children) {
      paths.push(...getFlatPaths(route.children, fullPath));
    }
  });
  return paths;
}

router.beforeEach(async (to, from, next) => {

  NProgress.start();

  document.title = getPageTitle(to.meta.title);

  const hasToken = getToken();

  if (hasToken) {
    if (to.path === '/login') {

      NProgress.done();
      next({ path: '/dashboard' });
    } else {

      const hasRoles = userStore().roles && userStore().roles.length > 0;

      if (hasRoles) {

        const isErrorPage = to.path === '/401' || to.path === '/404';
        const isNotFound = !isErrorPage && (to.matched.length === 0 || to.matched.some(record => record.path.includes(':pathMatch') || record.path === '/404'));

        if (isNotFound) {
          const permStore = permissionStore();

          const authorizedPaths = getFlatPaths(permStore.routes);
          const isAuthorized = authorizedPaths.some(path => {
            if (path.includes(':pathMatch') || path === '/404' || path === '/401') return false;
            if (path === to.path) return true;
            const regexPath = path.replace(/:\w+/g, '[^/]+');
            const regex = new RegExp(`^${regexPath}$`);
            return regex.test(to.path);
          });

          if (!isAuthorized) {

            if (from.path === '/login' || from.path === '/' || from.path === '/404') {

              next({ path: '/dashboard', replace: true });
            } else {

              next({ path: '/401', replace: true });
            }
            NProgress.done();
            return;
          }
        }

        next();
      } else {
        try {

          const infoRes = await userStore().getInfo() as any;
          let roles = [];
          if (infoRes.roles) {
            roles = infoRes.roles;
          }

          const accessRoutes = await permissionStore().generateRoutes(roles);

          accessRoutes.forEach(item => {
            router.addRoute(item);
          });

          const authorizedPaths = getFlatPaths(accessRoutes);

          const isAuthorized = authorizedPaths.some(path => {
            if (path.includes(':pathMatch') || path === '/404' || path === '/401') return false;
            const regexPath = path.replace(/:\w+/g, '[^/]+');
            const regex = new RegExp(`^${regexPath}$`);
            const isMatch = (path === to.path || regex.test(to.path));
            if (isMatch) 
            return isMatch;
          });

          if (!isAuthorized) {

            next({ path: '/dashboard', replace: true });
          } else {

            next({ ...to, replace: true });
          }
        } catch (error: any) {

          await userStore().resetToken();
          ElMessage.error(error.message || 'Has Error');
          NProgress.done();
          next(`/login?redirect=${to.path}`);
        }
      }
    }
  } else {

    if (whiteList.indexOf(to.path) !== -1) {

      next();
    } else {

      NProgress.done();
      next(`/login?redirect=${to.path}`);
    }
  }
});

router.afterEach(() => {

  NProgress.done();
});
