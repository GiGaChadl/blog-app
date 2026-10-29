import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Title, Meta } from '@angular/platform-browser';
import { AdSlotComponent } from '../../shared/components/ad-slot/ad-slot.component';
import { PostCardComponent } from '../../shared/components/post-card/post-card.component';
import { StarRatingComponent } from '../../shared/components/star-rating/star-rating.component';
import { OtpReviewFormComponent, NewReviewSubmit } from '../../shared/components/otp-review-form/otp-review-form.component';
import { CategorySidebarComponent } from '../../shared/components/category-sidebar/category-sidebar.component';
import { PostDetail, PostReview, PostSummary, Category } from '../../shared/models/post.model';
import { BlogApiService } from '../../core/services/blog-api.service';

@Component({
  selector: 'app-post-detail',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    FormsModule,
    AdSlotComponent,
    PostCardComponent,
    StarRatingComponent,
    OtpReviewFormComponent,
    CategorySidebarComponent
  ],
  templateUrl: './post-detail.component.html',
  styleUrl: './post-detail.component.scss'
})
export class PostDetailComponent {
  private blogApi = inject(BlogApiService);
  private route = inject(ActivatedRoute);
  private titleService = inject(Title);
  private meta = inject(Meta);

  post = signal<PostDetail | null>(null);
  notFound = signal(false);
  reviews = signal<PostReview[]>([]);
  categoryBreadcrumb = signal<Category[]>([]);
  relatedPosts = signal<PostSummary[]>([]);

  // State chỉnh sửa review
  editingReviewId = signal<string | null>(null);
  editRating = signal<number>(5);
  editComment = signal<string>('');
  isSubmittingEdit = signal<boolean>(false);
  editError = signal<string>('');

  constructor() {
    this.route.paramMap.subscribe((params) => {
      const slug = params.get('slug');
      this.loadPost(slug);
    });
  }

  private loadPost(slug: string | null) {
    if (!slug) {
      this.notFound.set(true);
      this.post.set(null);
      return;
    }

    this.blogApi.getPostBySlug(slug, true).subscribe({
      next: (dto) => {
        const detail: PostDetail = {
          id: dto.id,
          title: dto.title,
          slug: dto.slug,
          excerpt: dto.excerpt || '',
          thumbnailUrl: dto.thumbnailUrl || 'https://picsum.photos/seed/' + dto.slug + '/800/500',
          category: {
            id: dto.categoryId,
            name: dto.categoryName,
            slug: dto.categorySlug,
            parentId: null
          },
          tags: (dto.tags || []).map((t) => t.name),
          publishedAt: dto.publishedAt,
          readingTimeMinutes: dto.readingTimeMinutes || 3,
          viewCount: dto.viewCount || 0,
          avgRating: Number(dto.avgRating) || 0,
          ratingCount: dto.ratingCount || 0,
          contentHtml: dto.contentHtml || '<p>Nội dung bài viết đang được cập nhật.</p>',
          reviews: []
        };

        this.post.set(detail);
        this.notFound.set(false);
        this.categoryBreadcrumb.set([detail.category]);

        // SEO / Open Graph
        this.titleService.setTitle(`${detail.title} — Nhật Ký Của Tôi`);
        this.meta.updateTag({ name: 'description', content: detail.excerpt });
        this.meta.updateTag({ property: 'og:title', content: detail.title });
        this.meta.updateTag({ property: 'og:description', content: detail.excerpt });
        this.meta.updateTag({ property: 'og:image', content: detail.thumbnailUrl });
        this.meta.updateTag({ property: 'og:type', content: 'article' });

        // Tải bài viết liên quan
        this.blogApi.getRelatedPosts(dto.id, 3).subscribe({
          next: (related) => this.relatedPosts.set(related),
          error: () => this.relatedPosts.set([])
        });

        // Tải danh sách đánh giá
        this.loadReviews(dto.id);
      },
      error: () => {
        this.notFound.set(true);
        this.post.set(null);
      }
    });
  }

  loadReviews(postId: string): void {
    const sessionId = this.getSessionId();
    this.blogApi.getPostReviews(postId, 1, 20, sessionId).subscribe({
      next: (res) => {
        const myReviewIds = this.getMyReviewIds();
        const mapped: PostReview[] = (res.items || []).map((r) => {
          const isOwned = r.canModify === true || myReviewIds.includes(r.id);
          return {
            id: r.id,
            displayName: r.maskedEmail || 'Bạn đọc',
            rating: r.rating,
            comment: r.comment || '',
            createdAt: r.createdAt,
            isOwnedByCurrentSession: isOwned
          };
        });
        this.reviews.set(mapped);
      },
      error: () => this.reviews.set([])
    });
  }

