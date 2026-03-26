import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-reject-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './reject-modal.component.html',
  styleUrls: ['./reject-modal.component.scss'],
})
export class RejectModalComponent {
  @Input() show = false;
  @Input() productName = '';
  @Output() confirm = new EventEmitter<string>();
  @Output() cancel = new EventEmitter<void>();

  reason = '';
  submitted = false;

  get isReasonEmpty(): boolean {
    return this.reason.trim().length === 0;
  }

  onConfirm(): void {
    this.submitted = true;
    if (this.isReasonEmpty) return;
    this.confirm.emit(this.reason.trim());
    this.reason = '';
    this.submitted = false;
  }

  onCancel(): void {
    this.reason = '';
    this.submitted = false;
    this.cancel.emit();
  }
}