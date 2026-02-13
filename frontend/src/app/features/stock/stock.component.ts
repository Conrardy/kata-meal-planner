import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { RouterLink } from '@angular/router';
import {
  LucideAngularModule,
  Package,
  Apple,
  Milk,
  Beef,
  AlertTriangle,
  Clock,
} from 'lucide-angular';
import { StockService } from '../../core/services/stock.service';
import { StockItem } from '../../core/models/stock.model';

interface StockCategoryGroup {
  category: string;
  items: StockItem[];
}

@Component({
  selector: 'app-stock',
  standalone: true,
  imports: [RouterLink, LucideAngularModule],
  templateUrl: './stock.component.html',
})
export class StockComponent implements OnInit {
  private readonly stockService = inject(StockService);

  readonly stockItems = signal<StockItem[]>([]);
  readonly isLoading = signal(true);
  readonly error = signal<string | null>(null);

  readonly Package = Package;
  readonly Apple = Apple;
  readonly Milk = Milk;
  readonly Beef = Beef;
  readonly AlertTriangle = AlertTriangle;
  readonly Clock = Clock;

  readonly categoryOrder = ['Produce', 'Dairy', 'Meat', 'Pantry'];

  readonly groupedByCategory = computed<StockCategoryGroup[]>(() => {
    const items = this.stockItems();
    const groups = new Map<string, StockItem[]>();

    for (const category of this.categoryOrder) {
      const categoryItems = items.filter(item => item.category === category);
      if (categoryItems.length > 0) {
        groups.set(category, categoryItems);
      }
    }

    return Array.from(groups.entries()).map(([category, categoryItems]) => ({
      category,
      items: categoryItems,
    }));
  });

  readonly totalItems = computed(() => this.stockItems().length);

  ngOnInit(): void {
    this.loadStock();
  }

  private loadStock(): void {
    this.isLoading.set(true);
    this.error.set(null);

    this.stockService.getStockItems().subscribe({
      next: (list) => {
        this.stockItems.set(list.items);
        this.isLoading.set(false);
      },
      error: () => {
        this.error.set('Impossible de charger le stock. Veuillez réessayer.');
        this.isLoading.set(false);
      },
    });
  }

  isLowStock(item: StockItem): boolean {
    return item.lowStockThreshold !== null && item.quantity <= item.lowStockThreshold;
  }

  isExpiringSoon(item: StockItem): boolean {
    if (!item.expirationDate) return false;
    const expDate = new Date(item.expirationDate);
    const now = new Date();
    const threeDaysFromNow = new Date(now);
    threeDaysFromNow.setDate(threeDaysFromNow.getDate() + 3);
    return expDate <= threeDaysFromNow;
  }

  formatQuantity(item: StockItem): string {
    return `${item.quantity} ${item.unit}`;
  }

  formatExpirationDate(dateStr: string): string {
    const date = new Date(dateStr);
    return date.toLocaleDateString('fr-FR', { day: 'numeric', month: 'short' });
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
}
