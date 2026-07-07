export const formatPrice = (price: any): string => {
  if (price === null || price === undefined) return '0';
  return (Math.floor(Number(price) || 0)).toLocaleString();
};

export const formatGoobPrice = (price: any, quantity: any): string => {
  if (price === null || price === undefined) return '0';
  if (quantity === null || quantity === undefined) return '0';

  return (Math.floor((Number(price) || 0) * (Number(quantity) || 0))).toLocaleString();
};
