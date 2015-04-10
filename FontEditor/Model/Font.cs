﻿using System;
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
                return;

            m_letters.Add(letter);
			m_letterNames.Add(Char.ToUpper(letter.Name));

            var serializedLetter = letter.Serialize();

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
