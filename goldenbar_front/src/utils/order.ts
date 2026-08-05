export const getOrderStatusFlow = (status: string, userType: string = 'retailer') => {
  if (userType === 'retailer') {
    return [
      {
        name: '주문접수',
        codes: ['ORDERED']
      },
      {
        name: (status === 'ANY' || status !== 'Cancelled') ? '물류승인' : '주문취소',
        codes: status === 'Cancelled' ? ['Cancelled'] : ['LogisticsApproved']
      },
      {
        name: '제품준비중',
        codes: ['FactoryRequested', 'InspectedRequested', 'REQUEST_CLOSE_BY_AGREEMENT', 'CLOSED_BY_AGREEMENT', 'Inspected', 'WorkOrderCreated']
      },
      {
        name: '정산대기',
        codes: ['PENDING']
      },
      {
        name: '정산중',
        codes: ['PROCESSING']
      },
      {
        name: (status === 'ANY' || status !== 'SETTLED_CANCELLED') ? '정산완료' : '정산취소',
        codes: status === 'SETTLED_CANCELLED' ? ['SETTLED_CANCELLED'] : ['SETTLED']
      },
      {
        name: '제품출고',
        codes: ['DELIVERY_READY', 'DELIVERY_IN_TRANSIT', 'DELIVERED']
      },
      {
        name: '수령완료',
        codes: ['Completed']
      },

      ...(status === 'ANY' || status === 'Cancelled' || status === 'SETTLED_CANCELLED' ? [{
        name: '취소됨',
        codes: ['Cancelled', 'SETTLED_CANCELLED']
      }] : [])
    ];
  }

  const baseFlow = [
    { code: 'ORDERED', name: '주문접수' },
    { code: 'LogisticsApproved', name: '물류승인' },
    { code: 'FactoryRequested', name: '공장의뢰' },
    { code: 'InspectedRequested', name: '검수요청' },
    { code: 'Inspected', name: '검수완료' },
    { code: 'PENDING', name: '정산대기' },
    { code: 'PROCESSING', name: '정산중' }
  ];

  if (status === 'Cancelled') {
    return [
      { code: 'ORDERED', name: '주문접수' },
      { code: 'Cancelled', name: '주문취소' }
    ];
  }

  if (status === 'SETTLED_CANCELLED') {
    return [
      ...baseFlow,
      { code: 'SETTLED_CANCELLED', name: '정산취소' }
    ];
  }

  return [
    ...baseFlow,
    { code: 'SETTLED', name: '정산완료' },
    { code: 'DELIVERY_READY', name: '출고대기' },
    { code: 'DELIVERY_IN_TRANSIT', name: '이송중' },
    { code: 'DELIVERED', name: '수령대기' },
    { code: 'Completed', name: '수령완료' }
  ];
};

export const postPendingStatuses = [
  'LogisticsApproved', 'FactoryRequested', 'WorkOrderCreated', 'FactoryApproved', 'FactoryRejected', 'InspectedRequested',
  'REQUEST_CLOSE_BY_AGREEMENT', 'CLOSED_BY_AGREEMENT', 'Inspected',
  'PENDING', 'PROCESSING', 'SETTLED', 'DELIVERY_READY',
  'DELIVERY_IN_TRANSIT', 'DELIVERED', 'Completed'
];

export const statementVisibleStatuses = [
  'PENDING', 'PROCESSING', 'SETTLED', 'DELIVERY_READY',
  'DELIVERY_IN_TRANSIT', 'DELIVERED', 'Completed'
];

export const isPostPendingStatus = (status: string): boolean => {
  return postPendingStatuses.includes(status);
};

export const isStatementVisibleStatus = (status: string): boolean => {
  return statementVisibleStatuses.includes(status);
};

const compactStatusNameMap: Record<string, string> = {
  'ORDERED': '주문접수',
  'LogisticsApproved': '공장의뢰',
  'FactoryRequested': '공장의뢰',
  'WorkOrderCreated': '공장의뢰',
  'FactoryApproved': '공장승인',
  'FactoryRejected': '공장거절',
  'InspectedRequested': '제품출고',
  'Inspected': '물류도착',
  'PENDING': '정산',
  'PROCESSING': '정산',
  'SETTLED': '정산',
  'SETTLED_CANCELLED': '정산취소',
  'DELIVERY_READY': '수령완료',
  'DELIVERY_IN_TRANSIT': '수령완료',
  'DELIVERED': '수령완료',
  'Completed': '수령완료',
  'Cancelled': '주문취소',
  'REQUEST_CLOSE_BY_AGREEMENT': '검수불가 협의요청',
  'CLOSED_BY_AGREEMENT': '검수불가 종결'
};

