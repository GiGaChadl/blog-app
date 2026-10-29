import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { BlogApiService } from '../../services/blog-api.service';

const STORAGE_KEY = 'cookie_consent_choice'; // 'accepted' | 'declined'

function setCookie(name: string, value: string, days: number = 365): void {
  if (typeof document === 'undefined') return;
  const expires = new Date();
  expires.setTime(expires.getTime() + days * 24 * 60 * 60 * 1000);
  document.cookie = `${name}=${encodeURIComponent(value)};expires=${expires.toUTCString()};path=/;SameSite=Lax`;
}

function getCookie(name: string): string | null {
  if (typeof document === 'undefined') return null;
  const match = document.cookie.match(new RegExp('(^|;\\s*)(' + name + ')=([^;]*)'));
  return match ? decodeURIComponent(match[3]) : null;
}

function getOrCreateVisitorToken(): string {
  if (typeof window === 'undefined') return '';
  let token = getCookie('visitor_token') || window.localStorage.getItem('visitor_token');
  if (!token) {
    token = 'v_' + Math.random().toString(36).substring(2, 15) + '_' + Date.now().toString(36);
    setCookie('visitor_token', token, 365);
    try {
      window.localStorage.setItem('visitor_token', token);
    } catch {}
  }
  return token;
}

@Component({
  selector: 'app-cookie-consent',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './cookie-consent.component.html',
  styleUrl: './cookie-consent.component.scss'
})
export class CookieConsentComponent implements OnInit {
  private blogApi = inject(BlogApiService);
  isVisible = signal(false);

  ngOnInit() {
    const cookieChoice = getCookie(STORAGE_KEY);
    const storageChoice = typeof window !== 'undefined' ? window.localStorage.getItem(STORAGE_KEY) : null;
    const choice = cookieChoice || storageChoice;

    if (!choice) {
      this.isVisible.set(true);
    }
  }

  accept() {
    setCookie(STORAGE_KEY, 'accepted', 365);
    if (typeof window !== 'undefined') {
      try {
        window.localStorage.setItem(STORAGE_KEY, 'accepted');
      } catch {}
    }

    const token = getOrCreateVisitorToken();
    if (token) {
      this.blogApi.recordCookieConsent(token, 'Accepted').subscribe({
        next: () => {},
        error: () => {}
      });
    }

    this.isVisible.set(false);
  }

  decline() {
    setCookie(STORAGE_KEY, 'declined', 365);
    if (typeof window !== 'undefined') {
      try {
        window.localStorage.setItem(STORAGE_KEY, 'declined');
      } catch {}
    }

    const token = getOrCreateVisitorToken();
    if (token) {
      this.blogApi.recordCookieConsent(token, 'Declined').subscribe({
        next: () => {},
        error: () => {}
      });
    }

    this.isVisible.set(false);
  }
}
