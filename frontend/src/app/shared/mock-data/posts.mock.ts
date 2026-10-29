import { Category, CategoryNode, PostSummary } from '../models/post.model';

/**
 * Danh mục 3 cấp: Danh mục cha (cấp 1) > Danh mục con (cấp 2) > Danh mục cháu (cấp 3).
 * Mỗi bài viết chỉ thuộc đúng 1 danh mục cấp 3 (đã chốt trong buổi trao đổi yêu cầu).
 */
export const MOCK_CATEGORIES: Category[] = [
  // Cấp 1 — Danh mục cha
  { id: 'c1', name: 'Công việc', slug: 'cong-viec', parentId: null },
  { id: 'c2', name: 'Học tập', slug: 'hoc-tap', parentId: null },
  { id: 'c3', name: 'Sức khoẻ', slug: 'suc-khoe', parentId: null },
  { id: 'c4', name: 'Du lịch', slug: 'du-lich', parentId: null },

  // Cấp 2 — Danh mục con (của "Công việc")
  { id: 'c1-1', name: 'Làm việc từ xa', slug: 'lam-viec-tu-xa', parentId: 'c1' },
  { id: 'c1-2', name: 'Quản lý dự án', slug: 'quan-ly-du-an', parentId: 'c1' },

  // Cấp 2 — của "Học tập"
  { id: 'c2-1', name: 'Kỹ năng mới', slug: 'ky-nang-moi', parentId: 'c2' },
  { id: 'c2-2', name: 'Đọc sách', slug: 'doc-sach', parentId: 'c2' },

  // Cấp 2 — của "Sức khoẻ"
  { id: 'c3-1', name: 'Vận động', slug: 'van-dong', parentId: 'c3' },

  // Cấp 2 — của "Du lịch"
  { id: 'c4-1', name: 'Trong nước', slug: 'trong-nuoc', parentId: 'c4' },

  // Cấp 3 — Danh mục cháu (của "Làm việc từ xa")
  { id: 'c1-1-1', name: 'Năng suất', slug: 'nang-suat', parentId: 'c1-1' },
  { id: 'c1-1-2', name: 'Cân bằng cuộc sống', slug: 'can-bang-cuoc-song', parentId: 'c1-1' },

  // Cấp 3 — của "Quản lý dự án"
  { id: 'c1-2-1', name: 'Công cụ cá nhân', slug: 'cong-cu-ca-nhan', parentId: 'c1-2' },

  // Cấp 3 — của "Kỹ năng mới"
  { id: 'c2-1-1', name: 'Thói quen học tập', slug: 'thoi-quen-hoc-tap', parentId: 'c2-1' },

  // Cấp 3 — của "Đọc sách"
  { id: 'c2-2-1', name: 'Review sách', slug: 'review-sach', parentId: 'c2-2' },

  // Cấp 3 — của "Vận động"
  { id: 'c3-1-1', name: 'Thói quen tập luyện', slug: 'thoi-quen-tap-luyen', parentId: 'c3-1' },

  // Cấp 3 — của "Trong nước"
  { id: 'c4-1-1', name: 'Đà Lạt', slug: 'da-lat', parentId: 'c4-1' }
];

/** Dựng cây danh mục 3 cấp từ danh sách phẳng — dùng cho mega-menu header & sidebar filter */
export function buildCategoryTree(categories: Category[] = MOCK_CATEGORIES): CategoryNode[] {
  const nodes = new Map<string, CategoryNode>(
    categories.map((c) => [c.id, { ...c, children: [] }])
  );
  const roots: CategoryNode[] = [];

  for (const cat of categories) {
    const node = nodes.get(cat.id)!;
    if (cat.parentId) {
      nodes.get(cat.parentId)?.children.push(node);
    } else {
      roots.push(node);
    }
  }
  return roots;
}

