﻿using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using FontEditor.Contoller;
using FontEditor.Model;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using Path = System.Windows.Shapes.Path;
using TextBox = System.Windows.Controls.TextBox;

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

        private Font m_currentFont;
        private Text m_text;
        private int m_textSize = 0;
        private string m_fontName = null;

        // To make everything faster - don't load a font if it's already loaded
        private Dictionary<string, Font> fontsCache = new Dictionary<string, Font>();
            
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
                Font.Letter newLetter = Font.getLetter(ch);
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


        private void ChooseFontButton_OnClick(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".fnt",
                Filter =
                    "Fonts (*.fnt)|*.fnt"
            };

            var result = dlg.ShowDialog();
            if (result != true) return;

            string fontName = System.IO.Path.GetFileNameWithoutExtension(dlg.FileName) ?? "";
            
            // Load font from cache if possible
            if (fontsCache.ContainsKey(fontName))
                m_currentFont = fontsCache[fontName];
            else
            {
                m_currentFont = new Font(dlg.FileName);
                fontsCache[fontName] = m_currentFont;
            }

            m_fontName = dlg.FileName;
            TELoadedFontLabel.Content = System.IO.Path.GetFileNameWithoutExtension(dlg.FileName);


            // Show available letters in the bottom of the window
            AvailableLettersWrapPanel.Children.Clear();
            foreach (var letterPath in m_currentFont.LettersPaths)
            {
                var p = ClonePath(letterPath);
                Grid letterGrid = new Grid
                {
                    Width = AvailableLettersWrapPanel.Height - 10,
                    Height = AvailableLettersWrapPanel.Height - 10
                };
                letterGrid.Children.Add(p);

                AvailableLettersWrapPanel.Children.Add(letterGrid);
            }

            TextEditTextBox.Text += "<font=" + fontName + ">";
			if (m_textSize == 0)
				SetSizeButton_Click(null, null);
        }

        private Path ClonePath(Path source)
        {
            Path clone = new Path();
            foreach (System.Reflection.PropertyInfo pi in typeof(Path).GetProperties())
            {
                if (pi.CanWrite && pi.CanRead)
                {
                    pi.SetValue(clone, pi.GetValue(source, null), null);
                }
            }
            return clone;
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            /*if (m_text == null)
                m_text = new Text(m_currentFont);

            if (m_currentFont == null) return;

            var textBox = sender as TextBox;

            if (!textBox.Text.Any())
                return;
            var letterName = textBox.Text[textBox.Text.Count() - 1];
            
            if (e.Key == Key.Back)
            {
                m_text.RemoveLast();
                TextEditorWrapPanel.Children.RemoveAt(TextEditorWrapPanel.Children.Count - 1);
                return;
            }

            Path letterPath = m_currentFont.FindLetter(letterName);
            if (letterPath == null) return;

            m_text.AddLast(new Letter(letterName, letterPath, null));

            var p = ClonePath(letterPath);
            Grid letterGrid = new Grid {Width = TextEditorWrapPanel.Width/10, Height = TextEditorWrapPanel.Height/10};
            letterGrid.Children.Add(p);

            TextEditorWrapPanel.Children.Add(letterGrid);*/
        }

        private void SaveText_OnClick(object sender, RoutedEventArgs e)
        {

            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                FileName = "MyFontText",
                DefaultExt = ".fntxt",
                Filter = "Font Text (*.fntxt)|*.fntxt"
            };

            // Show save font dialog box
            var result = dlg.ShowDialog();

            if (result != true) return;

            // Save font
            var filename = dlg.FileName;
