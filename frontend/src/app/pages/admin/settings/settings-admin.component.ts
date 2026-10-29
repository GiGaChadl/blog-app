import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, FormsModule } from '@angular/forms';
import { AdminApiService, ContactInfoDto, SeoSettingsDto, SmtpSettingsDto } from '../../../core/services/admin-api.service';
import { SiteSettingsService } from '../../../core/services/site-settings.service';
import { extractErrorMessage } from '../../../core/utils/error-formatter';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputTextarea } from 'primeng/inputtextarea';
import { InputSwitchModule } from 'primeng/inputswitch';
import { DialogModule } from 'primeng/dialog';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-settings-admin',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    ButtonModule,
    InputTextModule,
    InputTextarea,
    InputSwitchModule,
    DialogModule
  ],
  templateUrl: './settings-admin.component.html',
  styleUrl: './settings-admin.component.scss'
})
export class SettingsAdminComponent implements OnInit {
  private api = inject(AdminApiService);
  private siteSettings = inject(SiteSettingsService);
  private fb = inject(FormBuilder);
  private messageService = inject(MessageService);

  activeTab = signal<'contact' | 'seo' | 'smtp'>('contact');
  isLoading = signal(false);
  isSaving = signal(false);

  // SMTP Test Dialog
  testMailDialogVisible = signal(false);
  testRecipientEmail = '';
  isSendingTest = signal(false);

  // Upload state
  isUploadingLogo = signal(false);
  isUploadingFavicon = signal(false);
  isUploadingOgImage = signal(false);

