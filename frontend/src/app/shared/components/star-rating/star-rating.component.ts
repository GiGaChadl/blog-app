import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-star-rating',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './star-rating.component.html',
  styleUrl: './star-rating.component.scss'
})
export class StarRatingComponent {
  @Input() value = 0;
  @Input() readOnly = true;
  @Input() size: 'sm' | 'md' | 'lg' = 'md';
  @Output() valueChange = new EventEmitter<number>();

  stars = [1, 2, 3, 4, 5];
  hovered = 0;

  select(star: number) {
    if (this.readOnly) return;
    this.value = star;
    this.valueChange.emit(star);
  }

  hover(star: number) {
    if (this.readOnly) return;
    this.hovered = star;
  }

  resetHover() {
    this.hovered = 0;
  }

  isFilled(star: number): boolean {
    const ref = this.hovered || this.value;
    return star <= Math.round(ref);
  }
}
