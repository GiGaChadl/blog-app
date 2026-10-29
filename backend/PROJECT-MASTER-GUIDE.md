# BLOG CÁ NHÂN — TÀI LIỆU TỔNG (Project Master Guide)

> **Đọc file này ĐẦU TIÊN, trước bất kỳ file nào khác trong dự án.** Đây là nguồn sự thật
> duy nhất (single source of truth) tổng hợp toàn bộ quyết định đã chốt qua suốt quá trình
> làm việc — cả phần **frontend** (Angular, đã xong 1 phiên bản public site) lẫn **backend**
> (.NET, đang làm dở). Nếu bạn là AI được giao tiếp tục dự án này: đọc hết file này trước,
> đừng tự suy đoán lại các quyết định nghiệp vụ đã ghi rõ ở đây.
>
> Các file liên quan khác (không lặp lại nội dung ở đây, chỉ tham chiếu):
> - `TOM-TAT-NHANH.md`, `HUONG-DAN-CHI-TIET.md`, `CHANGELOG.md` — chi tiết frontend Angular
> - `blog-database-schema.dbml` — sơ đồ ERD đầy đủ (dán vào dbdiagram.io để xem trực quan)
> - `blog-database-schema.sql` — SQL DDL đã test thật trên PostgreSQL 16
> - `backend-roadmap.md` — checklist tiến độ backend từng file, công thức tái sử dụng

---

## 1. Project Overview

Blog cá nhân bằng tiếng Việt, dùng để chia sẻ bài học/kinh nghiệm/trải nghiệm cá nhân. Gồm
2 phần: **Public site** (người đọc, không cần đăng nhập) và **Admin panel** (1 tài khoản
quản trị duy nhất, quản lý toàn bộ nội dung/cấu hình site). Public site (Angular) đã có 1
phiên bản chạy được với dữ liệu giả (mock). Database đã thiết kế và test đầy đủ. Backend
(.NET) đang xây dở phần API.

## 2. Project Goals

- Xây được 1 blog hoàn chỉnh, tự vận hành được (admin tự quản lý toàn bộ nội dung không cần
  biết code).
- Tối ưu SEO (slug thân thiện, sitemap tự sinh, Open Graph khi chia sẻ mạng xã hội).
- Tôn trọng quyền riêng tư người đọc (chỉ ghi log truy cập khi được đồng ý cookie).
- Chi phí vận hành thấp — hướng tới hosting free tier (Oracle Cloud Free Tier từng được nhắc
  tới làm mục tiêu, chưa triển khai thật).
- Codebase dễ đọc, dễ bảo trì bởi 1 người không chuyên sâu — ưu tiên sự rõ ràng hơn tối ưu
  kỹ thuật phức tạp.

## 3. Functional Requirements

**Public site:**
- Trang chủ: banner (slogan + tối đa 3 ảnh, không link), khối bài viết nổi bật (thuật toán:
  ưu tiên view → rating → fallback bài mới nhất), quảng cáo AdSense, form đăng ký nhận bài.
- Danh mục 3 cấp (Cha → Con → Cháu), sidebar dạng cây thu/mở, giới hạn hiện 5 mục cấp 3 gần
  nhất kèm "Xem tiếp…".
- Danh sách bài viết: lọc kết hợp danh mục (gồm cả nhánh con) + tag + tìm kiếm + phân trang.
- Chi tiết bài viết: nội dung, tag, chia sẻ MXH (Open Graph), bài liên quan (3 tầng: cùng
  danh mục cấp 3 → anh em cấp 2 → fallback mới nhất), đánh giá (sao + bình luận).
- Đánh giá bài viết: cần xác thực OTP qua email (+ reCAPTCHA) lần đầu, tạo session 24h để
  sửa/xoá không cần OTP lại. Giới hạn 5 lần gửi OTP/email/24h.