export const getStatusLabel = (status: string, userCategory: string = '') => {
  if (userCategory === 'MFG' || userCategory === 'DCC') {
    return compactStatusNameMap[status] || status;
  }

  const statusNameMap: Record<string, string> = {
    'ORDERED': '주문접수',
    'LogisticsApproved': '물류승인',
    'FactoryRequested': '공장의뢰',
    'FactoryApproved': '공장승인',
    'FactoryRejected': '공장거절',
    'InspectedRequested': '검수요청',
    'Inspected': '검수완료',
    'PENDING': '정산대기',
    'PROCESSING': '정산중',
    'SETTLED': '정산완료',
    'SETTLED_CANCELLED': '정산취소',
    'DELIVERY_READY': '출고대기',
    'DELIVERY_IN_TRANSIT': '이송중',
    'DELIVERED': '수령대기',
    'Completed': '수령완료',
    'Cancelled': '주문취소',
    'REQUEST_CLOSE_BY_AGREEMENT': '검수불가 협의요청',
    'CLOSED_BY_AGREEMENT': '검수불가 종결',
    'WorkOrderCreated': '작업서작성'
  };

  return statusNameMap[status] || status;
};

const compactStatusTagMap: Record<string, string> = {
  'ORDERED': 'info',
  'LogisticsApproved': 'warning',
  'FactoryRequested': 'warning',
  'WorkOrderCreated': 'warning',
  'FactoryApproved': 'success',
  'FactoryRejected': 'danger',
  'InspectedRequested': 'primary',
  'Inspected': 'success',
  'PENDING': 'warning',
  'PROCESSING': 'warning',
  'SETTLED': 'warning',
  'SETTLED_CANCELLED': 'info',
  'DELIVERY_READY': 'success',
  'DELIVERY_IN_TRANSIT': 'success',
  'DELIVERED': 'success',
  'Completed': 'success',
  'Cancelled': 'danger',
  'REQUEST_CLOSE_BY_AGREEMENT': 'warning',
  'CLOSED_BY_AGREEMENT': 'danger'
};

export const getStatusTagType = (status: string, userCategory: string = '') => {
  if (userCategory === 'MFG' || userCategory === 'DCC') {
    return compactStatusTagMap[status] || 'info';
  }

  const map: Record<string, string> = {
    'ORDERED': 'info',
    'LogisticsApproved': 'success',
    'FactoryRequested': 'warning',
    'FactoryApproved': 'success',
    'FactoryRejected': 'danger',
    'InspectedRequested': 'primary',
    'Inspected': 'success',
    'PENDING': 'warning',
    'PROCESSING': 'warning',
    'SETTLED': 'success',
    'SETTLED_CANCELLED': 'info',
    'DELIVERY_READY': 'primary',
    'DELIVERY_IN_TRANSIT': 'warning',
    'DELIVERED': 'success',
    'Completed': 'info',
    'Cancelled': 'danger',
    'REQUEST_CLOSE_BY_AGREEMENT': 'warning',
    'CLOSED_BY_AGREEMENT': 'danger',
    'WorkOrderCreated': 'warning'
  };
  return map[status] || 'info';
};

export const getOrderTotalAmount = (order: any, userCategory: string = '') => {
  const status = order.status;
  const settlementStatuses = ['PENDING', 'PROCESSING', 'SETTLED', 'SETTLED_CANCELLED', 'DELIVERY_READY', 'DELIVERY_IN_TRANSIT', 'DELIVERED', 'Completed'];
  const factoryInputStatuses = ['WorkOrderCreated', 'InspectedRequested', 'Inspected'];

  // MFG is only ever owed what THEY themselves declared (factory input cost) - never
  // the logistics-confirmed retailer price, which may include a markup that's
  // logistics' own margin, not something the manufacturer should see or be paid.
  if (userCategory === 'MFG') {
    if ((settlementStatuses.includes(status) || factoryInputStatuses.includes(status)) && order.orderItems && order.orderItems.length > 0) {
      const topLevelItems = order.orderItems.filter((item: any) => !item.parentId);
      return topLevelItems.reduce((acc: number, item: any) => {
        const material = item.factoryInputMaterialCost || 0;
        const labor = item.factoryInputLaborCost || 0;
        return acc + (material + labor) * item.quantity;
      }, 0);
    }
  }

  if (userCategory === 'DCC') {
    if (settlementStatuses.includes(status)) {
      if (order.orderItems && order.orderItems.length > 0) {
        const topLevelItems = order.orderItems.filter((item: any) => !item.parentId);
        return topLevelItems.reduce((acc: number, item: any) => {
          const material = item.retailerConfirmMaterialCost || 0;
          const labor = item.retailerConfirmLaborCost || 0;
          return acc + (material + labor) * item.quantity;
        }, 0);
      }
    } else if (factoryInputStatuses.includes(status)) {
      if (order.orderItems && order.orderItems.length > 0) {
        const topLevelItems = order.orderItems.filter((item: any) => !item.parentId);
        return topLevelItems.reduce((acc: number, item: any) => {
          const material = item.factoryInputMaterialCost || 0;
          const labor = item.factoryInputLaborCost || 0;
          return acc + (material + labor) * item.quantity;
        }, 0);
      }
    }
  }

  const isPostPending = isPostPendingStatus(status);
  if (isPostPending && order.orderItems && order.orderItems.length > 0) {
    const topLevelItems = order.orderItems.filter((item: any) => !item.parentId);
    return topLevelItems.reduce((acc: number, item: any) => {
      const material = item.retailerConfirmMaterialCost || 0;
      const labor = item.retailerConfirmLaborCost || 0;
      return acc + (material + labor) * item.quantity;
    }, 0);
  }
  return order.totalAmount;
};
