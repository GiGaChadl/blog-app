import { Component, OnInit, OnChanges, SimpleChanges, Input, Output, EventEmitter, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminApiService, MediaLibraryDto } from '../../../core/services/admin-api.service';
import { extractErrorMessage } from '../../../core/utils/error-formatter';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-media-picker-dialog',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    DialogModule,
    ButtonModule,
    InputTextModule
  ],
  templateUrl: './media-picker-dialog.component.html',
  styleUrl: './media-picker-dialog.component.scss'
})
export class MediaPickerDialogComponent implements OnInit, OnChanges {
  private api = inject(AdminApiService);
  private messageService = inject(MessageService);

  @Input() visible: boolean = false;
  @Input() header: string = 'Chọn Hình Ảnh Từ Thư Viện';
  @Output() visibleChange = new EventEmitter<boolean>();
  @Output() select = new EventEmitter<MediaLibraryDto>();

  mediaList = signal<MediaLibraryDto[]>([]);
  searchQuery = signal<string>('');
  isLoading = signal(false);
  isUploading = signal(false);

  filteredMedia = computed(() => {
    const q = this.searchQuery().trim().toLowerCase();
    const list = this.mediaList();
    if (!q) return list;
    return list.filter(m => (m.fileName || '').toLowerCase().includes(q));
  });

  ngOnInit(): void {
    if (this.visible) {
      this.loadMedia();
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['visible'] && changes['visible'].currentValue === true) {
      this.loadMedia();
    }
  }

  loadMedia(): void {
    this.isLoading.set(true);
    this.api.getMedia(1, 100).subscribe({
      next: (res) => {
        const items = Array.isArray(res) ? res : (res?.items || []);
        this.mediaList.set(items);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.isLoading.set(false);
        const msg = extractErrorMessage(err, 'Không thể tải danh sách ảnh từ thư viện.');
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
      next: (uploaded) => {
        this.isUploading.set(false);
        event.target.value = '';
        this.messageService.add({
          severity: 'success',
          summary: 'Tải lên thành công',
          detail: `Đã lưu ảnh "${file.name}" vào thư viện và tự động chọn.`
        });
        // Prepend to current list
        this.mediaList.update(list => [uploaded, ...list]);
        // Auto select and close
        this.onSelect(uploaded);
      },
      error: (err) => {
        this.isUploading.set(false);
        event.target.value = '';
        const msg = extractErrorMessage(err, 'Tải ảnh lên thư viện thất bại.');
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi tải ảnh',
          detail: msg
        });
      }
    });
  }

  onSelect(media: MediaLibraryDto): void {
    this.select.emit(media);
    this.close();
  }

  close(): void {
    this.visible = false;
    this.visibleChange.emit(false);
  }
}
