using FoodDrinkApp.Models;
using FoodDrinkApp.Services;

namespace FoodDrinkApp;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        AccessibilityService.ApplyFontScale(this);
        await LoadFoodItemsAsync(SearchFoodBar.Text);
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

    private async void OnRandomPickClicked(object? sender, EventArgs e)
    {
        try
        {
            // Get the currently displayed food list
            var items = FoodCollection.ItemsSource as IReadOnlyList<FoodItem>;

            if (items == null || !items.Any())
            {
                await DisplayAlert("No data available", "There are currently no recommended foods or drinks, please try again later.", "OK");
                return;
            }

            // Randomly select one
            var random = new Random();
            var randomIndex = random.Next(items.Count);
            var pick = items[randomIndex];

            // Display recommended results
            var message = $"{pick.Name}\n\n{pick.CaloriesLabel}\n\n{pick.Description}";
            await DisplayAlert("Today's Recommendation", message, "Not bad");

            // Optional: Add haptic feedback
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);

            // Optional: Voice Broadcast Recommended Results
            await SpeechService.SpeakAsync($"I recommend you try {pick.Name}");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error occurred", $"Random recommendation failed£º{ex.Message}", "OK");
        }
    }
}
