import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import {
  LucideAngularModule,
  Settings,
  Save,
  Loader2,
  Check,
  X,
  Plus,
} from 'lucide-angular';
import { PreferencesService } from '../../core/services/preferences.service';
import { UserPreferences } from '../../core/models/preferences.model';

@Component({
  selector: 'app-preferences',
  standalone: true,
  imports: [RouterLink, FormsModule, LucideAngularModule],
  templateUrl: './preferences.component.html',
})
export class PreferencesComponent implements OnInit {
  private readonly preferencesService = inject(PreferencesService);

  readonly preferences = signal<UserPreferences | null>(null);
  readonly isLoading = signal(true);
  readonly isSaving = signal(false);
  readonly error = signal<string | null>(null);
  readonly successMessage = signal<string | null>(null);

  selectedDietaryPreference = 'None';
  selectedAllergies: string[] = [];
  selectedMealsPerDay = 3;
  selectedPlanLength = 1;
  selectedIncludeLeftovers = false;
  selectedAutoGenerateShoppingList = true;
  excludedIngredients: string[] = [];
  newExcludedIngredient = '';

  readonly Settings = Settings;
  readonly Save = Save;
  readonly Loader2 = Loader2;
  readonly Check = Check;
  readonly X = X;
  readonly Plus = Plus;

  ngOnInit(): void {
    this.loadPreferences();
  }

  private loadPreferences(): void {
    this.isLoading.set(true);
    this.error.set(null);

    this.preferencesService.getPreferences().subscribe({
      next: (prefs: UserPreferences) => {
        this.preferences.set(prefs);
        this.selectedDietaryPreference = prefs.dietaryPreference;
        this.selectedAllergies = [...prefs.allergies];
        this.selectedMealsPerDay = prefs.mealsPerDay;
        this.selectedPlanLength = prefs.planLength;
        this.selectedIncludeLeftovers = prefs.includeLeftovers;
        this.selectedAutoGenerateShoppingList = prefs.autoGenerateShoppingList;
        this.excludedIngredients = [...prefs.excludedIngredients];
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Failed to load preferences:', err);
        this.error.set('Failed to load preferences. Please try again later.');
        this.isLoading.set(false);
      },
    });
  }

  onDietaryPreferenceChange(value: string): void {
    this.selectedDietaryPreference = value;
    this.successMessage.set(null);
  }

  onAllergyToggle(allergy: string): void {
    if (this.selectedAllergies.includes(allergy)) {
      this.selectedAllergies = this.selectedAllergies.filter((a) => a !== allergy);
    } else {
      this.selectedAllergies = [...this.selectedAllergies, allergy];
    }
    this.successMessage.set(null);
  }

  isAllergySelected(allergy: string): boolean {
    return this.selectedAllergies.includes(allergy);
  }

  onMealsPerDayChange(value: number): void {
    this.selectedMealsPerDay = value;
    this.successMessage.set(null);
  }

  onPlanLengthChange(value: number): void {
    this.selectedPlanLength = value;
    this.successMessage.set(null);
  }

  onLeftoversToggle(): void {
    this.selectedIncludeLeftovers = !this.selectedIncludeLeftovers;
    this.successMessage.set(null);
  }

  onAutoGenerateShoppingListToggle(): void {
    this.selectedAutoGenerateShoppingList = !this.selectedAutoGenerateShoppingList;
    this.successMessage.set(null);
  }

  addExcludedIngredient(): void {
    const ingredient = this.newExcludedIngredient.trim();
    if (ingredient && !this.excludedIngredients.includes(ingredient)) {
      this.excludedIngredients = [...this.excludedIngredients, ingredient];
      this.newExcludedIngredient = '';
      this.successMessage.set(null);
    }
  }

  removeExcludedIngredient(ingredient: string): void {
    this.excludedIngredients = this.excludedIngredients.filter((i) => i !== ingredient);
    this.successMessage.set(null);
  }

  onSavePreferences(): void {
    this.isSaving.set(true);
    this.error.set(null);
    this.successMessage.set(null);

    this.preferencesService
      .updatePreferences({
        dietaryPreference: this.selectedDietaryPreference,
        allergies: this.selectedAllergies,
        mealsPerDay: this.selectedMealsPerDay,
        planLength: this.selectedPlanLength,
        includeLeftovers: this.selectedIncludeLeftovers,
        autoGenerateShoppingList: this.selectedAutoGenerateShoppingList,
        excludedIngredients: this.excludedIngredients,
      })
      .subscribe({
        next: (prefs: UserPreferences) => {
          this.preferences.set(prefs);
          this.isSaving.set(false);
          this.successMessage.set('Preferences saved successfully!');
        },
        error: (err) => {
          console.error('Failed to save preferences:', err);
          this.error.set('Failed to save preferences. Please try again.');
          this.isSaving.set(false);
        },
      });
  }
}
