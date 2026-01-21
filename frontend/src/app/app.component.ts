import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { SidebarComponent } from './shared/components/sidebar/sidebar.component';
import { Router } from '@angular/router';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, SidebarComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  title = 'meal-planner';

  constructor(private router: Router) {}

  onAddRecipe(): void {
    this.router.navigate(['/recipes/new']);
  }

  onCreatePlan(): void {
    console.log('Create Meal Plan quick action triggered');
  }

  onGenerateList(): void {
    console.log('Generate Shopping List quick action triggered');
  }
}
