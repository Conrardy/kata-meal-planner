import { Component, output } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { LucideAngularModule, Plus, CalendarDays, ShoppingCart, Home, Calendar } from 'lucide-angular';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, LucideAngularModule],
  templateUrl: './sidebar.component.html',
})
export class SidebarComponent {
  addRecipe = output<void>();
  createPlan = output<void>();
  generateList = output<void>();

  readonly Home = Home;
  readonly Calendar = Calendar;
  readonly Plus = Plus;
  readonly CalendarDays = CalendarDays;
  readonly ShoppingCart = ShoppingCart;

  onAddRecipe(): void {
    this.addRecipe.emit();
  }

  onCreatePlan(): void {
    this.createPlan.emit();
  }

  onGenerateList(): void {
    this.generateList.emit();
  }
}
