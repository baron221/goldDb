import { computed } from 'vue';
import useUserStore from '@/store/modules/user';

// Sourced from the retailer's OWN company (Company.HidePrice, toggled per-company in
// 거래처관리 회사 정보 수정), not a role-wide permission - lets an admin hide price for
// specific retailers only, without affecting every 소매점 account.
export function useCanViewPrice() {
  const userStore = useUserStore();

  return computed(() => {
    if (userStore.companyType !== 'RTL') return true;
    return userStore.canViewPrice !== false;
  });
}
