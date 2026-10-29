import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AdminApiService, AdDto } from '../../../core/services/admin-api.service';
import { extractErrorMessage } from '../../../core/utils/error-formatter';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { InputTextarea } from 'primeng/inputtextarea';
import { InputSwitchModule } from 'primeng/inputswitch';
import { TagModule } from 'primeng/tag';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-ads-admin',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    CardModule,
    ButtonModule,
    InputTextarea,
    InputSwitchModule,
    TagModule
  ],
  templateUrl: './ads-admin.component.html',
  styleUrl: './ads-admin.component.scss'
})
export class AdsAdminComponent implements OnInit {
  private api = inject(AdminApiService);
  private messageService = inject(MessageService);

  ads = signal<AdDto[]>([]);
  isLoading = signal(false);
  savingPosition = signal<string | null>(null);

  positionLabels: { [key: string]: { label: string; desc: string; icon: string } } = {
    HomeMid: { label: 'Giữa trang chủ (Home Mid Ad)', desc: 'Hiển thị ngay sau khối bài viết nổi bật trên trang chủ', icon: 'pi pi-home' },
    PostDetail: { label: 'Trong bài viết (Post Detail Ad)', desc: 'Hiển thị sau ảnh bìa và phần đầu của bài viết chi tiết', icon: 'pi pi-file' }
  };

  getPositionInfo(position: string) {
    return this.positionLabels[position] || {
      label: position,
      desc: 'Vị trí quảng cáo AdSense',
      icon: 'pi pi-megaphone'
    };
  }

  ngOnInit(): void {
    this.loadAds();
  }

  loadAds(): void {
    this.isLoading.set(true);
    this.api.getAds().subscribe({
      next: (data) => {
        const list = data || [];
        const positions = ['HomeMid', 'PostDetail'];
        const fullList: AdDto[] = positions.map((pos) => {
          const found = list.find((a) => (a.position + '').toLowerCase() === pos.toLowerCase());
          return found
            ? {
                ...found,
                position: pos,
                adsenseSlotId: found.adsenseSlotId || found.adCode || '',
                adCode: found.adCode || found.adsenseSlotId || ''
              }
            : { position: pos, adsenseSlotId: '', adCode: '', isActive: false };
        });
        this.ads.set(fullList);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.isLoading.set(false);
        const msg = extractErrorMessage(err, 'Không thể tải cấu hình quảng cáo.');
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi tải quảng cáo',
          detail: msg
        });
      }
    });
  }

  onSaveAd(ad: AdDto): void {
    let slotId = (ad.adsenseSlotId || ad.adCode || '').trim();
    // Trích xuất nếu người dùng dán toàn bộ đoạn mã HTML của Google AdSense
    const match = slotId.match(/data-ad-slot=["']?(\d+)["']?/i);
    if (match) {
      slotId = match[1];
      ad.adsenseSlotId = slotId;
    }

    if (!slotId && ad.isActive) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Cảnh báo',
        detail: 'Vui lòng nhập Google AdSense Slot ID trước khi bật quảng cáo.'
      });
      return;
    }

    this.savingPosition.set(ad.position);
    this.api.updateAd(ad.position, {
      adsenseSlotId: slotId || '0',
      isActive: ad.isActive,
      startDate: ad.startDate || null,
      endDate: ad.endDate || null
    }).subscribe({
      next: () => {
        this.savingPosition.set(null);
        this.messageService.add({
          severity: 'success',
          summary: 'Đã lưu',
          detail: `Cập nhật cấu hình quảng cáo [${this.positionLabels[ad.position]?.label || ad.position}] thành công!`
        });
      },
      error: (err) => {
        this.savingPosition.set(null);
        const msg = extractErrorMessage(err, 'Không thể lưu cấu hình quảng cáo.');
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi lưu quảng cáo',
          detail: msg
        });
      }
    });
  }
}
