import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';

// --- Interfaces ---
export interface PostDto {
  id: string | number;
  title: string;
  slug: string;
  summary?: string;
  excerpt?: string;
  content: string;
  contentHtml?: string;
  categoryId: string | number;
  categoryName?: string;
  thumbnailMediaId?: string | number;
  thumbnailUrl?: string;
  ogImageMediaId?: string | number;
  ogImageUrl?: string;
  status: string; // 'Published' | 'Hidden'
  viewCount?: number;
  ratingAverage?: number;
  avgRating?: number;
  ratingCount?: number;
  readingTimeMinutes?: number;
  metaTitle?: string;
  metaDescription?: string;
  metaKeywords?: string;
  canonicalUrl?: string;
  publishedAt?: string;
  createdAt?: string;
  updatedAt?: string;
  tagIds?: (string | number)[];
  tags?: any[];
}

export interface PostCreateUpdateDto {
  title: string;
  slug: string;
  summary?: string;
  excerpt?: string;
  content: string;
  contentHtml?: string;
  categoryId: string | number;
  thumbnailMediaId?: string | number | null;
  ogImageMediaId?: string | number | null;
  status: string;
  tagIds?: (string | number)[];
  metaTitle?: string;
  metaDescription?: string;
  metaKeywords?: string;
  canonicalUrl?: string;
}

export interface CategoryDto {
  id: string | number;
  name: string;
  slug: string;
  description?: string;
  parentId?: string | number | null;
  parentName?: string;
  level: number;
  sortOrder: number;
  isActive: boolean;
  children?: CategoryDto[];
}

export interface TagDto {
  id: string | number;
  name: string;
  slug: string;
  postCount?: number;
}

export interface MediaLibraryDto {
  id: string | number;
  fileName: string;
  fileUrl: string;
  url?: string;
  fileType?: string;
  fileSizeBytes?: number;
  width?: number;
  height?: number;
  createdAt?: string;
  uploadedAt?: string;
}

export interface ReviewDto {
  id: string | number;
  postId: string | number;
  postTitle?: string;
  email?: string;
  authorName?: string;
  authorEmail?: string;
  rating: number;
  comment?: string;
  status: string; // 'Visible' | 'Hidden' | 'Flagged'
  createdAt: string;
  updatedAt?: string;
}

export interface SubscriberDto {
  id: string | number;
  email: string;
  status?: string | number;
  isConfirmed?: boolean;
  isActive?: boolean;
  confirmedAt?: string | null;
  unsubscribedAt?: string | null;
  createdAt: string;
}

export interface BannerDto {
  id: string;
  mediaId: string;
  mediaUrl?: string;
  imageUrl?: string;
  mediaAltText?: string;
  title?: string;
  type?: string;
  linkUrl?: string;
  targetUrl?: string;
  sortOrder: number;
  isActive: boolean;
  createdAt?: string;
}

export interface SloganDto {
  id: number;
  content: string;
  author?: string;
  isActive: boolean;
  createdAt?: string;
}

export interface AdDto {
  id?: string | number;
  position: string; // 'HomeMid' | 'PostDetail'
  adsenseSlotId: string;
  startDate?: string | null;
  endDate?: string | null;
  isActive: boolean;
  adCode?: string;
}

export interface ProjectDto {
  id: string;
  mediaId: string;
  mediaUrl?: string;
  mediaAltText?: string;
  title: string;
  shortDescription?: string;
  tag?: string;
  sortOrder: number;
}

export interface ContactInfoDto {
  id?: number;
  email: string;
  phone?: string;
  address?: string;
  location?: string;
  facebookUrl?: string;
  githubUrl?: string;
  linkedinUrl?: string;
  youtubeUrl?: string;
  bio?: string;
  about?: string;
  copyright?: string;
  authorName?: string;
  slogan?: string;
  socialLinks?: { platform: string; url: string; icon?: string }[];
}

export interface SeoSettingsDto {
  id?: number;
  siteName: string;
  defaultTitle: string;
  defaultMetaTitle?: string;
  defaultDescription?: string;
  defaultMetaDescription?: string;
  defaultKeywords?: string;
  defaultOgImage?: string;
  googleSiteVerification?: string;
  robotsTxtContent?: string;
  siteLogoUrl?: string;
  faviconUrl?: string;
}

export interface SmtpSettingsDto {
  id?: number;
  host: string;
  port: number;
  username: string;
  password?: string;
  enableSsl: boolean;
  senderEmail: string;
  senderName: string;
}

export interface MonthlyAnalyticsDto {
  year: number;
  month: number;
  totalViews: number;
  uniqueVisitors: number;
  topPosts: { postId: number; title: string; views: number }[];
}