- Đăng ký nhận bài: double opt-in (đăng ký → email xác nhận → active), link huỷ trong mỗi mail.
- Cookie consent: banner nhỏ, "Đồng ý" mới bắt đầu ghi log truy cập, "Từ chối" không tắt quảng cáo.
- Trang About/Contact tĩnh nhưng admin sửa được nội dung.
- Trang Chính sách dữ liệu, trang Huỷ đăng ký, trang 404.
- Dark/light theme toggle.

**Admin (CHƯA XÂY UI, chỉ mới có API nền tảng):**
- Quản lý bài viết, danh mục (cây kéo-thả), tag, ảnh (media library), banner/slider/slogan,
  quảng cáo (AdSense slot), dự án (trang About), thông tin liên hệ.
- Quản lý đánh giá (duyệt/ẩn/đánh cờ).
- Xem thống kê truy cập (bảng/biểu đồ), theo ngày (30 ngày gần nhất) hoặc theo tháng (xa hơn).
- Cấu hình SMTP, cấu hình SEO (meta mặc định, robots.txt).
- Chỉ 1 tài khoản admin, có chức năng quên mật khẩu qua OTP email.

## 4. Non-Functional Requirements

- **Bảo mật**: JWT cho admin, BCrypt hash mật khẩu (1 chiều), mã hoá 2 chiều riêng cho mật
  khẩu SMTP (khác hash — cần lấy lại được để tự đăng nhập SMTP).
- **Riêng tư**: không ghi log IP/truy cập nếu chưa được đồng ý cookie; email đánh giá không
  hiển thị công khai.
- **Hiệu năng**: quy mô nhỏ (blog cá nhân) — không cần tối ưu cho traffic lớn, nhưng vẫn có
  index cho các cột khoá ngoại/thường lọc, giới hạn giữ log 30 ngày để bảng không phình vô hạn.
- **SEO**: bắt buộc dùng SSR cho Angular trước khi lên production (**CHƯA LÀM** — hiện là CSR).
- **Khả năng bảo trì**: Clean Architecture ở backend, tách rõ tầng nghiệp vụ khỏi tầng hạ tầng.

## 5. Technology Stack

| Thành phần | Công nghệ |
|---|---|
| Frontend | Angular 18 (standalone components) + PrimeNG 18, SCSS |
| Backend | .NET 10, ASP.NET Core Web API (controller-based, không phải Minimal API) |
| ORM | Entity Framework Core 10 + Npgsql.EntityFrameworkCore.PostgreSQL |
| Database | PostgreSQL 16 |
| Auth | JWT Bearer (System.IdentityModel.Tokens.Jwt) + BCrypt.Net-Next |
| Hosting dự kiến | Oracle Cloud Free Tier (chưa triển khai) |

## 6. System Architecture

```
[Người đọc / Admin]
       │  HTTPS
       ▼
[Angular Public Site] ──(chưa nối)──► [.NET Web API] ──► [PostgreSQL]
  (đã có, mock data)                  (đang xây dở)      (đã test xong)
       │
       ▼
[Angular Admin UI] — CHƯA XÂY, kể cả 1 dòng code
```

Backend theo **Clean Architecture** 4 tầng (chi tiết mục 7):
```
Api → Infrastructure → Application → Domain
```
Mũi tên = "phụ thuộc vào" (tầng bên trái biết tầng bên phải, không có chiều ngược lại).

## 7. Project Structure

**Frontend** (`blog-app/src/app/`):
```
core/            # layout dùng chung (header, footer, theme, cookie-consent)
shared/          # component/model/mock-data dùng lại nhiều nơi
pages/           # 1 thư mục = 1 route
```

