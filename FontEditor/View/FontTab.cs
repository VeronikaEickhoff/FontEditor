﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;
using System.IO;
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
        private Font m_font;

        private void initFontTab()
        {
            m_segmentController = new SegmentController(fontCanvas);
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
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                FileName = "MyLovelyFont",
                DefaultExt = ".fnt",
                Filter = "Fonts (*.fnt)|*.fnt"
            };

            // Show save font dialog box
            var result = dlg.ShowDialog();

            if (result != true) return;

            // Save font
            var filename = dlg.FileName;
            if (File.Exists(filename))
                File.Delete(filename);
            var fs = File.Create(filename);
            fs.Close();
        }

        private void LoadFontButtonClick(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".fnt",
                Filter =
                    "Fonts (*.fnt)|*.fnt"
            };

            var result = dlg.ShowDialog();
            if (result != true) return;

            m_font = new Font(dlg.FileName);
            LoadedFontLabel.Content = Path.GetFileNameWithoutExtension(dlg.FileName);
            LetterTextBox.IsEnabled = true;
            SaveLetterButton.IsEnabled = true;
            LetterLabel.IsEnabled = true;
        }

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            m_segmentController.undo();
			Undo.IsEnabled = !m_segmentController.hasEmptyActions();
        }

        private void Done_Click(object sender, RoutedEventArgs e)
        {
            m_segmentController.showPreview(previewGrid);
        }

        private void SaveLetterButton_OnClick(object sender, RoutedEventArgs e)
        {
            var letterPath = m_segmentController.CreateLetterPath();
            var letter = new Letter(Char.ToUpper(LetterTextBox.Text[0]), letterPath);
            m_font.AddLetterToFont(letter);
        }


		private void Clear_Click(object sender, RoutedEventArgs e)
		{
			m_segmentController.clear();
			Undo.IsEnabled = false;
		}


    }
}
