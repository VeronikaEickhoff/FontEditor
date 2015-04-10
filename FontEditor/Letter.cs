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

namespace FontEditor
{
    class Letter
    {
        public char Name;
        public Path LetterPath;
		public LinkedList<LinkedList<Curve>> m_curves;

        // (letter from font editor)
        public Letter(char name, Path letterPath)
        {
            Name = name;
            LetterPath = letterPath;
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
        }

        // Serialize
        public string Serialize()
        {
            return Name.ToString() + "*" + XamlWriter.Save(LetterPath);
        }
    }
}
