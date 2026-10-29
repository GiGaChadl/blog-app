# HƯỚNG DẪN CHI TIẾT — Nhật Ký Của Tôi (Public Site)

> Đọc `TOM-TAT-NHANH.md` trước nếu bạn mới mở dự án lần đầu. File này đi sâu vào kỹ thuật.

---

## 1. Cài đặt & chạy

```bash
cd blog-app
npm install
npm start              # ng serve, http://localhost:4200
npm run build          # build production vào dist/blog-app
```

Yêu cầu: Node.js 18+ (đã test với Node 22), Angular CLI 18.

---

## 2. Hệ thống thiết kế ("Sổ tay cá nhân")

### 2.1. Token màu — `src/styles.scss`

Toàn bộ màu sắc định nghĩa dưới dạng CSS variable trong `:root`. **Không hard-code mã hex trong
component `.scss`** — luôn dùng `var(--color-...)` để khi đổi theme (sáng/tối) mọi nơi tự động cập nhật.

| Biến | Dùng cho | Sáng | Tối |
|---|---|---|---|
| `--color-paper` | nền trang | `#F5F1E8` | `#1B1F24` |
| `--color-paper-raised` | thẻ/card nổi trên nền | `#FFFFFF` | `#242A30` |
| `--color-ink` | chữ chính | `#22282E` | `#F1EDE3` |
| `--color-ink-soft` | chữ phụ | `#4A5158` | `#C7C2B4` |
| `--color-amber` | accent hành động (link, nút chính) | `#B9702E` | `#E0A165` |
| `--color-sage` | accent phụ (badge danh mục) | `#56685A` | `#8FBF9A` |
| `--color-margin` | chi tiết "lề đỏ sổ tay" — dùng RẤT tiết chế | `#9B3B3B` | `#D97A7A` |
| `--color-inverse-*` | **riêng cho footer & cookie-toast** — luôn nền tối/chữ sáng bất kể site đang theme nào | — | — |

**Vì sao có `--color-inverse-*` riêng?** Footer và cookie-toast được thiết kế cố ý là "khối tối"
để tạo tương phản, kể cả khi site đang ở chế độ sáng. Nếu chúng dùng thẳng `--color-ink`, khi bật
dark mode `--color-ink` sẽ đảo thành màu sáng và làm hỏng footer. Do đó 2 khối này **luôn** dùng
`--color-inverse-bg` / `--color-inverse-text` — bộ biến này tự đổi sắc độ nhẹ giữa 2 theme nhưng
luôn giữ tinh thần "nền tối, chữ sáng".

### 2.2. Dark mode — cách hoạt động

- `core/services/theme.service.ts`: signal `mode: 'light' | 'dark'`, lưu vào `localStorage` key
  `blog_theme_mode`, gắn/gỡ class `theme-dark` lên thẻ `<html>`.
- `src/styles.scss`: khối `html.theme-dark { ... }` override toàn bộ token màu.
- `core/theme/blog-preset.ts`: có `colorScheme.dark` riêng để các component PrimeNG (Paginator,
  MultiSelect, Checkbox...) cũng đổi màu đồng bộ.
- `app.config.ts`: `darkModeSelector: '.theme-dark'` — khớp đúng class trên.
- Nút bật/tắt: `core/layout/theme-toggle/` — đặt trong header, icon mặt trời/mặt trăng.

**Muốn thêm màu mới ăn theo theme?** Định nghĩa biến trong cả 2 khối `:root` và `html.theme-dark`
trong `styles.scss`, rồi dùng `var(--ten-bien)` ở component — không cần đụng vào TypeScript.

### 2.3. Typography

- **Fraunces** (serif, có `font-style: italic` dùng cho slogan) — tiêu đề, khẩu hiệu
- **Inter** — nội dung chính
- **IBM Plex Mono** — ngày tháng, nhãn danh mục, reading time, mọi thứ mang tính "con dấu/nhãn"

Cả 3 font load qua Google Fonts trong `src/index.html`. Nếu deploy nội bộ không có internet ra
ngoài, cần tự host font (tải file `.woff2` về `src/assets/fonts/` và đổi `@font-face` trong
`styles.scss` thay vì link Google Fonts).

### 2.4. Hệ thống Button — `.btn` trong `styles.scss`

