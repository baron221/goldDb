import { ref } from 'vue';

const isMobile = ref(false);

if (typeof window !== 'undefined') {
  isMobile.value = window.innerWidth <= 991;

  window.addEventListener('resize', () => {
    isMobile.value = window.innerWidth <= 991;
  });
}

export function useMobile() {
  return { isMobile };
}
