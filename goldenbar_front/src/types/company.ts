export interface Company {
  id: number;
  name: string;
  ceo: string;
  region?: string;
  businessNumber: string;
  corporateNumber?: string;
  businessLicenseFileUrl?: string;
  businessType?: string;
  businessCategory?: string;
  phone?: string;
  addressBase?: string;
  addressDetail?: string;
  zipCode?: string;
  logisticsCode?: string;
  category: string;
}

export interface CompanyCreateRequest {
  name: string;
  ceo: string;
  region?: string;
  businessNumber: string;
  corporateNumber?: string;
  businessLicenseFileUrl?: string;
  businessType?: string;
  businessCategory?: string;
  phone?: string;
  addressBase?: string;
  addressDetail?: string;
  zipCode?: string;
  logisticsCode?: string;
  category: string;
}
