import { describe, it, expect, beforeEach, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { RecipeCreateComponent } from './recipe-create.component';
import { RecipeService } from '../../core/services/recipe.service';

describe('RecipeCreateComponent', () => {
  let recipeServiceMock: { createRecipe: ReturnType<typeof vi.fn> };
  let router: Router;

  beforeEach(async () => {
    recipeServiceMock = {
      createRecipe: vi.fn(),
    };

    await TestBed.configureTestingModule({
      imports: [RecipeCreateComponent],
      providers: [
        provideRouter([]),
        { provide: RecipeService, useValue: recipeServiceMock },
      ],
    }).compileComponents();

    router = TestBed.inject(Router);
    vi.spyOn(router, 'navigate').mockResolvedValue(true);
  });

  it('should create the component', () => {
    const fixture = TestBed.createComponent(RecipeCreateComponent);
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should initialize with one ingredient and one step', () => {
    const fixture = TestBed.createComponent(RecipeCreateComponent);
    const component = fixture.componentInstance;

    expect(component.ingredients.length).toBe(1);
    expect(component.steps.length).toBe(1);
  });

  it('should add ingredient when addIngredient is called', () => {
    const fixture = TestBed.createComponent(RecipeCreateComponent);
    const component = fixture.componentInstance;
    const initialCount = component.ingredients.length;

    component.addIngredient();

    expect(component.ingredients.length).toBe(initialCount + 1);
  });

  it('should remove ingredient when removeIngredient is called', () => {
    const fixture = TestBed.createComponent(RecipeCreateComponent);
    const component = fixture.componentInstance;
    component.addIngredient();
    const countAfterAdd = component.ingredients.length;

    component.removeIngredient(0);

    expect(component.ingredients.length).toBe(countAfterAdd - 1);
  });

  it('should add step when addStep is called', () => {
    const fixture = TestBed.createComponent(RecipeCreateComponent);
    const component = fixture.componentInstance;
    const initialCount = component.steps.length;

    component.addStep();

    expect(component.steps.length).toBe(initialCount + 1);
  });

  it('should renumber steps when a step is removed', () => {
    const fixture = TestBed.createComponent(RecipeCreateComponent);
    const component = fixture.componentInstance;
    component.addStep();
    component.addStep();

    component.removeStep(0);

    expect(component.steps.at(0)?.get('stepNumber')?.value).toBe(1);
    expect(component.steps.at(1)?.get('stepNumber')?.value).toBe(2);
  });

  it('should be invalid when name is empty', () => {
    const fixture = TestBed.createComponent(RecipeCreateComponent);
    const component = fixture.componentInstance;

    component.form.patchValue({ name: '' });

    expect(component.form.get('name')?.invalid).toBe(true);
  });

  it('should be invalid when description is empty', () => {
    const fixture = TestBed.createComponent(RecipeCreateComponent);
    const component = fixture.componentInstance;

    component.form.patchValue({ description: '' });

    expect(component.form.get('description')?.invalid).toBe(true);
  });

  it('should be invalid when imageUrl is not a valid URL', () => {
    const fixture = TestBed.createComponent(RecipeCreateComponent);
    const component = fixture.componentInstance;

    component.form.patchValue({ imageUrl: 'not-a-url' });

    expect(component.form.get('imageUrl')?.invalid).toBe(true);
  });

  it('should be valid when imageUrl is empty', () => {
    const fixture = TestBed.createComponent(RecipeCreateComponent);
    const component = fixture.componentInstance;

    component.form.patchValue({ imageUrl: '' });

    expect(component.form.get('imageUrl')?.invalid).toBe(false);
  });

  it('should be valid when imageUrl is a valid URL', () => {
    const fixture = TestBed.createComponent(RecipeCreateComponent);
    const component = fixture.componentInstance;

    component.form.patchValue({ imageUrl: 'https://example.com/image.jpg' });

    expect(component.form.get('imageUrl')?.invalid).toBe(false);
  });

  it('canSave should be false when form is invalid', () => {
    const fixture = TestBed.createComponent(RecipeCreateComponent);
    const component = fixture.componentInstance;

    component.form.patchValue({ name: '' });

    expect(component.canSave).toBe(false);
  });

  it('canSave should be false when no ingredients', () => {
    const fixture = TestBed.createComponent(RecipeCreateComponent);
    const component = fixture.componentInstance;
    component.form.patchValue({ name: 'Test', description: 'Test desc' });
    component.ingredients.at(0)?.patchValue({ name: 'Flour', quantity: '1' });
    component.removeIngredient(0);

    expect(component.canSave).toBe(false);
  });

  it('canSave should be false when no steps', () => {
    const fixture = TestBed.createComponent(RecipeCreateComponent);
    const component = fixture.componentInstance;
    component.form.patchValue({ name: 'Test', description: 'Test desc' });
    component.ingredients.at(0)?.patchValue({ name: 'Flour', quantity: '1' });
    component.steps.at(0)?.patchValue({ instruction: 'Mix' });
    component.removeStep(0);

    expect(component.canSave).toBe(false);
  });

  it('canSave should be true when form is valid with ingredients and steps', () => {
    const fixture = TestBed.createComponent(RecipeCreateComponent);
    const component = fixture.componentInstance;
    component.form.patchValue({ name: 'Test Recipe', description: 'Test description' });
    component.ingredients.at(0)?.patchValue({ name: 'Flour', quantity: '1' });
    component.steps.at(0)?.patchValue({ instruction: 'Mix ingredients' });

    expect(component.canSave).toBe(true);
  });

  it('should call recipeService.createRecipe when save is called with valid form', async () => {
    const fixture = TestBed.createComponent(RecipeCreateComponent);
    const component = fixture.componentInstance;
    component.form.patchValue({ name: 'Test Recipe', description: 'Test description' });
    component.ingredients.at(0)?.patchValue({ name: 'Flour', quantity: '1' });
    component.steps.at(0)?.patchValue({ instruction: 'Mix ingredients' });

    recipeServiceMock.createRecipe.mockReturnValue(of({ id: 'test-id-123' }));

    await component.save();

    expect(recipeServiceMock.createRecipe).toHaveBeenCalled();
  });

  it('should navigate to recipe details on successful save', async () => {
    const fixture = TestBed.createComponent(RecipeCreateComponent);
    const component = fixture.componentInstance;
    component.form.patchValue({ name: 'Test Recipe', description: 'Test description' });
    component.ingredients.at(0)?.patchValue({ name: 'Flour', quantity: '1' });
    component.steps.at(0)?.patchValue({ instruction: 'Mix ingredients' });

    recipeServiceMock.createRecipe.mockReturnValue(of({ id: 'test-id-123' }));

    await component.save();

    expect(router.navigate).toHaveBeenCalledWith(['/recipe', 'test-id-123']);
  });

  it('should not call recipeService when form is invalid', async () => {
    const fixture = TestBed.createComponent(RecipeCreateComponent);
    const component = fixture.componentInstance;
    component.form.patchValue({ name: '' });

    await component.save();

    expect(recipeServiceMock.createRecipe).not.toHaveBeenCalled();
  });

  it('should handle save error gracefully', async () => {
    const fixture = TestBed.createComponent(RecipeCreateComponent);
    const component = fixture.componentInstance;
    component.form.patchValue({ name: 'Test Recipe', description: 'Test description' });
    component.ingredients.at(0)?.patchValue({ name: 'Flour', quantity: '1' });
    component.steps.at(0)?.patchValue({ instruction: 'Mix ingredients' });

    recipeServiceMock.createRecipe.mockReturnValue(throwError(() => new Error('Network error')));
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});

    await component.save();

    expect(consoleSpy).toHaveBeenCalled();
    consoleSpy.mockRestore();
  });
});