/** Tìm danh mục theo slug, dùng ở trang danh sách/lọc */
export function findCategoryBySlug(slug: string): Category | undefined {
  return MOCK_CATEGORIES.find((c) => c.slug === slug);
}

/** Trả về id của chính danh mục + toàn bộ danh mục con/cháu — dùng để lọc bài viết theo cả nhánh cây */
export function collectDescendantIds(categoryId: string): string[] {
  const result = [categoryId];
  const children = MOCK_CATEGORIES.filter((c) => c.parentId === categoryId);
  for (const child of children) {
    result.push(...collectDescendantIds(child.id));
  }
  return result;
}

export const ALL_TAGS = [
  'remote-work',
  'kinh-nghiem',
  'nang-suat',
  'hoc-tap',
  'thoi-quen',
  'du-lich',
  'suy-ngam',
  'suc-khoe',
  'cong-cu',
  'sach'
];

export const MOCK_POSTS: PostSummary[] = [
  {
    id: 'p1',
    title: 'Những bài học tôi rút ra sau 5 năm làm việc từ xa',
    slug: 'bai-hoc-5-nam-lam-viec-tu-xa',
    excerpt:
      'Làm việc từ xa không chỉ là thay đổi nơi ngồi làm — đó là một cách tổ chức lại toàn bộ nhịp sống. Đây là những điều tôi ước mình biết sớm hơn.',
    thumbnailUrl: 'https://picsum.photos/seed/blog-remote/800/600',
    category: MOCK_CATEGORIES.find(c => c.id === 'c1-1-1')!,
    tags: ['remote-work', 'kinh-nghiem', 'nang-suat'],
    publishedAt: '2026-08-12',
    readingTimeMinutes: 7,
    viewCount: 1420,
    avgRating: 4.6,
    ratingCount: 38
  },
  {
    id: 'p2',
    title: 'Cách tôi học một kỹ năng mới mỗi quý mà không bị kiệt sức',
    slug: 'hoc-ky-nang-moi-moi-quy',
    excerpt:
      'Bí quyết không nằm ở việc học nhiều hơn, mà ở việc chọn đúng thứ để học và biết khi nào nên dừng lại.',
    thumbnailUrl: 'https://picsum.photos/seed/blog-learning/800/600',
    category: MOCK_CATEGORIES.find(c => c.id === 'c2-1-1')!,
    tags: ['hoc-tap', 'thoi-quen'],
    publishedAt: '2026-08-05',
    readingTimeMinutes: 5,
    viewCount: 980,
    avgRating: 4.8,
    ratingCount: 22
  },
  {
    id: 'p3',
    title: 'Một buổi sáng ở Đà Lạt đã thay đổi cách tôi nhìn về nghỉ ngơi',
    slug: 'mot-buoi-sang-o-da-lat',
    excerpt:
      'Tôi từng nghĩ nghỉ ngơi là không làm gì cả. Chuyến đi ngắn này dạy tôi một định nghĩa khác.',
    thumbnailUrl: 'https://picsum.photos/seed/blog-dalat/800/600',
    category: MOCK_CATEGORIES.find(c => c.id === 'c4-1-1')!,
    tags: ['du-lich', 'suy-ngam'],
    publishedAt: '2026-07-28',
    readingTimeMinutes: 6,
    viewCount: 2210,
    avgRating: 4.9,
    ratingCount: 51
  },
  {
    id: 'p4',
    title: 'Tôi đã bỏ lịch tập gym 5 ngày/tuần — và khoẻ hơn',
    slug: 'bo-lich-tap-gym-5-ngay',
    excerpt: 'Ít hơn nhưng đều đặn hơn. Đây là cách tôi thiết kế lại thói quen vận động của mình.',
    thumbnailUrl: 'https://picsum.photos/seed/blog-fitness/800/600',
    category: MOCK_CATEGORIES.find(c => c.id === 'c3-1-1')!,
    tags: ['suc-khoe', 'thoi-quen'],
    publishedAt: '2026-07-19',
    readingTimeMinutes: 4,
    viewCount: 640,
    avgRating: 4.3,
    ratingCount: 15
  },
  {
    id: 'p5',
    title: 'Sổ tay quản lý dự án cá nhân tôi dùng suốt 3 năm qua',
    slug: 'so-tay-quan-ly-du-an-ca-nhan',
    excerpt: 'Không phải Notion, không phải Trello — chỉ là vài nguyên tắc đơn giản tôi tự đặt ra.',
    thumbnailUrl: 'https://picsum.photos/seed/blog-project/800/600',
    category: MOCK_CATEGORIES.find(c => c.id === 'c1-2-1')!,
    tags: ['nang-suat', 'cong-cu'],
    publishedAt: '2026-07-10',
    readingTimeMinutes: 8,
    viewCount: 512,
    avgRating: 4.5,
    ratingCount: 11
  },
  {
    id: 'p6',
    title: 'Những cuốn sách đã âm thầm định hình cách tôi suy nghĩ',
    slug: 'nhung-cuon-sach-dinh-hinh-suy-nghi',
    excerpt: 'Không phải sách best-seller nào cũng thay đổi tôi — mà là những cuốn tôi đọc đúng lúc.',
    thumbnailUrl: 'https://picsum.photos/seed/blog-books/800/600',
    category: MOCK_CATEGORIES.find(c => c.id === 'c2-2-1')!,
    tags: ['hoc-tap', 'sach'],
    publishedAt: '2026-06-30',
    readingTimeMinutes: 6,
    viewCount: 305,
    avgRating: 4.7,
    ratingCount: 9
  }
];

