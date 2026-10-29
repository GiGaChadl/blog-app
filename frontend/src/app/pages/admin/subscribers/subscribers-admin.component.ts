import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminApiService, SubscriberDto } from '../../../core/services/admin-api.service';
import { extractErrorMessage } from '../../../core/utils/error-formatter';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { ConfirmationService, MessageService } from 'primeng/api';

@Component({
  selector: 'app-subscribers-admin',
  standalone: true,
  imports: [CommonModule, TableModule, ButtonModule, TagModule],
  templateUrl: './subscribers-admin.component.html',
  styleUrl: './subscribers-admin.component.scss'
})
export class SubscribersAdminComponent implements OnInit {
  private api = inject(AdminApiService);
  private confirmationService = inject(ConfirmationService);
  private messageService = inject(MessageService);

  subscribers = signal<SubscriberDto[]>([]);
  isLoading = signal(false);
  totalRecords = signal(0);
  page = 1;
  pageSize = 15;

  ngOnInit(): void {
    this.loadSubscribers();
  }

  loadSubscribers(): void {
    this.isLoading.set(true);
    this.api.getSubscribers({ page: this.page, pageSize: this.pageSize }).subscribe({
      next: (res) => {
        const items = Array.isArray(res) ? res : (res?.items || []);
        this.subscribers.set(items);
        this.totalRecords.set(res?.totalCount || items.length);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.isLoading.set(false);
        const msg = extractErrorMessage(err, 'Không thể tải danh sách người theo dõi.');
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi tải danh sách',
          detail: msg
        });
      }
    });
  }

  isConfirmed(sub: SubscriberDto): boolean {
    if (sub.isConfirmed !== undefined) return sub.isConfirmed;
    return !!sub.confirmedAt || sub.status === 'Active' || sub.status === 1;
  }

  isActive(sub: SubscriberDto): boolean {
    if (sub.isActive !== undefined) return sub.isActive;
    return sub.status === 'Active' || sub.status === 1;
  }

  onDelete(sub: SubscriberDto): void {
    this.confirmationService.confirm({
      message: `Bạn có chắc chắn muốn xóa email "${sub.email}"?`,
      header: 'Xác nhận xóa người theo dõi',
      icon: 'pi pi-trash',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.api.deleteSubscriber(sub.id).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'Thành công',
              detail: 'Đã xóa email khỏi danh sách.'
            });
            this.loadSubscribers();
          },
          error: (err) => {
            const msg = extractErrorMessage(err, 'Không thể xóa email này.');
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
