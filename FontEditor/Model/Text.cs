using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontEditor.Model
{
    class Text
    {
        public List<Letter> text;
        private Letter m_currentLetter;

        // Load text from file
        public Text(string filename)
        {

        }

        // Create new text
        public Text()
        {
            text = new List<Letter>();
        }

        public void SaveText(string filename)
        {

        }

        public void RemoveLast()
        {
            text.RemoveAt(text.Count - 1);
        }

        public void AddLast(Letter letter)
        {
            text.Add(letter);
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
