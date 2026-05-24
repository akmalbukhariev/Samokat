using System;
using Microsoft.Maui.Controls;

namespace Ninimum.Views.DetailProduct
{
    public partial class LeaveCommentPage : BasePage
    {
        private int _selectedRating;

        public LeaveCommentPage()
        {
            InitializeComponent();

            InitializePage();
        }

        private void InitializePage()
        {
            _selectedRating = 0;
            UpdateStars(_selectedRating);

            // Uncomment this if you want to test the second screenshot state
            // SetUsedState();
        }

        private async void OnBackTapped(object sender, TappedEventArgs e)
        {
            if (Navigation?.NavigationStack?.Count > 1)
            {
                await Navigation.PopAsync();
            }
        }

        private void OnStar1Tapped(object sender, TappedEventArgs e)
        {
            SetRating(1);
        }

        private void OnStar2Tapped(object sender, TappedEventArgs e)
        {
            SetRating(2);
        }

        private void OnStar3Tapped(object sender, TappedEventArgs e)
        {
            SetRating(3);
        }

        private void OnStar4Tapped(object sender, TappedEventArgs e)
        {
            SetRating(4);
        }

        private void OnStar5Tapped(object sender, TappedEventArgs e)
        {
            SetRating(5);
        }

        private void SetRating(int rating)
        {
            _selectedRating = rating;
            UpdateStars(_selectedRating);
        }

        private void UpdateStars(int rating)
        {
            if (Star1 != null) Star1.Source = rating >= 1 ? "star.png" : "star_gray.png";
            if (Star2 != null) Star2.Source = rating >= 2 ? "star.png" : "star_gray.png";
            if (Star3 != null) Star3.Source = rating >= 3 ? "star.png" : "star_gray.png";
            if (Star4 != null) Star4.Source = rating >= 4 ? "star.png" : "star_gray.png";
            if (Star5 != null) Star5.Source = rating >= 5 ? "star.png" : "star_gray.png";
        }

        private async void OnSubmitClicked(object sender, EventArgs e)
        {
            try
            {
                var comment = CommentEditor?.Text?.Trim() ?? string.Empty;

                if (_selectedRating <= 0)
                {
                    await DisplayAlert("Ogohlantirish", "Iltimos, yulduzcha orqali baho bering.", "OK");
                    return;
                }

                if (string.IsNullOrWhiteSpace(comment))
                {
                    await DisplayAlert("Ogohlantirish", "Iltimos, sharh yozing.", "OK");
                    return;
                }

                // TODO:
                // 1. Collect selected images
                // 2. Send rating + comment + images to API
                // 3. Handle loading state if needed

                await DisplayAlert("Muvaffaqiyatli", "Sharhingiz yuborildi.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Xatolik", ex.Message, "OK");
            }
        }

        private void SetUsedState()
        {
            _selectedRating = 4;
            UpdateStars(_selectedRating);

            if (CommentEditor != null)
            {
                CommentEditor.Text = "Mahsulotga gap yo’q, zo’r ekan. Menga juda yoqdi. Endi faqat Ninimum.uz dan sotib olaman.";
            }
        }
    }
}