```html
<button class="btn btn--primary">Nút chính</button>
<button class="btn btn--secondary">Nút phụ</button>
<button class="btn btn--ghost">Nút trong suốt</button>
<button class="btn btn--danger">Nút xoá/nguy hiểm</button>
<button class="btn btn--primary btn--sm">Nút nhỏ</button>
```

Không tự viết CSS nút riêng trong component nữa — nếu cần biến thể mới, thêm class `.btn--xxx` vào
`styles.scss` theo đúng pattern (`--_bg`, `--_fg`, `--_border`, `--_bg-hover`) để thừa hưởng animation/
focus-state có sẵn.

`.icon-btn` (cũng trong `styles.scss`) dùng cho nút chỉ có icon (kính lúp, hamburger, theme toggle).

---

## 3. Quản lý danh mục 3 cấp

Model: `shared/models/post.model.ts`

```ts
interface Category {
  id: string;
  name: string;
  slug: string;
  parentId: string | null;   // null = danh mục cấp 1 (cha)
}
```

Dữ liệu phẳng nằm trong `shared/mock-data/posts.mock.ts` (mảng `MOCK_CATEGORIES`). Các hàm tiện ích:

- `buildCategoryTree()` — dựng cây 3 cấp từ mảng phẳng, dùng cho mega-menu header & sidebar lọc.
- `findCategoryBySlug(slug)` — tìm 1 danh mục theo slug.
- `collectDescendantIds(categoryId)` — trả về id của chính nó + toàn bộ con/cháu, dùng để lọc bài
  viết khi người dùng bấm vào danh mục **cha** (phải gồm cả bài trong danh mục con/cháu bên dưới).

**Khi nối API .NET thật**: các hàm trên có thể giữ nguyên chữ ký, chỉ đổi phần thân thành gọi
`HttpClient` tới endpoint `/api/categories/tree` (backend trả sẵn dạng cây) thay vì tính lại từ mảng
phẳng ở client — tránh phải gửi toàn bộ danh mục về rồi build cây bằng JS mỗi lần.

**Mỗi bài viết chỉ thuộc 1 danh mục cấp 3** — đây là quyết định đã chốt, không phải nhiều danh mục
(many-to-many). Trường `PostSummary.category` trỏ thẳng vào 1 `Category` cấp 3 (cháu).

### 3.1. Component `category-sidebar` — sidebar cây danh mục dùng chung

**Từ 25/8/2026**, toàn bộ giao diện cây danh mục 3 cấp được gom vào 1 component dùng chung:
`shared/components/category-sidebar/`. Dùng ở **2 nơi**:

- `pages/post-list/` — sidebar lọc trang danh sách bài viết
- `pages/post-detail/` — sidebar điều hướng ở trang chi tiết bài viết (thêm mới cùng đợt)

**Không sửa cây danh mục riêng ở từng trang nữa** — sửa 1 lần trong `category-sidebar.component.*`
sẽ tự động áp dụng ở cả 2 nơi.

**Hành vi thu/mở từng cấp:**
- Cấp 1 (cha): mặc định **mở**, bấm mũi tên để thu gọn. Quản lý bởi signal `expandedParents`.
- Cấp 2 (con): mặc định **đóng**, bấm mũi tên để mở ra cấp 3. Quản lý bởi signal `expandedChildren`
  (độc lập với `expandedParents`, đây là điểm khác so với bản trước đó chỉ có cấp 1 thu/mở được).
- Cấp 3 (cháu): khi cấp 2 tương ứng mở ra, chỉ hiện tối đa **5 mục đầu tiên** (hằng số
  `MAX_GRANDCHILDREN_SHOWN` trong `category-sidebar.component.ts`). Nếu nhiều hơn 5, hiện thêm dòng
  **"Xem tiếp…"** dẫn tới trang danh sách của chính danh mục cấp 2 đó — nơi hiển thị đầy đủ.

**Về từ "5 mục gần nhất":** hiện tại `Category` chưa có trường ngày tạo, nên "gần nhất" tạm hiểu là
5 phần tử đầu tiên theo thứ tự khai báo trong `MOCK_CATEGORIES`. **Khi nối API thật**, nếu muốn đúng
nghĩa "gần nhất theo thời gian tạo", cần: (1) thêm `createdAt` vào bảng `Categories` ở backend, (2)
backend trả danh sách cấp 3 đã sort theo `createdAt DESC`, (3) `visibleGrandchildren()` trong
component chỉ cần giữ nguyên logic `slice(0, 5)` vì mảng đã được sort sẵn từ server.

