using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontEditor.Model;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Diagnostics;

namespace FontEditor.View
{
    class DrawableCurve
    {
        enum PointState
        { 
            TOUCHED,
            UNTOUCHED,
            INVISIBLE
        }

        private Curve m_curve;
        private PathFigure m_figure;
        private BezierSegment m_segment;
        private Ellipse[] m_points;
 
        private DrawableCurve m_next;
        private DrawableCurve m_prev;
        private static double outerPointRadius = 5;
        private static double innerPointRadius = 3;
        private static double[] radiuses = { outerPointRadius, innerPointRadius, innerPointRadius, outerPointRadius };
        private static Brush[] untouchedBrushes = {Brushes.Black, Brushes.Gray, Brushes.Gray, Brushes.Black};

        public DrawableCurve(Curve curve, PathGeometry path, Canvas canvas) 
        {
            m_curve = curve;
            m_figure = new PathFigure();

            Point[] p = curve.getPoints();
            m_figure.StartPoint = p[0];
            m_figure.IsClosed = false;

            m_segment = new BezierSegment(p[1], p[2], p[3], true);
            m_figure.Segments.Add(m_segment);
            path.Figures.Add(m_figure);

            m_points = new Ellipse[4];
            for (int i = 0; i < 4; i++)
            {
                Ellipse ellipse = new Ellipse();
                ellipse.Stroke = untouchedBrushes[i];
                ellipse.Fill = untouchedBrushes[i];
                ellipse.Width = radiuses[i] * 2;
                ellipse.Height = radiuses[i] * 2;
                Canvas.SetLeft(ellipse, p[i].X - radiuses[i]);
                Canvas.SetTop(ellipse, p[i].Y - radiuses[i]);
                canvas.Children.Add(ellipse);
                m_points[i] = ellipse;
            }
            
            
            m_next = null;
        }

        public DrawableCurve(Curve curve, DrawableCurve prev, Canvas canvas)
        {
            m_curve = curve;
            m_figure = prev.m_figure;

            Point[] p = curve.getPoints();
            m_segment = new BezierSegment(p[1], p[2], p[3], true);
            m_figure.Segments.Add(m_segment);
            m_next = null;
            m_prev = prev;
            prev.m_next = this;
        }

        public void translate(int idx, Vector dv)
        {
            m_curve.translate(idx, dv);
            Point[] p = m_curve.getPoints();
            m_segment.Point1 = p[1];
            m_segment.Point2 = p[2];
            m_segment.Point3 = p[3];

            for (int i = 0; i < 4; i++)
            {
                Canvas.SetLeft(m_points[i], p[i].X - radiuses[i]);
                Canvas.SetTop(m_points[i], p[i].Y - radiuses[i]);
            }
        }

        public bool hasNext()
        {
            return m_next == null;
        }

        public Point[] getPoints()
        {
            return m_curve.getPoints();
        }
    }
}
