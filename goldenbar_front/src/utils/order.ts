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
        name: '배송중',
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
    { code: 'DELIVERY_READY', name: '배송시작' },
    { code: 'DELIVERY_IN_TRANSIT', name: '배송중' },
    { code: 'DELIVERED', name: '배송완료' },
    { code: 'Completed', name: '수령완료' }
  ];
};

export const postPendingStatuses = [
  'LogisticsApproved', 'FactoryRequested', 'WorkOrderCreated', 'InspectedRequested',
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

export const getStatusLabel = (status: string, userCategory: string = '') => {
  if (userCategory === 'MFG') {
    if (status === 'Inspected') return '검수완료';

    const completedStatuses = [
      'PENDING', 'PROCESSING', 'SETTLED', 'SETTLED_CANCELLED',
      'DELIVERY_READY', 'DELIVERY_IN_TRANSIT', 'DELIVERED', 'Completed'
    ];
    if (completedStatuses.includes(status)) return '완료';
  }

  const statusNameMap: Record<string, string> = {
    'ORDERED': '주문접수',
    'LogisticsApproved': '물류승인',
    'FactoryRequested': '공장의뢰',
    'InspectedRequested': '검수요청',
    'Inspected': '검수완료',
    'PENDING': '정산대기',
    'PROCESSING': '정산중',
    'SETTLED': '정산완료',
    'SETTLED_CANCELLED': '정산취소',
    'DELIVERY_READY': '배송준비',
    'DELIVERY_IN_TRANSIT': '배송중',
    'DELIVERED': '배송완료',
    'Completed': '수령완료',
    'Cancelled': '주문취소',
    'REQUEST_CLOSE_BY_AGREEMENT': '검수불가 협의요청',
    'CLOSED_BY_AGREEMENT': '검수불가 종결',
    'WorkOrderCreated': '작업서작성'
  };

  return statusNameMap[status] || status;
};

export const getStatusTagType = (status: string, userCategory: string = '') => {
  if (userCategory === 'MFG') {
    const map: Record<string, string> = {
      'ORDERED': 'info',
      'LogisticsApproved': 'success',
      'FactoryRequested': 'warning',
      'WorkOrderCreated': 'warning',
      'InspectedRequested': 'primary',
      'Inspected': 'success',
      'Completed': 'info',
      'Cancelled': 'danger'
    };
    return map[status] || 'info';
  }

  const map: Record<string, string> = {
    'ORDERED': 'info',
    'LogisticsApproved': 'success',
    'FactoryRequested': 'warning',
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
  const isMfgOrDcc = userCategory === 'MFG' || userCategory === 'DCC';
  const settlementStatuses = ['PENDING', 'PROCESSING', 'SETTLED', 'SETTLED_CANCELLED', 'DELIVERY_READY', 'DELIVERY_IN_TRANSIT', 'DELIVERED', 'Completed'];
  const factoryInputStatuses = ['WorkOrderCreated', 'InspectedRequested', 'Inspected'];

  if (isMfgOrDcc) {
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
