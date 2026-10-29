# TÓM TẮT NHANH — Đọc file này trước tiên

> Cập nhật lần cuối: **25/8/2026**. Xem `CHANGELOG.md` để biết lịch sử từng đợt sửa theo ngày.

> File này dành cho **bất kỳ ai** (bạn, đồng nghiệp, hay một phiên AI khác) mở dự án lần đầu và cần
> hiểu toàn bộ bối cảnh trong 2 phút, không cần đọc lại lịch sử chat.

## Dự án này là gì

Blog cá nhân (tiếng Việt) để chia sẻ bài học/kinh nghiệm/trải nghiệm. Đây là phần **public site**
(giao diện người đọc) — **chưa có trang Admin**.

- **Frontend**: Angular 18 (standalone components) + PrimeNG 18
- **Backend dự kiến**: .NET API (chưa làm — hiện tại toàn bộ dữ liệu là mock trong code)
- **Database dự kiến**: PostgreSQL (chưa làm)
- **Auth dự kiến**: JWT cho Admin (chưa làm — public site không cần đăng nhập)
- **Trạng thái hiện tại**: UI thật (không phải mockup), chạy được bằng `ng serve`, build production
  đã test pass, nhưng **toàn bộ dữ liệu là mock**, chưa nối API thật.

## Chạy dự án

```bash
cd blog-app
npm install
npm start        # http://localhost:4200
```

## Bối cảnh thiết kế (đã chốt qua nhiều vòng trao đổi yêu cầu)

Đây là các quyết định nghiệp vụ **đã chốt**, đừng tự ý đổi nếu không được yêu cầu:

1. **Danh mục 3 cấp bắt buộc**: Cha → Con → Cháu. Mỗi bài viết thuộc đúng **1 danh mục cấp 3** (không phải nhiều danh mục). Sidebar cây danh mục (`shared/components/category-sidebar/`) dùng chung ở cả trang danh sách VÀ trang chi tiết bài viết — cấp 1 mặc định mở, cấp 2 mặc định đóng (bấm để mở), cấp 3 giới hạn hiện tối đa 5 mục gần nhất kèm "Xem tiếp…" nếu nhiều hơn.
2. **Đánh giá bài viết cần OTP email**: nhập email + reCAPTCHA checkbox → gửi OTP 6 số → xác thực → tạo **session 24h** (không phải tài khoản đăng nhập thật) → trong 24h có thể sửa/xoá không cần OTP lại → hết session hoặc đổi máy phải xác thực lại. Giới hạn **5 lần gửi OTP/email/24h**.
3. **Cookie consent**: banner nhỏ góc dưới trái. "Đồng ý" → mới bắt đầu ghi log IP/thiết bị/vị trí. "Từ chối" → chỉ tắt banner, **không** tắt quảng cáo AdSense mặc định. Có trang `/chinh-sach-du-lieu` giải thích chi tiết.
4. **Quảng cáo AdSense**: 2 vị trí cố định — giữa khối "Bài viết nổi bật" ở trang chủ, và ngay sau ảnh chính ở trang chi tiết bài viết. Tự ẩn ở vị trí trang chủ trên mobile.
5. **Bài viết liên quan**: ưu tiên cùng danh mục cấp 3 → mở rộng lên anh em cùng danh mục cấp 2 → fallback bài mới nhất toàn site nếu vẫn chưa đủ 3 bài.
6. **Bài viết nổi bật (trang chủ)**: thuật toán ưu tiên view → rating → fallback bài mới nhất.
7. **Newsletter**: double opt-in (đăng ký → gửi email xác nhận → bấm link mới active). Luôn có link huỷ đăng ký trong mỗi email, có trang `/huy-dang-ky` riêng.
8. **Chỉ tiếng Việt**, không đa ngôn ngữ.
9. **1 admin duy nhất** (không có phân quyền nhiều role) — áp dụng cho phần Admin sẽ làm sau.
10. **Dark mode**: toggle sáng/tối ở header (`core/services/theme.service.ts`), lưu `localStorage`, footer/cookie-toast dùng bộ biến `--color-inverse-*` riêng để luôn giữ tương phản dù theme nào.

## Bản đồ thư mục (điều hướng nhanh)

