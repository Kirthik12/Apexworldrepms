export interface PropertyDto {
  id: number;
  title: string;
  description: string;
  price: number;
  category?: { id: number; name: string };
  address: string;
  carpetArea: number;
  facing: string;
  projectName: string;
  bedrooms: number;
  bathrooms: number;
  areaSize: number;
  furnishing: string;
  totalFloors: number;
  maintenance: number;
  carParking: number;
  isAvailable: boolean;
  status: string;
  images: { id: number; imageUrl: string }[];
  createdAt: string;
}

export interface PropertyCreateDto {
  title: string;
  description: string;
  price: number;
  category: string;
  address: string;
  carpetArea: number;
  facing: string;
  projectName: string;
  bedrooms: number;
  bathrooms: number;
  areaSize: number;
  furnishing: string;
  totalFloors: number;
  maintenance: number;
  carParking: number;
}

export interface PropertyUpdateDto {
  title: string;
  description: string;
  price: number;
  projectName: string;
  furnishing: string;
  totalFloors: number;
  maintenance: number;
}

export interface PropertyStatusUpdateDto {
  status: string;
  isAvailable: boolean;
}
