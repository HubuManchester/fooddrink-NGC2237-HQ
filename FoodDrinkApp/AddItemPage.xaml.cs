using FoodDrinkApp.Models;
using FoodDrinkApp.Services;
using Microsoft.Maui.Media;
using Microsoft.Maui.ApplicationModel;

namespace FoodDrinkApp;

public partial class AddItemPage : ContentPage
{
    private string? capturedImagePath;
    private string? capturedLocation;
    public AddItemPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AccessibilityService.ApplyFontScale(this);
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        try
        {
            var validationMessage = ValidateForm(out var calories, out var protein, out var carbs, out var fat);
            if (validationMessage is not null)
            {
                ShowValidation(validationMessage);
                Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(1200));
                return;
            }

            var item = new FoodItem
            {
                Name = NameEntry.Text!.Trim(),
                Category = CategoryPicker.SelectedItem?.ToString() ?? "Snack",
                Description = DescriptionEditor.Text!.Trim(),
                Calories = calories,
                Protein = protein,
                Carbs = carbs,
                Fat = fat,
                AllergyNote = string.IsNullOrWhiteSpace(AllergyEntry.Text)
                    ? "No allergy note provided."
                    : AllergyEntry.Text.Trim(),
                Tags = $"{NameEntry.Text} {CategoryPicker.SelectedItem} {DescriptionEditor.Text}",
                ImagePath = capturedImagePath,      // 新增
                Location = capturedLocation          // 新增
            };

            await FoodCatalogService.AddAsync(item);
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
            SemanticScreenReader.Announce("Food record saved.");

            await DisplayAlert(
                "Saved",
                MockApiConfig.IsConfigured
                    ? "The record has been saved to mockapi.io."
                    : "The record has been saved to local fallback data.",
                "OK");

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            ShowValidation($"The record could not be saved: {ex.Message}");
        }
    }

    private string? ValidateForm(out int calories, out int protein, out int carbs, out int fat)
    {
        calories = protein = carbs = fat = 0;

        if (string.IsNullOrWhiteSpace(NameEntry.Text))
        {
            return "Please enter a food or drink name.";
        }

        if (CategoryPicker.SelectedIndex < 0)
        {
            return "Please choose a category.";
        }

        if (string.IsNullOrWhiteSpace(DescriptionEditor.Text))
        {
            return "Please add a short description.";
        }

        return TryReadNumber(CaloriesEntry.Text, "calories", out calories)
            ?? TryReadNumber(ProteinEntry.Text, "protein", out protein)
            ?? TryReadNumber(CarbsEntry.Text, "carbs", out carbs)
            ?? TryReadNumber(FatEntry.Text, "fat", out fat);
    }

    private static string? TryReadNumber(string? value, string fieldName, out int number)
    {
        if (int.TryParse(value, out number) && number >= 0)
        {
            return null;
        }

        return $"Please enter a valid non-negative number for {fieldName}.";
    }

    private async void OnTakePhotoClicked(object? sender, EventArgs e)
    {
        try
        {
            if (!MediaPicker.Default.IsCaptureSupported)
            {
                await DisplayAlert("Not supported", "Camera is not available on this device.", "OK");
                return;
            }

            var photo = await MediaPicker.Default.CapturePhotoAsync();
            if (photo is null) return;

            // Save photo to app data directory
            var localPath = Path.Combine(FileSystem.AppDataDirectory, $"{Guid.NewGuid()}.jpg");
            using var sourceStream = await photo.OpenReadAsync();
            using var destStream = File.OpenWrite(localPath);
            await sourceStream.CopyToAsync(destStream);

            capturedImagePath = localPath;
            PhotoPreview.Source = ImageSource.FromStream(() => File.OpenRead(localPath));
            PhotoPreviewBorder.IsVisible = true;

            SemanticScreenReader.Announce("Photo captured");
        }
        catch (PermissionException)
        {
            await DisplayAlert("Permission required", "Camera permission is required to take photos.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to capture photo: {ex.Message}", "OK");
        }
    }

    private async void OnGetLocationClicked(object? sender, EventArgs e)
    {
        try
        {
            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            var location = await Geolocation.Default.GetLocationAsync(request);

            if (location is null)
            {
                await DisplayAlert("Location failed", "Could not get current location.", "OK");
                return;
            }

            var placemarks = await Geocoding.Default.GetPlacemarksAsync(location);
            var placemark = placemarks?.FirstOrDefault();

            if (placemark != null)
            {
                var parts = new[] { placemark.CountryName, placemark.AdminArea, placemark.Locality }
                    .Where(p => !string.IsNullOrWhiteSpace(p));
                capturedLocation = string.Join(" / ", parts);
            }
            else
            {
                capturedLocation = $"Lat: {location.Latitude:F4}, Lng: {location.Longitude:F4}";
            }

            LocationLabel.Text = capturedLocation;
            LocationLabel.TextColor = Colors.Green;
            SemanticScreenReader.Announce("Location captured");
        }
        catch (PermissionException)
        {
            await DisplayAlert("Permission required", "Location permission is required.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to get location: {ex.Message}", "OK");
        }
    }

    private void ShowValidation(string message)
    {
        ValidationLabel.Text = message;
        ValidationPanel.IsVisible = true;
        SemanticScreenReader.Announce(message);
    }
}
