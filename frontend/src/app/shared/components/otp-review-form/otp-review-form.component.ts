import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';
import { CheckboxModule } from 'primeng/checkbox';
import { StarRatingComponent } from '../star-rating/star-rating.component';
import { BlogApiService } from '../../../core/services/blog-api.service';

type Step = 'write' | 'otp-sent' | 'verified';

const EMAIL_PATTERN = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
const OTP_SESSION_KEY = 'review_session_email';
const OTP_SESSION_ID = 'review_session_id';

export interface NewReviewSubmit {
  rating: number;
  comment: string;
  email: string;
  sessionId?: string;
}

@Component({
  selector: 'app-otp-review-form',
  standalone: true,
  imports: [CommonModule, FormsModule, InputTextModule, CheckboxModule, StarRatingComponent],
  templateUrl: './otp-review-form.component.html',
  styleUrl: './otp-review-form.component.scss'
})
export class OtpReviewFormComponent {
  private blogApi = inject(BlogApiService);

  @Input() postId = '';
  @Output() submitted = new EventEmitter<NewReviewSubmit>();

  rating = 0;
  comment = '';
  email = '';
  otpCode = '';
  recaptchaChecked = false;

  step = signal<Step>('write');
  sendingOtp = signal(false);
  verifying = signal(false);
  errorMsg = signal('');
  otpAttemptsUsed = signal(0);
  readonly maxOtpAttempts = 5;

  constructor() {
    const existing = typeof window !== 'undefined'
      ? (window.sessionStorage.getItem(OTP_SESSION_KEY) || window.localStorage.getItem(OTP_SESSION_KEY))
      : null;
    if (existing) {
      this.email = existing;
      this.step.set('verified');
    }
  }

  requestOtp() {
    this.errorMsg.set('');

    if (this.rating === 0) {
      this.errorMsg.set('Vui lòng chọn số sao trước khi gửi đánh giá.');
      return;
    }
    if (!this.comment.trim()) {
      this.errorMsg.set('Vui lòng nhập nội dung nhận xét.');
      return;
    }
    if (!EMAIL_PATTERN.test(this.email.trim())) {
      this.errorMsg.set('Email chưa đúng định dạng.');
      return;
    }
    if (this.otpAttemptsUsed() >= this.maxOtpAttempts) {
      this.errorMsg.set('Bạn đã đạt giới hạn 5 lần nhận mã OTP trong 24h. Vui lòng thử lại sau.');
      return;
    }

    this.sendingOtp.set(true);
    this.blogApi.sendReviewOtp(this.postId || '', this.email.trim()).subscribe({
      next: () => {
        this.sendingOtp.set(false);
        this.otpAttemptsUsed.update((v) => v + 1);
        this.step.set('otp-sent');
      },
      error: (err) => {
        this.sendingOtp.set(false);
        this.errorMsg.set(err?.error?.message || 'Không thể gửi mã OTP. Vui lòng thử lại sau.');
      }
    });
  }

  verifyOtp() {
    this.errorMsg.set('');
    if (this.otpCode.trim().length !== 6) {
      this.errorMsg.set('Mã OTP gồm 6 chữ số.');
      return;
    }

    this.verifying.set(true);
    this.blogApi.verifyReviewOtp(this.email.trim(), this.otpCode.trim()).subscribe({
      next: (res) => {
        this.verifying.set(false);
        if (typeof window !== 'undefined') {
          window.sessionStorage.setItem(OTP_SESSION_KEY, res.email);
          window.sessionStorage.setItem(OTP_SESSION_ID, res.sessionId);
          window.localStorage.setItem(OTP_SESSION_KEY, res.email);
          window.localStorage.setItem(OTP_SESSION_ID, res.sessionId);
        }
        this.step.set('verified');
        this.emitReview(res.sessionId);
      },
      error: (err) => {
        this.verifying.set(false);
        this.errorMsg.set(err?.error?.message || 'Mã OTP không đúng hoặc đã hết hạn.');
      }
    });
  }

  private emitReview(sessionId?: string) {
    this.submitted.emit({
      rating: this.rating,
      comment: this.comment,
      email: this.email,
      sessionId
    });
    this.comment = '';
    this.rating = 0;
  }

  submitWithExistingSession() {
    if (this.rating === 0 || !this.comment.trim()) {
      this.errorMsg.set('Vui lòng chọn số sao và nhập nhận xét.');
      return;
    }
    const sessionId =
      typeof window !== 'undefined'
        ? (window.sessionStorage.getItem(OTP_SESSION_ID) || window.localStorage.getItem(OTP_SESSION_ID)) || undefined
        : undefined;
    this.emitReview(sessionId);
  }

  changeEmail() {
    if (typeof window !== 'undefined') {
      window.sessionStorage.removeItem(OTP_SESSION_KEY);
      window.sessionStorage.removeItem(OTP_SESSION_ID);
      window.localStorage.removeItem(OTP_SESSION_KEY);
      window.localStorage.removeItem(OTP_SESSION_ID);
    }
    this.step.set('write');
    this.email = '';
    this.otpCode = '';
  }
}