**Backend** (`BlogApp/src/`):
```
BlogApp.Domain/          # Entity (class) + Enum thuần, KHÔNG phụ thuộc package ngoài
  Entities/              # 20 file, mỗi file 1 class khớp 1 bảng SQL
  Enums/Enums.cs         # 8 enum, PascalCase, không gắn attribute Npgsql
  ValueObjects/          # SocialLink (không phải Entity, không có Id)

BlogApp.Application/     # Business logic — chỉ phụ thuộc Domain + interface
  Common/Interfaces/     # IApplicationDbContext, IJwtTokenService, IPasswordHasher
  Common/Exceptions/     # ValidationException
  Features/<TenNhom>/    # 1 thư mục/nhóm nghiệp vụ: <Ten>Dtos.cs (record) + <Ten>Service.cs (class)
  DependencyInjection.cs # AddApplication() — đăng ký mọi Service

BlogApp.Infrastructure/  # Cài đặt thật — nơi DUY NHẤT được biết Postgres/JWT/BCrypt
  Persistence/ApplicationDbContext.cs
  Persistence/Configurations/  # 20 file IEntityTypeConfiguration<T>, khớp đúng SQL đã test
  Persistence/Seed/AdminSeeder.cs
  Identity/               # JwtTokenService, PasswordHasher, JwtSettings
  DependencyInjection.cs  # AddInfrastructure() — đăng ký DbContext + service thật

BlogApp.Api/              # Mỏng nhất — chỉ nhận HTTP, gọi thẳng Service
  Controllers/
  Middleware/ExceptionHandlingMiddleware.cs
  Program.cs, appsettings.json
```

## 8. Coding Methodology

- **Clean Architecture bắt buộc** — 4 project `.csproj` riêng (không phải 4 folder chung 1
  project), để compiler tự chặn vi phạm hướng phụ thuộc, không dựa vào ý thức tự giác.
- **Constructor Injection** cho mọi phụ thuộc — không bao giờ `new XxxService()` tay, luôn
  khai báo qua constructor, để ASP.NET Core DI tự bơm vào.
- **2 tầng bảo vệ cho mọi validate quan trọng**: Application tự kiểm tra trước (báo lỗi thân
  thiện qua `ValidationException`), DB constraint/trigger là lưới an toàn cuối cùng.
- **DB-first, không phải Code-first**: `blog-database-schema.sql` là nguồn sự thật của schema
  (đã test thật). EF Core chỉ *map vào* schema có sẵn, KHÔNG dùng Migrations để tự sinh/sửa
  schema ở giai đoạn này.
- **Đi từng bước nhỏ khi làm cùng người dùng** — không dồn nhiều file/nhiều giải thích 1 lúc;
  giải thích xong 1 khái niệm, dừng lại xác nhận hiểu trước khi qua tiếp.

## 9. Coding Conventions

- **DTO dùng `record`** (bất biến, so sánh theo giá trị) — **Entity dùng `class`** (EF Core
  cần track thay đổi để Update).
- Không viết `IXxxService` cho Service ở tầng Application nếu implementation của nó cũng nằm
  ngay trong Application (không băng qua ranh giới tầng) — chỉ viết interface khi cài đặt thật
  nằm ở tầng khác (`IApplicationDbContext`, `IJwtTokenService`, `IPasswordHasher`).
- Comment giải thích **lý do nghiệp vụ** viết bằng tiếng Việt, ngay tại chỗ liên quan (không
  gom hết vào 1 file riêng) — để không ai (kể cả AI đọc lại) quên mất "tại sao lại làm vậy".
- Tên route tiếng Việt không dấu, kebab-case ở frontend (`/danh-muc`, `/bai-viet`); tên
  API endpoint tiếng Anh chuẩn REST (`/api/categories`, `/api/posts`).
- Mỗi tính năng backend theo đúng công thức 4 bước (xem `backend-roadmap.md` mục "CÔNG THỨC").

## 10. Database Architecture

**20 bảng, 4 nhóm** (chi tiết đầy đủ + lý do từng quyết định: xem `blog-database-schema.dbml`):

