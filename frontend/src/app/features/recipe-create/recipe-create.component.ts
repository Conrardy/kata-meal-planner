import { Component } from '@angular/core';
import { FormArray, FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { RecipeService } from '../../core/services/recipe.service';

@Component({
  selector: 'app-recipe-create',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './recipe-create.component.html'
})
export class RecipeCreateComponent {
  form: FormGroup;

  constructor(private fb: FormBuilder, private recipeService: RecipeService, private router: Router) {
    this.form = this.fb.group({
      name: ['', Validators.required],
      description: ['', Validators.required],
      imageUrl: ['', Validators.pattern('https?://.+')],
      ingredients: this.fb.array([]),
      steps: this.fb.array([]),
      tags: [[]],
      mealType: ['Lunch']
    });

    this.addIngredient();
    this.addStep();
  }

  get ingredients(): FormArray {
    return this.form.get('ingredients') as FormArray;
  }

  get steps(): FormArray {
    return this.form.get('steps') as FormArray;
  }

  addIngredient() {
    this.ingredients.push(this.fb.group({ name: ['', Validators.required], quantity: ['', Validators.required], unit: [''] }));
  }

  removeIngredient(index: number) {
    this.ingredients.removeAt(index);
  }

  addStep() {
    const stepNumber = this.steps.length + 1;
    this.steps.push(this.fb.group({ stepNumber: [stepNumber], instruction: ['', Validators.required] }));
  }

  removeStep(index: number) {
    this.steps.removeAt(index);
    // renumber
    this.steps.controls.forEach((c, i) => c.get('stepNumber')!.setValue(i + 1));
  }

  async save() {
    if (this.form.invalid) return;
    if (this.ingredients.length === 0 || this.steps.length === 0) return;

    const fv = this.form.value;
    const payload = {
      name: fv.name,
      imageUrl: fv.imageUrl || null,
      description: fv.description,
      ingredients: fv.ingredients.map((i: any) => ({ name: i.name, quantity: i.quantity, unit: i.unit || null })),
      steps: fv.steps.map((s: any, idx: number) => ({ stepNumber: s.stepNumber ?? idx + 1, instruction: s.instruction })),
      tags: fv.tags,
      mealType: fv.mealType
    };

    try {
      const result: any = await firstValueFrom(this.recipeService.createRecipe(payload));
      const id = result?.id ?? result?.Id ?? result?.ID;
      if (id) {
        this.router.navigate(['/recipe', id.toString()]);
      } else {
        console.warn('Create recipe succeeded but no id returned', result);
      }
    } catch (err) {
      console.error('Failed to create recipe', err);
    }
  }

  get canSave(): boolean {
    return this.form.valid && this.ingredients.length > 0 && this.steps.length > 0;
  }
}
