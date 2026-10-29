# CHANGELOG — Lịch sử thay đổi giao diện

> File này ghi theo **từng đợt sửa**, mới nhất ở trên cùng. Nếu bạn đã tải zip cũ về máy và không
> muốn tải lại toàn bộ, dùng file này để biết chính xác file nào đã đổi và đổi những gì, để tự vá
> tay vào bản cũ. Với thay đổi cấu trúc lớn (thêm component mới, đổi model dữ liệu), khuyến nghị
> vẫn nên lấy nguyên file mới thay vì vá tay — ghi rõ ở từng mục bên dưới.

---

## 25/8/2026 — Lần 2 — Đổi màu cho phần lọc theo tag

### 1. Đổi màu cho phần lọc theo tag

**Vấn đề:** trang tất cả bài viết (`/bai-viet/`) phần lọc theo tag khi chuyển sang dark theme thì background trùng vs màu chữ là màu trắng, khi hover vào cũng kh có thay đổi.

**Nguyên nhân gốc rễ:** Có 2 cơ chế xung đột

- 1: Cơ chế Angular View Encapsulation (Emulated)
  -- Mặc định, Angular áp dụng chế độ cô lập CSS (ViewEncapsulation.Emulated)
  -- Khi biên dịch, Angular mặc định gắn một thuộc tính định danh duy nhất (ví dụ: \_ngcontent-ng-c1234567890) vào tất cả các phần tử HTML bên trong template của component, đồng thời rewrite toàn bộ CSS selector trong file .component.css
  -- Do đó: trong CSS có đoạn
  ---- .p-multiselect-option {
  color: var(--color-ink-soft) !important;
  }
  dịch thành:
  ----- .p-multiselect-option[_ngcontent-ng-c1234567890] {
  color: var(--color-ink-soft) !important;
  }
- 2: Cơ chế Overlay Portal của PrimeNG (appendTo="body")
  -- Các thành phần dropdown của PrimeNG (như dropdown panel của p-multiselect) được di chuyển ra ngoài cây DOM.
  -- Vì nằm ngoài DOM của component, phần panel dropdown `hoàn toàn không có thuộc tính scope_ngcontent-...` của Angular

**Hệ quả:**

- Trình duyệt tìm kiếm phần tử .p-multiselect-option có thuộc tính `[_ngcontent0ng0c1234567890]` để áp dụng style, nhưng thực tế phần tử dropdown option nằm dưới <body> nên không thể áp dụng thuộc tính này của CSS.
- Selector không khớp (mismatch) dẫn tới quy tắc CSS bị loại bỏ. Dùng `!important` không có tác dụng vì không đọc thấy.

**Giải pháp kỹ thuật:** Tận dụng PrimeNG 18 Design Token/ Preset. Dự án đang dùng hệ thống Theme Style Mode (@primeng/themes)

**File đã đổi:**
`src/app/core/theme/blog-preset.ts`

- trong component thêm:
  multiselect: {
  option: {
  color: '{content.hoverBackground}',
  focusColor: '{text.color}',
  focusBackground: '{content.hoverBackground}',
  selectedColor: '{primary.color}',
  selectedBackground: '{highlight.background}',
  },
  },

---

## 25/8/2026 — Lần 1 — Sidebar danh mục ở trang chi tiết + cây danh mục thu gọn thông minh hơn

### 1. Thêm sidebar danh mục vào trang chi tiết bài viết

**Vấn đề:** trang chi tiết bài viết (`/bai-viet/:slug`) trước đây không có sidebar điều hướng danh
mục như trang danh sách (`/danh-muc/:slug`), khiến người đọc không dễ nhảy sang danh mục khác khi
đang đọc một bài.

**Giải pháp kỹ thuật:** thay vì copy code cây danh mục vào 2 nơi (dễ lệch nhau sau này), toàn bộ
logic + giao diện cây danh mục được **tách thành 1 component dùng chung**:

**File MỚI (bắt buộc phải copy nguyên, không thể vá tay vì là component hoàn toàn mới):**

- `shared/components/category-sidebar/category-sidebar.component.ts`
- `shared/components/category-sidebar/category-sidebar.component.html`
- `shared/components/category-sidebar/category-sidebar.component.scss`

**File đã đổi:**

- `pages/post-list/post-list.component.ts` — xoá `categoryTree`, `expandedParents`, `toggleParent()`,
  `isExpanded()` (đã chuyển hết vào component mới), thêm `CategorySidebarComponent` vào `imports`.
- `pages/post-list/post-list.component.html` — thay toàn bộ khối `<nav class="filters__categories">`
  bằng 1 dòng `<app-category-sidebar [activeCategorySlug]="categorySlug()"></app-category-sidebar>`.
- `pages/post-list/post-list.component.scss` — xoá các class `.filters__cat-*` và `.filters__categories`
  (đã chuyển sang `category-sidebar.component.scss`), bỏ layout `flex-direction: row` trên mobile
  của `.filters` vì giờ sidebar là 1 component khối riêng, xếp chồng dọc gọn hơn khi thu nhỏ.
- `pages/post-detail/post-detail.component.ts` — import thêm `CategorySidebarComponent`.
- `pages/post-detail/post-detail.component.html` — bọc phần nội dung bài viết (nội dung, tag, chia
  sẻ) trong layout 2 cột `.post-detail__layout`, cột trái là `<app-category-sidebar>`. **Lưu ý:**
  ảnh đại diện, quảng cáo, "Bài viết liên quan" và phần "Đánh giá" vẫn giữ full-width như cũ — chỉ
  phần nội dung chính có thêm sidebar bên cạnh, để không phá vỡ các khối full-bleed đã thiết kế.
- `pages/post-detail/post-detail.component.scss` — thêm `.post-detail__layout`, `.post-detail__sidebar`
  (sticky, dời xuống dưới bài viết bằng `order: 2` trên mobile để ưu tiên đọc nội dung trước).

### 2. Danh mục cấp 2 (con) giờ cũng tự thu/mở được — trước đây chỉ cấp 1 thu/mở được

Trong `category-sidebar.component.ts`, thêm `expandedChildren` (signal Set) song song với
`expandedParents` đã có. Cấp 1 mặc định **mở sẵn** (như trước), cấp 2 mặc định **đóng** — giữ sidebar
gọn gàng, nhất là khi dùng lại ở trang chi tiết bài viết vốn đã có nhiều nội dung bên cạnh.

### 3. Giới hạn tối đa 5 danh mục cấp 3 hiện ra khi mở 1 danh mục cấp 2, có "Xem tiếp…"

Hằng số `MAX_GRANDCHILDREN_SHOWN = 5` trong `category-sidebar.component.ts`. Khi 1 danh mục cấp 2
được mở và có nhiều hơn 5 danh mục cấp 3 con, sidebar chỉ hiện 5 mục đầu tiên (theo thứ tự khai báo
trong `MOCK_CATEGORIES` — xem ghi chú "gần nhất" trong `HUONG-DAN-CHI-TIET.md` mục 3.1 để biết cách
làm đúng nghĩa "gần nhất theo thời gian" khi nối API thật), kèm 1 dòng liên kết **"Xem tiếp…"** dẫn
thẳng tới trang danh sách của chính danh mục cấp 2 đó (`/danh-muc/:slug-cap-2`) — nơi người đọc thấy
đầy đủ mọi bài viết thuộc nhánh này, không giới hạn 5.

Với dữ liệu mock hiện tại (mỗi danh mục cấp 2 chỉ có 1 danh mục cấp 3), giới hạn này chưa có tác
dụng hiển thị rõ — nhưng logic đã sẵn sàng khi danh mục thật có nhiều nhánh con hơn.

### 4. Cách tự vá tay đợt này (nếu không muốn tải lại toàn bộ zip)