| Nhóm | Bảng |
|---|---|
| Nội dung | `media_library`, `categories`, `tags`, `posts`, `post_tags` |
| Tương tác | `review_sessions`, `post_reviews`, `otp_verifications`, `subscribers` |
| Quản trị | `banners`, `slogans`, `ads`, `projects`, `admin_users`, `contact_info` |
| Hệ thống | `visitor_logs`, `visitor_stats_monthly`, `cookie_consents`, `smtp_settings`, `seo_settings` |

**Quy tắc nghiệp vụ quan trọng nhất (không được đoán sai lại):**
- `categories.level` (1/2/3) **tự tính bằng trigger DB**, không set tay. Tối đa 3 cấp — tạo
  cấp 4 bị trigger chặn.
- `posts.category_id` **bắt buộc trỏ vào danh mục cấp 3** — chặn 2 lớp: Application pre-check
  (thân thiện) + DB trigger (lưới an toàn cuối).
- `posts.status` chỉ có `published`/`hidden`, **không có `draft`** — lưu là hiện ngay.
- `posts.published_at` (không phải `created_at`) — **NOT NULL**, đặt tên vậy để khớp thẳng
  field `publishedAt` frontend đã dùng, khỏi map lại tên khi nối API.
- FK delete behavior **khác nhau theo bản chất quan hệ**: `RESTRICT` cho quan hệ bảo vệ nội
  dung thật (`categories.parent_id`, `posts.category_id`, `posts.thumbnail/og_image_media_id`,
  `banners/projects.media_id`); `CASCADE` cho bảng liên kết thuần (`post_tags`, cả 2 chiều);
  `SET NULL` cho quan hệ tới dữ liệu tạm bị cron dọn (`post_reviews.session_id`).
- `otp_verifications.ip_address` **chỉ bắt buộc khi `purpose = review_verification`** (CHECK
  constraint 2 chiều) — không bắt buộc khi `admin_password_reset`.
- `slogans` chỉ cho phép **tối đa 1 dòng `is_active = true`** — ép bằng partial unique index.
- `ads` dùng **Google AdSense thật** (`adsense_slot_id`) — KHÔNG tự upload ảnh quảng cáo (đã
  sửa lại giữa chừng sau khi phát hiện thiết kế ban đầu sai hướng).
- `cookie_consents` **chỉ ghi dòng khi Đồng ý** (hoặc khi sau này chủ động thu hồi) — bấm Từ
  chối không tạo dòng nào.
- `visitor_logs` giữ **30 ngày**, cron xoá cũ hơn — PHẢI chạy SAU job tổng hợp
  `visitor_stats_monthly` của tháng đó (thứ tự bắt buộc, làm ngược sẽ mất dữ liệu).
- `smtp_settings.password_encrypted` dùng **mã hoá 2 chiều** (khác hash) — khoá mã hoá để ở
  biến môi trường, không lưu trong DB.

**Đã test thật** (không chỉ đọc code): cài PostgreSQL 16 thật, chạy 10 tình huống thật (tạo
cây danh mục, chặn cấp 4, chặn gắn bài viết sai cấp, CHECK rating, CHECK ip_address theo
purpose, partial unique index slogans, RESTRICT chặn xoá cha có con, CASCADE dọn post_tags mà
không đụng tags) — toàn bộ **10/10 pass**.

## 11. API Architecture

- REST, route `api/[controller]`, controller-based (không Minimal API).
- GET công khai (đọc dữ liệu người đọc thấy) → `[AllowAnonymous]`. POST/PUT/DELETE (thay đổi
  dữ liệu, thao tác admin) → `[Authorize]`.
- Danh sách có phân trang trả về bọc trong `PagedResult<T>` (`Items`, `Total`, `Page`, `PageSize`).
- Response lỗi chuẩn hoá qua `ExceptionHandlingMiddleware`: `ValidationException` → 400 kèm
  `message` thân thiện; lỗi khác → 500, ẩn chi tiết thật (chỉ log server-side).
