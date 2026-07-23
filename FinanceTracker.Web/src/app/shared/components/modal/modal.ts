import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-modal',
  imports: [],
  templateUrl: './modal.html',
  styleUrl: './modal.scss',
})
export class Modal {
  readonly title = input.required<string>();
  readonly close = output<void> ();

  onBackdropClick(): void {
    this.close.emit();
  }

  onCloseButton(): void {
    this.close.emit();
  }
}