```
src/
├─ styles.scss                     ← TOÀN BỘ design token (màu/font/spacing) + hệ thống .btn dùng chung
├─ index.html                      ← font Google Fonts (Fraunces/Inter/IBM Plex Mono)
└─ app/
   ├─ app.routes.ts                ← toàn bộ route của public site
   ├─ app.config.ts                ← đăng ký PrimeNG theme, animations
   ├─ core/
   │  ├─ theme/blog-preset.ts      ← theme PrimeNG tuỳ biến (map token vào PrimeNG)
   │  ├─ services/theme.service.ts ← quản lý sáng/tối, lưu localStorage
   │  └─ layout/
   │     ├─ header/                ← nav + mega-menu 3 cấp + tìm kiếm + nút theme
   │     ├─ footer/                ← danh mục cấp 1 + newsletter + social
   │     ├─ cookie-consent/        ← banner cookie góc dưới trái
   │     └─ theme-toggle/          ← nút bật/tắt dark mode
   ├─ shared/
   │  ├─ models/post.model.ts      ← toàn bộ interface (Category, PostSummary, PostDetail, PostReview...)
   │  ├─ mock-data/                ← DỮ LIỆU GIẢ — thay bằng gọi API thật khi có backend
   │  │  ├─ posts.mock.ts          ← bài viết, danh mục 3 cấp, hàm buildCategoryTree()
   │  │  └─ about.mock.ts          ← dự án + contact cho trang About
   │  └─ components/               ← component dùng lại nhiều nơi
   │     ├─ hero-banner/           ← banner trang chủ (slogan + 3 ảnh, KHÔNG link)
   │     ├─ post-card/             ← thẻ bài viết dùng trong mọi grid
   │     ├─ category-sidebar/      ← sidebar cây danh mục 3 cấp, DÙNG CHUNG ở post-list & post-detail
   │     ├─ ad-slot/                ← ô quảng cáo AdSense (placeholder)
   │     ├─ star-rating/           ← sao đánh giá (dùng cả hiển thị & chọn)
   │     └─ otp-review-form/       ← form đánh giá kèm luồng OTP đầy đủ
   └─ pages/                       ← 1 thư mục = 1 route
      ├─ home/                     ← "/"
      ├─ post-list/                ← "/danh-muc/:slug" và "/tim-kiem" (dùng chung component)
      ├─ post-detail/              ← "/bai-viet/:slug"
      ├─ about/                    ← "/gioi-thieu"
      ├─ data-policy/              ← "/chinh-sach-du-lieu"
      ├─ unsubscribe/              ← "/huy-dang-ky"
      └─ not-found/                ← route "**"
```

## Việc CHƯA làm (rõ ràng, để không ai nhầm là đã xong)

- [ ] Trang Admin (toàn bộ — quản lý bài viết/danh mục/banner/quảng cáo/subscriber/đánh giá/thống kê...)
- [ ] Kết nối API .NET thật — hiện tại 100% mock trong `shared/mock-data/`
- [ ] Angular Universal (SSR) — hiện tại là CSR thuần, **bắt buộc phải bật SSR trước khi lên production** vì đã chốt yêu cầu SEO dùng SSR
- [ ] CKEditor/TinyMCE — chỉ cần ở Admin, chưa làm
- [ ] sitemap.xml, robots.txt thật (mới là ghi chú yêu cầu, chưa sinh file)
- [ ] Trang xác nhận double opt-in khi đăng ký newsletter (mới có bước 1 — gửi email giả lập, chưa có trang xác nhận link bấm vào)
- [ ] Test thực tế trên thiết bị thật (mọi fix responsive trong dự án đều dựa trên đọc code, KHÔNG có browser để test trực tiếp trong môi trường dựng dự án này — xem CHANGELOG.md để biết cụ thể)

## Nếu bạn là AI đọc lại dự án này

Đọc thêm `CHANGELOG.md` (lịch sử từng thay đổi theo yêu cầu, kèm lý do) và `HUONG-DAN-CHI-TIET.md`
(hướng dẫn kỹ thuật sâu hơn: cách thêm trang mới, cách đổi màu, kích thước ảnh chuẩn, cách nối API
thật). Đừng đoán lại các quyết định nghiệp vụ ở mục "Bối cảnh thiết kế" phía trên — chúng đã được
người dùng chốt qua nhiều vòng trao đổi, không phải giả định của AI.
