import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AdminApiService, PostDto } from '../../../core/services/admin-api.service';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { forkJoin, catchError, of } from 'rxjs';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, ButtonModule, TableModule, CardModule, TagModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  private api = inject(AdminApiService);

  isLoading = signal(true);
  totalPosts = signal(0);
  totalViews = signal(0);
  pendingReviewsCount = signal(0);
  subscribersCount = signal(0);

  recentPosts = signal<PostDto[]>([]);
  recentReviews = signal<any[]>([]);

  ngOnInit(): void {
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    this.isLoading.set(true);

    forkJoin({
      stats: this.api.getPostsStats().pipe(catchError(() => of({ totalPosts: 0, totalViews: 0 }))),
      posts: this.api.getPosts({ page: 1, pageSize: 5 }).pipe(catchError(() => of({ items: [], total: 0 }))),
      reviewStats: this.api.getReviewStats().pipe(catchError(() => of({ pendingCount: 0, visibleCount: 0, flaggedCount: 0, totalCount: 0 }))),
      pendingReviews: this.api.getReviews({ page: 1, pageSize: 5, status: 'Hidden' }).pipe(catchError(() => of({ items: [], total: 0 }))),
      subscribers: this.api.getSubscribers({ page: 1, pageSize: 1 }).pipe(catchError(() => of({ items: [], total: 0 })))
    }).subscribe({
      next: (res) => {
        // 1. Posts & Views Stats
        const postItems = Array.isArray(res.posts) ? res.posts : (res.posts?.items || []);
        this.recentPosts.set(postItems);

        const tPosts = res.stats?.totalPosts ?? (res.posts?.total || postItems.length);
        this.totalPosts.set(tPosts);

        const tViews = res.stats?.totalViews ?? postItems.reduce((acc: number, p: PostDto) => acc + (p.viewCount || 0), 0);
        this.totalViews.set(tViews);

        // 2. Reviews Stats
        const reviewItems = Array.isArray(res.pendingReviews) ? res.pendingReviews : (res.pendingReviews?.items || []);
        this.recentReviews.set(reviewItems);
        this.pendingReviewsCount.set(res.reviewStats?.pendingCount ?? res.pendingReviews?.total ?? reviewItems.length);

        // 3. Subscribers Stats
        const tSubscribers = res.subscribers?.total ?? (Array.isArray(res.subscribers) ? res.subscribers.length : 0);
        this.subscribersCount.set(tSubscribers);

        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }

  approveReview(review: any): void {
    if (!review?.id) return;
    this.api.updateReviewStatus(review.id, 'Visible').subscribe({
      next: () => {
        this.loadDashboardData();
      }
    });
  }
}
