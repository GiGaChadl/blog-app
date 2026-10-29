import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AdminApiService, CategoryDto } from '../../../core/services/admin-api.service';
import { extractErrorMessage } from '../../../core/utils/error-formatter';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputTextarea } from 'primeng/inputtextarea';
import { DropdownModule } from 'primeng/dropdown';
import { CheckboxModule } from 'primeng/checkbox';
import { DialogModule } from 'primeng/dialog';
import { TagModule } from 'primeng/tag';
import { ConfirmationService, MessageService } from 'primeng/api';

@Component({
  selector: 'app-categories-admin',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TableModule,
    ButtonModule,
    InputTextModule,
    InputTextarea,
    DropdownModule,
    CheckboxModule,
    DialogModule,
    TagModule
  ],
  templateUrl: './categories-admin.component.html',
  styleUrl: './categories-admin.component.scss'
})
export class CategoriesAdminComponent implements OnInit {
  private api = inject(AdminApiService);
  private fb = inject(FormBuilder);
  private confirmationService = inject(ConfirmationService);
  private messageService = inject(MessageService);

  categories = signal<CategoryDto[]>([]);
  isLoading = signal(false);
  dialogVisible: boolean = false;
  isEditMode = signal(false);
  selectedId: string | number | null = null;
  isSaving = signal(false);
  errorMessage = signal<string | null>(null);

  categoryForm: FormGroup = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    slug: ['', [Validators.required, Validators.maxLength(100)]],
    description: [''],
    parentId: [null],
    sortOrder: [1, [Validators.required, Validators.min(1)]],
    isActive: [true]
  });

  parentOptions = signal<{ label: string; value: string | number | null }[]>([]);

  ngOnInit(): void {
    this.loadCategories();
  }

  loadCategories(): void {
    this.isLoading.set(true);
    this.api.getCategories().subscribe({
      next: (data) => {
        const list = data || [];
        const nameMap = new Map<string, string>();
        list.forEach(c => nameMap.set(String(c.id).toLowerCase(), c.name));
        list.forEach(c => {
          if (c.parentId && !c.parentName) {
            c.parentName = nameMap.get(String(c.parentId).toLowerCase()) || undefined;
          }
        });
        this.categories.set(list);
        this.updateParentOptions(list);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.isLoading.set(false);
        const msg = extractErrorMessage(err, 'Không thể tải danh sách chuyên mục.');
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi tải danh mục',
          detail: msg
        });
      }
    });
  }

  updateParentOptions(list: CategoryDto[]): void {
    // Only allow level 1 and level 2 to be parent (max level is 3)
    const options: { label: string; value: string | number | null }[] = [
      { label: '--- Không có (Cấp cao nhất - Level 1) ---', value: null }
    ];

    list.filter(c => c.level < 3).forEach(c => {
      const indent = c.level === 2 ? '— ' : '';
      options.push({ label: `${indent}${c.name} (Cấp ${c.level})`, value: c.id });
    });

    this.parentOptions.set(options);
  }

  openCreateDialog(): void {
    this.isEditMode.set(false);
    this.selectedId = null;
    this.errorMessage.set(null);
    this.categoryForm.reset({
      name: '',
      slug: '',
      description: '',
      parentId: null,
      sortOrder: 1,
      isActive: true
    });
    this.dialogVisible = true;
  }

  openEditDialog(cat: CategoryDto): void {
    this.isEditMode.set(true);
    this.selectedId = cat.id;
    this.errorMessage.set(null);
    this.categoryForm.patchValue({
      name: cat.name,
      slug: cat.slug,
      description: cat.description || '',
      parentId: cat.parentId || null,
      sortOrder: Math.max(1, cat.sortOrder ?? 1),
      isActive: cat.isActive
    });
    this.dialogVisible = true;
  }

  generateSlug(): void {
    if (!this.isEditMode()) {
      const name = this.categoryForm.get('name')?.value || '';
      const slug = name
        .toLowerCase()
        .normalize('NFD')
        .replace(/[\u0300-\u036f]/g, '')
        .replace(/[đĐ]/g, 'd')
        .replace(/([^0-9a-z-\s])/g, '')
        .replace(/(\s+)/g, '-')
        .replace(/-+/g, '-')
        .replace(/^-+|-+$/g, '');
      this.categoryForm.patchValue({ slug });
    }
  }

  onSave(): void {
    if (this.categoryForm.invalid) {
      this.categoryForm.markAllAsTouched();
      return;
    }

    this.errorMessage.set(null);
    this.isSaving.set(true);
    const val = this.categoryForm.value;
    const payload = {
      ...val,
      sortOrder: Math.max(1, Number(val.sortOrder) || 1)
    };

    const request$ = this.isEditMode() && this.selectedId
      ? this.api.updateCategory(this.selectedId, payload)
      : this.api.createCategory(payload);

    request$.subscribe({
      next: () => {
        this.isSaving.set(false);
        this.dialogVisible = false;
        this.errorMessage.set(null);
        this.messageService.add({
          severity: 'success',
          summary: 'Thành công',
          detail: this.isEditMode() ? 'Cập nhật chuyên mục thành công!' : 'Tạo chuyên mục thành công!'
        });
        this.loadCategories();
      },
      error: (err) => {
        this.isSaving.set(false);
        const msg = extractErrorMessage(err, 'Có lỗi xảy ra khi lưu chuyên mục.');
        this.errorMessage.set(msg);
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi lưu chuyên mục',
          detail: msg
        });
      }
    });
  }

  onDelete(cat: CategoryDto): void {
    this.confirmationService.confirm({
      message: `Bạn có chắc chắn muốn xóa chuyên mục "${cat.name}"?`,
      header: 'Xác nhận xóa',
      icon: 'pi pi-trash',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.api.deleteCategory(cat.id).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'Thành công',
              detail: 'Đã xóa chuyên mục.'
            });
            this.loadCategories();
          },
          error: (err) => {
            const msg = extractErrorMessage(err, 'Không thể xóa chuyên mục.');
            this.messageService.add({
              severity: 'error',
              summary: 'Lỗi xóa chuyên mục',
              detail: msg
            });
          }
        });
      }
    });
  }
}
