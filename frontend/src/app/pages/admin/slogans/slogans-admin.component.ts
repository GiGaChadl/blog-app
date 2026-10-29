import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AdminApiService, SloganDto } from '../../../core/services/admin-api.service';
import { extractErrorMessage } from '../../../core/utils/error-formatter';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputTextarea } from 'primeng/inputtextarea';
import { CheckboxModule } from 'primeng/checkbox';
import { DialogModule } from 'primeng/dialog';
import { TagModule } from 'primeng/tag';
import { ConfirmationService, MessageService } from 'primeng/api';

@Component({
  selector: 'app-slogans-admin',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TableModule,
    ButtonModule,
    InputTextModule,
    InputTextarea,
    CheckboxModule,
    DialogModule,
    TagModule
  ],
  templateUrl: './slogans-admin.component.html',
  styleUrl: './slogans-admin.component.scss'
})
export class SlogansAdminComponent implements OnInit {
  private api = inject(AdminApiService);
  private fb = inject(FormBuilder);
  private confirmationService = inject(ConfirmationService);
  private messageService = inject(MessageService);

  slogans = signal<SloganDto[]>([]);
  isLoading = signal(false);
  dialogVisible: boolean = false;
  isEditMode = signal(false);
  selectedId: number | null = null;
  isSaving = signal(false);

  form: FormGroup = this.fb.group({
    content: ['', [Validators.required, Validators.maxLength(500)]],
    author: ['', [Validators.maxLength(100)]],
    isActive: [true]
  });

  ngOnInit(): void {
    this.loadSlogans();
  }

  loadSlogans(): void {
    this.isLoading.set(true);
    this.api.getSlogans().subscribe({
      next: (data) => {
        this.slogans.set(data || []);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.isLoading.set(false);
        const msg = extractErrorMessage(err, 'Không thể tải danh sách slogan.');
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi tải slogan',
          detail: msg
        });
      }
    });
  }

  openCreateDialog(): void {
    this.isEditMode.set(false);
    this.selectedId = null;
    this.form.reset({ content: '', author: '', isActive: true });
    this.dialogVisible = true;
  }

  openEditDialog(slogan: SloganDto): void {
    this.isEditMode.set(true);
    this.selectedId = slogan.id;
    this.form.patchValue({
      content: slogan.content,
      author: slogan.author || '',
      isActive: slogan.isActive
    });
    this.dialogVisible = true;
  }

  onSave(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSaving.set(true);
    const val = this.form.value;

    const req$ = this.isEditMode() && this.selectedId
      ? this.api.updateSlogan(this.selectedId, val)
      : this.api.createSlogan(val);

    req$.subscribe({
      next: () => {
        this.isSaving.set(false);
        this.dialogVisible = false;
        this.messageService.add({
          severity: 'success',
          summary: 'Thành công',
          detail: this.isEditMode() ? 'Cập nhật slogan thành công!' : 'Thêm slogan mới thành công!'
        });
        this.loadSlogans();
      },
      error: (err) => {
        this.isSaving.set(false);
        const msg = extractErrorMessage(err, 'Có lỗi khi lưu slogan.');
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi lưu slogan',
          detail: msg
        });
      }
    });
  }

  onDelete(slogan: SloganDto): void {
    this.confirmationService.confirm({
      message: 'Bạn có chắc chắn muốn xóa câu slogan này?',
      header: 'Xác nhận xóa',
      icon: 'pi pi-trash',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.api.deleteSlogan(slogan.id).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'Thành công',
              detail: 'Đã xóa slogan.'
            });
            this.loadSlogans();
          },
          error: (err) => {
            const msg = extractErrorMessage(err, 'Không thể xóa slogan.');
            this.messageService.add({
              severity: 'error',
              summary: 'Lỗi xóa slogan',
              detail: msg
            });
          }
        });
      }
    });
  }
}
