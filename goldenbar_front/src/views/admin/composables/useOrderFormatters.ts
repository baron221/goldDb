import { computed } from 'vue';
import { parseTime } from '@/utils';
import { formatPrice } from '@/utils/format';
import { isPostPendingStatus } from '@/utils/order';

export function useOrderFormatters(codeMap: any) {
  const formatDate = (date: string) => {
    return parseTime(date, '{y}-{m}-{d} {h}:{i}');
  };

  const userFormatter = (row: any) => {
    return `${row.userDisplayName} (${row.userName})`;
  };

  const getOrderTotalAmount = (order: any) => {
    const isPostPending = isPostPendingStatus(order.status);
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
    if (options.length > 0) text += `\n옵션: ${options.join(', ')}`;
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
