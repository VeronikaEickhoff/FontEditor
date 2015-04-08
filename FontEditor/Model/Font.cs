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
                while ((s = sr.ReadLine()) != "-text-" && s != null)
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
            if (!m_letters.ContainsKey(Char.ToUpper(c)))
                return null;
            return m_letters[Char.ToUpper(c)];
        }

        public void SaveFont(string filename)
        {
            using (var sw = File.CreateText(filename))
            {
                foreach (var letterElement in m_letters)
                {
                    var letter = new Letter(letterElement.Key, letterElement.Value);
                    var serializedLetter = letter.Serialize();

                    sw.WriteLine(serializedLetter);
                }
            }
        }
    }
}
