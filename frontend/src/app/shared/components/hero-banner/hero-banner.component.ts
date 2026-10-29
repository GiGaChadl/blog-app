import { CommonModule } from '@angular/common';
import { Component, Input, OnInit, OnDestroy, signal } from '@angular/core';

export interface HeroBannerImage {
  url: string;
  linkUrl?: string;
}

@Component({
  selector: 'app-hero-banner',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './hero-banner.component.html',
  styleUrl: './hero-banner.component.scss',
})
export class HeroBannerComponent implements OnInit, OnDestroy {
  /** Khẩu hiệu / Danh ngôn cá nhân — nội dung admin chỉnh trong mục Banner/Slogan */
  @Input() slogan =
    'Ghi lại những gì thật — không chỉnh sửa cho hoàn hảo hơn nó vốn có.';
  @Input() author?: string;

  private defaultBanner: HeroBannerImage = {
    url: 'https://picsum.photos/seed/bloghero/1920/600',
  };

  currentIndex = signal(0);
  private autoSlideTimer: any = null;
  private _banners: HeroBannerImage[] = [];

  @Input()
  set banners(val: HeroBannerImage[] | undefined | null) {
    if (val && val.length > 0) {
      this._banners = val;
    } else {
      this._banners = [this.defaultBanner];
    }
    this.currentIndex.set(0);
    this.resetTimer();
  }
  get banners(): HeroBannerImage[] {
    return this._banners.length > 0 ? this._banners : [this.defaultBanner];
  }

  /** Hỗ trợ tương thích ngược với [images] */
  @Input()
  set images(val: HeroBannerImage[] | undefined | null) {
    this.banners = val;
  }
  get images(): HeroBannerImage[] {
    return this.banners;
  }

  ngOnInit(): void {
    if (this._banners.length === 0) {
      this._banners = [this.defaultBanner];
    }
    this.startTimer();
  }

  ngOnDestroy(): void {
    this.stopTimer();
  }

  prevSlide(): void {
    const total = this.banners.length;
    if (total <= 1) return;
    this.currentIndex.update((i) => (i - 1 + total) % total);
    this.resetTimer();
  }

  nextSlide(): void {
    const total = this.banners.length;
    if (total <= 1) return;
    this.currentIndex.update((i) => (i + 1) % total);
    this.resetTimer();
  }

  goToSlide(index: number): void {
    this.currentIndex.set(index);
    this.resetTimer();
  }

  private startTimer(): void {
    if (typeof window === 'undefined') return;
    this.stopTimer();
    if (this.banners.length > 1) {
      this.autoSlideTimer = setInterval(() => {
        this.nextSlide();
      }, 5000);
    }
  }

  private stopTimer(): void {
    if (this.autoSlideTimer) {
      clearInterval(this.autoSlideTimer);
      this.autoSlideTimer = null;
    }
  }

  private resetTimer(): void {
    this.stopTimer();
    this.startTimer();
  }
}