  private getSessionId(): string | undefined {
    if (typeof window === 'undefined') return undefined;
    return window.sessionStorage.getItem('review_session_id') ||
           window.localStorage.getItem('review_session_id') ||
           undefined;
  }

  private getMyReviewIds(): string[] {
    if (typeof window === 'undefined') return [];
    try {
      const raw = window.localStorage.getItem('my_review_ids');
      if (!raw) return [];
      const items: { id: string; time: number }[] = JSON.parse(raw);
      // Chỉ giữ các đánh giá gửi trong vòng 24 giờ
      const valid = items.filter((x) => Date.now() - x.time <= 24 * 60 * 60 * 1000);
      return valid.map((x) => x.id);
    } catch {
      return [];
    }
  }

  private addMyReviewId(id: string): void {
    if (typeof window === 'undefined') return;
    try {
      const raw = window.localStorage.getItem('my_review_ids');
      const items: { id: string; time: number }[] = raw ? JSON.parse(raw) : [];
      items.push({ id, time: Date.now() });
      window.localStorage.setItem('my_review_ids', JSON.stringify(items));
    } catch {}
  }

  formattedDate(iso: string): string {
    return new Date(iso).toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' });
  }

  reviewPendingMsg = signal('');

  onReviewSubmitted(payload: NewReviewSubmit) {
    const current = this.post();
    if (!current) return;

    const sessionId = payload.sessionId || this.getSessionId();
    this.blogApi
      .createReview(
        current.id,
        { rating: payload.rating, comment: payload.comment },
        sessionId
      )
      .subscribe({
        next: (created) => {
          if (created?.id) {
            this.addMyReviewId(created.id);
          }
          this.reviewPendingMsg.set(
            'Cảm ơn bạn! Đánh giá của bạn đã được gửi thành công và đang chờ quản trị viên duyệt trước khi hiển thị công khai.'
          );
          this.loadReviews(current.id);
          this.refreshPostStats(current.slug);
        },
        error: () => {
          this.loadReviews(current.id);
        }
      });
  }

  startEdit(review: PostReview): void {
    this.editingReviewId.set(review.id);
    this.editRating.set(review.rating);
    this.editComment.set(review.comment);
    this.editError.set('');
  }

  cancelEdit(): void {
    this.editingReviewId.set(null);
    this.editError.set('');
  }

  saveEdit(reviewId: string): void {
    const current = this.post();
    if (!current) return;

    if (this.editRating() < 1 || this.editRating() > 5) {
      this.editError.set('Vui lòng chọn số sao từ 1 đến 5.');
      return;
    }

    this.isSubmittingEdit.set(true);
    this.editError.set('');

    const sessionId = this.getSessionId();
    this.blogApi
      .updateReview(
        reviewId,
        { rating: this.editRating(), comment: this.editComment() },
        sessionId
      )
      .subscribe({
        next: () => {
          this.isSubmittingEdit.set(false);
          this.editingReviewId.set(null);
          this.loadReviews(current.id);
          this.refreshPostStats(current.slug);
        },
        error: (err) => {
          this.isSubmittingEdit.set(false);
          this.editError.set(err?.error?.message || 'Không thể chỉnh sửa đánh giá. Đã quá 24h hoặc phiên hết hạn.');
        }
      });
  }

  deleteReview(reviewId: string): void {
    const current = this.post();
    if (!current) return;

    if (!confirm('Bạn có chắc chắn muốn xoá đánh giá của mình không?')) {
      return;
    }

    const sessionId = this.getSessionId();
    this.blogApi.deleteReview(reviewId, sessionId).subscribe({
      next: () => {
        this.reviews.update((list) => list.filter((r) => r.id !== reviewId));
        this.refreshPostStats(current.slug);
      },
      error: (err) => {
        alert(err?.error?.message || 'Không thể xoá đánh giá. Đã quá 24h hoặc phiên hết hạn.');
      }
    });
  }

  private refreshPostStats(slug: string): void {
    this.blogApi.getPostBySlug(slug, false).subscribe({
      next: (detail) => {
        const cur = this.post();
        if (cur && detail) {
          this.post.set({
            ...cur,
            avgRating: Number(detail.avgRating) || 0,
            ratingCount: detail.ratingCount || 0
          });
        }
      }
    });
  }

  shareUrl(platform: 'facebook' | 'linkedin' | 'zalo'): string {
    const urls: Record<string, string> = {
      facebook: 'https://www.facebook.com/sharer/sharer.php?u=',
      linkedin: 'https://www.linkedin.com/sharing/share-offsite/?url=',
      zalo: 'https://zalo.me/share?u='
    };
    return urls[platform];
  }
}

