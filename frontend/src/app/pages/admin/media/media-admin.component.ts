import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminApiService, MediaLibraryDto } from '../../../core/services/admin-api.service';
import { extractErrorMessage } from '../../../core/utils/error-formatter';
import { ButtonModule } from 'primeng/button';
import { FileUploadModule } from 'primeng/fileupload';
import { DialogModule } from 'primeng/dialog';
import { ConfirmationService, MessageService } from 'primeng/api';

@Component({
  selector: 'app-media-admin',
  standalone: true,
  imports: [
    CommonModule,
    ButtonModule,
    FileUploadModule,
    DialogModule
  ],
  templateUrl: './media-admin.component.html',
  styleUrl: './media-admin.component.scss'
})
export class MediaAdminComponent implements OnInit {
  private api = inject(AdminApiService);
  private confirmationService = inject(ConfirmationService);
  private messageService = inject(MessageService);

  mediaList = signal<MediaLibraryDto[]>([]);
  isLoading = signal(false);
  isUploading = signal(false);

  previewDialogVisible: boolean = false;
  previewMedia = signal<MediaLibraryDto | null>(null);

  ngOnInit(): void {
    this.loadMedia();
  }

  loadMedia(): void {
    this.isLoading.set(true);
    this.api.getMedia(1, 50).subscribe({
      next: (res) => {
        const items = Array.isArray(res) ? res : (res?.items || []);
        this.mediaList.set(items);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.isLoading.set(false);
        const msg = extractErrorMessage(err, 'Không thể tải danh sách media.');
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi tải media',
          detail: msg
        });
      }
    });
  }

  onFileSelected(event: any): void {
    const file = event.target?.files?.[0];
    if (!file) return;

    this.isUploading.set(true);
    this.api.uploadMedia(file).subscribe({
      next: () => {
        this.isUploading.set(false);
        this.messageService.add({
          severity: 'success',
          summary: 'Tải lên thành công',
          detail: `Đã lưu file ${file.name} vào thư viện.`
        });
        this.loadMedia();
      },
      error: (err) => {
        this.isUploading.set(false);
        const msg = extractErrorMessage(err, 'Tải file lên thất bại.');
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi tải ảnh lên',
          detail: msg
        });
      }
    });
  }

  copyUrl(media: MediaLibraryDto): void {
    navigator.clipboard.writeText(media.fileUrl).then(() => {
      this.messageService.add({
        severity: 'info',
        summary: 'Đã sao chép URL',
        detail: media.fileUrl
      });
    });
  }

  openPreview(media: MediaLibraryDto): void {
    this.previewMedia.set(media);
    this.previewDialogVisible = true;
  }

  onDelete(media: MediaLibraryDto): void {
    this.confirmationService.confirm({
      message: `Bạn có chắc chắn muốn xóa file "${media.fileName}" khỏi thư viện?`,
      header: 'Xác nhận xóa media',
      icon: 'pi pi-trash',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.api.deleteMedia(media.id).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'Thành công',
              detail: 'Đã xóa file media.'
            });
            this.loadMedia();
          },
          error: (err) => {
            const msg = extractErrorMessage(err, 'Không thể xóa file.');
            this.messageService.add({
              severity: 'error',
              summary: 'Lỗi xóa file',
              detail: msg
            });
          }
        });
      }
    });
  }
}