/*
            m_text.SaveText(filename, TextEditTextBox.Text);*/
            SaveText(filename);
        }

        private void SaveText(string filename)
        {
            File.WriteAllText(filename, string.Empty);
            using (var sw = File.AppendText(filename))
            {
                // Save cache of fonts
                foreach (var font in fontsCache)
                {
                    sw.WriteLine("$" + font.Key + "$");
                    sw.WriteLine(font.Value.SerializeFont());
                }
                // Save text
                sw.WriteLine("$text$");
                sw.Write(TextEditTextBox.Text);
            }
        }

        private void LoadText_Click(object sender, RoutedEventArgs e)
        {
            TextEditorWrapPanel.Children.Clear();

            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".fntxt",
                Filter =
                    "Font Text (*.fntxt)|*.fntxt"
            };

            var result = dlg.ShowDialog();
            if (result != true) return;

            /*m_text = new Text(dlg.FileName);
            TextEditTextBox.Text = m_text.m_text;

            foreach (var letter in m_text.listOfLetters)
            {
                var p = ClonePath(letter.LetterPath);
                Grid letterGrid = new Grid
                {
                    Width = TextEditorWrapPanel.Width/10,
                    Height = TextEditorWrapPanel.Height/10
                };
                letterGrid.Children.Add(p);

                TextEditorWrapPanel.Children.Add(letterGrid);
            }*/

            LoadText(dlg.FileName);
        }

        private void LoadText(string filename)
        {
            // Fill fonts cache
			if (m_textSize == 0)
				SetSizeButton_Click(null, null);

            using (var sr = File.OpenText(filename))
            {
                /*var s = "";
                while (s != null)
                {
                    s = sr.ReadLine();
                    var match = Regex.Match(s, @"\$(\S+?)\$\s+([^\$]+)");
                    if (match.Success)
                    {
                        if (match.Groups[1].Value != "text")
                            // Create font from the matched string
                            fontsCache[match.Groups[1].Value] = new Font(match.Groups[2].Value, true);
                        else
                            TextEditTextBox.Text = sr.ReadToEnd();
                    }
                }*/

                var s = sr.ReadToEnd();
                while (s.Length > 0)
                {
                    var match = Regex.Match(s, @"^(\$(\S+?)\$\s+([^\$]+))");
                    if (match.Success)
                    {
                        if (match.Groups[2].Value != "text")
                            // Create font from the matched string
                            fontsCache[match.Groups[2].Value] = new Font(match.Groups[3].Value, true);
                        else
                            TextEditTextBox.Text = match.Groups[3].Value;
                    }
                    s = s.Remove(0, match.Length);
                }

            }
        }


        private void SetSizeButton_Click(object sender, RoutedEventArgs e)
        {
            TextEditTextBox.Text += "<size=" + TextSize.Text + ">";
            m_textSize = Int32.Parse(TextSize.Text);
        }

        private void TextEditTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TextEditorWrapPanel.Children.Clear();

            // Parse new text format
            var text = TextEditTextBox.Text;
            while (text.Length > 0)
            {
                var match = Regex.Match(text, @"^(<size=(\d+)>([^<]*))");
                if (match.Success)
                {
                    text = text.Remove(0, match.Length);
                    m_textSize = Convert.ToInt32(match.Groups[2].Value);
                    DrawText(match.Groups[3].Value);
                }
                else
                {
                    match = Regex.Match(text, @"^(<font=(\S+?)>([^<]*))");
                    if (match.Success)
                    {
                        text = text.Remove(0, match.Length);
                        m_fontName = match.Groups[2].Value;
                        DrawText(match.Groups[3].Value);
                    }
                }
            }
        }

        private void DrawText(string text)
        {
            if (text.Length == 0)
                return;

            if (m_fontName == null)
                return;

            string fontName = System.IO.Path.GetFileNameWithoutExtension(m_fontName) ?? "";

            // Load font from cache if possible
            m_currentFont = fontsCache[fontName];

            foreach (var letterName in text)
            {
                Path letterPath = m_currentFont.FindLetter(letterName);
                if (letterPath == null)
                    continue;
                var p = ClonePath(letterPath);
                Grid letterGrid = new Grid { Width = m_textSize * 1000 / TextEditorWrapPanel.Width, Height = m_textSize * 1000 / TextEditorWrapPanel.Height};
                letterGrid.Children.Add(p);

                TextEditorWrapPanel.Children.Add(letterGrid);
            }
        }

    }
}
