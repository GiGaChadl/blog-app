import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AdminApiService, PostDto, CategoryDto } from '../../../core/services/admin-api.service';
import { extractErrorMessage } from '../../../core/utils/error-formatter';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { DropdownModule } from 'primeng/dropdown';
import { TagModule } from 'primeng/tag';
import { ConfirmationService, MessageService } from 'primeng/api';

@Component({
  selector: 'app-post-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    FormsModule,
    TableModule,
    ButtonModule,
    InputTextModule,
    DropdownModule,
    TagModule
  ],
  templateUrl: './post-list.component.html',
  styleUrl: './post-list.component.scss'
})
export class PostListComponent implements OnInit {
  private api = inject(AdminApiService);
  private router = inject(Router);
  private confirmationService = inject(ConfirmationService);
  private messageService = inject(MessageService);

  posts = signal<PostDto[]>([]);
  categories = signal<CategoryDto[]>([]);
  isLoading = signal(false);
  totalRecords = signal(0);

  // Filters
  searchTerm = '';
  selectedStatus: string | null = null;
  selectedCategoryId: string | number | null = null;
  page = 1;
  pageSize = 10;

  statusOptions = [
    { label: 'Tất cả trạng thái', value: null },
    { label: 'Xuất bản (Hiển thị)', value: 'Published' },
    { label: 'Đã ẩn bài viết', value: 'Hidden' }
  ];

  ngOnInit(): void {
    this.loadCategories();
    this.loadPosts();
  }

  loadCategories(): void {
    this.api.getCategories().subscribe({
      next: (res) => {
        this.categories.set(res || []);
      },
      error: (err) => {
        const msg = extractErrorMessage(err, 'Không thể tải danh sách chuyên mục.');
        this.messageService.add({ severity: 'error', summary: 'Lỗi', detail: msg });
      }
    });
  }

  loadPosts(): void {
    this.isLoading.set(true);
    this.api.getPosts({
      page: this.page,
      pageSize: this.pageSize,
      status: this.selectedStatus || undefined,
      categoryId: this.selectedCategoryId || undefined,
      search: this.searchTerm.trim() || undefined
    }).subscribe({
      next: (res) => {
        const items = Array.isArray(res) ? res : (res?.items || []);
        this.posts.set(items);
        this.totalRecords.set(res?.totalCount || items.length);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.isLoading.set(false);
        const msg = extractErrorMessage(err, 'Không thể tải danh sách bài viết.');
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: msg
        });
      }
    });
  }

  onSearch(): void {
    this.page = 1;
    this.loadPosts();
  }

  onFilterChange(): void {
    this.page = 1;
    this.loadPosts();
  }

  onEdit(post: PostDto): void {
    this.router.navigate(['/admin/posts/edit', post.id]);
  }

  onDelete(post: PostDto): void {
    this.confirmationService.confirm({
      message: `Bạn có chắc chắn muốn xóa bài viết "${post.title}"? Thao tác này không thể hoàn tác.`,
      header: 'Xác nhận xóa bài viết',
      icon: 'pi pi-trash',
      acceptButtonStyleClass: 'p-button-danger',
      acceptLabel: 'Xóa',
      rejectLabel: 'Hủy',
      accept: () => {
        this.api.deletePost(post.id).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'Thành công',
              detail: 'Đã xóa bài viết thành công.'
            });
            this.loadPosts();
          },
          error: (err) => {
            const msg = extractErrorMessage(err, 'Không thể xóa bài viết.');
            this.messageService.add({
              severity: 'error',
              summary: 'Lỗi xóa bài viết',
              detail: msg
            });
          }
        });
      }
    });
  }
}
