using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

using FontEditor.Model;
using FontEditor.Controller;

// Veronika codes here
namespace FontEditor.View
{
    public partial class MainWindow : Window
    {
        private Point m_startMouseClick;
        private SegmentController m_segmentController;

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
            m_startMouseClick = e.GetPosition(fontCanvas);
            m_segmentController.onMouseDown(m_startMouseClick);
        }

        private void fontCanvas_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Point endPoint = e.GetPosition(fontCanvas);
            m_segmentController.onMouseUp(endPoint);
        }

        private void fontCanvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            m_segmentController.onMouseMove(e.GetPosition(fontCanvas));
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
    }
}