export const MOCK_POST_DETAIL: Record<string, { contentHtml: string; reviews: import('../models/post.model').PostReview[] }> = {
  'bai-hoc-5-nam-lam-viec-tu-xa': {
    contentHtml: `
      <p>Năm năm trước, khi lần đầu chuyển sang làm việc từ xa hoàn toàn, tôi nghĩ điều khó nhất sẽ là kỷ luật bản thân. Hoá ra điều khó nhất lại là một thứ tôi chưa từng nghĩ tới: ranh giới.</p>
      <h2>1. Không gian quyết định nhịp độ</h2>
      <p>Tôi từng làm việc ngay trên giường trong ba tháng đầu. Năng suất giảm, nhưng điều tệ hơn là tôi không còn cảm giác "tan làm" — công việc len vào mọi góc trong nhà.</p>
      <blockquote>Một chiếc bàn cố định, dù nhỏ, vẫn tốt hơn một không gian linh hoạt vô hạn.</blockquote>
      <h2>2. Giao tiếp chủ động thay vì chờ được hỏi</h2>
      <p>Làm từ xa nghĩa là im lặng dễ bị hiểu nhầm thành không làm gì. Tôi học cách cập nhật tiến độ ngắn gọn mỗi ngày, dù không ai yêu cầu.</p>
      <h2>3. Nghỉ ngơi cũng cần lên lịch</h2>
      <p>Không có tiếng chuông tan sở, tôi phải tự đặt ra tín hiệu kết thúc ngày làm việc cho chính mình — với tôi, đó là một cuốc đi bộ 15 phút, không mang điện thoại.</p>
    `,
    reviews: [
      {
        id: 'r1',
        displayName: 'minh.tran',
        rating: 5,
        comment: 'Bài viết đúng những gì mình đang trải qua, đặc biệt là phần ranh giới không gian làm việc.',
        createdAt: '2026-08-14',
        isOwnedByCurrentSession: false
      },
      {
        id: 'r2',
        displayName: 'lan.pham',
        rating: 4,
        comment: 'Hay, nhưng mong tác giả chia sẻ thêm về cách quản lý thời gian với múi giờ khác nhau.',
        createdAt: '2026-08-13',
        isOwnedByCurrentSession: false
      }
    ]
  }
};