- **Lưu ý kỹ thuật EF Core quan trọng**: KHÔNG BAO GIỜ gọi hàm map DTO tuỳ ý ngay trong
  `.Select()` của 1 IQueryable — phải `.ToListAsync()` lấy dữ liệu thật ra trước, rồi mới
  `.Select()` map sang DTO ở bước sau (LINQ-to-Objects). Vi phạm sẽ lỗi runtime "could not be
  translated", không phải lỗi biên dịch.

## 12. Authentication & Authorization

- Chỉ 1 tài khoản admin (`admin_users`, đúng 1 dòng theo nghiệp vụ, không ép bằng DB).
- Đăng nhập: `POST /api/auth/login` → JWT Bearer (HMAC-SHA256, secret ≥ 32 ký tự từ config).
- Mật khẩu admin: BCrypt hash (1 chiều) — `IPasswordHasher`/`PasswordHasher`.
- Quên mật khẩu admin: dùng lại `otp_verifications` với `purpose = admin_password_reset`
  (**CHƯA XÂY endpoint**, chỉ mới có trong thiết kế DB).
- Đánh giá bài viết công khai: xác thực qua OTP (hash lưu DB) + session 24h, KHÔNG phải JWT
  (đây là cơ chế riêng, không liên quan hệ thống Auth admin).
- `AdminSeeder` tự tạo admin đầu tiên lúc khởi động nếu bảng trống, lấy thông tin từ
  `appsettings.json` mục `SeedAdmin` — chỉ chạy 1 lần, tự kiểm tra trước khi tạo.

## 13. Testing Strategy

- **Domain**: build thật thành công, 0 lỗi (project duy nhất không cần NuGet nên build được
  trong môi trường soạn code bị chặn mạng).
- **Application/Infrastructure/Api**: viết dựa trên kiến thức chắc chắn, **CHƯA build/test
  được bằng compiler thật** trong môi trường soạn thảo (NuGet bị chặn) — người dùng tự
  `dotnet restore` + `dotnet build` trên máy có internet để xác nhận.
- **Database**: đã test thật 10 tình huống trên PostgreSQL 16 thật (xem mục 10).
- **Chưa có test tự động nào** (unit test/integration test) — nên bổ sung sau, ưu tiên test
  logic validate quan trọng (`CategoryService.CreateAsync` chặn cấp 4,
  `PostService.CreateAsync` chặn category không phải cấp 3).
- 1 lỗi thật đã xảy ra và tự sửa: `ApplicationDbContext` quên khai báo implement
  `IApplicationDbContext` (thiếu `: DbContext, IApplicationDbContext`) dù đã có đủ property
  khớp tên — lỗi build CS0266, sửa bằng cách thêm đúng phần khai báo interface.

## 14. Error Handling

- `Application.Common.Exceptions.ValidationException` — ném khi Application tự phát hiện dữ
  liệu sai TRƯỚC khi chạm DB.
- `Api.Middleware.ExceptionHandlingMiddleware` — bắt lỗi tập trung, đặt **đầu tiên** trong
  pipeline (`Program.cs`). `ValidationException` → 400 + message thật. Exception khác (kể cả
  từ trigger DB lọt qua) → 500 + message chung chung, log đầy đủ chi tiết ra server log.

## 15. Security Requirements

- JWT secret, connection string, SMTP encryption key: **luôn ở biến môi trường/`appsettings`
  không commit**, không hard-code trong code.
- Mật khẩu admin: BCrypt (1 chiều). Mật khẩu SMTP: mã hoá 2 chiều (khoá riêng, không lưu DB).
- Giới hạn 5 lần gửi OTP/email/24h (đếm ở tầng ứng dụng qua query, không có cột đếm riêng) +
  giới hạn theo IP (chỉ khi `purpose = review_verification`).
- reCAPTCHA khi gửi OTP (đã thiết kế UI frontend, backend endpoint xác thực reCAPTCHA
  **CHƯA XÂY**).
- Không ghi log IP/truy cập nếu chưa có cookie consent — trừ `otp_verifications.ip_address`
  (mục đích chống lạm dụng, không phải theo dõi truy cập, nên không cần cùng cơ chế đồng ý).

