using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Xml;
using Path = System.Windows.Shapes.Path;
using System.Runtime.Serialization;
using FontEditor.Model;
using System;
using System.Windows;

namespace FontEditor
{
    class Letter
    {
        public char Name;
        public Path LetterPath;
		public LinkedList<LinkedList<Curve>> m_curves = null;

        // (letter from font editor)
        public Letter(char name, Path letterPath, LinkedList<LinkedList<Curve>> curves)
        {
            Name = name;
            LetterPath = letterPath;

			if (curves != null)
			{
				m_curves = new LinkedList<LinkedList<Curve>>();
				foreach (LinkedList<Curve> lc in curves)
				{
					LinkedList<Curve> tmp = new LinkedList<Curve>();
					foreach (Curve c in lc)
					{
						tmp.AddLast(c.getCopy());
					}
					m_curves.AddLast(tmp);
				}
			}
        }

        // Deserialize (letter from file)
        public Letter(string serializedLetter)
        {
            var tokens = serializedLetter.Split('*');

            Name = tokens[0][0];

            var serializedPath = tokens[1];
            var stringReader = new StringReader(serializedPath);
            var xmlReader = XmlReader.Create(stringReader);
            LetterPath = (Path)XamlReader.Load(xmlReader);

			try
			{
				m_curves = new LinkedList<LinkedList<Curve>>();
				int n = Convert.ToInt32(tokens[2]);
				int idx = 3;
				for (int i = 0; i < n; i++)
				{
					LinkedList<Curve> tmp = new LinkedList<Curve>();
					int m = Convert.ToInt32(tokens[idx++]);
					for (int j = 0; j < m; j++)
					{
						Point[] p = new Point[4];
						for (int k = 0; k < 4; k++)
							p[k] = Point.Parse(tokens[idx++]);
						float t1 = Convert.ToSingle(tokens[idx++]);
						float t2 = Convert.ToSingle(tokens[idx++]);

						Curve c = new Curve(p, t1, t2);
						tmp.AddLast(c);
					}
					m_curves.AddLast(tmp);
				}
			}
			catch (Exception e)
			{
				m_curves = null;
			}
        }

        // Serialize
        public string Serialize()
        {
            string s = Name.ToString() + "*" + XamlWriter.Save(LetterPath) + "*" + m_curves.Count + "*";

			foreach (LinkedList<Curve> lc in m_curves)
			{
				s += lc.Count.ToString()+"*";
				foreach (Curve c in lc)
				{
					s += c.ToString() + "*";
				}
			}

			return s;
        }
    }
}
