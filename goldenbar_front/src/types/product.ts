export interface IPurchasable {
  id: number;
  displayName: string;
  displayNo: string;
  categoryLarge: string;
  categoryMedium?: string;
  categorySmall?: string;
  factoryPrice: number;
  laborCost?: number;
  photoUrl?: string;
  isSet: boolean;
}

export function toPurchasable(item: any, isSet: boolean): IPurchasable {
  if (isSet) {
    return {
      id: item.id,
      displayName: item.title,
      displayNo: item.setNo || '',
      categoryLarge: item.categoryLarge,
      categoryMedium: item.categoryMedium,
      categorySmall: item.categorySmall,
      factoryPrice: item.factoryPrice,
      laborCost: 0,
      photoUrl: item.photos?.[0]?.photoUrl,
      isSet: true
    };
  } else {
    return {
      id: item.id,
      displayName: item.name,
      displayNo: item.productNo || '',
      categoryLarge: item.categoryLarge,
      categoryMedium: item.categoryMedium,
      categorySmall: item.categorySmall,
      factoryPrice: item.factoryPrice,
      laborCost: item.laborCost,
      photoUrl: item.photos?.[0]?.photoUrl,
      isSet: false
    };
  }
}
