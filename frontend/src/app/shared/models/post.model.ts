export interface Category {
  id: string;
  name: string;
  slug: string;
  /** null nếu là danh mục cha (cấp 1) */
  parentId: string | null;
}

/** Danh mục đã dựng thành cây 3 cấp, dùng để render mega-menu / sidebar filter */
export interface CategoryNode extends Category {
  children: CategoryNode[];
}

export interface PostSummary {
  id: string;
  title: string;
  slug: string;
  excerpt: string;
  thumbnailUrl: string;
  category: Category;
  tags: string[];
  publishedAt: string; // ISO date
  readingTimeMinutes: number;
  viewCount: number;
  avgRating: number; // 0–5
  ratingCount: number;
}

export interface PostReview {
  id: string;
  displayName: string; // phần trước @ của email, ẩn phần còn lại
  rating: number;
  comment: string;
  createdAt: string;
  isOwnedByCurrentSession: boolean; // để hiện nút Sửa/Xoá
}

export interface PostDetail extends PostSummary {
  contentHtml: string;
  reviews: PostReview[];
}
