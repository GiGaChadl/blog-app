import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { InputTextModule } from 'primeng/inputtext';
import { BlogApiService } from '../../core/services/blog-api.service';

type State = 'idle' | 'submitting' | 'success' | 'error';

@Component({
  selector: 'app-unsubscribe',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, InputTextModule],
  templateUrl: './unsubscribe.component.html',
  styleUrl: './unsubscribe.component.scss'
})
export class UnsubscribeComponent {
  private blogApi = inject(BlogApiService);
  private route = inject(ActivatedRoute);

  email = '';
  state = signal<State>('idle');
  errorMsg = signal('');

  private readonly emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

  constructor() {
    const emailFromLink = this.route.snapshot.queryParamMap.get('email');
    if (emailFromLink) {
      this.email = emailFromLink;
    }
  }

  submit() {
    this.errorMsg.set('');
    if (!this.emailPattern.test(this.email.trim())) {
      this.errorMsg.set('Email chưa đúng định dạng.');
      this.state.set('error');
      return;
    }

    this.state.set('submitting');
    this.blogApi.unsubscribe(this.email.trim()).subscribe({
      next: () => {
        this.state.set('success');
      },
      error: (err) => {
        this.state.set('error');
        this.errorMsg.set(err?.error?.message || 'Không thể hủy đăng ký. Vui lòng thử lại sau.');
      }
    });
  }
}
