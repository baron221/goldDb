const menus = [
  { path: '/admin', component: 'Layout', name: 'Admin', title: '관리자 업무', icon: 'Setting', children: [
      { path: 'order-management', component: 'admin/order-management', name: 'OrderManagement', title: '주문 통합 관리', icon: 'List' },
      { path: 'order-tracking', component: 'admin/order-tracking', name: 'OrderTracking', title: '주문 이력 추적', icon: 'Clock' },
      { path: 'partner-retailers', component: 'admin/partner-retailers', name: 'PartnerRetailers', title: '협력 소매점 관리', icon: 'Connection' },
      { path: 'logistics-approval', component: 'admin/logistics-approval', name: 'LogisticsApproval', title: '물류 승인 내역', icon: 'Checked' },
      { path: 'receivable', component: 'admin/receivable-management', name: 'ReceivableManagement', title: '미수금/수납 관리', icon: 'Money' },
      { path: 'factory-request', component: 'admin/factory-request', name: 'FactoryRequest', title: '공장 의뢰 관리', icon: 'List' },
      { path: 'inspection-management', component: 'admin/inspection-management', name: 'InspectionManagement', title: '물류 검수 확인', icon: 'Checked' },
      { path: 'settlement-management', component: 'admin/settlement-management', name: 'SettlementManagement', title: '정산 대상 관리', icon: 'Money' },
      { path: 'settlement-history', component: 'admin/settlement-history', name: 'SettlementHistory', title: '정산 완료 내역', icon: 'Memo' }
  ]},
  { path: '/sys', component: 'Layout', name: 'Sys', title: '시스템 관리', icon: 'Setting', children: [
      { path: 'user', component: 'sys/user', name: 'UserManagement', title: '사용자 관리', icon: 'user' },
      { path: 'company', component: 'sys/company', name: 'CompanyManagement', title: '업체 관리', icon: 'office-building' },
      { path: 'company-mapping', component: 'sys/company-mapping', name: 'CompanyMapping', title: '물류-소매업 매핑', icon: 'Connection' },
      { path: 'menu', component: 'sys/menu', name: 'MenuManagement', title: '메뉴 관리', icon: 'list' },
      { path: 'code', component: 'sys/code', name: 'CodeManagement', title: '공통 코드 관리', icon: 'component' }
  ]},
  { path: '/stock', component: 'Layout', name: 'Stock', title: '재고 관리', icon: 'Management', children: [
      { path: 'index', component: 'stock/index', name: 'StockMain', title: '나의 재고 관리', icon: 'User' },
      { path: 'direct-stock', component: 'stock/direct-stock', name: 'DirectStock', title: '직영 소매점 재고', icon: 'OfficeBuilding' },
      { path: 'settlement', component: 'stock/settlement', name: 'RetailerSettlement', title: '나의 정산 관리', icon: 'Money' }
  ]},
  { path: '/notice', component: 'Layout', name: 'Notice', title: '공지 관리', icon: 'message', children: [
      { path: 'index', component: 'notice/index', name: 'NoticeMain', title: '공지 관리', icon: 'message' }
  ]},
  { path: '/logistics', component: 'Layout', name: 'Logistics', title: '물류업체', icon: 'Connection', children: [
      { path: 'index', component: 'logistics/index', name: 'LogisticsCompanyList', title: '물류업체 현황', icon: 'OfficeBuilding' }
  ]},
  { path: '/prd', component: 'Layout', name: 'Prd', title: '상품 주문', icon: 'ShoppingCart', children: [
      { path: 'market', component: 'product-market/index', name: 'ProductMarket', title: '상품 마켓', icon: 'Shop' },
      { path: 'catalog-viewer', component: 'product-market/catalog-viewer', name: 'CatalogViewer', title: 'E-카탈로그', icon: 'Reading' }
  ]}
];

let sql = ``;
let idCounter = 1;
for (const [parentIdx, parent] of menus.entries()) {
    const parentId = idCounter++;
    sql += `INSERT INTO goldb.menus (id, path, component, name, title, icon, sort_order, is_mobile, created_at, updated_at, is_deleted) VALUES (${parentId}, '${parent.path}', '${parent.component}', '${parent.name}', '${parent.title}', '${parent.icon}', ${parentIdx + 1}, false, NOW(), NOW(), false);\n`;
    
    for (const [childIdx, child] of (parent.children || []).entries()) {
        const childId = idCounter++;
        sql += `INSERT INTO goldb.menus (id, parent_id, path, component, name, title, icon, sort_order, is_mobile, created_at, updated_at, is_deleted) VALUES (${childId}, ${parentId}, '${child.path}', '${child.component}', '${child.name}', '${child.title}', '${child.icon}', ${childIdx + 1}, false, NOW(), NOW(), false);\n`;
    }
}
sql += `SELECT setval('goldb.menus_id_seq', ${idCounter});\n`;
const fs = require('fs');
fs.writeFileSync('seed_menus.sql', sql);
console.log('Generated seed_menus.sql');
