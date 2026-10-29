import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { CategoryNode, PostDetail, PostReview, PostSummary } from '../../shared/models/post.model';

export interface PagedResult<T> {
  items: T[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface PostSummaryDto {
  id: string;
  title: string;
  slug: string;
  excerpt?: string;
  thumbnailUrl: string;
  thumbnailAlt?: string;
  categoryId: string;
  categoryName: string;
  categorySlug: string;
  tags: string[];
  status: string;
  viewCount: number;
  avgRating: number;
  ratingCount: number;
  readingTimeMinutes?: number;
  publishedAt: string;
}

export interface PostDetailDto {
  id: string;
  title: string;
  slug: string;
  excerpt?: string;
  contentHtml?: string;
  thumbnailUrl: string;
  thumbnailAlt?: string;
  ogImageUrl?: string;
  categoryId: string;
  categoryName: string;
  categorySlug: string;
  tags: { id: string; name: string; slug: string }[];
  status: string;
  viewCount: number;
  avgRating: number;
  ratingCount: number;
  readingTimeMinutes?: number;
  metaTitle?: string;
  metaDescription?: string;
  publishedAt: string;
  updatedAt: string;
}

export interface CategoryTreeDto {
  id: string;
  name: string;
  slug: string;
  level: number;
  children: CategoryTreeDto[];
}

export interface CategoryDto {
  id: string;
  name: string;
  slug: string;
  parentId?: string | null;
  level: number;
  sortOrder: number;
}

export interface TagDto {
  id: string;
  name: string;
  slug: string;
  postCount?: number;
}

export interface ProjectDto {
  id: string;
  mediaId: string;
  mediaUrl: string;
  mediaAltText?: string;
  title: string;
  shortDescription?: string;
  tag?: string;
  sortOrder: number;
}

export interface SocialLinkDto {
  platform: string;
  url: string;
  icon?: string;
}

export interface ContactInfoDto {
  email: string;
  location?: string;
  address?: string;
  phone?: string;
  bio?: string;
  about?: string;
  copyright?: string;
  facebookUrl?: string;
  githubUrl?: string;
  linkedinUrl?: string;
  youtubeUrl?: string;
  authorName?: string;
  slogan?: string;
  socialLinks?: SocialLinkDto[];
  updatedAt?: string;
}

export interface ReviewDto {
  id: string;
  postId: string;
  maskedEmail: string;
  rating: number;
  comment?: string;
  status: string;
  createdAt: string;
  canModify?: boolean;
}

export interface SendReviewOtpRequest {
  postId: string;
  email: string;
}

export interface VerifyReviewOtpRequest {
  email: string;
  otpCode: string;
}

export interface ReviewSessionDto {
  sessionId: string;
  email: string;
  expiresAt: string;
}

export interface CreateReviewDto {
  rating: number;
  comment?: string;
  sessionId?: string;
}

export interface SloganDto {
  id: string;
  content: string;
  author?: string;
  isActive: boolean;
}

export interface BannerDto {
  id: string;
  mediaId?: string;
  mediaUrl?: string;
  imageUrl?: string;
  mediaAltText?: string;
  title?: string;
  type?: string;
  linkUrl?: string;
  targetUrl?: string;
  sortOrder: number;
  isActive: boolean;
}

export function toPostSummary(dto: PostSummaryDto): PostSummary {
  return {
    id: dto.id,
    title: dto.title,
    slug: dto.slug,
    excerpt: dto.excerpt || '',
    thumbnailUrl: dto.thumbnailUrl || 'https://picsum.photos/seed/' + dto.slug + '/600/400',
    category: {
      id: dto.categoryId,
      name: dto.categoryName,
      slug: dto.categorySlug,
      parentId: null
    },
    tags: dto.tags || [],
    publishedAt: dto.publishedAt,
    readingTimeMinutes: dto.readingTimeMinutes || 3,
    viewCount: dto.viewCount || 0,
    avgRating: Number(dto.avgRating) || 0,
    ratingCount: dto.ratingCount || 0
  };
}

export function toCategoryNode(dto: CategoryTreeDto, parentId: string | null = null): CategoryNode {
  return {
    id: dto.id,
    name: dto.name,
    slug: dto.slug,
    parentId,
    children: (dto.children || []).map((c) => toCategoryNode(c, dto.id))
  };
}

@Injectable({
  providedIn: 'root'
})
export class BlogApiService {
  private http = inject(HttpClient);

  // --- Posts ---
  getFeaturedPosts(count = 6): Observable<PostSummary[]> {
    return this.http
      .get<PostSummaryDto[]>(`/api/posts/featured?count=${count}`)
      .pipe(map((items) => items.map(toPostSummary)));
  }

  getPosts(params?: {
    categorySlug?: string;
    tagSlug?: string;
    search?: string;
    page?: number;
    pageSize?: number;
  }): Observable<{ items: PostSummary[]; total: number; page: number; pageSize: number; totalPages: number }> {
    let httpParams = new HttpParams();
    if (params) {
      if (params.categorySlug && params.categorySlug !== 'tat-ca') {
        httpParams = httpParams.set('categorySlug', params.categorySlug);
      }
      if (params.tagSlug) {
        httpParams = httpParams.set('tagSlug', params.tagSlug);
      }
      if (params.search) {
        httpParams = httpParams.set('search', params.search.trim());
      }
      if (params.page) {
        httpParams = httpParams.set('page', params.page.toString());
      }
      if (params.pageSize) {
        httpParams = httpParams.set('pageSize', params.pageSize.toString());
      }
    }

    return this.http.get<PagedResult<PostSummaryDto>>('/api/posts', { params: httpParams }).pipe(
      map((res) => ({
        items: (res.items || []).map(toPostSummary),
        total: res.total || 0,
        page: res.page || 1,
        pageSize: res.pageSize || 10,
        totalPages: res.totalPages || 0
      }))
    );
  }

  getPostBySlug(slug: string, incrementView = true): Observable<PostDetailDto> {
    return this.http.get<PostDetailDto>(`/api/posts/${slug}?incrementView=${incrementView}`);
  }

  getRelatedPosts(postId: string, count = 4): Observable<PostSummary[]> {
    return this.http
      .get<PostSummaryDto[]>(`/api/posts/${postId}/related?count=${count}`)
      .pipe(map((items) => items.map(toPostSummary)));
  }

  // --- Categories ---
  getCategoryTree(): Observable<CategoryNode[]> {
    return this.http
      .get<CategoryTreeDto[]>('/api/categories/tree')
      .pipe(map((tree) => tree.map((node) => toCategoryNode(node))));
  }

  getAllCategories(): Observable<CategoryDto[]> {
    return this.http.get<CategoryDto[]>('/api/categories');
  }

  getCategoryBySlug(slug: string): Observable<CategoryDto> {
    return this.http.get<CategoryDto>(`/api/categories/${slug}`);
  }

  // --- Tags ---
  getTags(page = 1, pageSize = 50): Observable<PagedResult<TagDto>> {
    return this.http.get<PagedResult<TagDto>>(`/api/tags?page=${page}&pageSize=${pageSize}`);
  }

  // --- Projects ---
  getProjects(): Observable<ProjectDto[]> {
    return this.http.get<ProjectDto[]>('/api/projects');
  }

  // --- Contact ---
  getContactInfo(): Observable<ContactInfoDto> {
    return this.http.get<ContactInfoDto>('/api/contact');
  }

  // --- Subscribers ---
  subscribe(email: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>('/api/subscribers', { email });
  }

  unsubscribe(email: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>('/api/subscribers/unsubscribe', { email });
  }

  // --- OTP & Reviews ---
  sendReviewOtp(postId: string, email: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>('/api/otp/send-review-otp', { postId, email });
  }

  verifyReviewOtp(email: string, otpCode: string): Observable<ReviewSessionDto> {
    return this.http.post<ReviewSessionDto>('/api/otp/verify-review-otp', { email, otpCode });
  }

  getPostReviews(postId: string, page = 1, pageSize = 20, sessionId?: string): Observable<PagedResult<ReviewDto>> {
    let params = new HttpParams().set('page', page.toString()).set('pageSize', pageSize.toString());
    let headers: { [header: string]: string } = {};
    if (sessionId) {
      params = params.set('sessionId', sessionId);
      headers['X-Review-Session'] = sessionId;
    }
    return this.http.get<PagedResult<ReviewDto>>(`/api/posts/${postId}/reviews`, { params, headers });
  }

  createReview(postId: string, dto: CreateReviewDto, sessionId?: string): Observable<ReviewDto> {
    let headers: { [header: string]: string } = {};
    if (sessionId) {
      headers['X-Review-Session'] = sessionId;
    }
    return this.http.post<ReviewDto>(`/api/posts/${postId}/reviews`, dto, { headers });
  }

  updateReview(reviewId: string, dto: { rating: number; comment?: string }, sessionId?: string): Observable<ReviewDto> {
    let headers: { [header: string]: string } = {};
    if (sessionId) {
      headers['X-Review-Session'] = sessionId;
    }
    return this.http.put<ReviewDto>(`/api/reviews/${reviewId}`, { ...dto, sessionId }, { headers });
  }

  deleteReview(reviewId: string, sessionId?: string): Observable<any> {
    let headers: { [header: string]: string } = {};
    let params = new HttpParams();
    if (sessionId) {
      headers['X-Review-Session'] = sessionId;
      params = params.set('sessionId', sessionId);
    }
    return this.http.delete(`/api/reviews/${reviewId}`, { headers, params, responseType: 'text' as 'json' });
  }

  // --- Cookie Consent ---
  recordCookieConsent(visitorToken: string, choice: 'Accepted' | 'Declined'): Observable<any> {
    return this.http.post('/api/cookie-consent', { visitorToken, choice });
  }

  // --- Ads ---
  getActiveAd(position: string): Observable<any> {
    return this.http.get(`/api/ads/${position}`);
  }

  // --- Banners & Slogans ---
  getActiveSlogan(): Observable<SloganDto | null> {
    return this.http.get<SloganDto | null>('/api/slogans/active');
  }

  getActiveBanners(): Observable<BannerDto[]> {
    return this.http.get<BannerDto[]>('/api/banners');
  }
}
