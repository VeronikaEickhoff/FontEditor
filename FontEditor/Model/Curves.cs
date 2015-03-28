using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms.VisualStyles;

namespace FontEditor.Model
{
    public class Curves
    {
        private List<Curve> _Curves = new List<Curve>();
        private Curve _CurrentCurve;

        public void AddCurve(List<Point> points)
        {
            _Curves.Add(new Curve(){ Points = points });
        }

        public List<Point> FindCurve(List<Point> points)
        {
            var curveToBeFound = new Curve(){ Points = points };
            _CurrentCurve = _Curves.Find(curve => Equals(curve, curveToBeFound));
           
            return _CurrentCurve != null ? _CurrentCurve.Points : null;  // returned points are used for drawing purposes
        }

        // Find curve point 'from' and move it to 'to'
        public void MoveCurvePoint(Point from, Point to)
        {
            // change 'from' point of _CurrentCurve
            _CurrentCurve.MovePoint(from, to);
            // TODO update _Curves
        }

        public void RemoveCurve()
        {
            // delete _CurrentCurve from _Curves
        }
    }

    internal class Curve
    {
        // 4 points of the curve
        public List<Point> Points;

        public override bool Equals(object curve2)
        {
            return AreAllClosePoints(Points, ((Curve) curve2).Points);
        }

        public void MovePoint(Point from, Point to)
        {
            const int tolerance = 3; // tolerance in pixels

            // first find 'from' point in the list of the curve points
            for (int i = 0; i < Points.Count; i++)
            {
                if (Points[i].X >= from.X - tolerance && Points[i].X <= from.X + tolerance
                    && Points[i].Y >= from.Y - tolerance && Points[i].Y <= from.Y + tolerance)
                {
                    var point = Points[i];
                    point.X = to.X;
                    point.Y = to.Y;
                }
            }

        }

        private bool AreAllClosePoints(List<Point> curve1Points, List<Point> curve2Points)
        {
            return curve1Points.All(point1 => ExistsClosePoint(point1, curve2Points));
        }

        private bool ExistsClosePoint(Point point, List<Point> points)
        {
            const int tolerance = 3; // tolerance in pixels
            return points.Any(_point => point.X >= _point.X - tolerance && point.X <= _point.X + tolerance
                                        && point.Y >= _point.Y - tolerance && point.Y <= _point.Y + tolerance);
        }
    }
}
