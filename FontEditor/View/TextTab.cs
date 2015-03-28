using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Forms;
/// Andrusha codes here
/// naming convention: all private attributes with prefix 'm_'
namespace FontEditor.View
{        
    public partial class MainWindow : Window
    {
        private Timer m_cursorTimer;
        private Line m_cursorLine;
        private bool m_blinkingState = true;
        private bool m_textTabIsInited = false;
        private Point m_cursorTop;
        private int m_cursorHeight = 40;
        private int m_leftTextMargin = 5;
        private int m_rightTextMargin = 5;
        private int m_topTextMargin = 5;
        private int m_bottomTextMargin = 5;
        private int m_cursorWidth = 2;
        private int m_blinkingInterval = 1000; 


        private void initTextTab()
        {
            if (!m_textTabIsInited)
            {
                m_textTabIsInited = true;
                m_cursorTimer = new Timer();
                m_cursorLine = new Line();
                m_cursorLine.Stroke = System.Windows.Media.Brushes.Black;
                m_cursorTop = new Point(m_leftTextMargin, m_topTextMargin);
                m_cursorLine.X1 = m_cursorTop.X;
                m_cursorLine.X2 = m_cursorTop.Y;
                m_cursorLine.Y1 = m_cursorTop.X;
                m_cursorLine.Y2 = m_cursorTop.Y + m_cursorHeight;
                //myLine.HorizontalAlignment = HorizontalAlignment.;
                m_cursorLine.VerticalAlignment = VerticalAlignment.Center;
                m_cursorLine.StrokeThickness = m_cursorWidth;
                m_cursorTimer.Interval = m_blinkingInterval / 2;
                m_cursorTimer.Tick += new EventHandler(timerTick);
            }
        }

        private void timerTick(object sender, EventArgs e)
        {
            if (m_blinkingState)
            {
                editCanvas.Children.Add(m_cursorLine);
            }
            else
            {
                editCanvas.Children.Remove(m_cursorLine);
            }

            m_blinkingState = !m_blinkingState;
        }

        private void onEditTabSelected()
        {
            m_cursorTimer.Start();
        }

        private void onEditTabUnselected()
        {
            m_cursorTimer.Stop();
        }
    }
}