## 16. Deployment

- Hosting dự kiến: Oracle Cloud Free Tier (ARM Ampere khuyến nghị vì RAM/CPU tốt hơn x86 free
  tier) — **CHƯA triển khai thật**, mới là mục tiêu đã nhắc từ đầu.
- Frontend cần bật **Angular Universal (SSR)** trước khi lên production — hiện tại là CSR
  thuần, chưa làm.
- CDN/cache cho frontend — đã nhắc là mục tiêu, chưa cấu hình.
- Chưa có Dockerfile/CI-CD nào được thiết lập.

## 17. Git / Branching Strategy

**CHƯA ĐƯỢC BÀN TỚI** trong suốt quá trình làm việc — chưa có quy ước branch, commit message,
hay CI/CD nào được thiết lập hoặc thảo luận. Cần quyết định khi bắt đầu làm việc nhóm hoặc
triển khai thật.

## 18. Current Implementation Status

- ✅ **Frontend public site**: Angular 18 + PrimeNG, dùng mock data, chưa nối API thật. Có
  đủ: trang chủ, danh sách, chi tiết bài viết, About, chính sách dữ liệu, huỷ đăng ký, 404,
  dark/light theme, sidebar danh mục 3 cấp.
- ✅ **Database**: schema đầy đủ 20 bảng, đã test thật trên PostgreSQL 16.
- 🟡 **Backend — Nền tảng**: Domain (20 entity + 8 enum), Application (3 interface + Auth),
  Infrastructure (DbContext + 20 config + JWT/BCrypt + Seeder), Api (Program.cs + Middleware +
  AuthController) — **đã test chạy được thật** (đăng nhập trả JWT thành công, xác nhận qua
  log thật từ Visual Studio của người dùng).
- 🟡 **Backend — Nhóm 1 (Nội dung)**: Categories (Dtos + Service đầy đủ CRUD, có validate cấp
  3 + partial unique) — đã viết xong, đang hướng dẫn người dùng tự gõ tay để học. Tags, Posts
  — đã có bản DTO/Service viết sẵn (do AI viết, người dùng CHƯA tự gõ/CHƯA test), Controller +
  đăng ký DI cho cả 2 **CHƯA LÀM XONG**.
- ❌ **Backend — Nhóm 2/3/4**: chưa bắt đầu, chỉ có trong roadmap.
- ❌ **Backend — MediaLibrary**: chưa có Controller/upload endpoint.
- ❌ **Admin UI (Angular)**: chưa có 1 dòng code nào.
- ❌ **Nối frontend thật với backend thật**: chưa làm — frontend vẫn 100% mock data.

## 19. Known Issues

- **Rủi ro kỹ thuật cao nhất chưa xác nhận được**: đoạn map 8 enum C# sang native PostgreSQL
  ENUM TYPE (`NpgsqlDataSourceBuilder.MapEnum<T>()` trong `Infrastructure/DependencyInjection.cs`)
  viết dựa trên hiểu biết chắc chắn nhưng **chưa build/test được** trong môi trường soạn code
  (NuGet bị chặn) — cần kiểm tra đầu tiên nếu gặp lỗi build ở máy thật.
- Chưa có test tự động.
- `CategoryService.UpdateAsync` **không hỗ trợ đổi `ParentId`** (di chuyển danh mục sang nhánh
  khác) — cần tính lại `Level` đệ quy cho cả cây con, để dành làm sau.
- Chưa có endpoint đổi mật khẩu/quên mật khẩu admin (dù đã thiết kế sẵn trong DB).
- Chưa có service mã hoá/giải mã cho `smtp_settings.password_encrypted`.
- Chưa có bất kỳ cron job nào (dọn OTP/session hết hạn, tổng hợp thống kê tháng, reset đếm
  mail hàng ngày, tự sinh sitemap.xml).
- MediaLibrary chưa tích hợp Oracle Cloud Storage thật.

