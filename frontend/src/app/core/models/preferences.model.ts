export interface UserPreferences {
  dietaryPreference: string;
  allergies: string[];
  availableDietaryPreferences: string[];
  availableAllergies: string[];
}

export interface UpdatePreferencesRequest {
  dietaryPreference: string;
  allergies: string[];
}
