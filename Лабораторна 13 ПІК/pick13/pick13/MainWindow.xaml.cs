using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace pick13
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isDragging = false;
        private Point startPoint;
        private Point imageStartPosition;

        public MainWindow()
        {
            InitializeComponent();
            UpdatePositionText();
        }
        private void BtnLoadImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif|All Files|*.*",
                Title = "Select an Image File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                LoadImage(openFileDialog.FileName);
            }
        }

        private void LoadImage(string filePath)
        {
            try
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(filePath);
                bitmap.EndInit();

                displayedImage.Source = bitmap;
                statusText.Text = $"Loaded: {System.IO.Path.GetFileName(filePath)}";

                // Reset position
                ResetImagePosition();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading image: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (displayedImage.Source == null) return;

            isDragging = true;
            startPoint = e.GetPosition(imageCanvas);
            imageStartPosition = new Point(dragTransform.X, dragTransform.Y);
            displayedImage.CaptureMouse();
            displayedImage.Cursor = Cursors.SizeAll;
        }

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isDragging) return;

            // Зупиняємо всі анімації перед переміщенням
            displayedImage.BeginAnimation(TranslateTransform.XProperty, null);
            displayedImage.BeginAnimation(TranslateTransform.YProperty, null);

            Point currentPoint = e.GetPosition(imageCanvas);
            Vector offset = currentPoint - startPoint;

            // Безпосередньо змінюємо значення трансформації
            dragTransform.X = imageStartPosition.X + offset.X;
            dragTransform.Y = imageStartPosition.Y + offset.Y;

            UpdatePositionText();
        }

        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isDragging)
            {
                isDragging = false;
                displayedImage.ReleaseMouseCapture();
                displayedImage.Cursor = Cursors.Hand;
            }
        }

        private void ResetImagePosition()
        {
            // Зупиняємо анімації
            displayedImage.BeginAnimation(TranslateTransform.XProperty, null);
            displayedImage.BeginAnimation(TranslateTransform.YProperty, null);

            // Анімація плавного повернення
            DoubleAnimation xAnimation = new DoubleAnimation(0,
                new Duration(TimeSpan.FromMilliseconds(300)));
            DoubleAnimation yAnimation = new DoubleAnimation(0,
                new Duration(TimeSpan.FromMilliseconds(300)));

            xAnimation.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };
            yAnimation.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };

            // Анімуємо безпосередньо елемент
            displayedImage.BeginAnimation(TranslateTransform.XProperty, xAnimation);
            displayedImage.BeginAnimation(TranslateTransform.YProperty, yAnimation);

            // Оновлюємо значення трансформації після анімації
            dragTransform.X = 0;
            dragTransform.Y = 0;

            UpdatePositionText();
        }

        private void BtnResetPosition_Click(object sender, RoutedEventArgs e)
        {
            ResetImagePosition();
        }

        private void BtnFitToScreen_Click(object sender, RoutedEventArgs e)
        {
            if (displayedImage.Source == null) return;

            displayedImage.Stretch = displayedImage.Stretch == Stretch.Uniform
                ? Stretch.UniformToFill
                : Stretch.Uniform;
        }

        private void UpdatePositionText()
        {
            positionText.Text = $"Position: {dragTransform.X:F0}, {dragTransform.Y:F0}";
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    LoadImage(files[0]);
                }
            }
        }

        protected override void OnDragEnter(DragEventArgs e)
        {
            base.OnDragEnter(e);
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                dropHint.Visibility = Visibility.Visible;
            }
        }

        protected override void OnDragLeave(DragEventArgs e)
        {
            base.OnDragLeave(e);
            dropHint.Visibility = Visibility.Collapsed;
        }

        protected override void OnDrop(DragEventArgs e)
        {
            base.OnDrop(e);
            dropHint.Visibility = Visibility.Collapsed;
        }
    }
}

