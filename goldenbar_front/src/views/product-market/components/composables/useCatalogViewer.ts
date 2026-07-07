import { ref, computed, onMounted, onUnmounted, watch } from 'vue';

export function useCatalogViewer(selectedCatalog: any, activeCatalogId: any) {
  const currentPageIndex = ref(0);
  const isMobile = ref(false);
  const userViewMode = ref<'auto' | 'single' | 'double'>('auto');

  const isLeftLoading = ref(false);
  const isRightLoading = ref(false);
  const isSingleLoading = ref(false);

  const handleLeftImageLoad = () => { isLeftLoading.value = false; };
  const handleRightImageLoad = () => { isRightLoading.value = false; };
  const handleSingleImageLoad = () => { isSingleLoading.value = false; };

  const viewMode = computed(() => {
    if (userViewMode.value === 'single') return 'single';
    if (userViewMode.value === 'double') return 'double';
    return isMobile.value ? 'single' : 'double';
  });

  const checkIfMobile = () => { isMobile.value = window.innerWidth < 768; };

  onMounted(() => { checkIfMobile(); window.addEventListener('resize', checkIfMobile); });
  onUnmounted(() => { window.removeEventListener('resize', checkIfMobile); });

  const touchStartX = ref(0);
  const touchEndX = ref(0);
  const handleTouchStart = (e: TouchEvent) => { touchStartX.value = e.touches[0].clientX; };
  const handleTouchEnd = (e: TouchEvent) => {
    touchEndX.value = e.changedTouches[0].clientX;
    const diff = touchEndX.value - touchStartX.value;
    if (diff > 50) prevPage();
    else if (diff < -50) nextPage();
  };

  const maxPageIndex = computed(() => {
    if (!selectedCatalog.value?.pages || selectedCatalog.value.pages.length === 0) return 0;
    const n = selectedCatalog.value.pages.length;
    const maxIdx = viewMode.value === 'single' ? n - 1 : Math.ceil((n - 1) / 2);
    return Math.max(0, maxIdx);
  });

  const singlePage = computed(() => {
    if (!selectedCatalog.value?.pages || viewMode.value !== 'single') return null;
    return selectedCatalog.value.pages[currentPageIndex.value] || null;
  });

  const leftPage = computed(() => {
    if (!selectedCatalog.value?.pages || viewMode.value === 'single') return null;
    const idx = currentPageIndex.value;
    if (idx === 0) return null;
    return selectedCatalog.value.pages[(idx - 1) * 2 + 1] || null;
  });

  const rightPage = computed(() => {
    if (!selectedCatalog.value?.pages || viewMode.value === 'single') return null;
    const idx = currentPageIndex.value;
    if (idx === 0) return selectedCatalog.value.pages[0] || null;
    return selectedCatalog.value.pages[(idx - 1) * 2 + 2] || null;
  });

  const currentPageIndicatorText = computed(() => {
    if (!selectedCatalog.value) return '';
    const total = selectedCatalog.value.pages?.length || 0;
    if (viewMode.value === 'single') {
      const single = singlePage.value;
      if (!single) return '';
      return currentPageIndex.value === 0 ? `Cover Page / Total ${total}p` : `${single.pageNumber}p / Total ${total}p`;
    } else {
      const left = leftPage.value;
      const right = rightPage.value;
      if (!left && right) return `Cover Page / Total ${total}p`;
      if (left && !right) return `${left.pageNumber}p / Total ${total}p`;
      if (left && right) return `${left.pageNumber} - ${right.pageNumber}p / Total ${total}p`;
      return '';
    }
  });

  const currentProducts = computed(() => {
    const prods: any[] = [];
    if (viewMode.value === 'single') {
      if (singlePage.value?.products) prods.push(...singlePage.value.products);
    } else {
      if (leftPage.value?.products) prods.push(...leftPage.value.products);
      if (rightPage.value?.products) prods.push(...rightPage.value.products);
    }
    return Array.from(new Map(prods.map(item => [item.id, item])).values());
  });

  watch([currentPageIndex, activeCatalogId, viewMode], () => {
    if (selectedCatalog.value) {
      if (viewMode.value === 'single') isSingleLoading.value = !!singlePage.value;
      else { isLeftLoading.value = !!leftPage.value; isRightLoading.value = !!rightPage.value; }
    }
  }, { immediate: true });

  watch(viewMode, (newMode) => {
    if (!selectedCatalog.value?.pages) return;
    const total = selectedCatalog.value.pages.length;
    if (newMode === 'single') {
      const spreadIdx = currentPageIndex.value;
      currentPageIndex.value = spreadIdx === 0 ? 0 : Math.min((spreadIdx - 1) * 2 + 1, total - 1);
    } else {
      const pageIdx = currentPageIndex.value;
      currentPageIndex.value = pageIdx === 0 ? 0 : Math.floor((pageIdx - 1) / 2) + 1;
    }
  });

  const prevPage = () => { if (currentPageIndex.value > 0) currentPageIndex.value--; };
  const nextPage = () => { if (currentPageIndex.value < maxPageIndex.value) currentPageIndex.value++; };
  const handleSliderInput = (val: any) => { currentPageIndex.value = val; };

  return {
    currentPageIndex, userViewMode, viewMode, isMobile,
    isLeftLoading, isRightLoading, isSingleLoading,
    handleLeftImageLoad, handleRightImageLoad, handleSingleImageLoad,
    handleTouchStart, handleTouchEnd,
    maxPageIndex, singlePage, leftPage, rightPage,
    currentPageIndicatorText, currentProducts,
    prevPage, nextPage, handleSliderInput
  };
}
