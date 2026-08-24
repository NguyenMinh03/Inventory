// Mirrors InventorySystem.Application.DTOs. Kept hand-written and in sync
// manually rather than code-generated from the OpenAPI spec - reasonable for
// a project this size, but the first thing to automate (NSwag/openapi-typescript)
// if this API's surface kept growing.

export type MovementType = "In" | "Out" | "Transfer" | "Adjustment";
export type PurchaseOrderStatus = "Draft" | "Sent" | "Received" | "Cancelled";
export type UserRole = "Admin" | "Manager" | "Staff";

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

// ---- Auth ----

export interface LoginRequest {
  username: string;
  password: string;
}

export interface AuthResult {
  token: string;
  expiresAtUtc: string;
  username: string;
  role: UserRole;
}

// ---- Category ----

export interface Category {
  id: number;
  name: string;
  description?: string | null;
  parentCategoryId?: number | null;
  parentCategoryName?: string | null;
}

export interface CreateCategoryRequest {
  name: string;
  description?: string | null;
  parentCategoryId?: number | null;
}

export type UpdateCategoryRequest = CreateCategoryRequest;

// ---- Warehouse ----

export interface Warehouse {
  id: number;
  name: string;
  address?: string | null;
  isActive: boolean;
}

export interface CreateWarehouseRequest {
  name: string;
  address?: string | null;
}

export interface UpdateWarehouseRequest extends CreateWarehouseRequest {
  isActive: boolean;
}

// ---- Supplier ----

export interface Supplier {
  id: number;
  name: string;
  contactName?: string | null;
  email?: string | null;
  phone?: string | null;
  address?: string | null;
  isActive: boolean;
}

export interface CreateSupplierRequest {
  name: string;
  contactName?: string | null;
  email?: string | null;
  phone?: string | null;
  address?: string | null;
}

export interface UpdateSupplierRequest extends CreateSupplierRequest {
  isActive: boolean;
}

// ---- Product ----

export interface Product {
  id: number;
  sku: string;
  name: string;
  description?: string | null;
  unitOfMeasure: string;
  unitPrice: number;
  reorderLevel: number;
  isActive: boolean;
  categoryId: number;
  categoryName?: string | null;
}

export interface CreateProductRequest {
  sku: string;
  name: string;
  description?: string | null;
  unitOfMeasure: string;
  unitPrice: number;
  reorderLevel: number;
  categoryId: number;
}

export interface UpdateProductRequest {
  name: string;
  description?: string | null;
  unitOfMeasure: string;
  unitPrice: number;
  reorderLevel: number;
  categoryId: number;
  isActive: boolean;
}

export interface ProductQuery {
  page?: number;
  pageSize?: number;
  search?: string;
  sortBy?: string;
}

// ---- Stock ----

export interface StockLevel {
  productId: number;
  productSku: string;
  productName: string;
  warehouseId: number;
  warehouseName: string;
  quantityOnHand: number;
  lastUpdatedUtc: string;
}

export interface StockMovement {
  id: number;
  productId: number;
  productName: string;
  warehouseId: number;
  warehouseName: string;
  relatedWarehouseId?: number | null;
  relatedWarehouseName?: string | null;
  type: MovementType;
  quantity: number;
  occurredUtc: string;
  reference?: string | null;
  notes?: string | null;
}

export interface CreateStockMovementRequest {
  productId: number;
  warehouseId: number;
  type: MovementType;
  quantity: number;
  reference?: string | null;
  notes?: string | null;
}

export interface CreateStockTransferRequest {
  productId: number;
  sourceWarehouseId: number;
  destinationWarehouseId: number;
  quantity: number;
  reference?: string | null;
  notes?: string | null;
}

// ---- Purchase orders ----

export interface PurchaseOrderItem {
  id: number;
  productId: number;
  productName: string;
  quantityOrdered: number;
  quantityReceived: number;
  unitCost: number;
}

export interface PurchaseOrder {
  id: number;
  supplierId: number;
  supplierName: string;
  orderDateUtc: string;
  expectedDeliveryDateUtc?: string | null;
  status: PurchaseOrderStatus;
  notes?: string | null;
  items: PurchaseOrderItem[];
}

export interface CreatePurchaseOrderItemRequest {
  productId: number;
  quantityOrdered: number;
  unitCost: number;
}

export interface CreatePurchaseOrderRequest {
  supplierId: number;
  expectedDeliveryDateUtc?: string | null;
  notes?: string | null;
  items: CreatePurchaseOrderItemRequest[];
}

export interface ReceivePurchaseOrderItemRequest {
  purchaseOrderItemId: number;
  quantityReceived: number;
}

export interface ReceivePurchaseOrderRequest {
  warehouseId: number;
  items: ReceivePurchaseOrderItemRequest[];
}

// ---- Reports ----

export interface StockValuation {
  groupId: number;
  groupName: string;
  totalQuantityOnHand: number;
  totalValue: number;
}

export interface MovementHistoryItem {
  id: number;
  productId: number;
  productSku: string;
  productName: string;
  warehouseId: number;
  warehouseName: string;
  type: string;
  quantity: number;
  occurredUtc: string;
  reference?: string | null;
}

export interface MovementHistoryQuery {
  from?: string;
  to?: string;
  productId?: number;
  page?: number;
  pageSize?: number;
}
