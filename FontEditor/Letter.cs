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

using FontEditor.View;

namespace FontEditor
{
    class Letter
    {
        public char Name;
        public Path LetterPath;
		public LinkedList<LinkedList<Curve>> m_curves = null;
		public LinkedList<bool> m_pathIsClosed = null;
		public LinkedList<int> m_startIndexes = null;

        // (letter from font editor)
        public Letter(char name, Path letterPath, LinkedList<LinkedList<DrawableCurve>> drawableCurves)
        {
            Name = name;
            LetterPath = letterPath;

			if (drawableCurves != null)
			{
				m_curves = new LinkedList<LinkedList<Curve>>();
				m_pathIsClosed = new LinkedList<bool>();
				m_startIndexes = new LinkedList<int>();

				foreach (LinkedList<DrawableCurve> lc in drawableCurves)
				{
					LinkedList<Curve> tmp = new LinkedList<Curve>();

					m_pathIsClosed.AddLast(lc.First.Value.getMyFigure().IsClosed);
					int i = 0;
					foreach (DrawableCurve c in lc)
					{
						tmp.AddLast(c.getMyCurve().getCopy());
						if (c.isStartCurve())
						{
							m_startIndexes.AddLast(i);
						}
						i++;
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
				m_pathIsClosed = new LinkedList<bool>();
				m_startIndexes = new LinkedList<int>();

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

					m_startIndexes.AddLast(Convert.ToInt32(tokens[idx++]));
					m_pathIsClosed.AddLast(Convert.ToBoolean(tokens[idx++]));
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
			LinkedListNode<bool> closedNode = m_pathIsClosed.First;
			LinkedListNode<int> firstIdxNode = m_startIndexes.First;

			foreach (LinkedList<Curve> lc in m_curves)
			{
				s += lc.Count.ToString()+"*";
				foreach (Curve c in lc)
				{
					s += c.ToString() + "*";
				}
				s += firstIdxNode.Value.ToString() + "*";
				s += closedNode.Value.ToString() + "*";

				firstIdxNode = firstIdxNode.Next;
				closedNode = closedNode.Next;
			}

			return s;
        }
    }
}
