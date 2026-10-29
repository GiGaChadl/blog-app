import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { CategoryNode } from '../../../shared/models/post.model';
import { BlogApiService } from '../../services/blog-api.service';
import { SiteSettingsService } from '../../services/site-settings.service';

type SubscribeState = 'idle' | 'submitting' | 'success' | 'error';

@Component({
  selector: 'app-footer',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, InputTextModule, ButtonModule],
  templateUrl: './footer.component.html',
  styleUrl: './footer.component.scss'
})
export class FooterComponent implements OnInit {
  private blogApi = inject(BlogApiService);
  public siteSettings = inject(SiteSettingsService);

  /** Footer chỉ hiện danh mục cấp 1 (cha) cho gọn — cấp 2/3 xem đầy đủ ở mega-menu header */
  topCategories: CategoryNode[] = [];
  currentYear = new Date().getFullYear();

  socials = signal<{
    facebook?: string;
    linkedin?: string;
    github?: string;
    youtube?: string;
    email?: string;
  }>({});

  email = '';
  state = signal<SubscribeState>('idle');
  errorMessage = signal('');

  private readonly emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

  ngOnInit(): void {
    this.blogApi.getCategoryTree().subscribe({
      next: (tree) => (this.topCategories = tree),
      error: () => (this.topCategories = [])
    });

    this.blogApi.getContactInfo().subscribe({
      next: (info) => {
        if (!info) return;
        const findLink = (name: string) =>
          info.socialLinks?.find((s) => s.platform.toLowerCase().includes(name))?.url;

        this.socials.set({
          facebook: info.facebookUrl || findLink('facebook'),
          linkedin: info.linkedinUrl || findLink('linkedin'),
          github: info.githubUrl || findLink('github'),
          youtube: info.youtubeUrl || findLink('youtube'),
          email: info.email
        });
      },
      error: () => {}
    });
  }

  subscribe() {
    const value = this.email.trim();
    if (!this.emailPattern.test(value)) {
      this.state.set('error');
      this.errorMessage.set('Email chưa đúng định dạng. Vui lòng kiểm tra lại.');
      return;
    }

    this.state.set('submitting');
    this.blogApi.subscribe(value).subscribe({
      next: () => {
        this.state.set('success');
      },
      error: (err) => {
        this.state.set('error');
        this.errorMessage.set(err?.error?.message || 'Không thể đăng ký nhận tin. Vui lòng thử lại sau.');
      }
    });
  }

  resetForm() {
    this.email = '';
    this.state.set('idle');
  }
}
