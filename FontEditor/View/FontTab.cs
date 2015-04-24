using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

using FontEditor.Model;
using FontEditor.Controller;
using Path = System.IO.Path;

// Veronika codes here
namespace FontEditor.View
{
    public partial class MainWindow : Window
    {
        private Point m_startMouseClick;
        private SegmentController m_segmentController;
        private Font m_font = null;
        private bool isTmpFont = false;

        private void initFontTab()
		{
            m_segmentController = new SegmentController(fontCanvas, previewGrid);
            var backgroundGrid = CreateGridBrush(new Rect( new Size((int) fontCanvas.Width, (int) fontCanvas.Height)), new Size(20, 20));
            
            // Draw grid
            fontCanvas.Background = backgroundGrid;

            isTmpFont = true;
            LoadedFontLabel.Content = "Temporary font";
             
		}

        static Brush CreateGridBrush(Rect bounds, Size tileSize)
        {
            var gridColor = Brushes.Gray;
            var gridThickness = 0.5;
            var tileRect = new Rect(tileSize);

            var gridTile = new DrawingBrush
            {
                Stretch = Stretch.None,
                TileMode = TileMode.Tile,
                Viewport = tileRect,
                ViewportUnits = BrushMappingMode.Absolute,
                Drawing = new GeometryDrawing
                {
                    Pen = new Pen(gridColor, gridThickness),
                    Geometry = new GeometryGroup
                    {
                        Children = new GeometryCollection
                {
                    new LineGeometry(tileRect.TopLeft, tileRect.BottomRight),
                    new LineGeometry(tileRect.BottomLeft, tileRect.TopRight)
                }
                    }
                }
            };

            var offsetGrid = new DrawingBrush
            {
                Stretch = Stretch.None,
                AlignmentX = AlignmentX.Left,
                AlignmentY = AlignmentY.Top,
                Transform = new TranslateTransform(bounds.Left, bounds.Top),
                Drawing = new GeometryDrawing
                {
                    Geometry = new RectangleGeometry(new Rect(bounds.Size)),
                    Brush = gridTile
                }
            };

            return offsetGrid;
        }

        private void onFontTabSelected()
        {
            // Veronika writes here ...
            // what should happen when user clicks her tab
        }

        private void onFontTabUnselected()
        { 
            // what should happen on unselection of your tab
        }

        private void fontCanvas_MouseLeftButtonDown(object sender, System.Windows.Input.MouseEventArgs e)
        {
			Point p = e.GetPosition(fontCanvas);
			if (p.X < fontCanvas.Width && p.X >= 0 && p.Y < fontCanvas.Height && p.Y >= 0) 
			{
				m_startMouseClick = e.GetPosition(fontCanvas);
				m_segmentController.onMouseDown(m_startMouseClick);
			}
        }

        private void fontCanvas_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(fontCanvas);
			if (p.X < fontCanvas.Width && p.X >= 0 && p.Y < fontCanvas.Height && p.Y >= 0)
			{
				m_segmentController.onMouseUp(p);
				Undo.IsEnabled = !m_segmentController.hasEmptyActions();
			}

        }

        private void fontCanvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
			Point p = e.GetPosition(fontCanvas);
			if (p.X < fontCanvas.Width && p.X >= 0 && p.Y < fontCanvas.Height && p.Y >= 0)
			{
				m_segmentController.onMouseMove(e.GetPosition(fontCanvas));
			}
        }


        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem item = ((sender as System.Windows.Controls.ComboBox).SelectedItem as ComboBoxItem);
        }

		private void RadioButton_Checked(object sender, RoutedEventArgs e)
		{
			RadioButton btn = (RadioButton)sender;

			if (m_segmentController != null)
			{
				switch (btn.Content.ToString())
				{
					case "Draw":
						m_segmentController.m_state = Controller.SegmentController.ControllerState.ADD;
						break;
					case "Move":
						m_segmentController.m_state = Controller.SegmentController.ControllerState.MOVE;
						break;
				}
			}
		}

        private void CreateFontButton_Click(object sender, RoutedEventArgs e)
        {
            CreateFontFile();
        }

		private bool CreateFontFile()
		{
			var dlg = new Microsoft.Win32.SaveFileDialog
			{
				FileName = "new_font",
				DefaultExt = ".fnt",
				Filter = "Fonts (*.fnt)|*.fnt"
			};

			// Show save font dialog box
			var result = dlg.ShowDialog();

			if (result != true)
				return false;

			// Save font
			var filename = dlg.FileName;
			if (File.Exists(filename))
				File.Delete(filename);
			var fs = File.Create(filename);
			fs.Close();
			SaveButton.IsEnabled = true;

			if (null == m_font)
				m_font = new Font(filename);
			else
				m_font.SaveFont(filename);
			isTmpFont = false;
			LoadedFontLabel.Content = Path.GetFileNameWithoutExtension(dlg.FileName);
			
			return true;
		}

        private void LoadFontButtonClick(object sender, RoutedEventArgs e)
        {
			Clear();
            LoadFont();
        }

        private void LoadFont()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".fnt",
                Filter =
                    "Fonts (*.fnt)|*.fnt"
            };

            var result = dlg.ShowDialog();
            if (result != true) return;

            var filename = dlg.FileName;
            m_font = new Font(filename);

            isTmpFont = false;
            LoadedFontLabel.Content = Path.GetFileNameWithoutExtension(dlg.FileName);
			if (LetterTextBox.Text != "")
				m_segmentController.showLetter(m_font.getLetter(LetterTextBox.Text[0]), true);
			else
				m_segmentController.clear();
			SaveButton.IsEnabled = true;
        }

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            m_segmentController.undo();
			Undo.IsEnabled = !m_segmentController.hasEmptyActions();
        }

        private void Done_Click(object sender, RoutedEventArgs e)
        {
            //m_segmentController.showPreview(previewGrid);
        }

        private void SaveLetterButton_OnClick(object sender, RoutedEventArgs e)
        {
            var letterPath = m_segmentController.CreateLetterPath();
			if (LetterTextBox.Text.Length > 0)
			{
				var letter = new Letter(Char.ToUpper(LetterTextBox.Text[0]), letterPath, m_segmentController.getCurveList());

				if (m_font == null)
					m_font = new Font();
                
				m_font.AddLetterToFont(letter);
			}
        }


		private void Clear_Click(object sender, RoutedEventArgs e)
		{
		    Clear();
		}

        private void Clear()
        {
            m_segmentController.clear();
            Undo.IsEnabled = false;
        }


        private void LetterTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (LetterTextBox.Text != "")
			{
				if (m_font != null)
				{
					bool result = m_segmentController.showLetter(m_font.getLetter(LetterTextBox.Text[0]));
					if (result)
						Undo.IsEnabled = false;
				}
			    SaveLetterButton.IsEnabled = true;
			}
		}


		private void CheckBox_Checked(object sender, RoutedEventArgs e)
		{
			m_segmentController.makeSmooth(true);
		}


		private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
		{
			m_segmentController.makeSmooth(false);
		}

		private void SaveFontButtonClicked(object sender, RoutedEventArgs e)
		{
			if (m_font != null)
				m_font.SaveFont();
		}

       
    }
}
