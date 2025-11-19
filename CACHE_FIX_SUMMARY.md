# Tóm tắt các thay đổi để sửa vấn đề Redis Cache

## Ngày: 19/11/2025

## Vấn đề ban đầu
- Cache Redis hoạt động chậm và không nhất quán
- UI không tự động refresh sau thao tác Create/Update/Delete
- Phải F5 nhiều lần mới thấy dữ liệu mới
- Dữ liệu không hiển thị ngay lập tức sau khi thêm/sửa/xóa

## Các thay đổi đã thực hiện

### 1. Backend (API)

#### a. `RedisCacheService.cs`
**Vấn đề**: Pattern matching không bao gồm InstanceName prefix (`TodoApp:`)
**Giải pháp**: 
```csharp
// Thêm prefix vào pattern khi xóa cache theo pattern
var fullPattern = $"TodoApp:{pattern}";
var keys = server.Keys(pattern: fullPattern).ToArray();
```

#### b. `ServiceExtensions.cs`
**Vấn đề**: Services không được đăng ký đúng thứ tự
**Giải pháp**:
```csharp
// Đăng ký base services trước
services.AddScoped<TodoItemService>();
services.AddScoped<TodoItemReportService>();

// Sau đó đăng ký cached versions
services.AddScoped<ITodoItemService, CacheTodoItemService>();
services.AddScoped<ITodoItemReportService, CachedTodoItemReportService>();
```

#### c. `CacheTodoItemService.cs`
**Cải thiện**: Thêm logging chi tiết hơn để dễ debug
```csharp
_logger.LogInformation("Created Todo-item {Id} successfully and cleared search cache", result.Data?.Id);
```

### 2. Frontend (UI)

#### a. `TodoItems/index.tsx`
**Vấn đề chính**: Logic reload không đúng sau thao tác CRUD

**Giải pháp CREATE**:
```typescript
// Thay vì dùng refreshKey, check điều kiện và reload trực tiếp
if (currentPage === 1 && searchText === '') {
  // Nếu đã ở trang 1 và không có search, force reload
  await fetchTasks(1, pageSize, '');
} else {
  // Chuyển về trang 1 sẽ tự động trigger useEffect
  setCurrentPage(1);
}
```

**Giải pháp UPDATE**:
```typescript
// Reload trang hiện tại để cập nhật dữ liệu từ cache mới
await fetchTasks(currentPage, pageSize, searchText);
```

**Giải pháp DELETE**:
```typescript
// Xử lý logic quay về trang trước nếu xóa item cuối cùng
const newTotal = total - 1;
const maxPage = Math.ceil(newTotal / pageSize);
const targetPage = currentPage > maxPage ? Math.max(1, maxPage) : currentPage;

if (targetPage !== currentPage) {
  setCurrentPage(targetPage);  // useEffect sẽ trigger
} else {
  fetchTasks(targetPage, pageSize, searchText);
}
```

**Loại bỏ `refreshKey`**: Không cần thiết vì đã có logic reload trực tiếp

#### b. `axios.ts`
**Cải thiện**: Thêm timeout và error handling
```typescript
const api = axios.create({
  baseURL: "https://localhost:7196",
  timeout: 30000, // 30 seconds
  headers: {
    'Content-Type': 'application/json',
  }
});

// Add response interceptor
api.interceptors.response.use(
  (response) => response,
  (error) => {
    // Handle timeout và connection errors
    if (error.code === 'ECONNABORTED') {
      console.error('Request timeout');
    }
    return Promise.reject(error);
  }
);
```

## Luồng hoạt động mới

### 1. Khi LOAD trang
1. useEffect trigger → gọi `fetchTasks()`
2. API nhận request → kiểm tra Redis cache
3. **Cache HIT**: Trả về data từ Redis ngay lập tức
4. **Cache MISS**: Query database → lưu vào Redis → trả về data
5. UI hiển thị data

