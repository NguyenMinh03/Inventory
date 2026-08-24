import { api } from "./client";
import type {
  AuthResult,
  Category,
  CreateCategoryRequest,
  CreateProductRequest,
  CreatePurchaseOrderRequest,
  CreateStockMovementRequest,
  CreateStockTransferRequest,
  CreateSupplierRequest,
  CreateWarehouseRequest,
  LoginRequest,
  MovementHistoryItem,
  MovementHistoryQuery,
  PagedResult,
  Product,
  ProductQuery,
  PurchaseOrder,
  ReceivePurchaseOrderRequest,
  StockLevel,
  StockMovement,
  StockValuation,
  Supplier,
  UpdateCategoryRequest,
  UpdateProductRequest,
  UpdateSupplierRequest,
  UpdateWarehouseRequest,
  Warehouse,
} from "./types";

export const authApi = {
  login: (body: LoginRequest) => api.post<AuthResult>("/auth/login", body),
};

export const categoriesApi = {
  getAll: () => api.get<Category[]>("/Categories"),
  getById: (id: number) => api.get<Category>(`/Categories/${id}`),
  create: (body: CreateCategoryRequest) => api.post<Category>("/Categories", body),
  update: (id: number, body: UpdateCategoryRequest) => api.put<void>(`/Categories/${id}`, body),
  remove: (id: number) => api.delete<void>(`/Categories/${id}`),
};

export const warehousesApi = {
  getAll: () => api.get<Warehouse[]>("/Warehouses"),
  create: (body: CreateWarehouseRequest) => api.post<Warehouse>("/Warehouses", body),
  update: (id: number, body: UpdateWarehouseRequest) => api.put<void>(`/Warehouses/${id}`, body),
  remove: (id: number) => api.delete<void>(`/Warehouses/${id}`),
};

export const suppliersApi = {
  getAll: () => api.get<Supplier[]>("/Suppliers"),
  create: (body: CreateSupplierRequest) => api.post<Supplier>("/Suppliers", body),
  update: (id: number, body: UpdateSupplierRequest) => api.put<void>(`/Suppliers/${id}`, body),
  remove: (id: number) => api.delete<void>(`/Suppliers/${id}`),
};

export const productsApi = {
  getAll: (query: ProductQuery) =>
    api.get<PagedResult<Product>>("/Products", { ...query }),
  create: (body: CreateProductRequest) => api.post<Product>("/Products", body),
  update: (id: number, body: UpdateProductRequest) => api.put<void>(`/Products/${id}`, body),
  remove: (id: number) => api.delete<void>(`/Products/${id}`),
};

export const stockApi = {
  getLevels: () => api.get<StockLevel[]>("/stock/levels"),
  getLevelsByProduct: (productId: number) => api.get<StockLevel[]>(`/stock/levels/product/${productId}`),
  getLowStock: () => api.get<Product[]>("/stock/low"),
  recordMovement: (body: CreateStockMovementRequest) => api.post<StockMovement>("/stock/movements", body),
  transfer: (body: CreateStockTransferRequest) => api.post<StockMovement[]>("/stock/transfers", body),
};

export const purchaseOrdersApi = {
  getAll: () => api.get<PurchaseOrder[]>("/PurchaseOrders"),
  getById: (id: number) => api.get<PurchaseOrder>(`/PurchaseOrders/${id}`),
  create: (body: CreatePurchaseOrderRequest) => api.post<PurchaseOrder>("/PurchaseOrders", body),
  send: (id: number) => api.post<void>(`/PurchaseOrders/${id}/send`),
  receive: (id: number, body: ReceivePurchaseOrderRequest) => api.post<void>(`/PurchaseOrders/${id}/receive`, body),
  cancel: (id: number) => api.post<void>(`/PurchaseOrders/${id}/cancel`),
};

export const reportsApi = {
  lowStock: () => api.get<Product[]>("/reports/low-stock"),
  stockValuation: (by: "warehouse" | "category") => api.get<StockValuation[]>("/reports/stock-valuation", { by }),
  movementHistory: (query: MovementHistoryQuery) =>
    api.get<PagedResult<MovementHistoryItem>>("/reports/movement-history", { ...query }),
};
