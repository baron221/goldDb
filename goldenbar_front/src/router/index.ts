import { createRouter, createWebHashHistory } from 'vue-router'; 
import type { Router, RouteRecordRaw, RouteComponent } from 'vue-router';

const Layout = ():RouteComponent => import('@/layout/index.vue');
const PublicLayout = ():RouteComponent => import('@/layout/PublicLayout.vue');

export const constantRoutes:RouteRecordRaw[] = [

  {
    path: '/',
    component: PublicLayout,
    redirect: '/home',
    meta: { title: 'Public', icon: 'dashboard' },
    children: [
      {
        path: 'home',
        component: () => import('@/views/home/index.vue'),
        name: 'Home',
        meta: { title: 'Home', icon: 'dashboard', keepAlive: true }
      },
      {
        path: 'about',
        component: () => import('@/views/about/index.vue'),
        name: 'About',
        meta: { title: 'About' }
      },
      {
        path: 'marketplace',
        component: () => import('@/views/marketplace/index.vue'),
        name: 'Marketplace',
        meta: { title: 'Marketplace', keepAlive: true }
      },
      {
        path: 'contact',
        component: () => import('@/views/contact/index.vue'),
        name: 'Contact',
        meta: { title: 'Contact' }
      },
      {
        path: 'terms',
        component: () => import('@/views/legal/terms.vue'),
        name: 'Terms',
        meta: { title: 'Terms' }
      },
      {
        path: 'privacy',
        component: () => import('@/views/legal/privacy.vue'),
        name: 'Privacy',
        meta: { title: 'Privacy' }
      },
      {
        path: 'login',
        component: () => import('@/views/login/index.vue'),
        name: 'Login',
        meta: { title: 'Login', hidden: true }
      },
      {
        path: 'register',
        component: () => import('@/views/login/register.vue'),
        name: 'Register',
        meta: { title: 'Register', hidden: true }
      },
      {
        path: 'find-id',
        component: () => import('@/views/login/find-id.vue'),
        name: 'FindId',
        meta: { title: 'Find ID', hidden: true }
      },
      {
        path: 'find-password',
        component: () => import('@/views/login/find-password.vue'),
        name: 'FindPassword',
        meta: { title: 'Find Password', hidden: true }
      },
      {
        path: 'reset-password',
        component: () => import('@/views/login/reset-password.vue'),
        name: 'ResetPassword',
        meta: { title: 'Reset Password', hidden: true }
      }
    ]
  },
  {
    path: '/auth-redirect',
    component: () => import('@/views/login/auth-redirect.vue'),
    meta: { hidden: true }
  },
  {
    path: '/redirect',
    component: Layout,
    meta: { hidden: true },
    children: [
      {
        path: '/redirect/:path(.*)',
        component: () => import('@/views/redirect/index.vue')
      }
    ]
  },

  {
    path: '/err',
    component: Layout,
    meta: { hidden: true },
    children: [

      {
        path: '/404',
        component: () => import('@/views/error-page/404.vue'),
        meta: { hidden: true }
      },
      {
        path: '/401',
        component: () => import('@/views/error-page/401.vue'),
        meta: { hidden: true }
      }
    ]
  },
  {
    path: '/search',
    component: Layout,
    meta: { hidden: true },
    children: [
      {
        path: '',
        component: () => import('@/views/search/index.vue'),
        name: 'IntegratedSearch',
        meta: { title: '통합 검색' }
      }
    ]
  }

];

