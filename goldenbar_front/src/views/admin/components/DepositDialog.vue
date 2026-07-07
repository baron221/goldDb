<template>
<base-popup v-model="visible" :title="$t('admin.deposit.title')" width="500px" @close="handleClose">
    <div v-if="user" style="margin-bottom: 1.25rem; padding: 0.9375rem; border-radius: 2px;">
      <div style="margin-bottom: 0.625rem;"><strong>{{ $t('admin.deposit.member') }}</strong> {{ user.userDisplayName }} ({{ user.userName }})</div>
      <div style="font-size: 1rem;"><strong>{{ $t('admin.deposit.totalReceivable') }}</strong> <span style="color: #f56c6c; font-weight: bold;">₩ {{ formatPrice(user.totalReceivable) }}</span></div>
    </div>

    <el-form :model="depositForm" label-width="120px" :rules="depositRules" ref="depositFormRef">
      <el-form-item :label="$t('admin.deposit.amountLabel')" prop="amount">
        <el-input-number
          v-model="depositForm.amount"
          :min="1"
          :max="user && user.totalReceivable > 0 ? user.totalReceivable : 1"
          :step="1000"
          style="width: 100%"
        />
      </el-form-item>
      <el-form-item :label="$t('admin.deposit.memoLabel')" prop="memo">
        <el-input
          v-model="depositForm.memo"
          type="textarea"
          :rows="3"
          :placeholder="$t('admin.deposit.placeholder')"
        />
      </el-form-item>
    </el-form>
    <template #footer>
      <span class="dialog-footer">
        <el-button @click="visible = false">{{ $t('common.cancel') }}</el-button>
        <el-button type="primary" :loading="submitting" @click="handleDepositSubmit">
          {{ $t('admin.deposit.submitBtn') }}
        </el-button>
      </span>
    </template>
  </base-popup>
</template>

<script setup lang="ts">
import { ref, reactive, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { processDeposit } from '@/api/receivable';
import { ElMessage, ElMessageBox } from 'element-plus';
import BasePopup from '@/components/BasePopup/index.vue';
import { formatPrice } from '@/utils/format';

const { t } = useI18n();

const props = defineProps({
  modelValue: Boolean,
  user: {
    type: Object,
    default: () => null
  }
});

const emit = defineEmits(['update:modelValue', 'saved']);

const visible = ref(false);
const submitting = ref(false);
const depositFormRef = ref();

const depositForm = reactive({
  amount: 0,
  memo: ''
});

const depositRules = {
  amount: [
    { required: true, message: t('admin.deposit.rules.amount'), trigger: 'blur' }
  ]
};

watch(() => props.modelValue, (val) => {
  visible.value = val;
  if (val && props.user) {
    depositForm.amount = props.user.totalReceivable;
    depositForm.memo = '';
  }
});

watch(visible, (val) => {
  emit('update:modelValue', val);
});

const handleClose = () => {
  visible.value = false;
  if (depositFormRef.value) {
    depositFormRef.value.resetFields();
  }
};

const handleDepositSubmit = () => {
  if (!depositFormRef.value) return;

  depositFormRef.value.validate(async (valid: boolean) => {
    if (valid) {
      if (depositForm.amount <= 0) {
        ElMessage.error(t('admin.deposit.messages.amountZero'));
        return;
      }

      ElMessageBox.confirm(
        t('admin.deposit.messages.confirm', { name: props.user.userDisplayName, amount: formatPrice(depositForm.amount) }),
        t('admin.deposit.messages.confirmTitle'),
        {
          confirmButtonText: t('common.ok'),
          cancelButtonText: t('common.cancel'),
          type: 'warning'
        }
      ).then(async () => {
        submitting.value = true;
        try {
          await processDeposit({
            userId: props.user.userId,
            amount: depositForm.amount,
            memo: depositForm.memo
          });
          ElMessage.success(t('admin.deposit.messages.success'));
          visible.value = false;
          emit('saved');
        } catch (error) {
          console.error('Failed to process deposit:', error);
          ElMessage.error(t('admin.deposit.messages.error'));
        } finally {
          submitting.value = false;
        }
      }).catch(() => {});
    }
  });
};
</script>

