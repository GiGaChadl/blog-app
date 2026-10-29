import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';
import { PublicLayoutComponent } from './core/layout/public-layout/public-layout.component';

export const routes: Routes = [
  // Admin Login (Standalone full screen)
  {
    path: 'admin/login',
    loadComponent: () =>
      import('./pages/admin/login/admin-login.component').then((m) => m.AdminLoginComponent),
    title: 'Đăng Nhập Quản Trị — Blog',
  },

  // Admin Portal (Protected with Layout)
  {
    path: 'admin',
    loadComponent: () =>
      import('./core/layout/admin-layout/admin-layout.component').then((m) => m.AdminLayoutComponent),
    canActivate: [authGuard],
    children: [
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
      },
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./pages/admin/dashboard/dashboard.component').then((m) => m.DashboardComponent),
        title: 'Bảng Điều Khiển — Quản Trị Blog'
      },
      {
        path: 'posts',
        loadComponent: () =>
          import('./pages/admin/posts/post-list.component').then((m) => m.PostListComponent),
        title: 'Quản Lý Bài Viết — Quản Trị Blog'
      },
      {
        path: 'posts/new',
        loadComponent: () =>
          import('./pages/admin/posts/post-edit.component').then((m) => m.PostEditComponent),
        title: 'Thêm Bài Viết Mới — Quản Trị Blog'
      },
      {
        path: 'posts/edit/:id',
        loadComponent: () =>
          import('./pages/admin/posts/post-edit.component').then((m) => m.PostEditComponent),
        title: 'Chỉnh Sửa Bài Viết — Quản Trị Blog'
      },
      {
        path: 'categories',
        loadComponent: () =>
          import('./pages/admin/categories/categories-admin.component').then((m) => m.CategoriesAdminComponent),
        title: 'Quản Lý Chuyên Mục — Quản Trị Blog'
      },
      {
        path: 'tags',
        loadComponent: () =>
          import('./pages/admin/tags/tags-admin.component').then((m) => m.TagsAdminComponent),
        title: 'Quản Lý Thẻ Tags — Quản Trị Blog'
      },
      {
        path: 'media',
        loadComponent: () =>
          import('./pages/admin/media/media-admin.component').then((m) => m.MediaAdminComponent),
        title: 'Thư Viện Media — Quản Trị Blog'
      },
      {
        path: 'reviews',
        loadComponent: () =>
          import('./pages/admin/reviews/reviews-admin.component').then((m) => m.ReviewsAdminComponent),
        title: 'Kiểm Duyệt Đánh Giá — Quản Trị Blog'
      },
      {
        path: 'subscribers',
        loadComponent: () =>
          import('./pages/admin/subscribers/subscribers-admin.component').then((m) => m.SubscribersAdminComponent),
        title: 'Quản Lý Subscribers — Quản Trị Blog'
      },
      {
        path: 'banners',
        loadComponent: () =>
          import('./pages/admin/banners/banners-admin.component').then((m) => m.BannersAdminComponent),
        title: 'Quản Lý Banners — Quản Trị Blog'
      },
      {
        path: 'slogans',
        loadComponent: () =>
          import('./pages/admin/slogans/slogans-admin.component').then((m) => m.SlogansAdminComponent),
        title: 'Quản Lý Slogans — Quản Trị Blog'
      },
      {
        path: 'ads',
        loadComponent: () =>
          import('./pages/admin/ads/ads-admin.component').then((m) => m.AdsAdminComponent),
        title: 'Cấu Hình Quảng Cáo — Quản Trị Blog'
      },
      {
        path: 'projects',
        loadComponent: () =>
          import('./pages/admin/projects/projects-admin.component').then((m) => m.ProjectsAdminComponent),
        title: 'Quản Lý Dự Án — Quản Trị Blog'
      },
      {
        path: 'settings',
        loadComponent: () =>
          import('./pages/admin/settings/settings-admin.component').then((m) => m.SettingsAdminComponent),
        title: 'Cài Đặt Hệ Thống — Quản Trị Blog'
      }
    ]
  },

  // Public Blog Routes wrapped in PublicLayoutComponent
  {
    path: '',
    component: PublicLayoutComponent,
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./pages/home/home.component').then((m) => m.HomeComponent),
      },
      {
        path: 'danh-muc/:slug',
        loadComponent: () =>
          import('./pages/post-list/post-list.component').then(
            (m) => m.PostListComponent,
          ),
        title: 'Danh sách bài viết — Nhật Ký Của Tôi',
      },
      {
        path: 'bai-viet/:slug',
        loadComponent: () =>
          import('./pages/post-detail/post-detail.component').then(
            (m) => m.PostDetailComponent,
          ),
      },
      {
        path: 'gioi-thieu',
        loadComponent: () =>
          import('./pages/about/about.component').then((m) => m.AboutComponent),
        title: 'Giới thiệu — Nhật Ký Của Tôi',
      },
      {
        path: 'tim-kiem',
        loadComponent: () =>
          import('./pages/post-list/post-list.component').then(
            (m) => m.PostListComponent,
          ),
        data: { isSearch: true },
        title: 'Tìm kiếm — Nhật Ký Của Tôi',
      },
      {
        path: 'chinh-sach-du-lieu',
        loadComponent: () =>
          import('./pages/data-policy/data-policy.component').then(
            (m) => m.DataPolicyComponent,
          ),
        title: 'Chính sách dữ liệu — Nhật Ký Của Tôi',
      },
      {
        path: 'huy-dang-ky',
        loadComponent: () =>
          import('./pages/unsubscribe/unsubscribe.component').then(
            (m) => m.UnsubscribeComponent,
          ),
        title: 'Huỷ đăng ký — Nhật Ký Của Tôi',
      }
    ]
  },

  // Fallback 404
  {
    path: '**',
    loadComponent: () =>
      import('./pages/not-found/not-found.component').then(
        (m) => m.NotFoundComponent,
      ),
    title: 'Không tìm thấy trang — Nhật Ký Của Tôi',
  },
];
