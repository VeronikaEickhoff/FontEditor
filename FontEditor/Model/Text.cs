using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FontEditor.Model
{
    class Text
    {
        public List<Letter> listOfLetters;
        private Letter m_currentLetter;
        private Font m_font;
        public string m_text;

        // Create new text
        public Text(Font font)
        {
            listOfLetters = new List<Letter>();
            m_font = font;
        }

        // Load text from file
        public Text(string filename)
        {
            m_font = new Font(filename);
            
            using (var sr = File.OpenText(filename))
            {
                string s;
                while ((s = sr.ReadLine()) != "-text-")
                {
                }

                m_text = sr.ReadToEnd();
                CreateListOfLetters();
            }
        }

        private void CreateListOfLetters()
        {
            listOfLetters = new List<Letter>();

            foreach (var character in m_text)
            {
                var letterPath = m_font.FindLetter(character);
                if (letterPath == null) continue;

                AddLast(new Letter(character, letterPath));
            }
        }

        public void SaveText(string filename, string text)
        {
            m_font.SaveFont(filename);
            using (var sw = File.AppendText(filename))
            {
                sw.WriteLine("-text-");
                sw.Write(text);
            }
        }

        public void RemoveLast()
        {
            listOfLetters.RemoveAt(listOfLetters.Count - 1);
        }

        public void AddLast(Letter letter)
        {
            listOfLetters.Add(letter);
        }

        /*private void RemoveCurrentLetter()
        {
            m_text.Remove(m_currentLetter);
        }

        private void AddAfterCurrent()
        {
            m_text.Add(m_currentLetter);
        }
         
        private void SetCurrentLetter(Letter letter)
        {
            m_currentLetter = letter;
        }*/

    }
}