Đợt sửa này có **1 component hoàn toàn mới** (`category-sidebar`) — phần này **bắt buộc phải copy
nguyên 3 file mới**, không thể vá tay (không có "bản cũ" để so sánh từng dòng). Nhưng nếu bạn đã có
sẵn code, các bước dưới đây là toàn bộ việc cần làm, làm được thủ công trong khoảng 10 phút:

1. **Tạo 3 file mới** — copy nguyên nội dung từ bản zip mới:
   - `src/app/shared/components/category-sidebar/category-sidebar.component.ts`
   - `src/app/shared/components/category-sidebar/category-sidebar.component.html`
   - `src/app/shared/components/category-sidebar/category-sidebar.component.scss`

2. **Sửa `post-list.component.ts`**: xoá 4 thứ liên quan cây danh mục cũ (`categoryTree`,
   `expandedParents`, `toggleParent()`, `isExpanded()`), thêm `import { CategorySidebarComponent }`
   và thêm vào mảng `imports`.

3. **Sửa `post-list.component.html`**: tìm khối `<nav class="filters__categories">...</nav>` (khá
   dài, có `*ngFor` lồng 3 cấp), xoá toàn bộ, thay bằng:

   ```html
   <app-category-sidebar [activeCategorySlug]="categorySlug()"></app-category-sidebar>
   ```

4. **Sửa `post-list.component.scss`**: xoá các class bắt đầu bằng `.filters__cat-` và
   `.filters__categories` (đã chuyển sang file component mới ở bước 1). Trong class `.filters`, bỏ
   đoạn `@media (max-width: 860px) { flex-direction: row; flex-wrap: wrap; gap: ...; }` — chỉ giữ
   `position: static;` ở breakpoint đó.

5. **Sửa `post-detail.component.ts`**: thêm `import { CategorySidebarComponent }` và thêm vào
   `imports`.

6. **Sửa `post-detail.component.html`**: tìm `<div class="container post-detail__body">`, đổi
   thành cấu trúc 2 lớp:

   ```html
   <div class="container post-detail__layout">
     <aside class="post-detail__sidebar">
       <app-category-sidebar [activeCategorySlug]="p.category.slug"></app-category-sidebar>
     </aside>
     <div class="post-detail__body">
       <!-- giữ nguyên toàn bộ nội dung cũ bên trong (post-detail__content, tags, share) -->
     </div>
   </div>
   ```

7. **Sửa `post-detail.component.scss`**: thêm 3 class mới `.post-detail__layout` (grid 2 cột
   `240px 1fr`, gap `var(--space-8)`, sập về 1 cột dưới `860px`), `.post-detail__sidebar` (sticky,
   `order: 2` trên mobile), và bỏ `padding: var(--space-7) 0` cũ trong `.post-detail__body` sang
   `.post-detail__layout` — xem file mới trong zip để copy chính xác đoạn CSS này (khoảng 20 dòng).

**Lời khuyên thật lòng:** bước 3 và 6 dễ gõ sai vì phải xác định đúng ranh giới thẻ HTML lồng nhau.
Nếu không tự tin, cách an toàn nhất vẫn là tải lại zip mới rồi diff thủ công 2 thư mục bằng công cụ
như VS Code ("Compare Folders") để chỉ lấy đúng phần đã đổi, tránh copy nhầm đè lên các chỉnh sửa
riêng bạn có thể đã tự làm thêm trên bản cũ.

---

## 24/8/2026 — Lần 1 — Đợt sửa theo 10 góp ý

### 1. Button chưa hiện đại → Hệ thống `.btn` dùng chung

**File thay đổi:** `src/styles.scss` (thêm mới đoạn lớn), và **toàn bộ** file `.html`/`.scss` có
nút bấm (header, footer, cookie-consent, post-list, post-detail, otp-review-form, not-found).

