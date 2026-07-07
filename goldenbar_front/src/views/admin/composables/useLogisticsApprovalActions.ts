import { ElMessageBox, ElMessage } from 'element-plus';
import { updateOrderStatus } from '@/api/order';

export function useLogisticsApprovalActions(t: any, getList: () => void) {
  const handleStatusUpdate = (orderId: number, status: string, label: string) => {
    ElMessageBox.confirm(`${t('common.confirmStatusUpdate')} "${label}"?`, t('common.ok'), {
      confirmButtonText: t('common.ok'),
      cancelButtonText: t('common.cancel'),
      type: 'primary'
    }).then(async () => {
      try {
        await updateOrderStatus(orderId, { status });
        ElMessage.success(t('admin.deposit.messages.success'));
        getList();
      } catch (error) {
        ElMessage.error(t('admin.deposit.messages.error'));
      }
    }).catch(() => {});
  };

  const handleCancelOrder = (order: any) => {
    ElMessageBox.prompt(t('order.list.cancellationReason'), t('order.list.cancelOrder'), {
      confirmButtonText: t('common.ok'),
      cancelButtonText: t('common.cancel'),
      inputPlaceholder: t('order.list.cancellationReason'),
      inputValidator: (value) => {
        if (!value || value.trim().length === 0) {
          return t('order.list.cancellationReason');
        }
        return true;
      }
    }).then(async ({ value }) => {
      try {
        await updateOrderStatus(order.id, {
          status: 'Cancelled',
          cancellationReason: value
        });
        ElMessage.success(t('admin.deposit.messages.success'));
        getList();
      } catch (error) {
        ElMessage.error(t('admin.deposit.messages.error'));
      }
    }).catch(() => {});
  };

  const handleRequestModification = (order: any) => {
    ElMessageBox.prompt(t('orderDetail.memos.modificationHistory'), t('orderDetail.memos.modificationHistory'), {
      confirmButtonText: t('common.ok'),
      cancelButtonText: t('common.cancel'),
      inputPlaceholder: t('orderDetail.memos.modificationHistory'),
      inputValidator: (value) => {
        if (!value || value.trim().length === 0) {
          return t('orderDetail.memos.modificationHistory');
        }
        return true;
      }
    }).then(async ({ value }) => {
      try {
        await updateOrderStatus(order.id, {
          status: 'FactoryRequested',
          modificationMemo: value
        });
        ElMessage.success(t('admin.deposit.messages.success'));
        getList();
      } catch (error) {
        ElMessage.error(t('admin.deposit.messages.error'));
      }
    }).catch(() => {});
  };

  const handleCloseByAgreement = (order: any) => {
    ElMessageBox.prompt(t('orderDetail.memos.closeMemo'), t('orderDetail.memos.closeMemo'), {
      confirmButtonText: t('common.ok'),
      cancelButtonText: t('common.cancel'),
      inputPlaceholder: t('orderDetail.memos.closeMemo'),
      inputValidator: (value) => {
        if (!value || value.trim().length === 0) {
          return t('orderDetail.memos.closeMemo');
        }
        return true;
      }
    }).then(async ({ value }) => {
      try {
        await updateOrderStatus(order.id, {
          status: 'REQUEST_CLOSE_BY_AGREEMENT',
          closeMemo: value
        });
        ElMessage.success(t('admin.deposit.messages.success'));
        getList();
      } catch (error) {
        ElMessage.error(t('admin.deposit.messages.error'));
      }
    }).catch(() => {});
  };

  return {
    handleStatusUpdate,
    handleCancelOrder,
    handleRequestModification,
    handleCloseByAgreement
  };
}
