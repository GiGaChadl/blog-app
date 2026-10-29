import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { BlogApiService } from '../../core/services/blog-api.service';

interface DisplayProject {
  id: string;
  title: string;
  shortDescription: string;
  tag: string;
  imageUrl: string;
}

interface DisplayContact {
  email: string;
  location: string;
  phone?: string;
  bio?: string;
  about?: string;
  socials: { label: string; url: string; icon: string }[];
}

@Component({
  selector: 'app-about',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './about.component.html',
  styleUrl: './about.component.scss'
})
export class AboutComponent implements OnInit {
  private blogApi = inject(BlogApiService);

  projects = signal<DisplayProject[]>([]);
  contact = signal<DisplayContact>({
    email: '',
    location: '',
    socials: []
  });

  ngOnInit(): void {
    this.blogApi.getProjects().subscribe({
      next: (list) => {
        const mapped: DisplayProject[] = list.map((p) => ({
          id: p.id,
          title: p.title,
          shortDescription: p.shortDescription || '',
          tag: p.tag || 'Dự án',
          imageUrl: p.mediaUrl || 'https://picsum.photos/seed/' + p.id + '/600/400'
        }));
        this.projects.set(mapped);
      },
      error: () => this.projects.set([])
    });

    this.blogApi.getContactInfo().subscribe({
      next: (dto) => {
        if (!dto) return;
        this.contact.set({
          email: dto.email || '',
          location: dto.location || 'Việt Nam',
          phone: dto.phone,
          bio: dto.bio,
          about: dto.about,
          socials: (dto.socialLinks || []).map((s) => ({
            label: s.platform,
            url: s.url,
            icon: s.icon || this.getPlatformIcon(s.platform)
          }))
        });
      },
      error: () => {}
    });
  }

  private getPlatformIcon(platform: string): string {
    const p = platform.toLowerCase();
    if (p.includes('facebook')) return 'pi-facebook';
    if (p.includes('github')) return 'pi-github';
    if (p.includes('linkedin')) return 'pi-linkedin';
    if (p.includes('youtube')) return 'pi-youtube';
    if (p.includes('twitter') || p.includes('x')) return 'pi-twitter';
    return 'pi-link';
  }
}