**Việc đã làm:** Thêm class `.btn` + biến thể `--primary/--secondary/--ghost/--danger/--sm` trong
`styles.scss` (bo góc 8px, shadow nhẹ, hover nâng lên + đổ bóng, transition mượt). Xoá toàn bộ CSS
nút tự phát trước đây (`.text-btn`, `.submit-btn`, `.link-btn`, `.cookie-btn`) và thay bằng class
`.btn` tương ứng ở từng template.

**Nếu tự vá tay:** copy đoạn `/* HỆ THỐNG BUTTON DÙNG CHUNG */` trong `styles.scss` (nằm ngay sau
khối `a:focus-visible...`), rồi tìm-thay mọi `class="text-btn"` → `class="btn btn--primary"` (hoặc
`--secondary`/`--ghost` tuỳ ngữ cảnh, xem file mới để đối chiếu từng chỗ).

---

### 2. Danh mục chưa chia 3 cấp → Cấu trúc cây thật

**File thay đổi:**

- `shared/models/post.model.ts` — `Category` có thêm `parentId: string | null`, thêm interface `CategoryNode`
- `shared/mock-data/posts.mock.ts` — `MOCK_CATEGORIES` viết lại thành 15 mục đủ 3 tầng, thêm hàm
  `buildCategoryTree()`, `findCategoryBySlug()`, `collectDescendantIds()`
- `core/layout/header/` — nav giờ có **mega-menu hover** hiện cấp 2/3
- `pages/post-list/` — sidebar lọc giờ là **cây thu/mở** 3 cấp, lọc theo cả nhánh cây
- `pages/post-detail/` — breadcrumb hiện đủ 3 cấp (dựng ngược từ `parentId`)
- `core/layout/footer/` — chỉ hiện danh mục cấp 1 (dùng `buildCategoryTree()`, lấy root)

**Đây là thay đổi cấu trúc dữ liệu lớn nhất đợt này — khuyến nghị lấy nguyên file mới thay vì vá tay.**

---

### 3. Ảnh đại diện bài viết chi tiết lệch trái → Canh giữa tường minh

**File thay đổi:** `pages/post-detail/post-detail.component.scss`

**Việc đã làm:** class `.post-detail__hero-media` thêm `max-width: var(--content-max)`,
`margin: 0 auto`, và `img` thêm `object-position: center` + `margin: 0 auto`.

**Vá tay:**

```scss
.post-detail__hero-media {
  max-width: var(--content-max);
  margin: 0 auto;
  /* ...giữ nguyên các dòng khác... */
  border-radius: 12px;

  img {
    /* ...giữ nguyên width/height/object-fit... */
    object-position: center;
    display: block;
    margin: 0 auto;
  }
}
```

---

### 4. Cookie banner cần trang chi tiết → Trang Chính sách dữ liệu

**File mới:** `pages/data-policy/data-policy.component.ts` + `.html` + `.scss`
**File thay đổi:** `app.routes.ts` (thêm route `/chinh-sach-du-lieu`)

Nội dung giải thích: dữ liệu IP/thiết bị (chỉ ghi khi đồng ý cookie), email đánh giá (ẩn, chỉ dùng
gửi OTP), email newsletter (không bán, có unsubscribe), quảng cáo AdSense (từ chối cookie không tắt
quảng cáo). Link đã có sẵn từ cookie-consent trỏ tới route này từ trước, giờ trang đích đã tồn tại.

---

### 5. Trang huỷ đăng ký nhận bài viết → Đã tạo

**File mới:** `pages/unsubscribe/unsubscribe.component.ts` + `.html` + `.scss`
**File thay đổi:** `app.routes.ts` (thêm route `/huy-dang-ky`)

Nhận email qua query param (`?email=...`, để link trong mail newsletter có thể điền sẵn) hoặc nhập
tay. Mock gọi API huỷ, hiện trạng thái thành công.

---

### 6. Nút chuyển theme sáng/tối → Đã thêm vào header

**File mới:**

