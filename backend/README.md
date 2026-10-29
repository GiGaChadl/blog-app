# BlogApp.Api — Backend .NET (nền tảng Clean Architecture)

## ⚠️ Đọc trước khi build

Code này được viết **không có khả năng build/test trực tiếp** — môi trường soạn code chặn
NuGet (`api.nuget.org` bị 403 Forbidden). Chỉ riêng **`BlogApp.Domain`** đã được build thật
và **pass 0 lỗi** (không cần package ngoài). 3 project còn lại (`Application`, `Infrastructure`,
`Api`) viết dựa trên kiến thức chắc chắn về .NET/EF Core/JWT nhưng **chưa build thật** —
bạn cần tự `dotnet restore` + `dotnet build` trên máy có internet đầy đủ.

## Cài đặt & chạy

```bash
cd BlogApp
dotnet restore
dotnet build
```

### Trước khi chạy — cần có PostgreSQL với schema đã tạo sẵn

Dự án này **không dùng EF Core Migrations** để tạo schema ở giai đoạn hiện tại — schema
thật đã được viết tay và **test kỹ bằng 10 tình huống thật** (xem `blog-database-schema.sql`
và lịch sử CHANGELOG). Chạy file đó trước:

```bash
psql -U postgres -d blog_ca_nhan -f blog-database-schema.sql
```

Sau đó sửa `src/BlogApp.Api/appsettings.json`:
- `ConnectionStrings:DefaultConnection` — đổi đúng thông tin DB của bạn
- `Jwt:Secret` — đổi thành chuỗi bí mật riêng, **tối thiểu 32 ký tự**, không commit lên Git thật

Chạy:
```bash
cd src/BlogApp.Api
dotnet run
```

## ⚠️ Chỗ RỦI RO CAO NHẤT khi build — kiểm tra đầu tiên nếu lỗi

File `src/BlogApp.Infrastructure/DependencyInjection.cs` — đoạn `NpgsqlDataSourceBuilder.MapEnum<T>()`
dùng để ánh xạ 8 enum C# sang 8 native PostgreSQL ENUM TYPE đã tạo trong file SQL. Đây là API
khá đặc thù của gói `Npgsql.EntityFrameworkCore.PostgreSQL`, và **mình không build/test được**
để xác nhận chính xác 100% cú pháp khớp đúng phiên bản package sẽ tải về máy bạn.

Nếu build lỗi ở đây, tìm docs chính thức Npgsql mục **"Enum Mapping"** ứng với version cụ
thể bạn restore — nhiều khả năng chỉ cần chỉnh nhỏ tên method/tham số, không phải sai toàn bộ
cách tiếp cận.

## Đã làm trong đợt "nền tảng chung" này

**Cấu trúc Clean Architecture — 4 project:**
- `BlogApp.Domain` — 20 entity + 8 enum, **không phụ thuộc bất kỳ package ngoài nào** (đã build
  thật, 0 lỗi)
- `BlogApp.Application` — interface (`IApplicationDbContext`, `IJwtTokenService`,
  `IPasswordHasher`) + `AuthService` (business logic đăng nhập, chỉ phụ thuộc interface)
- `BlogApp.Infrastructure` — `ApplicationDbContext` + 20 Fluent API configuration (khớp
  chính xác từng cột/FK/CHECK/index của `blog-database-schema.sql`), `JwtTokenService`
  (dùng `System.IdentityModel.Tokens.Jwt`), `PasswordHasher` (dùng BCrypt)
- `BlogApp.Api` — `Program.cs` wiring JWT Bearer authentication, `AuthController` với
  endpoint `POST /api/auth/login`

**Điểm thiết kế quan trọng cần nhớ:**
1. **DbContext KHÔNG tự tạo/sửa schema** — schema là file SQL đã test, EF Core chỉ *map vào*
   schema có sẵn. Chưa cấu hình EF Core Migrations ở bước này.
2. Mọi FK Fluent API (`OnDelete(...)`) khớp đúng quyết định đã chốt lúc thiết kế DB: `Restrict`
   cho `categories.parent_id`/`posts.category_id`/`posts.thumbnail_media_id`/`banners.media_id`/
   `projects.media_id`; `Cascade` cho `post_tags` (cả 2 chiều); `SetNull` cho
   `post_reviews.session_id`.
3. `categories.level` KHÔNG set tay trong C# — trigger DB tự tính (`ValueGeneratedOnAddOrUpdate()`
   đánh dấu để EF biết đọc lại giá trị sau khi Insert, không phải EF tự sinh ra nó).
4. `posts.category_id` phải là cấp 3 — **không thể** validate bằng Fluent API/CHECK constraint
   thuần (Postgres CHECK không nhìn được bảng khác). Trigger DB đã xử lý, nhưng **Application
   layer nên tự kiểm tra trước khi gọi `SaveChangesAsync`** để trả lỗi thân thiện thay vì để
   exception thô từ trigger vọt lên tận API — **CẦN LÀM ở bước viết endpoint tạo/sửa bài viết**,
   chưa làm ở đợt nền tảng này.
5. `smtp_settings.password_encrypted` dùng mã hoá 2 chiều — **chưa** viết service mã hoá/giải
   mã cụ thể (cần `IDataProtector` của ASP.NET Core hoặc thư viện riêng) — để ở bước làm nhóm
   Quản trị sau này.

## Việc CHƯA làm (rõ ràng để không nhầm là đã xong)

- [ ] Controller cho 20 bảng theo từng nhóm (Content/Interaction/Admin/System) — mới có `AuthController`
- [ ] Validate `posts.category_id` ở Application trước khi lưu (điểm 4 ở trên)
- [ ] Service mã hoá/giải mã `smtp_settings.password_encrypted`
- [ ] Cron jobs (xoá OTP/session hết hạn, tổng hợp `visitor_stats_monthly`, reset
      `emails_sent_today`) — có thể dùng `IHostedService`/Hangfire, chưa chọn công cụ
- [ ] Middleware xử lý exception tập trung (chuẩn hoá lỗi trả về JSON, ẩn chi tiết exception
      thật ở production)
- [ ] Seed dữ liệu ban đầu (1 dòng `admin_users` để đăng nhập lần đầu — hiện DB trống,
      `POST /api/auth/login` sẽ luôn trả 401 cho tới khi có dòng admin đầu tiên, phải tự
      insert tay hoặc viết 1 script seed riêng)
