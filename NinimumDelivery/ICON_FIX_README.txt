Ninimum Delivery Android icon fix

What was actually different from the working Ninimum app:
- Ninimum has Platforms/Android/AndroidManifest.xml with:
    android:icon="@mipmap/appicon"
    android:roundIcon="@mipmap/appicon_round"
- Ninimum Delivery did not have a Platforms/Android/AndroidManifest.xml at all.
  Android therefore showed the generic Android launcher icon.

Changes in this package:
1. Added Platforms/Android/AndroidManifest.xml with explicit MAUI icon resources.
2. Matched the working Ninimum MauiIcon line exactly.
3. Kept the existing Ninimum Delivery truck appicon.svg/appiconfg.svg.

After replacing the project:
1. Uninstall Ninimum Delivery completely from the Android device.
2. Run:
   dotnet clean
   rm -rf bin obj
3. Build/install again.
