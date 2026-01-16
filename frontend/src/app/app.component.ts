import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { SidebarComponent } from './shared/components/sidebar/sidebar.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, SidebarComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  title = 'meal-planner';

  readonly showAddRecipeModal = signal(false);
  readonly showCreatePlanModal = signal(false);
  readonly showGenerateListModal = signal(false);

  onAddRecipe(): void {
    this.showAddRecipeModal.set(true);
    console.log('Add Recipe quick action triggered');
  }

  onCreatePlan(): void {
    this.showCreatePlanModal.set(true);
    console.log('Create Meal Plan quick action triggered');
  }

  onGenerateList(): void {
    this.showGenerateListModal.set(true);
    console.log('Generate Shopping List quick action triggered');
  }
}
