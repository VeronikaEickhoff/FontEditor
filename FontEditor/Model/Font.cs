using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Path = System.Windows.Shapes.Path;

namespace FontEditor.Model
{
    class Font
    {
        private string m_fontFileName;
        private Dictionary<char, Path> m_letters;

        // Load existing or just created font
        public Font(string fontFileName)
        {
            m_fontFileName = fontFileName;
            
            m_letters = new Dictionary<char, Path>();
            
            using (var sr = File.OpenText(m_fontFileName))
            {
                var s = "";
                while ((s = sr.ReadLine()) != null)
                {
                    var letter = new Letter(s);
                    m_letters.Add(letter.Name, letter.LetterPath);
                }
            }
        }

        // Create letter and append it to the end of font file
        public void AddLetterToFont(Letter letter)
        {
            m_letters.Add(letter.Name, letter.LetterPath);

            var serializedLetter = letter.Serialize();

            using (var sw = File.AppendText(m_fontFileName))
            {
                sw.WriteLine(serializedLetter);
            }
        }

        public Path FindLetter(char c)
        {
            if (!m_letters.ContainsKey(c))
                return null;
            return m_letters[c];
        }
    }
}
