import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AdminApiService, TagDto } from '../../../core/services/admin-api.service';
import { extractErrorMessage } from '../../../core/utils/error-formatter';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { DialogModule } from 'primeng/dialog';
import { ConfirmationService, MessageService } from 'primeng/api';

@Component({
  selector: 'app-tags-admin',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TableModule,
    ButtonModule,
    InputTextModule,
    DialogModule
  ],
  templateUrl: './tags-admin.component.html',
  styleUrl: './tags-admin.component.scss'
})
export class TagsAdminComponent implements OnInit {
  private api = inject(AdminApiService);
  private fb = inject(FormBuilder);
  private confirmationService = inject(ConfirmationService);
  private messageService = inject(MessageService);

  tags = signal<TagDto[]>([]);
  isLoading = signal(false);
  dialogVisible: boolean = false;
  isEditMode = signal(false);
  selectedId: string | number | null = null;
  isSaving = signal(false);

  tagForm: FormGroup = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(50)]],
    slug: ['', [Validators.required, Validators.maxLength(50)]]
  });

  ngOnInit(): void {
    this.loadTags();
  }

  loadTags(): void {
    this.isLoading.set(true);
    this.api.getTags().subscribe({
      next: (data) => {
        const list = Array.isArray(data) ? data : ((data as any)?.items || []);
        this.tags.set(list);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.isLoading.set(false);
        const msg = extractErrorMessage(err, 'Không thể tải danh sách thẻ tag.');
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi tải tag',
          detail: msg
        });
      }
    });
  }

  openCreateDialog(): void {
    this.isEditMode.set(false);
    this.selectedId = null;
    this.tagForm.reset({ name: '', slug: '' });
    this.dialogVisible = true;
  }

  openEditDialog(tag: TagDto): void {
    this.isEditMode.set(true);
    this.selectedId = tag.id;
    this.tagForm.patchValue({
      name: tag.name,
      slug: tag.slug
    });
    this.dialogVisible = true;
  }

  generateSlug(): void {
    if (!this.isEditMode()) {
      const name = this.tagForm.get('name')?.value || '';
      const slug = name
        .toLowerCase()
        .normalize('NFD')
        .replace(/[\u0300-\u036f]/g, '')
        .replace(/[đĐ]/g, 'd')
        .replace(/([^0-9a-z-\s])/g, '')
        .replace(/(\s+)/g, '-')
        .replace(/-+/g, '-')
        .replace(/^-+|-+$/g, '');
      this.tagForm.patchValue({ slug });
    }
  }

  onSave(): void {
    if (!this.tagForm.get('slug')?.value) {
      this.generateSlug();
    }

    if (this.tagForm.invalid) {
      this.tagForm.markAllAsTouched();
      this.messageService.add({
        severity: 'warn',
        summary: 'Thiếu thông tin',
        detail: 'Vui lòng nhập tên thẻ tag và đường dẫn (slug).'
      });
      return;
    }

    this.isSaving.set(true);
    const val = this.tagForm.value;

    const req$ = this.isEditMode() && this.selectedId
      ? this.api.updateTag(this.selectedId, val)
      : this.api.createTag(val);

    req$.subscribe({
      next: () => {
        this.isSaving.set(false);
        this.dialogVisible = false;
        this.messageService.add({
          severity: 'success',
          summary: 'Thành công',
          detail: this.isEditMode() ? 'Cập nhật thẻ tag thành công!' : 'Tạo thẻ tag thành công!'
        });
        this.loadTags();
      },
      error: (err) => {
        this.isSaving.set(false);
        const msg = extractErrorMessage(err, 'Có lỗi xảy ra khi lưu thẻ tag.');
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi lưu tag',
          detail: msg
        });
      }
    });
  }

  onDelete(tag: TagDto): void {
    this.confirmationService.confirm({
      message: `Bạn có chắc chắn muốn xóa thẻ tag "${tag.name}"?`,
      header: 'Xác nhận xóa',
      icon: 'pi pi-trash',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.api.deleteTag(tag.id).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'Thành công',
              detail: 'Đã xóa thẻ tag.'
            });
            this.loadTags();
          },
          error: (err) => {
            const msg = extractErrorMessage(err, 'Không thể xóa thẻ tag.');
            this.messageService.add({
              severity: 'error',
              summary: 'Lỗi xóa tag',
              detail: msg
            });
          }
        });
      }
    });
  }
}
