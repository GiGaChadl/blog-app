import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AdminApiService, ProjectDto, MediaLibraryDto } from '../../../core/services/admin-api.service';
import { extractErrorMessage } from '../../../core/utils/error-formatter';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputTextarea } from 'primeng/inputtextarea';
import { DialogModule } from 'primeng/dialog';
import { ConfirmationService, MessageService } from 'primeng/api';
import { MediaPickerDialogComponent } from '../../../shared/components/media-picker-dialog/media-picker-dialog.component';

@Component({
  selector: 'app-projects-admin',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TableModule,
    ButtonModule,
    InputTextModule,
    InputTextarea,
    DialogModule,
    MediaPickerDialogComponent
  ],
  templateUrl: './projects-admin.component.html',
  styleUrl: './projects-admin.component.scss'
})
export class ProjectsAdminComponent implements OnInit {
  private api = inject(AdminApiService);
  private fb = inject(FormBuilder);
  private confirmationService = inject(ConfirmationService);
  private messageService = inject(MessageService);

  projects = signal<ProjectDto[]>([]);
  isLoading = signal(false);
  dialogVisible: boolean = false;
  isEditMode = signal(false);
  selectedId: string | null = null;
  isSaving = signal(false);
  isUploadingImage = signal(false);
  showMediaPicker = signal(false);

  form: FormGroup = this.fb.group({
    title: ['', [Validators.required, Validators.maxLength(150)]],
    shortDescription: [''],
    tag: [''],
    mediaId: ['', [Validators.required]],
    mediaUrl: [''],
    sortOrder: [1, [Validators.required, Validators.min(1)]]
  });

  ngOnInit(): void {
    this.loadProjects();
  }

  loadProjects(): void {
    this.isLoading.set(true);
    this.api.getProjects().subscribe({
      next: (data) => {
        this.projects.set(data || []);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.isLoading.set(false);
        const msg = extractErrorMessage(err, 'Không thể tải danh sách dự án.');
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi tải dự án',
          detail: msg
        });
      }
    });
  }

  openCreateDialog(): void {
    this.isEditMode.set(false);
    this.selectedId = null;
    this.form.reset({
      title: '',
      shortDescription: '',
      tag: '',
      mediaId: '',
      mediaUrl: '',
      sortOrder: 1
    });
    this.dialogVisible = true;
  }

  openEditDialog(project: ProjectDto): void {
    this.isEditMode.set(true);
    this.selectedId = project.id;
    this.form.patchValue({
      title: project.title,
      shortDescription: project.shortDescription || '',
      tag: project.tag || '',
      mediaId: project.mediaId,
      mediaUrl: project.mediaUrl || '',
      sortOrder: Math.max(1, project.sortOrder ?? 1)
    });
    this.dialogVisible = true;
  }

  onProjectImageSelected(event: any): void {
    const file = event.target?.files?.[0];
    if (!file) return;

    this.isUploadingImage.set(true);
    this.api.uploadMedia(file).subscribe({
      next: (media) => {
        this.isUploadingImage.set(false);
        const url = media.fileUrl || media.url;
        this.form.patchValue({
          mediaId: media.id,
          mediaUrl: url
        });
        this.messageService.add({
          severity: 'success',
          summary: 'Tải ảnh thành công',
          detail: `Đã lưu ảnh đại diện dự án "${file.name}".`
        });
      },
      error: (err) => {
        this.isUploadingImage.set(false);
        const msg = extractErrorMessage(err, 'Tải ảnh dự án thất bại.');
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
      mediaUrl: media.fileUrl || media.url
    });
    this.messageService.add({
      severity: 'info',
      summary: 'Đã chọn ảnh',
      detail: `Đã chọn ảnh "${media.fileName}" từ thư viện.`
    });
  }

  removeProjectImage(): void {
    this.form.patchValue({ mediaId: '', mediaUrl: '' });
  }

  onSave(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.messageService.add({
        severity: 'warn',
        summary: 'Thiếu thông tin',
        detail: 'Vui lòng nhập tên dự án và chọn/tải ảnh đại diện dự án.'
      });
      return;
    }

    this.isSaving.set(true);
    const val = this.form.value;
    const payload = {
      mediaId: val.mediaId,
      title: val.title.trim(),
      shortDescription: val.shortDescription ? val.shortDescription.trim() : null,
      tag: val.tag ? val.tag.trim() : null,
      sortOrder: Math.max(1, Number(val.sortOrder) || 1)
    };

    const req$ = this.isEditMode() && this.selectedId
      ? this.api.updateProject(this.selectedId, payload)
      : this.api.createProject(payload);

    req$.subscribe({
      next: () => {
        this.isSaving.set(false);
        this.dialogVisible = false;
        this.messageService.add({
          severity: 'success',
          summary: 'Thành công',
          detail: this.isEditMode() ? 'Cập nhật dự án thành công!' : 'Thêm dự án mới thành công!'
        });
        this.loadProjects();
      },
      error: (err) => {
        this.isSaving.set(false);
        const msg = extractErrorMessage(err, 'Có lỗi khi lưu dự án.');
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi lưu dự án',
          detail: msg
        });
      }
    });
  }

  onDelete(project: ProjectDto): void {
    this.confirmationService.confirm({
      message: `Bạn có chắc chắn muốn xóa dự án "${project.title}"?`,
      header: 'Xác nhận xóa dự án',
      icon: 'pi pi-trash',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.api.deleteProject(project.id).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'Thành công',
              detail: 'Đã xóa dự án thành công.'
            });
            this.loadProjects();
          },
          error: (err) => {
            const msg = extractErrorMessage(err, 'Không thể xóa dự án.');
            this.messageService.add({
              severity: 'error',
              summary: 'Lỗi xóa dự án',
              detail: msg
            });
          }
        });
      }
    });
  }
}
