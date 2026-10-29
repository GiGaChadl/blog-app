import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterLink, RouterLinkActive, Router } from '@angular/router';
import { AuthService } from '../../auth/auth.service';
import { ThemeToggleComponent } from '../theme-toggle/theme-toggle.component';
import { ButtonModule } from 'primeng/button';
import { ConfirmationService } from 'primeng/api';

interface NavItem {
  label: string;
  icon: string;
  route: string;
  badge?: string;
}

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    ThemeToggleComponent,
    ButtonModule
  ],
  templateUrl: './admin-layout.component.html',
  styleUrl: './admin-layout.component.scss'
})
export class AdminLayoutComponent {
  authService = inject(AuthService);
  private router = inject(Router);
  private confirmationService = inject(ConfirmationService);

  sidebarCollapsed = signal(false);

  navGroups: { title: string; items: NavItem[] }[] = [
    {
      title: 'TỔNG QUAN',
      items: [
        { label: 'Bảng Điều Khiển', icon: 'pi pi-chart-pie', route: '/admin/dashboard' }
      ]
    },
    {
      title: 'NỘI DUNG',
      items: [
        { label: 'Bài Viết', icon: 'pi pi-file-edit', route: '/admin/posts' },
        { label: 'Chuyên Mục', icon: 'pi pi-folder', route: '/admin/categories' },
        { label: 'Thẻ Tags', icon: 'pi pi-tags', route: '/admin/tags' },
        { label: 'Thư Viện Media', icon: 'pi pi-images', route: '/admin/media' }
      ]
    },
    {
      title: 'TƯƠNG TÁC',
      items: [
        { label: 'Đánh Giá Độc Giả', icon: 'pi pi-comments', route: '/admin/reviews' },
        { label: 'Người Theo Dõi', icon: 'pi pi-users', route: '/admin/subscribers' }
      ]
    },
    {
      title: 'QUẢNG BÁ & DỰ ÁN',
      items: [
        { label: 'Banners Trang Chủ', icon: 'pi pi-image', route: '/admin/banners' },
        { label: 'Danh Ngôn / Slogan', icon: 'pi pi-comment', route: '/admin/slogans' },
        { label: 'Google AdSense', icon: 'pi pi-megaphone', route: '/admin/ads' },
        { label: 'Dự Án Cá Nhân', icon: 'pi pi-briefcase', route: '/admin/projects' }
      ]
    },
    {
      title: 'HỆ THỐNG',
      items: [
        { label: 'Cài Đặt Hệ Thống', icon: 'pi pi-cog', route: '/admin/settings' }
      ]
    }
  ];

  toggleSidebar(): void {
    this.sidebarCollapsed.update((v) => !v);
  }

  onLogout(): void {
    this.confirmationService.confirm({
      message: 'Bạn có chắc chắn muốn đăng xuất khỏi trang Quản trị?',
      header: 'Xác nhận đăng xuất',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Đăng xuất',
      rejectLabel: 'Hủy',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.authService.logout();
      }
    });
  }
}
