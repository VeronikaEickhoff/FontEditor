using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontEditor.Model
{
    class Curve
    {
        public Point[] ControlPoints; // Used to create the curve
        public Point[] AllPoints; // Used to draw the curve

        public Curve(Point[] controlPoints)
        {
            ControlPoints = controlPoints;
            MagicCurveCreation();
        }
        
        // TODO implement MagicCurveCreation()
        private void MagicCurveCreation()
        {
            // TODO using ContolPoints initialize AllPoints
        }
    }
}
