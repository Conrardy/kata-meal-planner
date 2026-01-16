import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { RouterLink } from '@angular/router';
import {
  LucideAngularModule,
  ChevronLeft,
  ChevronRight,
  ShoppingCart,
  Apple,
  Milk,
  Beef,
  Package,
  Printer,
} from 'lucide-angular';
import { ShoppingListService } from '../../core/services/shopping-list.service';
import {
  ShoppingList,
  ShoppingItem,
} from '../../core/models/shopping-list.model';

@Component({
  selector: 'app-shopping-list',
  standalone: true,
  imports: [RouterLink, LucideAngularModule],
  templateUrl: './shopping-list.component.html',
})
export class ShoppingListComponent implements OnInit {
  private readonly shoppingListService = inject(ShoppingListService);

  readonly startDate = signal<Date>(this.getStartOfWeek(new Date()));
  readonly shoppingList = signal<ShoppingList | null>(null);
  readonly isLoading = signal(true);
  readonly error = signal<string | null>(null);

  readonly ChevronLeft = ChevronLeft;
  readonly ChevronRight = ChevronRight;
  readonly ShoppingCart = ShoppingCart;
  readonly Apple = Apple;
  readonly Milk = Milk;
  readonly Beef = Beef;
  readonly Package = Package;
  readonly Printer = Printer;

  readonly formattedDateRange = computed(() => {
    const list = this.shoppingList();
    if (!list) return '';
    return `${this.formatDisplayDate(list.startDate)} - ${this.formatDisplayDate(list.endDate)}`;
  });

  readonly totalItems = computed(() => {
    const list = this.shoppingList();
    if (!list) return 0;
    return list.categories.reduce((sum, cat) => sum + cat.items.length, 0);
  });

  ngOnInit(): void {
    this.loadShoppingList();
  }

  private loadShoppingList(): void {
    this.isLoading.set(true);
    this.error.set(null);

    this.shoppingListService.generateShoppingList(this.startDate()).subscribe({
      next: (list: ShoppingList) => {
        this.shoppingList.set(list);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Failed to generate shopping list:', err);
        this.error.set(
          'Failed to generate shopping list. Please try again later.'
        );
        this.isLoading.set(false);
      },
    });
  }

  onPreviousWeek(): void {
    const newStart = new Date(this.startDate());
    newStart.setDate(newStart.getDate() - 7);
    this.startDate.set(newStart);
    this.loadShoppingList();
  }

  onNextWeek(): void {
    const newStart = new Date(this.startDate());
    newStart.setDate(newStart.getDate() + 7);
    this.startDate.set(newStart);
    this.loadShoppingList();
  }

  onPrint(): void {
    window.print();
  }

  getCategoryIcon(category: string): typeof Apple {
    switch (category) {
      case 'Produce':
        return Apple;
      case 'Dairy':
        return Milk;
      case 'Meat':
        return Beef;
      case 'Pantry':
      default:
        return Package;
    }
  }

  getCategoryColor(category: string): string {
    switch (category) {
      case 'Produce':
        return 'bg-green-100 text-green-700';
      case 'Dairy':
        return 'bg-blue-100 text-blue-700';
      case 'Meat':
        return 'bg-red-100 text-red-700';
      case 'Pantry':
      default:
        return 'bg-amber-100 text-amber-700';
    }
  }

  formatQuantity(item: ShoppingItem): string {
    if (item.unit) {
      return `${item.quantity} ${item.unit}`;
    }
    return item.quantity;
  }

  private getStartOfWeek(date: Date): Date {
    const d = new Date(date);
    const day = d.getDay();
    d.setDate(d.getDate() - day);
    d.setHours(0, 0, 0, 0);
    return d;
  }

  private formatDisplayDate(dateStr: string): string {
    const date = new Date(dateStr);
    return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
  }
}
