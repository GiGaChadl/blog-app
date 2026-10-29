import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminApiService, ReviewDto } from '../../../core/services/admin-api.service';
import { extractErrorMessage } from '../../../core/utils/error-formatter';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { DropdownModule } from 'primeng/dropdown';
import { TagModule } from 'primeng/tag';
import { ConfirmationService, MessageService } from 'primeng/api';

@Component({
  selector: 'app-reviews-admin',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TableModule,
    ButtonModule,
    DropdownModule,
    TagModule
  ],
  templateUrl: './reviews-admin.component.html',
  styleUrl: './reviews-admin.component.scss'
})
export class ReviewsAdminComponent implements OnInit {
  private api = inject(AdminApiService);
  private confirmationService = inject(ConfirmationService);
  private messageService = inject(MessageService);

  reviews = signal<ReviewDto[]>([]);
  isLoading = signal(false);
  totalRecords = signal(0);

  selectedStatus: string = 'all';
  page = 1;
  pageSize = 10;

  statusOptions = [
    { label: 'Tất cả trạng thái', value: 'all' },
    { label: 'Chờ duyệt / Đã ẩn (Hidden)', value: 'Hidden' },
    { label: 'Đang hiển thị (Visible)', value: 'Visible' },
    { label: 'Bị gắn cờ (Flagged)', value: 'Flagged' }
  ];

  ngOnInit(): void {
    this.loadReviews();
  }

  loadReviews(): void {
    this.isLoading.set(true);
    const statusParam = this.selectedStatus === 'all' ? undefined : this.selectedStatus;

    this.api.getReviews({
      status: statusParam,
      page: this.page,
      pageSize: this.pageSize
    }).subscribe({
      next: (res) => {
        const items = Array.isArray(res) ? res : (res?.items || []);
        this.reviews.set(items);
        this.totalRecords.set(res?.totalCount || items.length);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.isLoading.set(false);
        const msg = extractErrorMessage(err, 'Không thể tải danh sách đánh giá.');
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi tải đánh giá',
          detail: msg
        });
      }
    });
  }

  updateStatus(review: ReviewDto, targetStatus: string): void {
    const statusMap: Record<string, string> = {
      approved: 'Visible',
      Visible: 'Visible',
      rejected: 'Hidden',
      Hidden: 'Hidden',
      flagged: 'Flagged',
      Flagged: 'Flagged'
    };
    const newStatus = statusMap[targetStatus] || targetStatus;

    this.api.updateReviewStatus(review.id, newStatus).subscribe({
      next: () => {
        const statusLabel = newStatus === 'Visible' ? 'Hiển thị' : (newStatus === 'Hidden' ? 'Đã ẩn' : newStatus);
        this.messageService.add({
          severity: 'success',
          summary: 'Thành công',
          detail: `Đã chuyển trạng thái sang "${statusLabel}".`
        });
        this.loadReviews();
      },
      error: (err) => {
        const msg = extractErrorMessage(err, 'Không thể cập nhật trạng thái đánh giá.');
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi cập nhật',
          detail: msg
        });
      }
    });
  }

  onDelete(review: ReviewDto): void {
    const author = review.email || review.authorName || 'độc giả';
    this.confirmationService.confirm({
      message: `Bạn có chắc chắn muốn xóa đánh giá của "${author}"?`,
      header: 'Xác nhận xóa đánh giá',
      icon: 'pi pi-trash',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.api.deleteReview(review.id).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'Thành công',
              detail: 'Đã xóa đánh giá.'
            });
            this.loadReviews();
          },
          error: (err) => {
            const msg = extractErrorMessage(err, 'Không thể xóa đánh giá.');
            this.messageService.add({
              severity: 'error',
              summary: 'Lỗi xóa',
              detail: msg
            });
          }
        });
      }
    });
  }
}
