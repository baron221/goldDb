export interface Customer {
  id: number;
  name: string;
  phone: string;
  email?: string;
  birthDate?: string;
  note?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface CustomerCreateRequest {
  name: string;
  phone: string;
  email?: string;
  birthDate?: string;
  note?: string;
}
