# NinimumStock

Warehouse worker .NET MAUI application, project `NinimumWarehouse` in `../Ninimum.sln`.

- Android/iOS, .NET 10, MAUI 10.0.70; Android build verified. Device/camera behavior is to be checked by the user.
- Application ID remains `com.ninimum.warehouse`; display name is NinimumStock.
- White theme, warehouse icon, rounded inputs without Android underlines, icon tabs and preparation progress cards.
- Product cards show the order’s saved product image; missing or unavailable images use the warehouse icon. The XAML `ProductImage` component downloads images into a local cache and cancels loading when detached; failed downloads are reported as `[ProductImage]` in the Debug Console. The backend detail API supplies `product_image_url`, without a database migration.
- Screens and shared styles are in XAML; behavior is in `ViewModels/`.
- Uzbek/Russian/English strings are in `Resources/Languages/AppResource.uz.resx`, `AppResource.ru.resx` and the English `AppResource.resx`. Add matching keys to all three when introducing interface text. `LanguageService` persists the selection and rebuilds the current login/home screen to apply it.
- API addresses, storage keys and refresh intervals are centralized in `Services/AppConstants.cs`.
- Separate worker login with secure token storage and one active session per account.
- Queue, my preparation tasks, ready history, camera barcode/entered-code checks, quantity validation, problem reporting/resume, packing confirmation.
- API gateway is configured in `Services/AppConstants.cs` and matches the existing Ninimum server at port 8083.
- Camera library: https://github.com/Redth/ZXing.Net.Maui (ZXing.Net.Maui.Controls 0.10.4).

Install the backend `warehouse-step2.sql` migration, enable warehouse preparation, and create worker accounts in Admin → Warehouse → Warehouse workers. Full activation and testing steps are in the backend's `WAREHOUSE_STEP2.md`.

Select **NinimumWarehouse** as the startup project. Build/install it; this work does not deploy the backend, Admin or application.
