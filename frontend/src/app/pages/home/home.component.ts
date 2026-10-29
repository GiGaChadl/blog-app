import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal, effect } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { HeroBannerComponent, HeroBannerImage } from '../../shared/components/hero-banner/hero-banner.component';
import { PostCardComponent } from '../../shared/components/post-card/post-card.component';
import { AdSlotComponent } from '../../shared/components/ad-slot/ad-slot.component';
import { PostSummary } from '../../shared/models/post.model';
import { BlogApiService } from '../../core/services/blog-api.service';
import { SiteSettingsService } from '../../core/services/site-settings.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink, HeroBannerComponent, PostCardComponent, AdSlotComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent implements OnInit {
  private blogApi = inject(BlogApiService);
  private titleService = inject(Title);
  siteSettings = inject(SiteSettingsService);

  featuredPosts: PostSummary[] = [];
  latestPosts: PostSummary[] = [];
  activeSlogan = signal<string | undefined>(undefined);
  sloganAuthor = signal<string | undefined>(undefined);
  bannerImages = signal<HeroBannerImage[]>([]);
  isLoadingFeatured = signal(true);
  isLoadingLatest = signal(true);

  constructor() {
    effect(() => {
      const title = this.siteSettings.defaultTitle();
      if (title) {
        this.titleService.setTitle(title);
      }
    });
  }

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.blogApi.getFeaturedPosts(6).subscribe({
      next: (posts) => {
        this.featuredPosts = posts;
        this.isLoadingFeatured.set(false);
      },
      error: () => {
        this.featuredPosts = [];
        this.isLoadingFeatured.set(false);
      }
    });

    this.blogApi.getPosts({ page: 1, pageSize: 6 }).subscribe({
      next: (res) => {
        this.latestPosts = res.items;
        this.isLoadingLatest.set(false);
      },
      error: () => {
        this.latestPosts = [];
        this.isLoadingLatest.set(false);
      }
    });

    this.blogApi.getActiveSlogan().subscribe({
      next: (slogan) => {
        if (slogan?.content) {
          this.activeSlogan.set(slogan.content);
          this.sloganAuthor.set(slogan.author);
        }
      },
      error: () => {}
    });

    this.blogApi.getActiveBanners().subscribe({
      next: (banners) => {
        if (banners && banners.length > 0) {
          this.bannerImages.set(
            banners.map((b) => ({
              url: b.imageUrl || b.mediaUrl || '',
              alt: b.title || b.mediaAltText || 'Banner',
              linkUrl: b.targetUrl || b.linkUrl
            }))
          );
        }
      },
      error: () => {}
    });
  }
}

