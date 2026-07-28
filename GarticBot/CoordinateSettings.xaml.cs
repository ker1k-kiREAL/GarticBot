using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using static GarticBot.Utils;
using Button = System.Windows.Controls.Button;
using Clipboard = System.Windows.Clipboard;
using Color = System.Drawing.Color;
using SolidColorBrush = System.Windows.Media.SolidColorBrush;

namespace GarticBot
{
	/// <summary>
	/// Interaction logic for CoordinateSettings.xaml
	/// </summary>
	public partial class CoordinateSettings : Window
	{
		Bitmap background;
		int currentSetting = -1;
		Settings settings;
		bool SetStopButton = false;
		bool SetSkipButton = false;
		uint closeThreadKey;
		uint skipColorKey;

		public CoordinateSettings(Settings sets)
		{
			InitializeComponent();
			settings = sets;

			#region Loading values
			closeThreadKey = settings.CloseThreadKeycode;
			skipColorKey = settings.SkipColorKeycode;

			SelectStopButton.Content = ((Keys)closeThreadKey).ToString();
			SelectSkipButton.Content = ((Keys)skipColorKey).ToString();

			openPaletteX.Text = settings.OpenPalette.X.ToString();
			openPaletteY.Text = settings.OpenPalette.Y.ToString();

			emptySpaceX.Text = settings.EmptySpace.X.ToString();
			emptySpaceY.Text = settings.EmptySpace.Y.ToString();

			redX.Text = settings.RedValue.X.ToString();
			redY.Text = settings.RedValue.Y.ToString();

			greenX.Text = settings.GreenValue.X.ToString();
			greenY.Text = settings.GreenValue.Y.ToString();

			blueX.Text = settings.BlueValue.X.ToString();
			blueY.Text = settings.BlueValue.Y.ToString();
			#endregion

			// Get the physical dimensions of the primary screen
			int screenWidth = (int)SystemParameters.PrimaryScreenWidth;
			int screenHeight = (int)SystemParameters.PrimaryScreenHeight;
			try
			{
				screenWidth = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
				screenHeight = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
			}
			catch { }

			var tmp = new Bitmap(screenWidth, screenHeight);
			using (Graphics gfx = Graphics.FromImage(tmp))
			{
				try
				{
					// Automatically capture a screenshot of the entire screen!
					gfx.CopyFromScreen(0, 0, 0, 0, tmp.Size);
				}
				catch
				{
					// Fallback to solid black if screenshot capture is not permitted/fails
					using (SolidBrush brush = new(Color.Black)) 
						gfx.FillRectangle(brush, 0, 0, tmp.Width, tmp.Height);
				}
				background = (Bitmap)tmp.Clone();
			}

			Bitmap freshClone = (Bitmap)background.Clone();
			DrawMarkers(freshClone);
			freshClone.Dispose();
			tmp.Dispose();
		}

		private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
		{
			Regex regex = new("[^0-9]+");
			e.Handled = regex.IsMatch(e.Text);
		}

		public static Bitmap ClipboardImage()
		{
			Bitmap returnImage = null;
			if (Clipboard.ContainsImage())
			{
				returnImage = BitmapFromSource(Clipboard.GetImage());
			}
			return returnImage;
		}

		private void DrawMarkers(Bitmap img)
		{
			using (Graphics gfx = Graphics.FromImage(img))
			{
				#region Open Palette
				using (SolidBrush brush = new(Color.White))
					gfx.FillEllipse(brush, TryParse(openPaletteX.Text) - 5, TryParse(openPaletteY.Text) - 5, 10, 10);
				#endregion

				#region EmptySpace
				using (SolidBrush brush = new(Color.Black))
					gfx.FillEllipse(brush, TryParse(emptySpaceX.Text) - 5, TryParse(emptySpaceY.Text) - 5, 10, 10);
				#endregion

				#region Red
				using (SolidBrush brush = new(Color.Red))
					gfx.FillEllipse(brush, TryParse(redX.Text) - 5, TryParse(redY.Text) - 5, 10, 10);
				#endregion

				#region Green
				using (SolidBrush brush = new(Color.Green))
					gfx.FillEllipse(brush, TryParse(greenX.Text) - 5, TryParse(greenY.Text) - 5, 10, 10);
				#endregion

				#region Blue
				using (SolidBrush brush = new(Color.Blue))
					gfx.FillEllipse(brush, TryParse(blueX.Text) - 5, TryParse(blueY.Text) - 5, 10, 10);
				#endregion
			}

			screenImage.Source = BitmapToBitmapSource(img);
		}

		private void PasteImageButton_Click(object sender, RoutedEventArgs e)
		{
			var tmp = ClipboardImage();
			if (tmp != null)
			{
				if (background != null) background.Dispose();
				background = (Bitmap)tmp.Clone();
				Bitmap freshClone = (Bitmap)background.Clone();
				DrawMarkers(freshClone);
				freshClone.Dispose();
				tmp.Dispose();
			}
		}

		private void SaveButton_Click(object sender, RoutedEventArgs e)
		{
			settings.OpenPalette = GetPointFromStrings(openPaletteX.Text, openPaletteY.Text);
			settings.EmptySpace = GetPointFromStrings(emptySpaceX.Text, emptySpaceY.Text);
			settings.RedValue = GetPointFromStrings(redX.Text, redY.Text);
			settings.GreenValue = GetPointFromStrings(greenX.Text, greenY.Text);
			settings.BlueValue = GetPointFromStrings(blueX.Text, blueY.Text);

			settings.SkipColorKeycode = skipColorKey;
			settings.CloseThreadKeycode = closeThreadKey;

			settings.Save();

			System.Windows.MessageBox.Show("Успешно сохранено!", "Сохранение", MessageBoxButton.OK, MessageBoxImage.Information);
		}

