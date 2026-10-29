import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { SeoSettingsDto } from './admin-api.service';

@Injectable({
  providedIn: 'root'
})
export class SiteSettingsService {
  private http = inject(HttpClient);

  siteName = signal<string>(localStorage.getItem('site_name') || 'Nhật Ký Của Tôi');
  logoUrl = signal<string | null>(localStorage.getItem('site_logo_url') || null);
  faviconUrl = signal<string | null>(localStorage.getItem('site_favicon_url') || null);
  defaultTitle = signal<string>(localStorage.getItem('site_default_title') || 'Nhật Ký Của Tôi — Blog cá nhân');
  defaultDescription = signal<string>(localStorage.getItem('site_default_desc') || 'Nơi tôi ghi lại những bài học, kinh nghiệm và trải nghiệm thật.');
  copyright = signal<string>(localStorage.getItem('site_copyright') || '');
  authorName = signal<string>(localStorage.getItem('site_author_name') || 'Tuấn Nguyễn');
  heroSlogan = signal<string>(localStorage.getItem('site_hero_slogan') || '');

  siteMark = computed(() => {
    const name = this.siteName().trim();
    return name ? name.charAt(0).toUpperCase() : 'N';
  });

  constructor() {
    this.loadSettings();
  }

  loadSettings(): void {
    this.http.get<SeoSettingsDto>('/api/seo').subscribe({
      next: (data) => {
        if (data) {
          const name = data.siteName || (data as any).SiteName || this.siteName();
          this.siteName.set(name);
          localStorage.setItem('site_name', name);

          const logo = data.siteLogoUrl || (data as any).SiteLogoUrl || null;
          this.logoUrl.set(logo);
          if (logo) localStorage.setItem('site_logo_url', logo);
          else localStorage.removeItem('site_logo_url');

          const icon = data.faviconUrl || (data as any).FaviconUrl || null;
          this.faviconUrl.set(icon);
          if (icon) {
            localStorage.setItem('site_favicon_url', icon);
            this.applyFavicon(icon);
          } else {
            localStorage.removeItem('site_favicon_url');
          }

          const title = data.defaultTitle || data.defaultMetaTitle || (data as any).DefaultTitle || (data as any).DefaultMetaTitle;
          if (title) {
            this.defaultTitle.set(title);
            localStorage.setItem('site_default_title', title);
          }

          const desc = data.defaultDescription || data.defaultMetaDescription || (data as any).DefaultDescription || (data as any).DefaultMetaDescription;
          if (desc) {
            this.defaultDescription.set(desc);
            localStorage.setItem('site_default_desc', desc);
            this.applyMetaDescription(desc);
          }
        }
      },
      error: () => {}
    });

    this.http.get<any>('/api/contact').subscribe({
      next: (contact) => {
        if (contact?.copyright) {
          this.copyright.set(contact.copyright);
          localStorage.setItem('site_copyright', contact.copyright);
        }
        if (contact?.authorName) {
          this.authorName.set(contact.authorName);
          localStorage.setItem('site_author_name', contact.authorName);
        }
        if (contact?.slogan) {
          this.heroSlogan.set(contact.slogan);
          localStorage.setItem('site_hero_slogan', contact.slogan);
        }
      },
      error: () => {}
    });
  }

  updateCopyright(text: string): void {
    this.copyright.set(text);
    if (text) localStorage.setItem('site_copyright', text);
    else localStorage.removeItem('site_copyright');
  }

  updateAuthorName(name: string): void {
    this.authorName.set(name);
    if (name) localStorage.setItem('site_author_name', name);
    else localStorage.removeItem('site_author_name');
  }

  updateHeroSlogan(slogan: string): void {
    this.heroSlogan.set(slogan);
    if (slogan) localStorage.setItem('site_hero_slogan', slogan);
    else localStorage.removeItem('site_hero_slogan');
  }

  updateSettingsLocally(settings: {
    siteName?: string;
    siteLogoUrl?: string;
    faviconUrl?: string;
    defaultTitle?: string;
    defaultDescription?: string;
  }): void {
    if (settings.siteName) {
      this.siteName.set(settings.siteName);
      localStorage.setItem('site_name', settings.siteName);
    }
    if (settings.siteLogoUrl !== undefined) {
      this.logoUrl.set(settings.siteLogoUrl || null);
      if (settings.siteLogoUrl) localStorage.setItem('site_logo_url', settings.siteLogoUrl);
      else localStorage.removeItem('site_logo_url');
    }
    if (settings.faviconUrl !== undefined) {
      this.faviconUrl.set(settings.faviconUrl || null);
      if (settings.faviconUrl) {
        localStorage.setItem('site_favicon_url', settings.faviconUrl);
        this.applyFavicon(settings.faviconUrl);
      } else {
        localStorage.removeItem('site_favicon_url');
      }
    }
    if (settings.defaultTitle) {
      this.defaultTitle.set(settings.defaultTitle);
      localStorage.setItem('site_default_title', settings.defaultTitle);
    }
    if (settings.defaultDescription) {
      this.defaultDescription.set(settings.defaultDescription);
      localStorage.setItem('site_default_desc', settings.defaultDescription);
      this.applyMetaDescription(settings.defaultDescription);
    }
  }

  applyMetaDescription(desc: string): void {
    if (typeof document === 'undefined') return;
    let meta: HTMLMetaElement | null = document.querySelector("meta[name='description']");
    if (!meta) {
      meta = document.createElement('meta');
      meta.name = 'description';
      document.head.appendChild(meta);
    }
    meta.content = desc;
  }

  applyFavicon(url: string): void {
    if (typeof document === 'undefined') return;
    let link: HTMLLinkElement | null = document.querySelector("link[rel*='icon']");
    if (!link) {
      link = document.createElement('link');
      link.rel = 'icon';
      document.head.appendChild(link);
    }
    link.href = url;
  }
}