@Injectable({
  providedIn: 'root'
})
export class AdminApiService {
  private http = inject(HttpClient);

  // --- Posts ---
  getPosts(params?: { page?: number; pageSize?: number; status?: string; categoryId?: string | number; search?: string }): Observable<any> {
    let httpParams = new HttpParams();
    if (params) {
      if (params.page) httpParams = httpParams.set('page', params.page.toString());
      if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());
      if (params.status) httpParams = httpParams.set('status', params.status);
      if (params.categoryId) httpParams = httpParams.set('categoryId', params.categoryId.toString());
      if (params.search) httpParams = httpParams.set('search', params.search);
    }
    return this.http.get('/api/posts', { params: httpParams });
  }

  getPostsStats(): Observable<{ totalPosts: number; totalViews: number }> {
    return this.http.get<{ totalPosts: number; totalViews: number }>('/api/posts/stats');
  }

  getPostById(id: string | number): Observable<PostDto> {
    return this.http.get<PostDto>(`/api/posts/${id}`);
  }

  createPost(dto: PostCreateUpdateDto): Observable<PostDto> {
    return this.http.post<PostDto>('/api/posts', dto);
  }

  updatePost(id: string | number, dto: PostCreateUpdateDto): Observable<PostDto> {
    return this.http.put<PostDto>(`/api/posts/${id}`, dto);
  }

  deletePost(id: string | number): Observable<any> {
    return this.http.delete(`/api/posts/${id}`);
  }

  // --- Categories ---
  getCategories(): Observable<CategoryDto[]> {
    return this.http.get<CategoryDto[]>('/api/categories');
  }

  createCategory(dto: Partial<CategoryDto>): Observable<CategoryDto> {
    return this.http.post<CategoryDto>('/api/categories', dto);
  }

  updateCategory(id: string | number, dto: Partial<CategoryDto>): Observable<CategoryDto> {
    return this.http.put<CategoryDto>(`/api/categories/${id}`, dto);
  }

  deleteCategory(id: string | number): Observable<any> {
    return this.http.delete(`/api/categories/${id}`);
  }

  // --- Tags ---
  getTags(page: number = 1, pageSize: number = 100): Observable<TagDto[]> {
    const params = new HttpParams().set('page', page.toString()).set('pageSize', pageSize.toString());
    return this.http.get<any>('/api/tags', { params }).pipe(
      map(res => Array.isArray(res) ? res : (res?.items || []))
    );
  }

  createTag(dto: { name: string; slug: string }): Observable<TagDto> {
    return this.http.post<TagDto>('/api/tags', dto);
  }

  updateTag(id: string | number, dto: { name: string; slug: string }): Observable<TagDto> {
    return this.http.put<TagDto>(`/api/tags/${id}`, dto);
  }

  deleteTag(id: string | number): Observable<any> {
    return this.http.delete(`/api/tags/${id}`);
  }

  // --- Media Library ---
  getMedia(page: number = 1, pageSize: number = 24, search?: string): Observable<any> {
    let params = new HttpParams().set('page', page.toString()).set('pageSize', pageSize.toString());
    if (search) params = params.set('search', search);
    return this.http.get<any>('/api/media', { params }).pipe(
      map(res => {
        const mapItem = (m: any) => ({
          ...m,
          fileUrl: m.url || m.fileUrl,
          url: m.url || m.fileUrl
        });
        if (Array.isArray(res)) {
          return res.map(mapItem);
        }
        if (res && Array.isArray(res.items)) {
          return {
            ...res,
            items: res.items.map(mapItem)
          };
        }
        return res;
      })
    );
  }

  uploadMedia(file: File): Observable<MediaLibraryDto> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<MediaLibraryDto>('/api/media/upload', formData).pipe(
      map((m: any) => ({
        ...m,
        fileUrl: m.url || m.fileUrl,
        url: m.url || m.fileUrl
      }))
    );
  }

  deleteMedia(id: string | number): Observable<any> {
    return this.http.delete(`/api/media/${id}`);
  }

  // --- Reviews ---
  getReviews(params?: { status?: string; page?: number; pageSize?: number }): Observable<any> {
    let httpParams = new HttpParams();
    if (params?.status) httpParams = httpParams.set('status', params.status);
    if (params?.page) httpParams = httpParams.set('page', params.page.toString());
    if (params?.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());
    return this.http.get('/api/admin/reviews', { params: httpParams });
  }

  updateReviewStatus(id: string | number, status: string): Observable<any> {
    return this.http.patch(`/api/admin/reviews/${id}/status`, { status }, { responseType: 'text' as 'json' });
  }

  deleteReview(id: string | number): Observable<any> {
    return this.http.delete(`/api/admin/reviews/${id}`, { responseType: 'text' as 'json' });
  }

  getReviewStats(): Observable<{ pendingCount: number; visibleCount: number; flaggedCount: number; totalCount: number }> {
    return this.http.get<{ pendingCount: number; visibleCount: number; flaggedCount: number; totalCount: number }>('/api/admin/reviews/stats');
  }

  // --- Subscribers ---
  getSubscribers(params?: { page?: number; pageSize?: number }): Observable<any> {
    let httpParams = new HttpParams();
    if (params?.page) httpParams = httpParams.set('page', params.page.toString());
    if (params?.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());
    return this.http.get('/api/admin/subscribers', { params: httpParams });
  }

  deleteSubscriber(id: string | number): Observable<any> {
    return this.http.delete(`/api/admin/subscribers/${id}`);
  }

  // --- Banners ---
  getBanners(): Observable<BannerDto[]> {
    return this.http.get<BannerDto[]>('/api/admin/banners');
  }

  createBanner(dto: any): Observable<BannerDto> {
    return this.http.post<BannerDto>('/api/admin/banners', dto);
  }

  updateBanner(id: string, dto: any): Observable<BannerDto> {
    return this.http.put<BannerDto>(`/api/admin/banners/${id}`, dto);
  }

  deleteBanner(id: string): Observable<any> {
    return this.http.delete(`/api/admin/banners/${id}`);
  }

  // --- Slogans ---
  getSlogans(): Observable<SloganDto[]> {
    return this.http.get<SloganDto[]>('/api/admin/slogans');
  }

  createSlogan(dto: Partial<SloganDto>): Observable<SloganDto> {
    return this.http.post<SloganDto>('/api/admin/slogans', dto);
  }

  updateSlogan(id: number, dto: Partial<SloganDto>): Observable<SloganDto> {
    return this.http.put<SloganDto>(`/api/admin/slogans/${id}`, dto);
  }

  deleteSlogan(id: number): Observable<any> {
    return this.http.delete(`/api/admin/slogans/${id}`);
  }

  // --- Ads ---
  getAds(): Observable<AdDto[]> {
    return this.http.get<AdDto[]>('/api/admin/ads');
  }

  updateAd(position: string, dto: { adsenseSlotId: string; isActive: boolean; startDate?: string | null; endDate?: string | null; adCode?: string }): Observable<AdDto> {
    return this.http.put<AdDto>(`/api/admin/ads/${position}`, {
      adsenseSlotId: dto.adsenseSlotId,
      isActive: dto.isActive,
      startDate: dto.startDate,
      endDate: dto.endDate
    });
  }

  // --- Projects ---
  getProjects(): Observable<ProjectDto[]> {
    return this.http.get<ProjectDto[]>('/api/admin/projects');
  }

  createProject(dto: Partial<ProjectDto>): Observable<ProjectDto> {
    return this.http.post<ProjectDto>('/api/admin/projects', dto);
  }

  updateProject(id: string, dto: Partial<ProjectDto>): Observable<ProjectDto> {
    return this.http.put<ProjectDto>(`/api/admin/projects/${id}`, dto);
  }

  deleteProject(id: string): Observable<any> {
    return this.http.delete(`/api/admin/projects/${id}`);
  }

  // --- Settings: Contact Info ---
  getContactInfo(): Observable<ContactInfoDto> {
    return this.http.get<ContactInfoDto>('/api/contact');
  }

  updateContactInfo(dto: ContactInfoDto): Observable<ContactInfoDto> {
    return this.http.put<ContactInfoDto>('/api/admin/contact', dto);
  }

  // --- Settings: SEO ---
  getSeoSettings(): Observable<SeoSettingsDto> {
    return this.http.get<SeoSettingsDto>('/api/seo');
  }

  updateSeoSettings(dto: SeoSettingsDto): Observable<SeoSettingsDto> {
    return this.http.put<SeoSettingsDto>('/api/admin/seo', dto);
  }

  // --- Settings: SMTP ---
  getSmtpSettings(): Observable<SmtpSettingsDto> {
    return this.http.get<SmtpSettingsDto>('/api/admin/smtp');
  }

  updateSmtpSettings(dto: SmtpSettingsDto): Observable<SmtpSettingsDto> {
    return this.http.put<SmtpSettingsDto>('/api/admin/smtp', dto);
  }

  testSmtp(toEmail: string): Observable<any> {
    return this.http.post('/api/admin/smtp/test', { toEmail, testRecipientEmail: toEmail });
  }

  // --- Analytics ---
  getMonthlyAnalytics(): Observable<MonthlyAnalyticsDto> {
    return this.http.get<MonthlyAnalyticsDto>('/api/admin/analytics/monthly');
  }
}
