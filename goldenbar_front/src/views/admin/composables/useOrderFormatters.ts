import { computed } from 'vue';
import { parseTime } from '@/utils';
import { formatPrice } from '@/utils/format';
import { isPostPendingStatus } from '@/utils/order';
import useUserStore from '@/store/modules/user';

export function useOrderFormatters(codeMap: any) {
  const userStore = useUserStore();

  const formatDate = (date: string) => {
    return parseTime(date, '{y}-{m}-{d} {h}:{i}');
  };

  const userFormatter = (row: any) => {
    return `${row.userDisplayName} (${row.userName})`;
  };

  const getOrderTotalAmount = (order: any) => {
    const isPostPending = isPostPendingStatus(order.status);

    // MFG is only ever owed what THEY themselves declared (factory input cost) -
    // never the logistics-confirmed retailer price (or the order's settlementAmount,
    // computed from that same figure), which may include logistics' own markup.
    if (isPostPending && userStore.companyType === 'MFG' && order.orderItems && order.orderItems.length > 0) {
      const topLevelItems = order.orderItems.filter((item: any) => !item.parentId);
      return topLevelItems.reduce((acc: number, item: any) => {
        const material = item.factoryInputMaterialCost || 0;
        const labor = item.factoryInputLaborCost || 0;
        return acc + (material + labor) * item.quantity;
      }, 0);
    }

    // Orders that skip straight from 제품출고 to 정산 (PENDING) never get a logistics
    // (retailerConfirm*) figure - only the factory's own input, or the amount the
    // backend already settled on at PENDING (order.settlementAmount). Prefer whichever
    // is actually populated instead of assuming retailerConfirm* is always set.
    if (isPostPending && order.settlementAmount) {
      return order.settlementAmount;
    }

    if (isPostPending && order.orderItems && order.orderItems.length > 0) {
      const topLevelItems = order.orderItems.filter((item: any) => !item.parentId);
      return topLevelItems.reduce((acc: number, item: any) => {
        const material = item.retailerConfirmMaterialCost || item.factoryInputMaterialCost || 0;
        const labor = item.retailerConfirmLaborCost || item.factoryInputLaborCost || 0;
        return acc + (material + labor) * item.quantity;
      }, 0);
    }
    return order.totalAmount;
  };

  const amountFormatter = (row: any) => {
    return `₩${formatPrice(getOrderTotalAmount(row))}`;
  };

  const innerProductInfoFormatter = (row: any) => {
    const setTag = row.productSetId ? '[SET] ' : '';
    let text = `${setTag}${row.productName || row.productSetTitle || ''}`;
    if (row.productNo) text += ` (${row.productNo})`;
    const options = [];
    if (row.purity) options.push(codeMap.value[row.purity] || row.purity);
    if (row.color) options.push(codeMap.value[row.color] || row.color);
    if (row.size && row.size !== 'EMPTY') options.push(codeMap.value[row.size] || row.size);
    if (options.length > 0) text += `\n옵션: ${options.join(', ')}`;
    if (row.memo) text += `\n메모: ${row.memo}`;
    return text;
  };

  const priceFormatter = (row: any) => `₩ ${formatPrice(row.price)}`;
  const itemTotalFormatter = (row: any) => `₩ ${formatPrice(row.price * row.quantity)}`;

  return {
    formatDate,
    userFormatter,
    getOrderTotalAmount,
    amountFormatter,
    innerProductInfoFormatter,
    priceFormatter,
    itemTotalFormatter
  };
}
