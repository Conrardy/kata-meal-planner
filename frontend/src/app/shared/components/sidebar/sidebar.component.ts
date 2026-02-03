import { Component, inject, output } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { LucideAngularModule, Plus, CalendarDays, ShoppingCart, Home, Calendar, Search, Settings, LogOut, Users } from 'lucide-angular';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, LucideAngularModule],
  templateUrl: './sidebar.component.html',
})
export class SidebarComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  addRecipe = output<void>();
  createPlan = output<void>();
  generateList = output<void>();

  readonly Home = Home;
  readonly Calendar = Calendar;
  readonly Search = Search;
  readonly Plus = Plus;
  readonly CalendarDays = CalendarDays;
  readonly ShoppingCart = ShoppingCart;
  readonly Settings = Settings;
  readonly LogOut = LogOut;
  readonly Users = Users;

  readonly currentUser = this.authService.currentUser;
  readonly isAdmin = this.authService.isAdmin;

  onAddRecipe(): void {
    this.addRecipe.emit();
  }

  onCreatePlan(): void {
    this.createPlan.emit();
  }

  onGenerateList(): void {
    this.generateList.emit();
  }

  onLogout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