export const asyncRoutes:RouteRecordRaw[] = [
  {
    path: '/admin',
    component: Layout,
    redirect: '/admin/order-management',
    meta: { title: '관리자 업무', icon: 'Setting' },
    children: [
      {
        path: 'order-management',
        component: () => import('@/views/admin/order-management.vue'),
        name: 'OrderManagement',
        meta: { title: '주문 통합 관리', icon: 'List' }
      },
      {
        path: 'order-tracking',
        component: () => import('@/views/admin/order-tracking.vue'),
        name: 'OrderTracking',
        meta: { title: '주문 이력 추적', icon: 'Clock' }
      },
      {
        path: 'partner-retailers',
        component: () => import('@/views/admin/partner-retailers.vue'),
        name: 'PartnerRetailers',
        meta: { title: '협력 소매점 관리', icon: 'Connection' }
      },
      {
        path: 'logistics-approval',
        component: () => import('@/views/admin/logistics-approval.vue'),
        name: 'LogisticsApproval',
        meta: { title: '물류 승인 내역', icon: 'Checked' }
      },
      {
        path: 'receivable',
        component: () => import('@/views/admin/receivable-management.vue'),
        name: 'ReceivableManagement',
        meta: { title: '미수금/수납 관리', icon: 'Money' },
        alias: 'receivable-management'
      },
      {
        path: 'factory-request',
        component: () => import('@/views/admin/factory-request.vue'),
        name: 'FactoryRequest',
        meta: { title: '공장 의뢰 관리', icon: 'List' }
      },
      {
        path: 'inspection-management',
        component: () => import('@/views/admin/inspection-management.vue'),
        name: 'InspectionManagement',
        meta: { title: '물류 검수 확인', icon: 'Checked' }
      },
      {
        path: 'settlement-management',
        component: () => import('@/views/admin/settlement-management.vue'),
        name: 'SettlementManagement',
        meta: { title: '정산 대상 관리', icon: 'Money' }
      },
      {
        path: 'settlement-history',
        component: () => import('@/views/admin/settlement-history.vue'),
        name: 'SettlementHistory',
        meta: { title: '정산 완료 내역', icon: 'Memo' }
      }
    ]
  },
  {
    path: '/sys',
    component: Layout,
    redirect: '/sys/user',
    meta: { title: '시스템 관리', icon: 'Setting' },
    children: [
      {
        path: 'user',
        component: () => import('@/views/sys/user.vue'),
        name: 'UserManagement',
        meta: { title: '사용자 관리', icon: 'user' }
      },
      {
        path: 'company',
        component: () => import('@/views/sys/company.vue'),
        name: 'CompanyManagement',
        meta: { title: '업체 관리', icon: 'office-building' }
      },
      {
        path: 'company-mapping',
        component: () => import('@/views/sys/company-mapping.vue'),
        name: 'CompanyMapping',
        meta: { title: '물류-소매업 매핑', icon: 'Connection' }
      },
      {
        path: 'menu',
        component: () => import('@/views/sys/menu.vue'),
        name: 'MenuManagement',
        meta: { title: '메뉴 관리', icon: 'list' }
      },
      {
        path: 'code',
        component: () => import('@/views/sys/code.vue'),
        name: 'CodeManagement',
        meta: { title: '공통 코드 관리', icon: 'component' }
      }
    ]
  },
  {
    path: '/stock',
    component: Layout,
    redirect: '/stock/index',
    meta: { title: '재고 관리', icon: 'Management' },
    children: [
      {
        path: 'index',
        component: () => import('@/views/stock/index.vue'),
        name: 'Stock',
        meta: { title: '나의 재고 관리', icon: 'User' }
      },
      {
        path: 'direct-stock',
        component: () => import('@/views/stock/direct-stock.vue'),
        name: 'DirectStock',
        meta: { title: '직영 소매점 재고', icon: 'OfficeBuilding' }
      },
      {
        path: 'settlement',
        component: () => import('@/views/stock/settlement.vue'),
        name: 'RetailerSettlement',
        meta: { title: '나의 정산 관리', icon: 'Money' }
      },
      {
        path: 'stock_detail/:id',
        component: () => import('@/views/stock/detail.vue'),
        name: 'StockDetail',
        meta: { title: '재고 상세', icon: 'InfoFilled', hidden: true }
      }
    ]
  },
  {
    path: '/notice',
    component: Layout,
    redirect: '/notice/index',
    children: [
      {
        path: 'index',
        component: () => import('@/views/notice/index.vue'),
        name: 'Notice',
        meta: { title: '공지 관리', icon: 'message' }
      }
    ]
  },
  {
    path: '/logistics',
    component: Layout,
    redirect: '/logistics/index',
    meta: { title: '물류업체', icon: 'Connection' },
    children: [
      {
        path: 'index',
        component: () => import('@/views/logistics/index.vue'),
        name: 'LogisticsCompanyList',
        meta: { title: '물류업체 현황', icon: 'OfficeBuilding' }
      }
    ]
  },
  {
    path: '/prd',
    component: Layout,
    redirect: '/prd/market',
    meta: { title: '상품 주문', icon: 'ShoppingCart' },
    children: [
      {
        path: 'market',
        component: () => import('@/views/product-market/index.vue'),
        name: 'ProductMarket',
        meta: { title: '상품 마켓', icon: 'Shop' }
      },
      {
        path: 'catalog-viewer',
        component: () => import('@/views/product-market/catalog-viewer.vue'),
        name: 'CatalogViewer',
        meta: { title: 'E-카탈로그', icon: 'Reading' }
      },
      {
        path: 'product-detail/:id',
        component: () => import('@/views/product/detail.vue'),
        name: 'ProductDetail',
        meta: { title: '제품 상세', hidden: true }
      },
      {
        path: 'product-set-detail/:id',
        component: () => import('@/views/product-set/detail.vue'),
        name: 'ProductSetDetail',
        meta: { title: '세트 상세', hidden: true }
      }
    ]
  }
];

const createTheRouter = ():Router => createRouter({
  history: createWebHashHistory(import.meta.env.BASE_URL),

  scrollBehavior: () => ({ top: 0 }),
  routes: constantRoutes
});

interface RouterPro extends Router {
  matcher: unknown;
}

const router = createTheRouter() as RouterPro;

export function resetRouter() {
  const constantRouteNames = getRouteNames(constantRoutes);

  router.getRoutes().forEach((route) => {
    const { name } = route;
    if (name && !constantRouteNames.has(name)) {
      router.removeRoute(name);
    }
  });
}

function getRouteNames(routes: RouteRecordRaw[]): Set<string | symbol> {
  const names = new Set<string | symbol>();
  routes.forEach(route => {
    if (route.name) names.add(route.name);
    if (route.children) {
      const childNames = getRouteNames(route.children);
      childNames.forEach(name => names.add(name));
    }
  });
  return names;
}

export default router;
