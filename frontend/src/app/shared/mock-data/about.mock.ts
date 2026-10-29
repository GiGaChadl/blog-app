export interface Project {
  id: string;
  title: string;
  imageUrl: string;
  shortDescription: string;
  tag: string;
}

export const MOCK_PROJECTS: Project[] = [
  {
    id: 'pr1',
    title: 'Công cụ theo dõi thói quen cá nhân',
    imageUrl: 'https://picsum.photos/seed/project-habit/600/600',
    shortDescription: 'Ứng dụng nhỏ giúp tôi theo dõi 5 thói quen quan trọng nhất mỗi ngày, không cần tài khoản.',
    tag: 'Sản phẩm cá nhân'
  },
  {
    id: 'pr2',
    title: 'Bộ mẫu ghi chú họp cho team remote',
    imageUrl: 'https://picsum.photos/seed/project-notes/600/600',
    shortDescription: 'Chia sẻ miễn phí cho vài đội nhóm nhỏ, giúp họp ngắn gọn và có action rõ ràng hơn.',
    tag: 'Công cụ làm việc'
  },
  {
    id: 'pr3',
    title: 'Chuỗi bài viết "30 ngày viết đều"',
    imageUrl: 'https://picsum.photos/seed/project-writing/600/600',
    shortDescription: 'Thử thách viết mỗi ngày trong 30 ngày để rèn thói quen diễn đạt suy nghĩ mạch lạc hơn.',
    tag: 'Viết lách'
  }
];

export const CONTACT_INFO = {
  email: 'hello@nhatkycuatoi.dev',
  location: 'TP. Hồ Chí Minh, Việt Nam',
  socials: [
    { label: 'Facebook', icon: 'pi-facebook', url: '#' },
    { label: 'LinkedIn', icon: 'pi-linkedin', url: '#' },
    { label: 'GitHub', icon: 'pi-github', url: '#' }
  ]
};
