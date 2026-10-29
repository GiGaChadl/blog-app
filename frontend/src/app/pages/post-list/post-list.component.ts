import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MultiSelectModule } from 'primeng/multiselect';
import { PaginatorModule, PaginatorState } from 'primeng/paginator';
import { InputTextModule } from 'primeng/inputtext';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { PostCardComponent } from '../../shared/components/post-card/post-card.component';
import { CategorySidebarComponent } from '../../shared/components/category-sidebar/category-sidebar.component';
import { PostSummary } from '../../shared/models/post.model';
import { BlogApiService } from '../../core/services/blog-api.service';

const PAGE_SIZE = 9;

@Component({
  selector: 'app-post-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    MultiSelectModule,
    PaginatorModule,
    InputTextModule,
    IconFieldModule,
    InputIconModule,
    PostCardComponent,
    CategorySidebarComponent
  ],
  templateUrl: './post-list.component.html',
  styleUrl: './post-list.component.scss'
})
export class PostListComponent implements OnInit {
  private blogApi = inject(BlogApiService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  allTags = signal<{ label: string; value: string }[]>([]);
  categorySlug = signal<string>('tat-ca');
  currentCategoryName = signal<string>('Tất Cả Bài Viết');
  selectedTagSlugs: string[] = [];
  searchQuery: string = '';
  first = signal(0);
  pageSize = PAGE_SIZE;

  posts = signal<PostSummary[]>([]);
  totalRecords = signal(0);
  isLoading = signal(false);

  ngOnInit(): void {
    this.loadTags();

    this.route.data.subscribe((data) => {
      if (data && data['isSearch']) {
        this.currentCategoryName.set('Tìm Kiếm Bài Viết');
      }
    });

    this.route.paramMap.subscribe((params) => {
      const slug = params.get('slug');
      if (slug) {
        this.categorySlug.set(slug);
        this.resolveCategoryName(slug);
      } else {
        this.categorySlug.set('tat-ca');
      }
      this.first.set(0);
      this.loadPosts();
    });

    this.route.queryParamMap.subscribe((qParams) => {
      const q = qParams.get('q');
      const tag = qParams.get('tag');
      let changed = false;

      if (q !== null && q !== this.searchQuery) {
        this.searchQuery = q;
        changed = true;
      }
      if (tag && !this.selectedTagSlugs.includes(tag)) {
        this.selectedTagSlugs = [tag];
        changed = true;
      }

      if (changed) {
        this.first.set(0);
        this.loadPosts();
      }
    });
  }

  loadTags(): void {
    this.blogApi.getTags(1, 100).subscribe({
      next: (res) => {
        this.allTags.set((res.items || []).map((t) => ({ label: t.name, value: t.slug })));
      },
      error: () => this.allTags.set([])
    });
  }

  resolveCategoryName(slug: string): void {
    if (slug === 'tat-ca') {
      this.currentCategoryName.set('Tất Cả Bài Viết');
      return;
    }
    this.blogApi.getCategoryBySlug(slug).subscribe({
      next: (cat) => this.currentCategoryName.set(cat.name),
      error: () => this.currentCategoryName.set(slug)
    });
  }

  loadPosts(): void {
    this.isLoading.set(true);
    const page = Math.floor(this.first() / this.pageSize) + 1;
    const catSlug = this.categorySlug();
    const tagSlug = this.selectedTagSlugs.length > 0 ? this.selectedTagSlugs[0] : undefined;
    const search = this.searchQuery ? this.searchQuery.trim() : undefined;

    this.blogApi
      .getPosts({
        categorySlug: catSlug === 'tat-ca' ? undefined : catSlug,
        tagSlug,
        search: search || undefined,
        page,
        pageSize: this.pageSize
      })
      .subscribe({
        next: (res) => {
          this.posts.set(res.items);
          this.totalRecords.set(res.total);
          this.isLoading.set(false);
        },
        error: () => {
          this.posts.set([]);
          this.totalRecords.set(0);
          this.isLoading.set(false);
        }
      });
  }

  onPageChange(e: PaginatorState): void {
    this.first.set(e.first ?? 0);
    this.loadPosts();
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  onFiltersChange(): void {
    this.first.set(0);
    this.loadPosts();
  }

  clearFilters(): void {
    this.selectedTagSlugs = [];
    this.searchQuery = '';
    this.first.set(0);
    this.router.navigate(['/danh-muc', 'tat-ca']);
  }
}