  contactForm: FormGroup = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    phone: [''],
    address: [''],
    bio: [''],
    about: [''],
    copyright: [''],
    facebookUrl: [''],
    githubUrl: [''],
    linkedinUrl: [''],
    youtubeUrl: [''],
    authorName: [''],
    slogan: ['']
  });

  seoForm: FormGroup = this.fb.group({
    siteName: ['', [Validators.required]],
    defaultTitle: ['', [Validators.required]],
    defaultDescription: [''],
    defaultKeywords: [''],
    defaultOgImage: [''],
    googleSiteVerification: [''],
    robotsTxtContent: [''],
    siteLogoUrl: [''],
    faviconUrl: ['']
  });

  smtpForm: FormGroup = this.fb.group({
    host: ['', [Validators.required]],
    port: [587, [Validators.required]],
    username: ['', [Validators.required]],
    password: [''],
    enableSsl: [true],
    senderEmail: ['', [Validators.required, Validators.email]],
    senderName: ['', [Validators.required]]
  });

  ngOnInit(): void {
    this.loadAllSettings();
  }

  loadAllSettings(): void {
    this.isLoading.set(true);

    this.api.getContactInfo().subscribe({
      next: (data) => {
        if (data) {
          this.contactForm.patchValue({
            ...data,
            address: data.address || data.location || '',
            about: data.about || '',
            copyright: data.copyright || '',
            authorName: data.authorName || '',
            slogan: data.slogan || ''
          });
          if (data.copyright) {
            this.siteSettings.updateCopyright(data.copyright);
          }
          if (data.authorName) {
            this.siteSettings.updateAuthorName(data.authorName);
          }
          if (data.slogan) {
            this.siteSettings.updateHeroSlogan(data.slogan);
          }
        }
      },
      error: (err) => {
        const msg = extractErrorMessage(err, 'Không thể tải thông tin liên hệ.');
        this.messageService.add({ severity: 'error', summary: 'Lỗi', detail: msg });
      }
    });

    this.api.getSeoSettings().subscribe({
      next: (data) => {
        if (data) {
          const siteName = data.siteName || localStorage.getItem('site_name') || 'Nhật Ký Của Tôi';
          const defaultTitle = data.defaultTitle || data.defaultMetaTitle || 'Nhật Ký Của Tôi — Blog cá nhân';
          const defaultDesc = data.defaultDescription || data.defaultMetaDescription || '';
          this.seoForm.patchValue({
            siteName,
            defaultTitle,
            defaultDescription: defaultDesc,
            defaultKeywords: data.defaultKeywords || '',
            defaultOgImage: data.defaultOgImage || '',
            googleSiteVerification: data.googleSiteVerification || '',
            robotsTxtContent: data.robotsTxtContent || '',
            siteLogoUrl: data.siteLogoUrl || localStorage.getItem('site_logo_url') || '',
            faviconUrl: data.faviconUrl || localStorage.getItem('site_favicon_url') || ''
          });
        }
      },
      error: (err) => {
        const msg = extractErrorMessage(err, 'Không thể tải cấu hình SEO.');
        this.messageService.add({ severity: 'error', summary: 'Lỗi', detail: msg });
      }
    });

    this.api.getSmtpSettings().subscribe({
      next: (data) => {
        if (data) {
          const cachedSsl = localStorage.getItem('admin_smtp_ssl');
          const isSsl = cachedSsl !== null
            ? cachedSsl === 'true'
            : (data.enableSsl !== undefined ? data.enableSsl : (data.port === 465 || data.port === 587 || true));

          this.smtpForm.patchValue({
            host: data.host,
            port: data.port,
            username: data.username,
            password: data.password || '',
            enableSsl: isSsl,
            senderEmail: data.senderEmail,
            senderName: data.senderName
          });
        }
        this.isLoading.set(false);
      },
      error: (err) => {
        this.isLoading.set(false);
        const msg = extractErrorMessage(err, 'Không thể tải cấu hình SMTP.');
        this.messageService.add({ severity: 'error', summary: 'Lỗi', detail: msg });
      }
    });
  }

  saveContact(): void {
    if (this.contactForm.invalid) {
      this.contactForm.markAllAsTouched();
      this.messageService.add({ severity: 'warn', summary: 'Thiếu thông tin', detail: 'Vui lòng kiểm tra lại địa chỉ email liên hệ.' });
      return;
    }
    this.isSaving.set(true);
    const val = this.contactForm.value;
    const payload = {
      ...val,
      location: val.address || val.location
    };
    this.api.updateContactInfo(payload).subscribe({
      next: (res) => {
        this.isSaving.set(false);
        if (res) {
          this.contactForm.patchValue({
            ...res,
            address: res.address || res.location || '',
            about: res.about || '',
            copyright: res.copyright || '',
            authorName: res.authorName || val.authorName || '',
            slogan: res.slogan || val.slogan || ''
          });
        }
        if (val.copyright) {
          this.siteSettings.updateCopyright(val.copyright);
        }
        if (val.authorName !== undefined) {
          this.siteSettings.updateAuthorName(val.authorName);
        }
        if (val.slogan !== undefined) {
          this.siteSettings.updateHeroSlogan(val.slogan);
        }
        this.messageService.add({
          severity: 'success',
          summary: 'Thành công',
          detail: 'Đã lưu thông tin liên hệ và giới thiệu tác giả.'
        });
      },
      error: (err) => {
        this.isSaving.set(false);
        const msg = extractErrorMessage(err, 'Không thể lưu thông tin liên hệ.');
        this.messageService.add({ severity: 'error', summary: 'Lỗi lưu thông tin', detail: msg });
      }
    });
  }

  onLogoSelected(event: any): void {
    const file = event.target?.files?.[0];
    if (!file) return;

    this.isUploadingLogo.set(true);
    this.api.uploadMedia(file).subscribe({
      next: (media) => {
        this.isUploadingLogo.set(false);
        const url = media.fileUrl || media.url;
        this.seoForm.patchValue({ siteLogoUrl: url });
        this.messageService.add({
          severity: 'success',
          summary: 'Tải logo thành công',
          detail: `Đã chọn logo "${file.name}". Bấm "Lưu Cấu Hình SEO" để hoàn tất.`
        });
      },
      error: (err) => {
        this.isUploadingLogo.set(false);
        const msg = extractErrorMessage(err, 'Tải logo thất bại.');
        this.messageService.add({ severity: 'error', summary: 'Lỗi tải logo', detail: msg });
      }
    });
  }

  onFaviconSelected(event: any): void {
    const file = event.target?.files?.[0];
    if (!file) return;

    this.isUploadingFavicon.set(true);
    this.api.uploadMedia(file).subscribe({
      next: (media) => {
        this.isUploadingFavicon.set(false);
        const url = media.fileUrl || media.url;
        this.seoForm.patchValue({ faviconUrl: url });
        this.messageService.add({
          severity: 'success',
          summary: 'Tải Favicon thành công',
          detail: `Đã chọn Favicon "${file.name}". Bấm "Lưu Cấu Hình SEO" để hoàn tất.`
        });
      },
      error: (err) => {
        this.isUploadingFavicon.set(false);
        const msg = extractErrorMessage(err, 'Tải Favicon thất bại.');
        this.messageService.add({ severity: 'error', summary: 'Lỗi tải Favicon', detail: msg });
      }
    });
  }

  onOgImageSelected(event: any): void {
    const file = event.target?.files?.[0];
    if (!file) return;

    this.isUploadingOgImage.set(true);
    this.api.uploadMedia(file).subscribe({
      next: (media) => {
        this.isUploadingOgImage.set(false);
        const url = media.fileUrl || media.url;
        this.seoForm.patchValue({ defaultOgImage: url });
        this.messageService.add({
          severity: 'success',
          summary: 'Tải ảnh Open Graph thành công',
          detail: `Đã chọn ảnh chia sẻ "${file.name}". Bấm "Lưu Cấu Hình SEO" để hoàn tất.`
        });
      },
      error: (err) => {
        this.isUploadingOgImage.set(false);
        const msg = extractErrorMessage(err, 'Tải ảnh Open Graph thất bại.');
        this.messageService.add({ severity: 'error', summary: 'Lỗi tải ảnh', detail: msg });
      }
    });
  }

  saveSeo(): void {
    if (this.seoForm.invalid) {
      this.seoForm.markAllAsTouched();
      this.messageService.add({ severity: 'warn', summary: 'Thiếu thông tin', detail: 'Vui lòng nhập tên website và tiêu đề mặc định.' });
      return;
    }
    this.isSaving.set(true);
    const formVal = this.seoForm.value;
    const payload = {
      ...formVal,
      defaultMetaTitle: formVal.defaultTitle,
      defaultMetaDescription: formVal.defaultDescription
    };

    this.api.updateSeoSettings(payload).subscribe({
      next: (res) => {
        this.isSaving.set(false);
        this.siteSettings.updateSettingsLocally({
          siteName: formVal.siteName,
          siteLogoUrl: formVal.siteLogoUrl,
          faviconUrl: formVal.faviconUrl,
          defaultTitle: formVal.defaultTitle,
          defaultDescription: formVal.defaultDescription
        });
        this.messageService.add({
          severity: 'success',
          summary: 'Thành công',
          detail: 'Đã cập nhật cấu hình SEO và thông tin Website thành công!'
        });
      },
      error: (err) => {
        this.isSaving.set(false);
        const msg = extractErrorMessage(err, 'Không thể lưu cấu hình SEO.');
        this.messageService.add({ severity: 'error', summary: 'Lỗi lưu SEO', detail: msg });
      }
    });
  }

  saveSmtp(): void {
    if (this.smtpForm.invalid) {
      this.smtpForm.markAllAsTouched();
      this.messageService.add({ severity: 'warn', summary: 'Thiếu thông tin', detail: 'Vui lòng điền đầy đủ các thông tin máy chủ SMTP.' });
      return;
    }
    this.isSaving.set(true);
    const val = this.smtpForm.value;
    localStorage.setItem('admin_smtp_ssl', String(val.enableSsl));

    this.api.updateSmtpSettings(val).subscribe({
      next: (res) => {
        this.isSaving.set(false);
        if (res) {
          this.smtpForm.patchValue({
            enableSsl: res.enableSsl !== undefined ? res.enableSsl : val.enableSsl
          });
        }
        this.messageService.add({
          severity: 'success',
          summary: 'Thành công',
          detail: 'Đã lưu cấu hình máy chủ Email SMTP.'
        });
      },
      error: (err) => {
        this.isSaving.set(false);
        const msg = extractErrorMessage(err, 'Không thể lưu cấu hình SMTP.');
        this.messageService.add({ severity: 'error', summary: 'Lỗi lưu SMTP', detail: msg });
      }
    });
  }

  openTestMailDialog(): void {
    this.testRecipientEmail = '';
    this.testMailDialogVisible.set(true);
  }

  sendTestMail(): void {
    if (!this.testRecipientEmail || !this.testRecipientEmail.includes('@')) {
      this.messageService.add({ severity: 'warn', summary: 'Cảnh báo', detail: 'Vui lòng nhập địa chỉ email hợp lệ.' });
      return;
    }

    this.isSendingTest.set(true);
    this.api.testSmtp(this.testRecipientEmail).subscribe({
      next: () => {
        this.isSendingTest.set(false);
        this.testMailDialogVisible.set(false);
        this.messageService.add({
          severity: 'success',
          summary: 'Thành công',
          detail: `Đã gửi email thử nghiệm tới ${this.testRecipientEmail}. Hãy kiểm tra hộp thư!`
        });
      },
      error: (err) => {
        this.isSendingTest.set(false);
        const msg = extractErrorMessage(err, 'Gửi email thử nghiệm thất bại. Vui lòng kiểm tra lại cấu hình SMTP.');
        this.messageService.add({ severity: 'error', summary: 'Gửi mail thất bại', detail: msg });
      }
    });
  }
}
