# Today's meal

> Author: Fu Nanjun
> Student ID: 21906415

This application is developed with .NET MAUI on the theme of "Food and Drink". It allows users to record food and drink items, view nutrition summaries, validate user input, and demonstrate mobile device hardware features.

## Main Features

- Food and drink list with search and detail page.
- Form for adding records with validation for required fields and numeric values.
- Camera capture for food photos saved to records.
- Location tracking to record meal or purchase places.
- Text-to-speech for reading nutrition summaries and help content.
- Vibration and haptic feedback for operation alerts.
- Shake device to get random food recommendation.
- Theme switching and large text mode support.
- Semantic labels, screen reader announcements, and clear validation messages.

## Core Features

- Local static data source with MockAPI cloud data synchronization
- Random recommendation via button click and device shake
- Hardware integration: camera, location, text-to-speech, vibration, haptic feedback, accelerometer
- UI style optimization and dark mode adaptation

## Assessment Criteria Coverage

- UI/UX and Accessibility: XAML pages, tab navigation, consistent visual style, dark mode, semantic descriptions, and screen reader announcements.
- Mobile Hardware: Camera, location, text-to-speech, vibration, haptic feedback, accelerometer (shake).
- Functionality: List, search, add, detail, settings, random recommendation, hardware demonstration flow.
- Validation and Error Handling: Required field checks, numeric validation, permission errors, and hardware unavailable alerts.
- Code Quality: Separation of models and services, clear naming, reusable catalog service, and well-scoped page code.
- Deployment: Cross-platform .NET MAUI application for Android and Windows.
- GitHub Usage: Continuous commits with clear messages.

## How to Run

Open FoodDrinkApp.csproj or FoodDrinkApp.sln with Visual Studio 2022 with .NET MAUI workload installed.

Recommended demonstration targets:

- Android emulator or physical device
- Windows Machine

Windows build command:

dotnet build .\FoodDrinkApp.csproj -f net9.0-windows10.0.19041.0

Android build command:

dotnet build .\FoodDrinkApp.csproj -f net9.0-android
