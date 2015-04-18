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
        private List<Letter> m_letters;
		private List<char> m_letterNames;

        public Font(string fontFileName)
        {
            m_fontFileName = fontFileName;

			m_letters = new List<Letter>();
			m_letterNames = new List<char>();

            using (var sr = File.OpenText(m_fontFileName))
            {
                var s = "";
                while ((s = sr.ReadLine()) != "-text-" && s != null)
                {
                    var letter = new Letter(s);
					m_letters.Add(letter);
					m_letterNames.Add(letter.Name);
                }
            }
        }

		// Create letter and append it to the end of font file
		public void AddLetterToFont(Letter letter)
		{
			if (m_letterNames.Contains(Char.ToUpper(letter.Name)))
			{
				m_letterNames.Remove(Char.ToUpper(letter.Name));
				m_letters.Remove(letter);
			}

			m_letters.Add(letter);
			m_letterNames.Add(Char.ToUpper(letter.Name));

			var serializedLetter = letter.Serialize();

			// it's shit to save letter each time in file; by the way, appending is also bad for we would like to 
			// be able to change letter after we have saved it once
			// we'd better just have stored letters in m_letters structure, and save them to file only when user explicitly 
			// pushes specified button (save font or smth like that), or ask about if he wants to save the font if he is about
			// to lose his results while creating new font or closing an app
			// TODO Veronika, rewrite following three lines, they are incorrect now
			using (var sw = File.AppendText(m_fontFileName))
			{
				sw.WriteLine(serializedLetter);
			}
		}

        public Path FindLetter(char c)
        {
			char name = Char.ToUpper(c);
            if (!m_letterNames.Contains(name))
                return null;
			return m_letters[m_letterNames.IndexOf(name)].LetterPath;
        }

		public bool hasLetter(char l)
		{ 
			return (m_letterNames.Contains(Char.ToUpper(l)));
		}

		public Letter getLetter(char l)
		{
			if (!hasLetter(l))
				return null;
			return m_letters[m_letterNames.IndexOf(Char.ToUpper(l))];
		}

        public void SaveFont(string filename)
        {
            using (var sw = File.CreateText(filename))
            {
                foreach (var letter in m_letters)
                {
                    var serializedLetter = letter.Serialize();

                    sw.WriteLine(serializedLetter);
                }
            }
        }
    }
}
