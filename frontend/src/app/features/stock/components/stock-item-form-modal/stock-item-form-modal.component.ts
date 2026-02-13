import { Component, input, output, signal, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { LucideAngularModule, X, Trash2 } from 'lucide-angular';
import { StockItem, CreateStockItemRequest, UpdateStockItemRequest } from '../../../../core/models/stock.model';
import { StockService } from '../../../../core/services/stock.service';

@Component({
  selector: 'app-stock-item-form-modal',
  standalone: true,
  imports: [ReactiveFormsModule, LucideAngularModule],
  templateUrl: './stock-item-form-modal.component.html',
})
export class StockItemFormModalComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly stockService = inject(StockService);

  item = input<StockItem | null>(null);
  close = output<void>();
  saved = output<void>();

  readonly form: FormGroup;
  readonly isSaving = signal(false);
  readonly isDeleting = signal(false);
  readonly showDeleteConfirm = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly XIcon = X;
  readonly Trash2Icon = Trash2;

  readonly categories = ['Produce', 'Dairy', 'Meat', 'Pantry'];
  readonly units = ['g', 'kg', 'ml', 'L', 'pcs', 'unit'];

  get isEditMode(): boolean {
    return this.item() !== null;
  }

  constructor() {
    this.form = this.fb.group({
      ingredientName: ['', [Validators.required, Validators.maxLength(200)]],
      quantity: [1, [Validators.required, Validators.min(0)]],
      unit: ['pcs', Validators.required],
      category: ['Produce', Validators.required],
      expirationDate: [''],
      lowStockThreshold: [null as number | null],
    });
  }

  ngOnInit(): void {
    const existing = this.item();
    if (existing) {
      this.form.patchValue({
        ingredientName: existing.ingredientName,
        quantity: existing.quantity,
        unit: existing.unit,
        category: existing.category,
        expirationDate: existing.expirationDate
          ? existing.expirationDate.substring(0, 10)
          : '',
        lowStockThreshold: existing.lowStockThreshold,
      });
    }
  }

  onClose(): void {
    this.close.emit();
  }

  onBackdropClick(event: MouseEvent): void {
    if ((event.target as HTMLElement).classList.contains('modal-backdrop')) {
      this.close.emit();
    }
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set(null);

    const formValue = this.form.value;
    const request: CreateStockItemRequest = {
      ingredientName: formValue.ingredientName.trim(),
      quantity: formValue.quantity,
      unit: formValue.unit,
      category: formValue.category,
      expirationDate: formValue.expirationDate || null,
      lowStockThreshold: formValue.lowStockThreshold ?? null,
    };

    const existing = this.item();
    if (existing) {
      this.stockService.updateStockItem(existing.id, request as UpdateStockItemRequest).subscribe({
        next: () => {
          this.isSaving.set(false);
          this.saved.emit();
        },
        error: () => {
          this.errorMessage.set('Impossible de modifier l\'article. Veuillez réessayer.');
          this.isSaving.set(false);
        },
      });
    } else {
      this.stockService.createStockItem(request).subscribe({
        next: () => {
          this.isSaving.set(false);
          this.saved.emit();
        },
        error: () => {
          this.errorMessage.set('Impossible d\'ajouter l\'article. Veuillez réessayer.');
          this.isSaving.set(false);
        },
      });
    }
  }

  onDeleteClick(): void {
    this.showDeleteConfirm.set(true);
  }

  onCancelDelete(): void {
    this.showDeleteConfirm.set(false);
  }

  onConfirmDelete(): void {
    const existing = this.item();
    if (!existing) return;

    this.isDeleting.set(true);
    this.errorMessage.set(null);

    this.stockService.deleteStockItem(existing.id).subscribe({
      next: () => {
        this.isDeleting.set(false);
        this.saved.emit();
      },
      error: () => {
        this.errorMessage.set('Impossible de supprimer l\'article. Veuillez réessayer.');
        this.isDeleting.set(false);
        this.showDeleteConfirm.set(false);
      },
    });
  }

  hasError(field: string): boolean {
    const control = this.form.get(field);
    return !!control && control.invalid && control.touched;
  }
}
