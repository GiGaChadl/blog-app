import { CommonModule } from '@angular/common';
import { Component, Input, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CategoryNode } from '../../models/post.model';
import { BlogApiService } from '../../../core/services/blog-api.service';

/** Số danh mục cấp 3 tối đa hiện ra khi mở 1 danh mục cấp 2 — phần còn lại xem qua "Xem tiếp…" */
const MAX_GRANDCHILDREN_SHOWN = 5;

@Component({
  selector: 'app-category-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './category-sidebar.component.html',
  styleUrl: './category-sidebar.component.scss'
})
export class CategorySidebarComponent implements OnInit {
  private blogApi = inject(BlogApiService);

  /** Slug danh mục đang active (bất kể cấp 1/2/3) — dùng để tô đậm mục đang chọn */
  @Input() activeCategorySlug: string | null = null;
  @Input() categoryTree: CategoryNode[] = [];

  maxGrandchildrenShown = MAX_GRANDCHILDREN_SHOWN;

  /** Danh mục cấp 1 (cha) nào đang mở — mặc định mở hết để người đọc thấy ngay cấu trúc tổng quan */
  expandedParents = signal<Set<string>>(new Set());

  ngOnInit(): void {
    if (!this.categoryTree || this.categoryTree.length === 0) {
      this.blogApi.getCategoryTree().subscribe({
        next: (tree) => {
          this.categoryTree = tree;
          this.expandedParents.set(new Set(tree.map((c) => c.id)));
        },
        error: () => (this.categoryTree = [])
      });
    } else {
      this.expandedParents.set(new Set(this.categoryTree.map((c) => c.id)));
    }
  }

  /**
   * Danh mục cấp 2 (con) nào đang mở — mặc định ĐÓNG hết (khác cấp 1) để sidebar gọn,
   * nhất là khi dùng lại ở trang chi tiết bài viết vốn đã có nhiều nội dung khác bên cạnh.
   */
  expandedChildren = signal<Set<string>>(new Set());

  toggleParent(id: string) {
    this.expandedParents.update((set) => {
      const next = new Set(set);
      next.has(id) ? next.delete(id) : next.add(id);
      return next;
    });
  }

  toggleChild(id: string) {
    this.expandedChildren.update((set) => {
      const next = new Set(set);
      next.has(id) ? next.delete(id) : next.add(id);
      return next;
    });
  }

  isParentExpanded(id: string): boolean {
    return this.expandedParents().has(id);
  }

  isChildExpanded(id: string): boolean {
    return this.expandedChildren().has(id);
  }

  /** Chỉ trả về tối đa 5 danh mục cấp 3 gần nhất để hiện trực tiếp trong sidebar */
  visibleGrandchildren(child: CategoryNode): CategoryNode[] {
    return child.children.slice(0, this.maxGrandchildrenShown);
  }

  hasMoreGrandchildren(child: CategoryNode): boolean {
    return child.children.length > this.maxGrandchildrenShown;
  }
}
