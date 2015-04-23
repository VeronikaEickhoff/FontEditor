using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.Generic;
using Path = System.Windows.Shapes.Path;

namespace FontEditor.Model
{
    class Font
    {
        private string m_fontFileName;
        private Dictionary<Char, Letter> m_letters;

        public Font(string fontFileName)
        {
            m_fontFileName = fontFileName;

			m_letters = new Dictionary<Char, Letter>();

            if (!File.Exists(m_fontFileName))
                return;

            using (var sr = File.OpenText(m_fontFileName))
            {
                while (true)
                {
                    var s = sr.ReadLine();
                    if (s == null || Regex.IsMatch(s, @"\$\S+\$")) break; // next font or text starts

                    var letter = new Letter(s);
					m_letters.Add(Char.ToUpper(letter.Name), letter);
                }
            }
        }

        public Font(string str, bool fromStirng)
        {
			m_letters = new Dictionary<Char, Letter>();

            using (var sr = new StringReader(str))
            {
                while (true)
                {
                    var s = sr.ReadLine();
                    if (s == null || Regex.IsMatch(s, @"\$\S+\$")) break; // next font or text starts

                    var letter = new Letter(s);
					m_letters.Add(Char.ToUpper(letter.Name), letter);
                }
            }
        }



        // Create letter and append it to the end of font file
		public void AddLetterToFont(Letter letter)
		{
			if (m_letters.ContainsKey(Char.ToUpper(letter.Name)))
			{
				m_letters.Remove(Char.ToUpper(letter.Name));
			}

			m_letters.Add(letter.Name, letter);

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
            if (!m_letters.ContainsKey(name))
                return null;
			return m_letters[name].LetterPath;
        }

		public bool hasLetter(char l)
		{ 
			return (m_letters.ContainsKey(Char.ToUpper(l)));
		}

		public Letter getLetter(char l)
		{
			if (!hasLetter(l))
				return null;
			return m_letters[Char.ToUpper(l)];
		}

        public void SaveFont(string filename)
        {
            using (var sw = File.CreateText(filename))
            {
                foreach (var pair in m_letters)
                {
                    var serializedLetter = pair.Value.Serialize();

                    sw.WriteLine(serializedLetter);
                }
            }
        }

		public IEnumerable<Path> LettersPaths
        {
            get
            {
                return m_letters.Select(letter => letter.Value.LetterPath).ToList();
            }
       }

        public string SerializeFont()
        {
            return m_letters.Select(letter => letter.Value.Serialize())
                            .Aggregate("", (current, serializedLetter) => current + Environment.NewLine + (serializedLetter));
        }
    }
}