## 20. Future Roadmap

Theo đúng thứ tự nên làm (xem chi tiết từng file trong `backend-roadmap.md`):
1. Hoàn thiện Nhóm 1: xong Posts Service/Controller, MediaLibrary (cần bàn cách upload trước).
2. Nhóm 2 — Tương tác: OtpVerifications + ReviewSessions trước, rồi PostReviews, rồi Subscribers.
3. Nhóm 3 — Quản trị: Banners/Slogans/Ads/Projects/ContactInfo, đổi mật khẩu admin.
4. Nhóm 4 — Hệ thống: VisitorLogs/Stats, CookieConsents, SmtpSettings (+ service mã hoá),
   SeoSettings, toàn bộ cron job.
5. Nối frontend Angular với API thật (thay mock data).
6. Xây Admin UI (Angular) — chưa có kế hoạch chi tiết, cần bàn riêng.
7. Bật SSR (Angular Universal) cho frontend.
8. Viết test tự động.
9. Triển khai thật lên Oracle Cloud.

## 21. Architectural Decisions

Ghi ngắn gọn quyết định + lý do — không lặp lại chi tiết đã có ở mục 8-10:

1. **4 project `.csproj` riêng thay vì 4 folder chung** — để compiler tự chặn vi phạm hướng
   phụ thuộc (build lỗi ngay nếu `Domain` lỡ `using Infrastructure`), không dựa vào tự giác.
2. **Không viết `IXxxService` cho Service ở Application** trừ khi thật sự băng qua ranh giới
   tầng — giảm số file/tầng gián tiếp không cần thiết ở quy mô 1 người viết code.
3. **DTO = `record`, Entity = `class`** — record cho tính bất biến + so sánh theo giá trị
   miễn phí; class cho khả năng EF Core track thay đổi.
4. **`categories.level` tính bằng trigger DB, không phải code C#** — tránh 2 nơi (C# và SQL)
   có logic tính cấp lệch nhau; đảm bảo đúng dù ai/hệ thống nào ghi thẳng vào DB.
5. **2 tầng bảo vệ cho category cấp 3** — Application pre-check (UX tốt) + DB trigger (an
   toàn tuyệt đối) — không chọn 1 trong 2, chọn cả hai vì mỗi tầng bù đắp điểm yếu của tầng kia.
6. **`post_status` chỉ 2 giá trị, bỏ `draft`** — bám sát đúng yêu cầu gốc, không tự thêm tính
   năng chưa ai yêu cầu.
7. **`published_at` thay vì `created_at`** — khớp tên field frontend đã dùng trước, tránh phải
   map lại DTO khi nối API thật sau này.
8. **FK delete behavior chọn riêng theo từng quan hệ**, không áp 1 kiểu cho tất cả — phân biệt
   rõ "nội dung cần bảo vệ" (RESTRICT) và "dữ liệu liên kết/tạm thời" (CASCADE/SET NULL).
9. **`ads` đổi từ tự upload ảnh sang AdSense thật** — sửa lại giữa chừng sau khi đối chiếu lại
   đúng yêu cầu gốc ("để Google tự hiển thị quảng cáo"), không phải lỗi đánh máy.
10. **`cookie_consents` chỉ ghi lúc Đồng ý** — nguyên tắc tối thiểu hoá dữ liệu: không thu gì
    thì không cần lưu bằng chứng "đã từ chối".
11. **Schema SQL viết tay, test thật, KHÔNG dùng EF Core Migrations để sinh schema** — vì
    schema đã qua rất nhiều vòng phân tích + test thật, tin tưởng nó hơn để EF Core tự sinh
    lại từ đầu (có thể lệch enum/index/constraint đã tinh chỉnh thủ công).
12. **Npgsql enum map dùng `NpgsqlSnakeCaseNameTranslator` tường minh** — không tin vào hành
    vi mặc định ngầm định, vì không compile-test được để xác nhận.
