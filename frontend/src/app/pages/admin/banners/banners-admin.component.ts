import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AdminApiService, BannerDto, MediaLibraryDto } from '../../../core/services/admin-api.service';
import { extractErrorMessage } from '../../../core/utils/error-formatter';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { CheckboxModule } from 'primeng/checkbox';
import { InputSwitchModule } from 'primeng/inputswitch';
import { DialogModule } from 'primeng/dialog';
import { TagModule } from 'primeng/tag';
import { ConfirmationService, MessageService } from 'primeng/api';
import { MediaPickerDialogComponent } from '../../../shared/components/media-picker-dialog/media-picker-dialog.component';

@Component({
  selector: 'app-banners-admin',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TableModule,
    ButtonModule,
    InputTextModule,
    CheckboxModule,
    InputSwitchModule,
    DialogModule,
    TagModule,
    MediaPickerDialogComponent
  ],
  templateUrl: './banners-admin.component.html',
  styleUrl: './banners-admin.component.scss'
})
export class BannersAdminComponent implements OnInit {
  private api = inject(AdminApiService);
  private fb = inject(FormBuilder);
  private confirmationService = inject(ConfirmationService);
  private messageService = inject(MessageService);

  banners = signal<BannerDto[]>([]);
  isLoading = signal(false);
  dialogVisible: boolean = false;
  isEditMode = signal(false);
  selectedId: string | null = null;
  isSaving = signal(false);
  isUploadingImage = signal(false);
  showMediaPicker = signal(false);

  form: FormGroup = this.fb.group({
    mediaId: ['', [Validators.required]],
    imageUrl: ['', [Validators.required]],
    title: ['', [Validators.maxLength(150)]],
    targetUrl: [''],
    sortOrder: [1, [Validators.required, Validators.min(1)]],
    isActive: [true],
    type: ['Banner']
  });

  ngOnInit(): void {
    this.loadBanners();
  }

  loadBanners(): void {
    this.isLoading.set(true);
    this.api.getBanners().subscribe({
      next: (data) => {
        this.banners.set(data || []);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.isLoading.set(false);
        const msg = extractErrorMessage(err, 'Không thể tải danh sách banner.');
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi tải banner',
          detail: msg
        });
      }
    });
  }

  openCreateDialog(): void {
    this.isEditMode.set(false);
    this.selectedId = null;
    this.form.reset({
      mediaId: '',
      imageUrl: '',
      title: '',
      targetUrl: '',
      sortOrder: 1,
      isActive: true,
      type: 'Banner'
    });
    this.dialogVisible = true;
  }

  openEditDialog(banner: BannerDto): void {
    this.isEditMode.set(true);
    this.selectedId = banner.id;
    this.form.patchValue({
      mediaId: banner.mediaId,
      imageUrl: banner.imageUrl || banner.mediaUrl || '',
      title: banner.title || banner.mediaAltText || '',
      targetUrl: banner.targetUrl || banner.linkUrl || '',
      sortOrder: Math.max(1, banner.sortOrder ?? 1),
      isActive: banner.isActive ?? true,
      type: banner.type || 'Banner'
    });
    this.dialogVisible = true;
  }

  onBannerFileSelected(event: any): void {
    const file = event.target?.files?.[0];
    if (!file) return;

    this.isUploadingImage.set(true);
    this.api.uploadMedia(file).subscribe({
      next: (media) => {
        this.isUploadingImage.set(false);
        const url = media.fileUrl || media.url;
        this.form.patchValue({
          mediaId: media.id,
          imageUrl: url
        });
        this.messageService.add({
          severity: 'success',
          summary: 'Tải ảnh thành công',
          detail: `Đã lưu ảnh "${file.name}" vào hệ thống.`
        });
      },
      error: (err) => {
        this.isUploadingImage.set(false);
        const msg = extractErrorMessage(err, 'Tải ảnh banner thất bại.');
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi tải ảnh',
          detail: msg
        });
      }
    });
  }

  openMediaPicker(): void {
    this.showMediaPicker.set(true);
  }

  onMediaSelected(media: MediaLibraryDto): void {
    this.form.patchValue({
      mediaId: media.id,
      imageUrl: media.fileUrl || media.url
    });
    this.messageService.add({
      severity: 'info',
      summary: 'Đã chọn ảnh',
      detail: `Đã chọn ảnh "${media.fileName}" từ thư viện.`
    });
  }

  removeBannerImage(): void {
    this.form.patchValue({ mediaId: '', imageUrl: '' });
  }

  onSave(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.messageService.add({
        severity: 'warn',
        summary: 'Thiếu thông tin',
        detail: 'Vui lòng chọn hoặc tải ảnh lên cho banner.'
      });
      return;
    }

    this.isSaving.set(true);
    const val = this.form.value;

    const payload = {
      mediaId: val.mediaId,
      imageUrl: val.imageUrl,
      title: val.title ? val.title.trim() : null,
      altText: val.title ? val.title.trim() : null,
      type: val.type || 'Banner',
      linkUrl: val.targetUrl ? val.targetUrl.trim() : null,
      targetUrl: val.targetUrl ? val.targetUrl.trim() : null,
      sortOrder: Math.max(1, Number(val.sortOrder) || 1),
      isActive: val.isActive ?? true
    };

    const req$ = this.isEditMode() && this.selectedId
      ? this.api.updateBanner(this.selectedId, payload)
      : this.api.createBanner(payload);

    req$.subscribe({
      next: () => {
        this.isSaving.set(false);
        this.dialogVisible = false;
        this.messageService.add({
          severity: 'success',
          summary: 'Thành công',
          detail: this.isEditMode() ? 'Cập nhật banner thành công!' : 'Tạo banner mới thành công!'
        });
        this.loadBanners();
      },
      error: (err) => {
        this.isSaving.set(false);
        const msg = extractErrorMessage(err, 'Lỗi khi lưu banner.');
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi lưu banner',
          detail: msg
        });
      }
    });
  }

  onDelete(banner: BannerDto): void {
    const title = banner.title || banner.mediaAltText || 'Banner';
    this.confirmationService.confirm({
      message: `Bạn có chắc muốn xóa banner "${title}"?`,
      header: 'Xác nhận xóa banner',
      icon: 'pi pi-trash',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.api.deleteBanner(banner.id).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'Thành công',
              detail: 'Đã xóa banner.'
            });
            this.loadBanners();
          },
          error: (err) => {
            const msg = extractErrorMessage(err, 'Không thể xóa banner.');
            this.messageService.add({
              severity: 'error',
              summary: 'Lỗi xóa banner',
              detail: msg
            });
          }
        });
      }
    });
  }
}
