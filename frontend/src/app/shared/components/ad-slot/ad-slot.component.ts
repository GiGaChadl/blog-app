import { CommonModule } from '@angular/common';
import { Component, Input, OnInit, inject, signal } from '@angular/core';
import { BlogApiService } from '../../../core/services/blog-api.service';

export interface ActiveAdDto {
  id?: string;
  position: string;
  adsenseSlotId: string;
  isActive: boolean;
}

@Component({
  selector: 'app-ad-slot',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './ad-slot.component.html',
  styleUrl: './ad-slot.component.scss'
})
export class AdSlotComponent implements OnInit {
  private blogApi = inject(BlogApiService);

  /** Vị trí quảng cáo — dùng để admin quản lý & để CSS hiển thị phù hợp */
  @Input() position: 'home-mid' | 'post-detail' | 'HomeMid' | 'PostDetail' = 'home-mid';

  ad = signal<ActiveAdDto | null>(null);
  isLoaded = signal(false);

  ngOnInit(): void {
    const apiPos = this.normalizePosition(this.position);
    this.blogApi.getActiveAd(apiPos).subscribe({
      next: (res) => {
        if (res && res.isActive && res.adsenseSlotId && res.adsenseSlotId !== '0') {
          this.ad.set(res);
          this.pushAdSense();
        } else {
          this.ad.set(null);
        }
        this.isLoaded.set(true);
      },
      error: () => {
        this.ad.set(null);
        this.isLoaded.set(true);
      }
    });
  }

  private normalizePosition(pos: string): string {
    if (pos === 'home-mid' || pos === 'HomeMid') return 'HomeMid';
    if (pos === 'post-detail' || pos === 'PostDetail') return 'PostDetail';
    return pos;
  }

  private pushAdSense(): void {
    if (typeof window !== 'undefined') {
      try {
        ((window as any)['adsbygoogle'] = (window as any)['adsbygoogle'] || []).push({});
      } catch (e) {
        // Ignored if adsbygoogle script is blocked by browser extension
      }
    }
  }
}
