export const formatPrice = (price: any): string => {
  if (price === null || price === undefined) return '0';
  return (Math.floor(Number(price) || 0)).toLocaleString();
};

export const formatGoobPrice = (price: any, quantity: any): string => {
  if (price === null || price === undefined) return '0';
  if (quantity === null || quantity === undefined) return '0';

  return (Math.floor((Number(price) || 0) * (Number(quantity) || 0))).toLocaleString();
};

// A purity-ratio conversion like `3 * 0.825` doesn't land on exactly 2.475 in IEEE-754
// floating point (it's ~2.4749999999999996), so a plain .toFixed(2) on the raw product
// silently rounds DOWN to "2.47" instead of "2.48" - while the same figure computed
// server-side (C# decimal, no binary-fraction error) rounds correctly. The float noise
// only shows up many decimals deep, so re-stringifying at 10 decimals first (which
// rounds it away) before scaling and rounding to 2 corrects exactly this class of case -
// a plain Number.EPSILON nudge is too small to matter at this magnitude and doesn't fix it.
export const roundTo2 = (value: number): number => {
  return Math.round(Number(value.toFixed(10)) * 100) / 100;
};