### 2. Khi CREATE item mới
1. User submit form → gọi `createTodoItem()`
2. API tạo item mới trong database
3. API xóa tất cả search cache: `todo-item:search:*`
4. API trả về success
5. UI:
   - Clear searchText
   - Nếu đang ở trang 1: force reload ngay
   - Nếu không: chuyển về trang 1 (trigger useEffect)
6. useEffect trigger → fetch lại data
7. API query database (cache đã clear) → cache kết quả mới
8. UI hiển thị data mới (bao gồm item vừa tạo)

### 3. Khi UPDATE item
1. User submit form → gọi `updateTodoItem()`
2. API update item trong database
3. API xóa:
   - Cache của item đó: `todo-item:{id}`
   - Tất cả search cache: `todo-item:search:*`
4. API trả về success
5. UI reload trang hiện tại: `fetchTasks(currentPage, ...)`
6. API query database → cache kết quả mới
7. UI hiển thị data đã update ở đúng trang hiện tại

### 4. Khi DELETE item
1. User confirm delete → gọi `deleteTodoItem()`
2. API soft delete item (set IsDeleted = true)
3. API xóa:
   - Cache của item đó: `todo-item:{id}`
   - Tất cả search cache: `todo-item:search:*`
4. API trả về success
5. UI:
   - Tính toán số trang mới
   - Nếu xóa item cuối cùng của trang → quay về trang trước
   - Nếu không → reload trang hiện tại
6. API query database → cache kết quả mới
7. UI hiển thị data đã cập nhật

## Kết quả mong đợi

✅ **CREATE**: Item mới xuất hiện ngay ở trang 1, không cần search lại
✅ **UPDATE**: Item được cập nhật ngay tại trang hiện tại
✅ **DELETE**: Danh sách tự động cập nhật, tự động quay về trang trước nếu cần
✅ **Không cần F5**: Tất cả thao tác đều tự động refresh
✅ **Cache hoạt động nhanh**: Data được lưu trong Redis với TTL 5 phút
✅ **Consistent**: Mọi thao tác đều invalidate cache đúng cách

## Test Steps

1. **Test Create**:
   - Ở trang 1, thêm item mới → Item xuất hiện ngay
   - Ở trang 2, thêm item mới → Tự động về trang 1 và hiện item mới
   
2. **Test Update**:
   - Ở trang bất kỳ, sửa item → Item cập nhật ngay tại trang đó
   - Không bị nhảy về trang 1
   
3. **Test Delete**:
   - Xóa item giữa danh sách → Danh sách cập nhật ngay
   - Xóa item cuối cùng của trang → Tự động quay về trang trước
   
4. **Test Cache**:
   - Load trang lần 1 → Kiểm tra log "Cache MISS"
   - Load trang lần 2 (trong 5 phút) → Kiểm tra log "Cache HIT"
   - Thêm/sửa/xóa item → Cache bị clear
   - Load lại → Kiểm tra log "Cache MISS" → "Cache HIT"

## Notes

- Redis cache TTL: 5 phút (có thể điều chỉnh trong appsettings)
- InstanceName: `TodoApp:` (được tự động thêm vào mọi cache key)
- Pattern matching khi clear cache: `TodoApp:todo-item:search:*`
- Axios timeout: 30 giây

## Troubleshooting

Nếu vẫn gặp vấn đề:

1. **Kiểm tra Redis có chạy không**: `redis-cli ping` → phải trả về `PONG`
2. **Kiểm tra logs**: Xem API logs để thấy Cache HIT/MISS
3. **Clear cache thủ công**: `redis-cli FLUSHALL`
4. **Kiểm tra connection string**: Đảm bảo `localhost:6379` đúng
5. **Tắt Redis để test**: Set `RedisSettings:Enabled = false` → dùng MemoryCache

## Configuration

File: `appsettings.Development.json`
```json
{
  "ConnectionStrings": {
    "RedisConnection": "localhost:6379,abortConnect=false"
  },
  "RedisSettings": {
    "Enabled": true,
    "DefaultExpirationMinutes": 10,
    "TaskCacheExpirationMinutes": 5,
    "ReportCacheExpirationMinutes": 30
  }
}
```