**Trang chi tiết bài viết** dùng sidebar này với `activeCategorySlug` = slug danh mục cấp 3 của
chính bài đang xem (`p.category.slug`), để tô đậm đúng vị trí trong cây — khác với trang danh sách
dùng `categorySlug()` (slug lấy từ URL, có thể là cấp 1/2/3 tuỳ người dùng đang lọc).

---

## 4. Luồng đánh giá + OTP (nghiệp vụ quan trọng nhất)

Component: `shared/components/otp-review-form/`

State machine 3 bước (`step` signal): `'write' → 'otp-sent' → 'verified'`

1. **write**: người dùng chọn sao + viết nhận xét + nhập email + tick reCAPTCHA → bấm "Gửi mã xác thực"
2. **otp-sent**: nhập mã 6 số → bấm "Xác thực & gửi đánh giá"
3. **verified**: xác thực xong, session lưu trong `sessionStorage` (mock — thực tế phải là token
   JWT ngắn hạn từ server, **không lưu email thô ở client**). Lần đánh giá tiếp theo trong cùng
   phiên trình duyệt sẽ nhảy thẳng vào bước này, không cần OTP lại.

**Khi nối API thật**, thay các `setTimeout(...)` mock bằng gọi API thật:
- `POST /api/reviews/request-otp` — gửi email + recaptcha token
- `POST /api/reviews/verify-otp` — gửi mã 6 số, backend trả về session token (nên set làm
  `httpOnly cookie` thay vì lưu ở `sessionStorage` phía client để an toàn hơn)
- `POST /api/reviews` — gửi đánh giá (kèm session token trong header)
- `DELETE /api/reviews/:id` — xoá đánh giá (backend tự kiểm tra session token có khớp chủ sở hữu không)

Giới hạn "5 lần/email/24h" hiện chỉ đếm ở client (`otpAttemptsUsed` signal, reset khi F5 trang) —
**bắt buộc phải kiểm tra lại ở backend**, client-side chỉ để UX (báo trước cho người dùng), không
phải cơ chế bảo mật thật.

---

## 5. Kích thước ảnh chuẩn (đọc kỹ trước khi thay ảnh mock)

| Vị trí | Component | Tỉ lệ | Khuyến nghị (px) |
|---|---|---|---|
| Ảnh trong Hero banner (trang chủ) | `hero-banner` | dọc, gần 5:6 | tối thiểu **500 × 620** |
| Thumbnail grid bài viết | `post-card` | 4:3 | **800 × 600** |
| Ảnh chính trang chi tiết bài viết | `post-detail` | 16:9 | **1200 × 675** |
| Ảnh chèn trong nội dung bài viết | trong `contentHtml` | tự do | tối đa **1000px chiều ngang** |
| Ảnh dự án trang About | `about` | 1:1 | **600 × 600** |
| Ảnh Open Graph (chia sẻ MXH) | set qua `Meta.updateTag('og:image', ...)` trong `post-detail.component.ts` | 1.91:1 | **1200 × 630** |

**Vì sao phải đúng tỉ lệ?** Mọi khung ảnh trong code dùng `aspect-ratio` CSS cố định + `object-fit:
cover` — ảnh sai tỉ lệ vẫn hiển thị được (không vỡ layout) nhưng sẽ bị cắt xén phần rìa không như ý
đồ ban đầu. Khi dựng trang Admin sau này, mục "Quản lý hình ảnh" nên hiển thị lại đúng bảng này ngay
tại form upload để admin đối chiếu.

**Định dạng file**: khuyến nghị `.webp` cho ảnh nội dung (nhẹ hơn `.jpg` 25-35% cùng chất lượng),
`.png` chỉ dùng cho ảnh cần nền trong suốt (hiếm khi cần ở blog này).

---

## 6. Hướng dẫn thêm 1 trang mới (quy trình chuẩn)

Ví dụ: thêm trang "Lưu trữ theo tháng" tại `/luu-tru/:thang`.

1. Tạo thư mục `src/app/pages/archive/`
2. Tạo 3 file chuẩn Angular standalone:
   - `archive.component.ts` — logic, luôn khai báo `standalone: true` và `imports: [...]`
   - `archive.component.html` — template
   - `archive.component.scss` — style riêng trang này (style dùng chung đặt ở `styles.scss`)
