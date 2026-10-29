import { Component, OnInit, AfterViewInit, OnDestroy, ElementRef, ViewChild, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Editor } from '@tiptap/core';
import StarterKit from '@tiptap/starter-kit';
import ImageResize from 'tiptap-extension-resize-image';
import Link from '@tiptap/extension-link';
import { Table, TableRow, TableHeader, TableCell } from '@tiptap/extension-table';
import { AdminApiService, CategoryDto, TagDto, PostCreateUpdateDto, MediaLibraryDto } from '../../../core/services/admin-api.service';
import { extractErrorMessage } from '../../../core/utils/error-formatter';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputTextarea } from 'primeng/inputtextarea';
import { DropdownModule } from 'primeng/dropdown';
import { MultiSelectModule } from 'primeng/multiselect';
import { CheckboxModule } from 'primeng/checkbox';
import { DialogModule } from 'primeng/dialog';
import { MessageService } from 'primeng/api';
import { MediaPickerDialogComponent } from '../../../shared/components/media-picker-dialog/media-picker-dialog.component';

@Component({
  selector: 'app-post-edit',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    ButtonModule,
    InputTextModule,
    InputTextarea,
    DropdownModule,
    MultiSelectModule,
    CheckboxModule,
    DialogModule,
    MediaPickerDialogComponent
  ],
  templateUrl: './post-edit.component.html',
  styleUrl: './post-edit.component.scss'
})
export class PostEditComponent implements OnInit, AfterViewInit, OnDestroy {
  private fb = inject(FormBuilder);
  private api = inject(AdminApiService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private messageService = inject(MessageService);

  @ViewChild('editorContainer') editorContainer!: ElementRef<HTMLDivElement>;
  editor?: Editor;
  isUploadingEditorImage = signal(false);
  isUploadingThumbnail = signal(false);
  isUploadingOgImage = signal(false);

  postId: string | null = null;
  isEditMode = signal(false);
  isLoading = signal(false);
  isSubmitting = signal(false);

  categories = signal<CategoryDto[]>([]);
  tags = signal<TagDto[]>([]);
  mediaList = signal<MediaLibraryDto[]>([]);
  showMediaPicker = signal(false);
  mediaPickerTarget: 'thumbnail' | 'ogImage' | 'editor' | null = null;

  selectedThumbnailUrl = signal<string | null>(null);
  selectedOgImageUrl = signal<string | null>(null);

  statusOptions = [
    { label: 'Xuất bản (Hiển thị ngay)', value: 'Published' },
    { label: 'Ẩn bài viết', value: 'Hidden' }
  ];

  categoryOptions = computed(() => {
    return this.categories().map((cat) => ({
      label: `${cat.name} (Cấp ${cat.level}${cat.level === 3 ? ' - Hợp lệ' : ' - Nhánh cha'})`,
      value: cat.id,
      disabled: cat.level !== 3
    }));
  });

  form: FormGroup = this.fb.group({
    title: ['', [Validators.required, Validators.maxLength(255)]],
    slug: ['', [Validators.required, Validators.maxLength(255)]],
    summary: ['', [Validators.maxLength(500)]],
    content: ['', [Validators.required]],
    categoryId: [null, [Validators.required]],
    status: ['Published', [Validators.required]],
    tagIds: [[]],
    thumbnailMediaId: [null, [Validators.required]],
    ogImageMediaId: [null],
    metaTitle: [''],
    metaDescription: [''],
    metaKeywords: [''],
    canonicalUrl: ['']
  });

  ngOnInit(): void {
    this.loadCategories();
    this.loadTags();
    this.loadMedia();

    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.postId = idParam;
      this.isEditMode.set(true);
      this.loadPostData(this.postId);
    }
  }

  ngAfterViewInit(): void {
    this.initTipTap();
  }

  ngOnDestroy(): void {
    this.editor?.destroy();
  }

  private initTipTap(): void {
    this.editor = new Editor({
      element: this.editorContainer.nativeElement,
      extensions: [
        StarterKit,
        ImageResize.configure({
          inline: false,
        }),
        Link.configure({
          openOnClick: false,
          HTMLAttributes: {
            class: 'editor-link',
            target: '_blank',
            rel: 'noopener noreferrer',
          },
        }),
        Table.configure({
          resizable: true,
        }),
        TableRow,
        TableHeader,
        TableCell,
      ],
      content: this.form.get('content')?.value || '',
      onUpdate: ({ editor }) => {
        const html = editor.getHTML();
        this.form.patchValue({ content: html }, { emitEvent: false });
      },
    });

    // If post data was loaded before editor initialization, update content
    const existingContent = this.form.get('content')?.value;
    if (existingContent && !this.editor.getText()) {
      this.editor.commands.setContent(existingContent);
    }
  }

