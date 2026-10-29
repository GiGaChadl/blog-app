import { Injectable, signal } from '@angular/core';

export type ThemeMode = 'light' | 'dark';

const STORAGE_KEY = 'blog_theme_mode';
/** Class gắn lên <html> khi ở chế độ tối — khớp với darkModeSelector trong app.config.ts và các biến trong styles.scss */
export const DARK_CLASS = 'theme-dark';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  mode = signal<ThemeMode>('light');

  constructor() {
    const saved = typeof window !== 'undefined' ? (window.localStorage.getItem(STORAGE_KEY) as ThemeMode | null) : null;
    const prefersDark =
      typeof window !== 'undefined' && window.matchMedia?.('(prefers-color-scheme: dark)').matches;
    const initial: ThemeMode = saved ?? (prefersDark ? 'dark' : 'light');
    this.apply(initial);
  }

  toggle() {
    this.apply(this.mode() === 'dark' ? 'light' : 'dark');
  }

  private apply(mode: ThemeMode) {
    this.mode.set(mode);
    if (typeof document === 'undefined') return;
    document.documentElement.classList.toggle(DARK_CLASS, mode === 'dark');
    window.localStorage.setItem(STORAGE_KEY, mode);
  }
}