3. Đăng ký route trong `src/app/app.routes.ts`:
   ```ts
   {
     path: 'luu-tru/:thang',
     loadComponent: () => import('./pages/archive/archive.component').then(m => m.ArchiveComponent),
     title: 'Lưu trữ — Nhật Ký Của Tôi'
   }
   ```
4. Nếu trang cần xuất hiện trong nav/footer, thêm link thủ công vào
   `core/layout/header/header.component.html` hoặc `footer.component.html`.

**Quy ước bắt buộc theo trong toàn dự án:**
- Mọi màu dùng `var(--color-...)`, không hard-code hex.
- Mọi khoảng cách dùng `var(--space-...)` (thang 8px: 4/8/12/16/24/32/48/64/96).
- Mọi nút dùng class `.btn` + biến thể, không viết CSS nút riêng.
- Route tiếng Việt không dấu, gạch ngang (`kebab-case`), khớp văn phong các route hiện có
  (`/danh-muc`, `/bai-viet`, `/gioi-thieu`...).

---

## 7. Cách nối API .NET thật (khi backend đã sẵn sàng)

Hiện tại mọi nơi đang import trực tiếp từ `shared/mock-data/posts.mock.ts`. Để nối API thật:

1. Tạo `core/services/post.service.ts` dùng `HttpClient`, các method cùng tên với hàm mock hiện có
   (`getFeaturedPosts()`, `getPostBySlug()`, ...) nhưng trả về `Observable<T>` thay vì mảng đồng bộ.
2. Trong `app.config.ts`, thêm `provideHttpClient()`.
3. Từng component (`home.component.ts`, `post-list.component.ts`, `post-detail.component.ts`...)
   đổi từ gán mảng trực tiếp sang `inject(PostService).getXxx().subscribe(...)` hoặc dùng
   `toSignal()` từ `@angular/core/rxjs-interop` để giữ style dùng signal như hiện tại.
4. Cấu hình base URL API qua `environment.ts` (`src/environments/`), chưa có file này — cần tạo
   mới theo chuẩn Angular (`environment.ts` / `environment.prod.ts`).
5. Danh mục: gọi thẳng `/api/categories/tree` nếu backend trả sẵn dạng cây (khuyến khích, đỡ phải
   build cây ở client).

---

## 8. SSR (bắt buộc trước khi lên production)

Dự án hiện tại là **CSR thuần** (không Angular Universal) — dựng nhanh để duyệt UI. Yêu cầu ban đầu
đã chốt **"SEO dùng SSR"**, nên trước khi deploy thật cần chạy:

```bash
ng add @angular/ssr
```

Sau khi thêm SSR, kiểm tra lại các chỗ đang dùng trực tiếp `window`/`document`/`localStorage`/
`sessionStorage` mà **chưa bọc kiểm tra `typeof window !== 'undefined'`** — SSR chạy trên Node,
không có các object này. Đã rà và bọc sẵn ở: `cookie-consent.component.ts`, `theme.service.ts`,
`otp-review-form.component.ts`, `header.component.ts` (`HostListener('window:scroll')` — cần kiểm
tra lại kỹ khi bật SSR vì decorator này chạy cả ở server, có thể cần thêm `isPlatformBrowser` guard).

---

## 9. Việc còn thiếu / giới hạn đã biết

- Không có trình duyệt trong môi trường dựng dự án này để chụp ảnh preview trực tiếp — mọi fix
  responsive (footer mobile, breakpoint...) dựa trên đọc CSS cẩn thận, không phải test bằng mắt
  trên thiết bị thật. Nên tự kiểm tra lại trên Chrome DevTools (chế độ responsive) trước khi lên
  production.
- Trang Admin: chưa làm.
- sitemap.xml / robots.txt: chưa sinh file thật, mới dừng ở việc đã lên kế hoạch.
- Trang xác nhận double opt-in (bấm link trong email newsletter): chưa có, mới có bước 1 (gửi email
  giả lập ở form footer).
- Trường `createdAt` cho `Category`: chưa có trong model — cần thêm khi muốn "5 danh mục cấp 3 gần
  nhất" trong sidebar đúng nghĩa sắp xếp theo thời gian thay vì theo thứ tự khai báo (xem mục 3.1).