- `core/services/theme.service.ts` — signal quản lý mode, lưu `localStorage`, gắn class `theme-dark` lên `<html>`
- `core/layout/theme-toggle/theme-toggle.component.ts` + `.html` + `.scss` — nút icon mặt trời/mặt trăng

**File thay đổi:**

- `src/styles.scss` — thêm khối `html.theme-dark { ... }` override toàn bộ token màu (xem mục 2.2 trong `HUONG-DAN-CHI-TIET.md`)
- `core/theme/blog-preset.ts` — thêm `colorScheme.dark` để PrimeNG components cũng đổi màu
- `app.config.ts` — đổi `darkModeSelector: false` → `darkModeSelector: '.theme-dark'`
- `core/layout/header/header.component.ts` + `.html` — import và chèn `<app-theme-toggle>` vào `site-header__actions`

**Vá tay:** đây là tính năng mới hoàn toàn (thêm state quản lý theme), khuyến nghị lấy nguyên các
file mới ở trên thay vì cố vá tay từng dòng.

---

### 7. Footer sát lề trái trên iPad/iPhone → Fix overflow + decouple màu

**File thay đổi:** `core/layout/footer/footer.component.scss`, `core/layout/cookie-consent/cookie-consent.component.scss`, `src/styles.scss`

**Nguyên nhân kỹ thuật đã xử lý:**

1. Thêm `overflow-x: hidden` ở `html, body` (trong `styles.scss`) và ở chính `.site-footer` — chặn
   khả năng nội dung tràn ngang kéo cả trang lệch.
2. Thêm `min-width: 0` cho từng cột trong `.site-footer__top` (và `.newsletter-form input`) — đây
   là lỗi CSS Grid/Flexbox kinh điển: phần tử con trong grid/flex mặc định có `min-width: auto`,
   khiến nội dung dài (input, đường viền) đẩy tràn cột ra ngoài khung thay vì co lại, gây lệch bố cục.
3. Tách `--color-inverse-*` riêng cho footer/cookie-toast (xem mục 6 — cần thiết vì dark mode mới
   thêm sẽ đảo `--color-ink`, nếu footer vẫn dùng `--color-ink` trực tiếp sẽ vỡ giao diện ở dark mode).

**Lưu ý trung thực:** môi trường dựng dự án này không có trình duyệt để test trực tiếp trên iPad/
iPhone thật — các fix trên dựa trên rà soát CSS kỹ (đây là nguyên nhân phổ biến nhất gây đúng triệu
chứng bạn mô tả). Bạn kiểm tra lại bằng Chrome DevTools chế độ responsive hoặc thiết bị thật, nếu
vẫn còn lệch, chụp ảnh gửi lại để mình khoanh vùng chính xác hơn.

---

### 8 & 9. Hai file tài liệu mới

- `HUONG-DAN-CHI-TIET.md` — thay thế hoàn toàn file hướng dẫn cũ, thêm: bảng token màu đầy đủ 2
  theme, cách hoạt động dark mode, quy tắc hệ thống `.btn`, cấu trúc danh mục 3 cấp + hàm tiện ích,
  luồng OTP chi tiết kèm endpoint API gợi ý khi nối backend thật, bảng kích thước ảnh đầy đủ, quy
  trình chuẩn để thêm 1 trang mới, hướng dẫn nối API .NET thật, lưu ý bắt buộc khi bật SSR.
- `TOM-TAT-NHANH.md` — file mới, dùng để bất kỳ ai (người hoặc AI) mở dự án lần đầu nắm được toàn bộ
  bối cảnh + các quyết định nghiệp vụ đã chốt trong 2 phút, không cần đọc lại lịch sử trao đổi.

### 10. File CHANGELOG.md này

Ghi lại chính xác file nào đổi, đổi gì, vì sao — để bạn tự vá vào bản cũ nếu không muốn tải lại toàn
bộ source. Với các mục có nhiều file mới liên quan (2, 6), khuyến nghị lấy nguyên file mới vì đây là
thay đổi cấu trúc, vá tay dễ sai sót hơn là copy nguyên.
