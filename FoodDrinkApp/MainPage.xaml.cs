using FoodDrinkApp.Models;
using FoodDrinkApp.Services;
using Microsoft.Maui.Devices.Sensors;

namespace FoodDrinkApp;

public partial class MainPage : ContentPage
{
    private bool isShakeListening = false;
    private DateTime lastShakeTime = DateTime.MinValue;
    private const double ShakeThreshold = 2.0;
    private bool isRecommendationShowing = false;  // Prevent duplicate dialog

    public MainPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        AccessibilityService.ApplyFontScale(this);
        await LoadFoodItemsAsync(SearchFoodBar.Text);

        // Activate Shake Monitoring
        StartShakeDetection();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // Stop Shake Monitoring
        StopShakeDetection();
    }

    private async Task LoadFoodItemsAsync(string? query = null)
    {
        FoodCollection.ItemsSource = await FoodCatalogService.SearchAsync(query);
    }

    private async void OnAddClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(AddItemPage));
    }

    private async void OnDetailsClicked(object? sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is string id)
        {
            await Shell.Current.GoToAsync($"{nameof(FoodDetailPage)}?id={Uri.EscapeDataString(id)}");
        }
    }

    private async void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        await LoadFoodItemsAsync(e.NewTextValue);
    }

    private async void OnSearchButtonPressed(object? sender, EventArgs e)
    {
        await LoadFoodItemsAsync(SearchFoodBar.Text);
    }

    private async void OnRefreshing(object? sender, EventArgs e)
    {
        await LoadFoodItemsAsync(SearchFoodBar.Text);
        FoodRefreshView.IsRefreshing = false;
        var source = FoodCatalogService.LastLoadUsedMockApi ? "mockapi.io" : "local fallback data";
        SemanticScreenReader.Announce($"Food and drink list refreshed. Current source: {source}.");
    }

    // Public random recommendation method
    private async Task ShowRandomRecommendationAsync(string source = "button")
    {
        // If a dialog is already showing, ignore new triggers
        if (isRecommendationShowing)
        {
            System.Diagnostics.Debug.WriteLine("Recommendation dialog already showing, ignoring new trigger");
            return;
        }

        try
        {
            isRecommendationShowing = true;

            var items = FoodCollection.ItemsSource as IReadOnlyList<FoodItem>;

            if (items == null || !items.Any())
            {
                await DisplayAlert("No data available", "There are currently no recommended foods or drinks, please try again later.", "OK");
                return;
            }

            var random = new Random();
            var randomIndex = random.Next(items.Count);
            var pick = items[randomIndex];

            var message = $"{pick.Name}\n\n{pick.CaloriesLabel}\n\n{pick.Description}";
            var title = source == "shake" ? "Shake and recommend" : "Today's Recommendation";
            await DisplayAlert(title, message, "Got it");

            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
            SemanticScreenReader.Announce($"Random recommendation: {pick.Name}");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error occurred", $"Random recommendation failed£º{ex.Message}", "OK");
        }
        finally
        {
            // Dialog closed, allow next recommendation
            isRecommendationShowing = false;
        }
    }

    // Button random recommendation
    private async void OnRandomPickClicked(object? sender, EventArgs e)
    {
        await ShowRandomRecommendationAsync("button");
    }

    // Shake function
    private void StartShakeDetection()
    {
        if (!Accelerometer.Default.IsSupported)
        {
            System.Diagnostics.Debug.WriteLine("The device does not support accelerometers");
            return;
        }

        try
        {
            Accelerometer.Default.ReadingChanged += OnShakeDetected;
            Accelerometer.Default.Start(SensorSpeed.Game);
            isShakeListening = true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Shake failed to start: {ex.Message}");
        }
    }

    private void StopShakeDetection()
    {
        if (!isShakeListening) return;

        try
        {
            Accelerometer.Default.ReadingChanged -= OnShakeDetected;
            Accelerometer.Default.Stop();
            isShakeListening = false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Stop shaking fail: {ex.Message}");
        }
    }

    private void OnShakeDetected(object? sender, AccelerometerChangedEventArgs e)
    {
        var data = e.Reading;

        var acceleration = Math.Sqrt(
            data.Acceleration.X * data.Acceleration.X +
            data.Acceleration.Y * data.Acceleration.Y +
            data.Acceleration.Z * data.Acceleration.Z
        );

        if (acceleration > ShakeThreshold)
        {
            if ((DateTime.Now - lastShakeTime).TotalMilliseconds < 300)
                return;

            lastShakeTime = DateTime.Now;

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await ShowRandomRecommendationAsync("shake");
            });
        }
    }
}