  // Formatting helpers
  toggleBold(): void {
    this.editor?.chain().focus().toggleBold().run();
  }

  toggleItalic(): void {
    this.editor?.chain().focus().toggleItalic().run();
  }

  toggleStrike(): void {
    this.editor?.chain().focus().toggleStrike().run();
  }

  toggleHeading(level: 1 | 2 | 3 | 4): void {
    this.editor?.chain().focus().toggleHeading({ level }).run();
  }

  toggleBulletList(): void {
    this.editor?.chain().focus().toggleBulletList().run();
  }

  toggleOrderedList(): void {
    this.editor?.chain().focus().toggleOrderedList().run();
  }

  toggleBlockquote(): void {
    this.editor?.chain().focus().toggleBlockquote().run();
  }

  toggleCodeBlock(): void {
    this.editor?.chain().focus().toggleCodeBlock().run();
  }

  setLink(): void {
    const previousUrl = this.editor?.getAttributes('link')['href'];
    const url = window.prompt('Nhập địa chỉ liên kết (URL):', previousUrl || 'https://');

    if (url === null) {
      return;
    }

    if (url.trim() === '') {
      this.editor?.chain().focus().extendMarkRange('link').unsetLink().run();
      return;
    }

    this.editor?.chain().focus().extendMarkRange('link').setLink({ href: url.trim() }).run();
  }

  insertTable(): void {
    this.editor?.chain().focus().insertTable({ rows: 3, cols: 3, withHeaderRow: true }).run();
  }

  addColumnAfter(): void {
    this.editor?.chain().focus().addColumnAfter().run();
  }

  deleteColumn(): void {
    this.editor?.chain().focus().deleteColumn().run();
  }

  addRowAfter(): void {
    this.editor?.chain().focus().addRowAfter().run();
  }

  deleteRow(): void {
    this.editor?.chain().focus().deleteRow().run();
  }

  deleteTable(): void {
    this.editor?.chain().focus().deleteTable().run();
  }

  undo(): void {
    this.editor?.chain().focus().undo().run();
  }

  redo(): void {
    this.editor?.chain().focus().redo().run();
  }

  onEditorImageFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) return;
    const file = input.files[0];

    this.isUploadingEditorImage.set(true);
    this.api.uploadMedia(file).subscribe({
      next: (res) => {
        this.isUploadingEditorImage.set(false);
        input.value = '';
        if (res && res.fileUrl) {
          this.editor?.chain().focus().setImage({ src: res.fileUrl, alt: res.fileName || file.name }).run();
          this.messageService.add({
            severity: 'success',
            summary: 'Tải ảnh thành công',
            detail: 'Đã chèn ảnh vào vị trí con trỏ trong bài viết.'
          });
        }
      },
      error: (err) => {
        this.isUploadingEditorImage.set(false);
        input.value = '';
        const msg = extractErrorMessage(err, 'Không thể tải ảnh lên.');
        this.messageService.add({ severity: 'error', summary: 'Lỗi tải ảnh', detail: msg });
      }
    });
  }

  onThumbnailFileSelected(event: any): void {
    const file = event.target?.files?.[0];
    if (!file) return;

    this.isUploadingThumbnail.set(true);
    this.api.uploadMedia(file).subscribe({
      next: (media) => {
        this.isUploadingThumbnail.set(false);
        event.target.value = '';
        this.form.patchValue({ thumbnailMediaId: media.id });
        this.selectedThumbnailUrl.set(media.fileUrl || media.url || null);
        this.messageService.add({
          severity: 'success',
          summary: 'Tải ảnh đại diện thành công',
          detail: `Đã lưu ảnh "${file.name}" vào thư viện và đặt làm ảnh đại diện.`
        });
      },
      error: (err) => {
        this.isUploadingThumbnail.set(false);
        event.target.value = '';
        const msg = extractErrorMessage(err, 'Không thể tải ảnh đại diện lên.');
        this.messageService.add({ severity: 'error', summary: 'Lỗi tải ảnh', detail: msg });
      }
    });
  }

  onOgImageFileSelected(event: any): void {
    const file = event.target?.files?.[0];
    if (!file) return;

    this.isUploadingOgImage.set(true);
    this.api.uploadMedia(file).subscribe({
      next: (media) => {
        this.isUploadingOgImage.set(false);
        event.target.value = '';
        this.form.patchValue({ ogImageMediaId: media.id });
        this.selectedOgImageUrl.set(media.fileUrl || media.url || null);
        this.messageService.add({
          severity: 'success',
          summary: 'Tải ảnh OG thành công',
          detail: `Đã lưu ảnh "${file.name}" vào thư viện và đặt làm ảnh chia sẻ MXH.`
        });
      },
      error: (err) => {
        this.isUploadingOgImage.set(false);
        event.target.value = '';
        const msg = extractErrorMessage(err, 'Không thể tải ảnh chia sẻ MXH lên.');
        this.messageService.add({ severity: 'error', summary: 'Lỗi tải ảnh', detail: msg });
      }
    });
  }

  generateSlugFromTitle(): void {
    if (!this.isEditMode()) {
      const title = this.form.get('title')?.value || '';
      const slug = title
        .toLowerCase()
        .normalize('NFD')
        .replace(/[\u0300-\u036f]/g, '')
        .replace(/[đĐ]/g, 'd')
        .replace(/([^0-9a-z-\s])/g, '')
        .replace(/(\s+)/g, '-')
        .replace(/-+/g, '-')
        .replace(/^-+|-+$/g, '');
      this.form.patchValue({ slug });
    }
  }

  loadCategories(): void {
    this.api.getCategories().subscribe({
      next: (data) => this.categories.set(data || []),
      error: (err) => {
        const msg = extractErrorMessage(err, 'Không thể tải danh sách chuyên mục.');
        this.messageService.add({ severity: 'error', summary: 'Lỗi', detail: msg });
      }
    });
  }

  loadTags(): void {
    this.api.getTags().subscribe({
      next: (data) => {
        const list = Array.isArray(data) ? data : ((data as any)?.items || []);
        this.tags.set(list);
      },
      error: (err) => {
        const msg = extractErrorMessage(err, 'Không thể tải danh sách thẻ tags.');
        this.messageService.add({ severity: 'error', summary: 'Lỗi', detail: msg });
      }
    });
  }

  loadMedia(): void {
    this.api.getMedia(1, 48).subscribe({
      next: (res) => {
        const items = Array.isArray(res) ? res : (res?.items || []);
        this.mediaList.set(items);
      },
      error: (err) => {
        const msg = extractErrorMessage(err, 'Không thể tải thư viện media.');
        this.messageService.add({ severity: 'error', summary: 'Lỗi', detail: msg });
      }
    });
  }

  loadPostData(id: string): void {
    this.isLoading.set(true);
    this.api.getPostById(id).subscribe({
      next: (post) => {
        const postContent = post.content || post.contentHtml || '';
        this.form.patchValue({
          title: post.title,
          slug: post.slug,
          summary: post.summary || post.excerpt || '',
          content: postContent,
          categoryId: post.categoryId,
          status: post.status === 'Hidden' ? 'Hidden' : 'Published',
          tagIds: post.tagIds || (post.tags ? post.tags.map((t: any) => t.id) : []),
          thumbnailMediaId: post.thumbnailMediaId,
          ogImageMediaId: post.ogImageMediaId,
          metaTitle: post.metaTitle || '',
          metaDescription: post.metaDescription || '',
          metaKeywords: post.metaKeywords || '',
          canonicalUrl: post.canonicalUrl || ''
        });
        if (this.editor) {
          this.editor.commands.setContent(postContent);
        }
        if (post.thumbnailUrl) this.selectedThumbnailUrl.set(post.thumbnailUrl);
        if (post.ogImageUrl) this.selectedOgImageUrl.set(post.ogImageUrl);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.isLoading.set(false);
        const msg = extractErrorMessage(err, 'Không thể tải thông tin bài viết.');
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi tải bài viết',
          detail: msg
        });
        this.router.navigate(['/admin/posts']);
      }
    });
  }

  openMediaPicker(target: 'thumbnail' | 'ogImage' | 'editor'): void {
    this.mediaPickerTarget = target;
    this.showMediaPicker.set(true);
  }

  selectMediaItem(media: MediaLibraryDto): void {
    if (this.mediaPickerTarget === 'thumbnail') {
      this.form.patchValue({ thumbnailMediaId: media.id });
      this.selectedThumbnailUrl.set(media.fileUrl);
    } else if (this.mediaPickerTarget === 'ogImage') {
      this.form.patchValue({ ogImageMediaId: media.id });
      this.selectedOgImageUrl.set(media.fileUrl);
    } else if (this.mediaPickerTarget === 'editor') {
      this.editor?.chain().focus().setImage({ src: media.fileUrl, alt: media.fileName }).run();
      this.messageService.add({
        severity: 'success',
        summary: 'Thành công',
        detail: 'Đã chèn ảnh từ thư viện vào bài viết.'
      });
    }
    this.showMediaPicker.set(false);
  }

  removeSelectedMedia(target: 'thumbnail' | 'ogImage'): void {
    if (target === 'thumbnail') {
      this.form.patchValue({ thumbnailMediaId: null });
      this.selectedThumbnailUrl.set(null);
    } else {
      this.form.patchValue({ ogImageMediaId: null });
      this.selectedOgImageUrl.set(null);
    }
  }

  onSubmit(): void {
    if (this.editor) {
      const html = this.editor.getHTML();
      const cleanHtml = (html === '<p></p>' || html.trim() === '') ? '' : html;
      this.form.patchValue({ content: cleanHtml });
    }

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      if (this.form.get('thumbnailMediaId')?.invalid) {
        this.messageService.add({
          severity: 'warn',
          summary: 'Thiếu ảnh đại diện',
          detail: 'Vui lòng chọn ảnh đại diện (Thumbnail) từ thư viện cho bài viết.'
        });
        return;
      }
      if (this.form.get('categoryId')?.invalid) {
        this.messageService.add({
          severity: 'warn',
          summary: 'Thiếu chuyên mục',
          detail: 'Vui lòng chọn chuyên mục cấp 3 cho bài viết.'
        });
        return;
      }
      this.messageService.add({
        severity: 'warn',
        summary: 'Thiếu thông tin bắt buộc',
        detail: 'Vui lòng kiểm tra lại các trường bắt buộc (Tiêu đề, slug, nội dung, chuyên mục, ảnh).'
      });
      return;
    }

    const val = this.form.value;

    // Check category level requirement
    const chosenCat = this.categories().find((c) => String(c.id) === String(val.categoryId));
    if (chosenCat && chosenCat.level !== 3) {
      this.messageService.add({
        severity: 'error',
        summary: 'Chuyên mục không hợp lệ',
        detail: `Chuyên mục "${chosenCat.name}" là cấp ${chosenCat.level}. Hệ thống chỉ cho phép bài viết gắn vào danh mục cấp 3.`
      });
      return;
    }

    this.isSubmitting.set(true);
    const dto: PostCreateUpdateDto = {
      title: val.title.trim(),
      slug: val.slug.trim(),
      summary: val.summary?.trim() || undefined,
      excerpt: val.summary?.trim() || undefined,
      content: val.content,
      contentHtml: val.content,
      categoryId: val.categoryId,
      thumbnailMediaId: val.thumbnailMediaId,
      ogImageMediaId: val.ogImageMediaId || null,
      status: val.status,
      tagIds: Array.isArray(val.tagIds) ? val.tagIds : [],
      metaTitle: val.metaTitle?.trim() || undefined,
      metaDescription: val.metaDescription?.trim() || undefined,
      metaKeywords: val.metaKeywords?.trim() || undefined,
      canonicalUrl: val.canonicalUrl?.trim() || undefined
    };

    const request$ = this.isEditMode() && this.postId
      ? this.api.updatePost(this.postId, dto)
      : this.api.createPost(dto);

    request$.subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.messageService.add({
          severity: 'success',
          summary: 'Thành công',
          detail: this.isEditMode() ? 'Cập nhật bài viết thành công!' : 'Tạo bài viết mới thành công!'
        });
        this.router.navigate(['/admin/posts']);
      },
      error: (err) => {
        this.isSubmitting.set(false);
        const msg = extractErrorMessage(err, 'Có lỗi xảy ra khi lưu bài viết.');
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi lưu bài viết',
          detail: msg,
          life: 8000
        });
      }
    });
  }
}
