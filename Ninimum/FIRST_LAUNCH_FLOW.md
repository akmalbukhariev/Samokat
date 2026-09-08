# Ninimum first-launch flow

## Normal behavior

On a fresh install (or when `has_completed_onboarding` is not present):

1. `AppEntryShell` opens `StartPage`.
2. `StartPage` continues to `OnboardingPage`.
3. Tapping **O‘tkazib yuborish** or **Boshlash** stores:
   - `has_completed_onboarding = true`
4. Control returns to `AppEntryShell`.
5. Existing startup behavior runs unchanged:
   - restore saved login when available, otherwise
   - start normal guest mode.

On every later app launch, `StartPage` and `OnboardingPage` are skipped.

## Testing first launch again

For development, either uninstall/clear the app data or remove the preference key:

```csharp
Preferences.Remove("has_completed_onboarding");
```

Do not remove this key during normal logout. Logout must not show onboarding again.
