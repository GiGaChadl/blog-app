import { CommonModule } from '@angular/common';
import { Component, HostListener, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { InputTextModule } from 'primeng/inputtext';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { CategoryNode } from '../../../shared/models/post.model';
import { ThemeToggleComponent } from '../theme-toggle/theme-toggle.component';
import { BlogApiService } from '../../services/blog-api.service';
import { SiteSettingsService } from '../../services/site-settings.service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    RouterLinkActive,
    InputTextModule,
    IconFieldModule,
    InputIconModule,
    ThemeToggleComponent
  ],
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss'
})
export class HeaderComponent implements OnInit {
  private blogApi = inject(BlogApiService);
  public siteSettings = inject(SiteSettingsService);
  private router = inject(Router);

  /** Cây danh mục 3 cấp: cha (hiện ở thanh nav) > con > cháu (hiện trong mega-menu khi hover/click) */
  categoryTree: CategoryNode[] = [];
  openMegaMenuId = signal<string | null>(null);

  isScrolled = signal(false);
  isMobileMenuOpen = signal(false);
  isSearchOpen = signal(false);
  searchTerm = '';

  ngOnInit(): void {
    this.blogApi.getCategoryTree().subscribe({
      next: (tree) => (this.categoryTree = tree),
      error: () => (this.categoryTree = [])
    });
  }

  @HostListener('window:scroll')
  onWindowScroll() {
    this.isScrolled.set(window.scrollY > 8);
  }

  toggleMobileMenu() {
    this.isMobileMenuOpen.update((v) => !v);
  }

  toggleSearch() {
    this.isSearchOpen.update((v) => !v);
    if (this.isSearchOpen()) {
      queueMicrotask(() => document.getElementById('header-search-input')?.focus());
    }
  }

  submitSearch() {
    const q = this.searchTerm.trim();
    if (!q) return;
    this.router.navigate(['/tim-kiem'], { queryParams: { q } });
    this.isSearchOpen.set(false);
    this.isMobileMenuOpen.set(false);
  }

  openMegaMenu(id: string) {
    this.openMegaMenuId.set(id);
  }

  closeMegaMenu() {
    this.openMegaMenuId.set(null);
  }
}
