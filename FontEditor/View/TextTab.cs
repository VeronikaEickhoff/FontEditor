using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;

using FontEditor.Contoller;
/// Andrusha codes here
/// naming convention: all private attributes with prefix 'm_'
namespace FontEditor.View
{        
    
    public partial class MainWindow : Window
    {
        enum EditorEvent
        { 
            LETTER_ADDED,
            LETTER_REMOVED
        }

        private Timer m_cursorTimer;
        private Line m_cursorLine;
        private bool m_cursorIsVisible = true;
        private bool m_textTabIsInited = false;
        private Point m_cursorTop;
        private int m_cursorHeight = 40;
        private int m_leftTextMargin = 5;
        private int m_rightTextMargin = 5;
        private int m_topTextMargin = 5;
        private int m_bottomTextMargin = 5;
        private int m_cursorWidth = 2;
        private int m_blinkingInterval = 1000;
        private int m_fontWidth = 30;
        private int m_letterDistanceX = 5;
        private int m_letterDistanceY = 5;
        private int m_fontHeight = 40;
        private bool m_editorFocused = false;
        private int m_selectionStartIdx = -1;
        private int m_selectionEndIdx = -1;

       // private List<Font.Letter> m_letters;
        private int m_lettersInLine = 0; // how many letters can fit in one line
        //private Font m_currentFont;


        [DllImport("kernel32.dll")]
        static extern bool AttachConsole(int dwProcessId);
        private const int ATTACH_PARENT_PROCESS = -1;

        private void initTextTab()
        {
            if (!m_textTabIsInited)
            {
                m_textTabIsInited = true;
                m_cursorTimer = new Timer();
                m_cursorLine = new Line();
                m_cursorLine.Stroke = System.Windows.Media.Brushes.Black;
                m_cursorTop = new Point(m_leftTextMargin, m_topTextMargin);
                updateCursorLinePos();
                m_cursorLine.VerticalAlignment = VerticalAlignment.Center;
                m_cursorLine.StrokeThickness = m_cursorWidth;
                m_cursorTimer.Interval = m_blinkingInterval / 2;
                m_cursorTimer.Tick += new EventHandler(timerTick);
                m_lettersInLine = (int)(editCanvas.Width - m_leftTextMargin - m_rightTextMargin) / (m_fontWidth + m_letterDistanceX);

              //  editCanvas.Focusable = true;
            }
        }

        private void timerTick(object sender, EventArgs e)
        {
            if (m_cursorIsVisible)
            {
                editCanvas.Children.Add(m_cursorLine);
            }
            else
            {
                editCanvas.Children.Remove(m_cursorLine);
            }

            m_cursorIsVisible = !m_cursorIsVisible;
        }

        private void onEditTabSelected()
        {
            if (m_editorFocused)
            {
                m_cursorTimer.Start();
            }
        }

        private void onEditTabUnselected()
        {
            m_cursorTimer.Stop();
        }


        private void editCanvas_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            System.Windows.MessageBox.Show("You are moving mouse", "My Application");
        }

        private void editCanvas_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {

        }

        private void editCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!m_editorFocused)
            {
                m_editorFocused = true;
                m_cursorIsVisible = false;
                m_cursorTimer.Start();
                System.Diagnostics.Debug.WriteLine("canvas down");
            }
        }

        private void textTabMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (m_editorFocused)
            {
                m_editorFocused = false;
                System.Diagnostics.Debug.WriteLine("canvas unfocused");
                var mouseWasDownOn = e.Source as UIElement;

                m_cursorTimer.Stop();
                editCanvas.Children.Remove(m_cursorLine);
            }
        }

        private void textTabKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (m_editorFocused)
            {
                char ch = KeyTranslator.GetCharFromKey(e.Key);
                if (m_selectionStartIdx != -1) // then first of all delete text if backspace, delete, or existing font letter was pressed
                {
                    if (e.Key == Key.Back /* ||  m_currentFont.contains(ch)*/) // by the way, we need our font to always contains space
                    {
                        //m_letters.Remove() from startIdx to endIdx
                        m_selectionEndIdx = -1;
                        getPositionByIndex(m_selectionStartIdx, m_cursorTop);
                        m_selectionStartIdx = -1;
                        updateCursorLinePos();
                    }
                }
               

                // check if font contains letter that user input
                // if not, just do nothing
                // else, write it

               /* if (!m_currentFont.contains(ch))
                {
                    if (e.Key == Key.Delete)
                    { 
                        // m_letters remove next letter after cursor

                        // NB this doesn't alter cursor pos
                    }
                    else if (e.Key == Key.Back)
                    { 
                        // remove prev letter before cursor
                        recountCursorPosition(EditorEvent.LETTER_REMOVED);
                    }
                    return;
                }
                Font.Letter newLetter = Add(Font.getLetter(ch));
                newLetter.scale(m_fontWidth, m_fontHeight);
                newLetter.setPosition(m_cursorTop.X, m_cursorTop.Y);
                editCanvas.Children.Add(newLetter);*/
                recountCursorPosition(EditorEvent.LETTER_ADDED);
                
            }
        }

        void recountCursorPosition(EditorEvent e)
        { 
            switch (e)
            {
                case EditorEvent.LETTER_ADDED:
                    if (m_cursorTop.X + 2 * m_fontWidth + m_letterDistanceX < editCanvas.Width - m_rightTextMargin)
                        m_cursorTop.X += m_fontWidth + m_letterDistanceX;
                    else
                    {
                        m_cursorTop.Y += m_fontHeight + m_letterDistanceY;
                        m_cursorTop.X = m_leftTextMargin;
                    }
                    break;
                case EditorEvent.LETTER_REMOVED:
                    if (m_cursorTop.X - m_fontWidth - m_letterDistanceX >= m_leftTextMargin)
                        m_cursorTop.X -= (m_fontWidth + m_letterDistanceX);
                    else
                    {
                        m_cursorTop.Y -= (m_fontHeight + m_letterDistanceY);
                        m_cursorTop.X = m_leftTextMargin + m_lettersInLine * (m_fontWidth + m_letterDistanceX);
                    }
                    
                    break;
            }

            updateCursorLinePos();

        }

        private void updateCursorLinePos()
        {
            m_cursorLine.X1 = m_cursorTop.X;
            m_cursorLine.X2 = m_cursorTop.X;
            m_cursorLine.Y1 = m_cursorTop.Y;
            m_cursorLine.Y2 = m_cursorTop.Y + m_cursorHeight;
        }

        private void getPositionByIndex(int idx, Point p)
        {
            p.X = m_leftTextMargin + (idx % m_lettersInLine) * (m_fontWidth + m_letterDistanceX);
            p.Y = m_topTextMargin + (idx / m_lettersInLine) * (m_fontHeight + m_letterDistanceY);
        }

    }
}