		private void SetCoordButton_Click(object sender, RoutedEventArgs e)
		{
			// Reset all selection buttons to default white background
			setOpenPalette.Background = System.Windows.Media.Brushes.White;
			setEmpty.Background = System.Windows.Media.Brushes.White;
			setRed.Background = System.Windows.Media.Brushes.White;
			setGreen.Background = System.Windows.Media.Brushes.White;
			setBlue.Background = System.Windows.Media.Brushes.White;

			Button clickedButton = (Button)sender;
			
			// Highlight the active selection button with a modern soft blue color
			clickedButton.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(180, 220, 255));

			switch (clickedButton.Name)
			{
				case "setOpenPalette":
					currentSetting = 0;
					break;
				case "setEmpty":
					currentSetting = 1;
					break;
				case "setRed":
					currentSetting = 2;
					break;
				case "setGreen":
					currentSetting = 3;
					break;
				case "setBlue":
					currentSetting = 4;
					break;
			}

			// Show crosshair cursor to indicate coordinate picking is armed
			this.Cursor = System.Windows.Input.Cursors.Cross;
		}

		private void Image_Click(object sender, MouseButtonEventArgs e)
		{
			if (currentSetting == -1) return;

			// Get cursor position relative to the screenImage control
			System.Windows.Point p = e.GetPosition(screenImage);

			double w_i = background.Width;
			double h_i = background.Height;
			double w_c = screenImage.ActualWidth;
			double h_c = screenImage.ActualHeight;

			if (w_c == 0 || h_c == 0) return;

			// Handle Uniform aspect ratio stretch of Image control perfectly
			double r_img = w_i / h_i;
			double r_ctrl = w_c / h_c;

			double w_render, h_render;
			double x_offset = 0;
			double y_offset = 0;

			if (r_img > r_ctrl)
			{
				w_render = w_c;
				h_render = w_c / r_img;
				y_offset = (h_c - h_render) / 2;
			}
			else
			{
				h_render = h_c;
				w_render = h_c * r_img;
				x_offset = (w_c - w_render) / 2;
			}

			// Adjust mouse coordinate by subtracting the aspect ratio offset margins
			double x_rel = p.X - x_offset;
			double y_rel = p.Y - y_offset;

			// Clamp coordinates to image boundaries
			if (x_rel < 0) x_rel = 0;
			if (x_rel > w_render) x_rel = w_render;
			if (y_rel < 0) y_rel = 0;
			if (y_rel > h_render) y_rel = h_render;

			// Calculate scale factors
			double scaleX = w_i / w_render;
			double scaleY = h_i / h_render;

			// Get final precise pixel coordinates on the screen screenshot
			int pixelX = (int)(x_rel * scaleX);
			int pixelY = (int)(y_rel * scaleY);

			switch (currentSetting)
			{
				case 0:
					openPaletteX.Text = pixelX.ToString();
					openPaletteY.Text = pixelY.ToString();
					break;
				case 1:
					emptySpaceX.Text = pixelX.ToString();
					emptySpaceY.Text = pixelY.ToString();
					break;
				case 2:
					redX.Text = pixelX.ToString();
					redY.Text = pixelY.ToString();
					break;
				case 3:
					greenX.Text = pixelX.ToString();
					greenY.Text = pixelY.ToString();
					break;
				case 4:
					blueX.Text = pixelX.ToString();
					blueY.Text = pixelY.ToString();
					break;
			}

			currentSetting = -1;

			// Reset all buttons backgrounds to default White
			setOpenPalette.Background = System.Windows.Media.Brushes.White;
			setEmpty.Background = System.Windows.Media.Brushes.White;
			setRed.Background = System.Windows.Media.Brushes.White;
			setGreen.Background = System.Windows.Media.Brushes.White;
			setBlue.Background = System.Windows.Media.Brushes.White;

			// Restore standard arrow cursor
			this.Cursor = System.Windows.Input.Cursors.Arrow;

			Bitmap freshClone = (Bitmap)background.Clone();
			DrawMarkers(freshClone);
			freshClone.Dispose();
		}

		private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
		{
			// Redraw markers on the fly when user manually modifies text boxes
			if (background != null && screenImage != null)
			{
				Bitmap freshClone = (Bitmap)background.Clone();
				DrawMarkers(freshClone);
				freshClone.Dispose();
			}
		}

		private void SelectStopButton_Click(object sender, RoutedEventArgs e)
		{
			SelectStopButton.Content = "(...)";
			SelectSkipButton.Content = ((Keys)skipColorKey).ToString();

			SetSkipButton = false;
			SetStopButton = true;
		}

		private void SelectSkipButton_Click(object sender, RoutedEventArgs e)
		{
			SelectSkipButton.Content = "(...)";
			SelectStopButton.Content = ((Keys)closeThreadKey).ToString();

			SetStopButton = false;
			SetSkipButton = true;
		}

		private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (!SetStopButton && !SetSkipButton) return;

			if (SetStopButton)
				closeThreadKey = (uint)GetKeycodeFromKey(e);

			else if (SetSkipButton)
				skipColorKey = (uint)GetKeycodeFromKey(e);

			SetStopButton = false;
			SetSkipButton = false;
			SelectStopButton.Content = ((Keys)closeThreadKey).ToString();
			SelectSkipButton.Content = ((Keys)skipColorKey).ToString();
		}
	}
}
