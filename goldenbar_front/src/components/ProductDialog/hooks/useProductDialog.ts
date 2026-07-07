import { ref, reactive, computed, watch, nextTick } from 'vue';
import { ElMessage } from 'element-plus';
import useUserStore from '@/store/modules/user';
import useCodeStore from '@/store/modules/code';
import { getProduct, createProduct, updateProduct } from '@/api/product';

export function useProductDialog(props: any, emit: any, dataForm: any) {
  const codeStore = useCodeStore();
  const userStore = useUserStore();

  const visible = ref(false);
  const activeTab = ref('basic');
  const productColors = ref<any[]>([]);
  const productSizes = ref<any[]>([]);
  const allCodesFullMap = ref<any>({});
  const productCategoryTree = ref<any>(null);
  const largeCategoryId = ref<number | null>(null);
  const mediumCategoryId = ref<number | null>(null);
  const photoManagerVisible = ref(false);
  const isCompanyUser = computed(() => !userStore.roles.includes('admin'));

  const temp = ref<any>({
    id: undefined,
    name: '',
    productNo: '',
    purity: [],
    weight: 0,
    companyId: undefined,
    categoryLarge: '',
    categoryMedium: '',
    categorySmall: '',
    factoryPrice: 0,
    designNotice: '',
    productSize: '',
    basicLoss: 0,
    centerStone: '',
    centerStoneShape: '',
    centerStoneSize: '',
    centerStoneCount: 0,
    sideStone: '',
    sideStoneShape: '',
    sideStoneSize: '',
    sideStoneCount: 0,
    photos: [],
    isPublic: false,
    laborCost: 0,
    colors: [],
    sizes: [],
    optionWeights: []
  });

  const activePhotoUrl = ref<string>('');
  const isDataLoading = ref(false);
  const combinationGridData = ref<any[]>([]);
  const calcBasePurity = ref<string>('');
  const calcBaseWeight = ref<number>(0);
  const mediumCategoryOptions = ref<any[]>([]);
  const smallCategoryOptions = ref<any[]>([]);

  const mainPhoto = computed(() => {
    if (temp.value.photos && temp.value.photos.length > 0) {
      return temp.value.photos.find((p: any) => p.isMain) || temp.value.photos[0];
    }
    return null;
  });

  const mainPhotoUrl = computed(() => {
    return mainPhoto.value ? mainPhoto.value.photoUrl : '';
  });

  const currentViewPhotoUrl = computed(() => {
    return activePhotoUrl.value || mainPhotoUrl.value;
  });

  const sortedPhotos = computed(() => {
    if (!temp.value.photos) return [];
    return [...temp.value.photos].sort((a: any, b: any) => {
      const aMain = a.isMain ? 1 : 0;
      const bMain = b.isMain ? 1 : 0;
      return bMain - aMain;
    });
  });

  const densityMap: Record<string, number> = {
    '24K': 19.30,
    'PURE_GOLD': 19.30,
    '18K': 15.50,
    '14K': 13.10,
    '10K': 11.30,
    '22K': 17.70,
    'PT950': 20.10,
    'PT900': 19.80,
    'PLATINUM': 20.10,
    'PT': 20.10,
    'SILVER': 10.30,
    'SV925': 10.30,
    'SV': 10.30
  };

  const getDensity = (purityCode: string): number => {
    const codeUpper = purityCode.toUpperCase();
    if (densityMap[codeUpper]) return densityMap[codeUpper];
    if (codeUpper.includes('24K') || codeUpper.includes('PURE_GOLD') || codeUpper.includes('PUREGOLD')) return 19.30;
    if (codeUpper.includes('18K')) return 15.50;
    if (codeUpper.includes('14K')) return 13.10;
    if (codeUpper.includes('10K')) return 11.30;
    if (codeUpper.includes('22K')) return 17.70;
    if (codeUpper.includes('PT') || codeUpper.includes('PLATINUM')) return 20.10;
    if (codeUpper.includes('SV') || codeUpper.includes('SILVER')) return 10.30;
    return 15.50;
  };

  const getCategoryChildren = (code: string): any[] => {
    if (!productCategoryTree.value) return [];
    const largeNode = productCategoryTree.value.children?.find((c: any) => c.code === code);
    if (largeNode) {
      return largeNode.children || [];
    }
    if (productCategoryTree.value.children) {
      for (const large of productCategoryTree.value.children) {
        const mediumNode = large.children?.find((c: any) => c.code === code);
        if (mediumNode) {
          return mediumNode.children || [];
        }
      }
    }
    return [];
  };

  const resetTemp = () => {
    largeCategoryId.value = null;
    mediumCategoryId.value = null;
    mediumCategoryOptions.value = [];
    smallCategoryOptions.value = [];
    activePhotoUrl.value = '';
    temp.value = {
      id: undefined,
      name: '',
      productNo: '',
      purity: [],
      weight: 0,
      companyId: isCompanyUser.value ? userStore.companyId || undefined : undefined,
      categoryLarge: '',
      categoryMedium: '',
      categorySmall: '',
      factoryPrice: 0,
      designNotice: '',
      productSize: '',
      basicLoss: 0,
      centerStone: '',
      centerStoneShape: '',
      centerStoneSize: '',
      centerStoneCount: 0,
      sideStone: '',
      sideStoneShape: '',
      sideStoneSize: '',
      sideStoneCount: 0,
      photos: [],
      isPublic: false,
      laborCost: 0,
      colors: [],
      sizes: [],
      optionWeights: []
    };
  };

  const fetchCodes = async () => {
    try {
      await codeStore.fetchCodes();
      const codes = codeStore.codes;

      const fullMap: any = {};
      const flatten = (list: any[]) => {
        list.forEach(item => {
          fullMap[item.code] = item;
          if (item.children) flatten(item.children);
        });
      };
      flatten(codes);
      allCodesFullMap.value = fullMap;

      const categoryGroup = codes.find((c: any) => c.code === 'PRODUCT_CATEGORY');
      productCategoryTree.value = categoryGroup;

      productColors.value = codeStore.getCodesByGroupStore('PRODUCT_COLOR');
      productSizes.value = codeStore.getCodesByGroupStore('PRODUCT_SIZE');
    } catch (error) {
      console.error('Failed to fetch codes:', error);
    }
  };

  const loadProductData = async (id: number) => {
    try {
      const response = await getProduct(id);
      const data = response.data;

      if (data.categoryLarge) {
        const selectedLarge = productCategoryTree.value?.children?.find((c: any) => c.code === data.categoryLarge);
        largeCategoryId.value = selectedLarge ? selectedLarge.id : null;
        mediumCategoryOptions.value = getCategoryChildren(data.categoryLarge);
      } else {
        largeCategoryId.value = null;
        mediumCategoryOptions.value = [];
      }

      if (data.categoryMedium) {
        let selectedMedium: any = null;
        if (productCategoryTree.value?.children) {
          for (const large of productCategoryTree.value.children) {
            const found = large.children?.find((c: any) => c.code === data.categoryMedium);
            if (found) {
              selectedMedium = found;
              break;
            }
          }
        }
        mediumCategoryId.value = selectedMedium ? selectedMedium.id : null;
        smallCategoryOptions.value = getCategoryChildren(data.categoryMedium);
      } else {
        mediumCategoryId.value = null;
        smallCategoryOptions.value = [];
      }

      temp.value = {
        ...data,
        photos: data.photos || [],
        colors: data.colors ? data.colors.split(',') : [],
        sizes: data.sizes ? data.sizes.split(',') : [],
        purity: data.purity ? data.purity.split(',') : [],
        optionWeights: data.optionWeights || []
      };
    } catch (error) {
      console.error(error);
      ElMessage.error('제품 정보를 불러오는데 실패했습니다.');
    }
  };

  const handleLargeCategoryChange = (val: string) => {
    if (isDataLoading.value) return;

    const children = getCategoryChildren(val);
    mediumCategoryOptions.value = children;

    const selectedLarge = productCategoryTree.value?.children?.find((c: any) => c.code === val);
    largeCategoryId.value = selectedLarge ? selectedLarge.id : null;

    temp.value.categoryMedium = '';
    temp.value.categorySmall = '';
    mediumCategoryId.value = null;
    smallCategoryOptions.value = [];
  };

  const handleMediumCategoryChange = (val: string) => {
    if (isDataLoading.value) return;

    const children = getCategoryChildren(val);
    smallCategoryOptions.value = children;

    let selectedMedium: any = null;
    if (productCategoryTree.value?.children) {
      for (const large of productCategoryTree.value.children) {
        const found = large.children?.find((c: any) => c.code === val);
        if (found) {
          selectedMedium = found;
          break;
        }
      }
    }
    mediumCategoryId.value = selectedMedium ? selectedMedium.id : null;

    temp.value.categorySmall = '';
  };

  const onLargeOptionsLoaded = (options: any[]) => {
    if (largeCategoryId.value) return;
    if (temp.value.categoryLarge) {
      const selectedLarge = productCategoryTree.value?.children?.find((c: any) => c.code === temp.value.categoryLarge);
      largeCategoryId.value = selectedLarge ? selectedLarge.id : null;
      mediumCategoryOptions.value = getCategoryChildren(temp.value.categoryLarge);
    }
  };

  const onMediumOptionsLoaded = (options: any[]) => {
    if (mediumCategoryId.value) return;
    if (temp.value.categoryMedium) {
      let selectedMedium: any = null;
      if (productCategoryTree.value?.children) {
        for (const large of productCategoryTree.value.children) {
          const found = large.children?.find((c: any) => c.code === temp.value.categoryMedium);
          if (found) {
            selectedMedium = found;
            break;
          }
        }
      }
      mediumCategoryId.value = selectedMedium ? selectedMedium.id : null;
      smallCategoryOptions.value = getCategoryChildren(temp.value.categoryMedium);
    }
  };

  const getCodeName = (code: string): string => {
    return codeStore.getCodeName(code);
  };

  const applyBulkWeightConversion = () => {
    if (!calcBasePurity.value) {
      ElMessage.warning('기준 함량을 선택해 주세요.');
      return;
    }
    if (!calcBaseWeight.value || calcBaseWeight.value <= 0) {
      ElMessage.warning('0보다 큰 기준 중량을 입력해 주세요.');
      return;
    }

    const basePurity = calcBasePurity.value;
    const baseWeight = calcBaseWeight.value;
    const baseDensity = getDensity(basePurity);

    if (combinationGridData.value.length === 0) {
      ElMessage.warning('환산할 함량 및 컬러 조합이 존재하지 않습니다.');
      return;
    }

    combinationGridData.value.forEach((row: any) => {
      const rowDensity = getDensity(row.purity);
      const converted = baseWeight * (rowDensity / baseDensity);
      row.weight = Math.round(converted * 100) / 100;
    });

    temp.value.optionWeights = [...combinationGridData.value];
    ElMessage.success(`기준 함량(${getCodeName(basePurity)}) ${baseWeight}g 대비 비중 환산 중량이 모든 조합에 일괄 적용되었습니다.`);
  };

  const createData = async (stayOpen = false) => {
    if (!dataForm.value) return;
    await dataForm.value.validate(async (valid: boolean) => {
      if (valid) {
        try {
          const payload = {
            ...temp.value,
            colors: temp.value.colors ? temp.value.colors.join(',') : '',
            sizes: temp.value.sizes ? temp.value.sizes.join(',') : '',
            purity: temp.value.purity ? temp.value.purity.join(',') : '',
            photos: temp.value.photos.map((p: any, idx: number) => ({
              ...p,
              sortOrder: idx
            })),
            optionWeights: temp.value.optionWeights
          };
          await createProduct(payload);

          if (!stayOpen) {
            visible.value = false;
          } else {
            resetTemp();
            activeTab.value = 'basic';
            nextTick(() => {
              if (dataForm.value) dataForm.value.clearValidate();
            });
          }

          ElMessage.success('제품이 성공적으로 등록되었습니다.');
          emit('saved');
        } catch (error) {
          console.error(error);
        }
      }
    });
  };

  const updateData = async () => {
    if (!dataForm.value) return;
    await dataForm.value.validate(async (valid: boolean) => {
      if (valid) {
        try {
          const payload = {
            ...temp.value,
            colors: temp.value.colors ? temp.value.colors.join(',') : '',
            sizes: temp.value.sizes ? temp.value.sizes.join(',') : '',
            purity: temp.value.purity ? temp.value.purity.join(',') : '',
            photos: temp.value.photos.map((p: any, idx: number) => ({
              ...p,
              sortOrder: idx
            })),
            optionWeights: temp.value.optionWeights
          };
          await updateProduct(temp.value.id, payload);
          visible.value = false;
          ElMessage.success('제품 정보가 수정되었습니다.');
          emit('saved');
        } catch (error) {
          console.error(error);
        }
      }
    });
  };

  watch(() => props.modelValue, async (val) => {
    visible.value = val;
    if (val) {
      activeTab.value = 'basic';
      resetTemp();
      await fetchCodes();
      if (props.productId) {
        isDataLoading.value = true;
        await loadProductData(props.productId);
        setTimeout(() => {
          isDataLoading.value = false;
          nextTick(() => {
            dataForm.value?.validate((valid: boolean) => {});
          });
        }, 200);
      } else {
        isDataLoading.value = false;
        nextTick(() => {
          dataForm.value?.clearValidate();
        });
      }
    } else {
      isDataLoading.value = false;
    }
  });

  watch(() => visible.value, (val) => {
    emit('update:modelValue', val);
    if (!val) {
      activePhotoUrl.value = '';
    }
  });

  watch(() => temp.value.photos, () => {
    activePhotoUrl.value = '';
  }, { deep: true });

  watch(() => temp.value.purity, (newPurities) => {
    if (newPurities && newPurities.length > 0) {
      if (!calcBasePurity.value || !newPurities.includes(calcBasePurity.value)) {
        calcBasePurity.value = newPurities[0];
      }
    } else {
      calcBasePurity.value = '';
    }
  }, { immediate: true });

  watch(() => temp.value.weight, (newVal) => {
    if (newVal && (!calcBaseWeight.value || calcBaseWeight.value === 0)) {
      calcBaseWeight.value = newVal;
    }
  }, { immediate: true });

  watch(() => [temp.value.purity, temp.value.colors], () => {
    if (!temp.value.purity || !temp.value.colors) {
      combinationGridData.value = [];
      return;
    }

    const currentWeightsMap = new Map();
    if (temp.value.optionWeights) {
      temp.value.optionWeights.forEach((ow: any) => {
        currentWeightsMap.set(`${ow.purity}_${ow.color}`, ow.weight);
      });
    }

    const grid: any[] = [];
    temp.value.purity.forEach((pCode: string) => {
      temp.value.colors.forEach((cCode: string) => {
        const key = `${pCode}_${cCode}`;
        const weight = currentWeightsMap.has(key) ? currentWeightsMap.get(key) : temp.value.weight;
        grid.push({
          purity: pCode,
          color: cCode,
          weight: weight
        });
      });
    });

    combinationGridData.value = grid;
    temp.value.optionWeights = grid;
  }, { deep: true });

  return {
    visible,
    activeTab,
    productColors,
    productSizes,
    temp,
    activePhotoUrl,
    currentViewPhotoUrl,
    sortedPhotos,
    mediumCategoryOptions,
    smallCategoryOptions,
    photoManagerVisible,
    isCompanyUser,
    combinationGridData,
    calcBasePurity,
    calcBaseWeight,
    handleLargeCategoryChange,
    handleMediumCategoryChange,
    onLargeOptionsLoaded,
    onMediumOptionsLoaded,
    applyBulkWeightConversion,
    createData,
    updateData
  };
}
