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
using System.Windows.Input;

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

        // TODO merge curves!
        // TODO Fill curves!
        // TODO delete curves on click

        private Curve m_curve;
        private Path m_path; // TODO remove from here and use in SegmentController
        private Ellipse[] m_points;

        private Canvas canvas;
        private BezierSegment m_segment;
        private PathFigure m_figure;

        private DrawableCurve m_next;
        private DrawableCurve m_prev;
        private static double outerPointRadius = 6;
        private static double innerPointRadius = 4;
        private static double[] radiuses = { outerPointRadius, innerPointRadius, innerPointRadius, outerPointRadius };
        private static Brush[] untouchedBrushes = {Brushes.Black, Brushes.Gray, Brushes.Gray, Brushes.Black};

        private void DrawCurve()
        {
            // use m_points here - when they are dragged, the curve position will be updated accordingly
            Point[] p =
            {
                new Point(Canvas.GetLeft(m_points[0]), Canvas.GetTop(m_points[0])), 
                new Point(Canvas.GetLeft(m_points[1]), Canvas.GetTop(m_points[1])),
                new Point(Canvas.GetLeft(m_points[2]), Canvas.GetTop(m_points[2])),
                new Point(Canvas.GetLeft(m_points[3]), Canvas.GetTop(m_points[3]))
            };
            m_segment = new BezierSegment(p[1], p[2], p[3], true);
            m_figure = new PathFigure { StartPoint = p[0], Segments = new PathSegmentCollection { m_segment } };
            var pathGeometry = new PathGeometry { Figures = new PathFigureCollection { m_figure } };
            
            canvas.Children.Remove(m_path);
            m_path = new Path { Stroke = new SolidColorBrush(Colors.Black), Data = pathGeometry, StrokeThickness = 2};
            m_path.MouseDown += onCurveMouseDown;
            canvas.Children.Add(m_path);
        }

        private void onCurveMouseDown(object sender, MouseButtonEventArgs e)
        {
            var path = (Path) sender;
            path.Stroke = new SolidColorBrush(Colors.DeepPink);
        }

        public DrawableCurve(Curve curve, PathGeometry pathGeometry1, Canvas myCanvas)
        {
            canvas = myCanvas;
            m_curve = curve;

            Point[] p = curve.getPoints();

            m_points = new Ellipse[4];
            for (int i = 0; i < 4; i++)
            {
                var ellipse = CreateEllipse(i);
                Canvas.SetLeft(ellipse, p[i].X - radiuses[i]);
                Canvas.SetTop(ellipse, p[i].Y - radiuses[i]);
                canvas.Children.Add(ellipse);
                m_points[i] = ellipse;
            }

            DrawCurve();
            
            m_next = null;
        }

        private Ellipse CreateEllipse(int i)
        {
            // First check if the point or a close point already exists, if yes, don't create a new one - two curves will share one point
            var ellipse = new Ellipse
            {
                Stroke = untouchedBrushes[i],
                Fill = untouchedBrushes[i],
                Width = radiuses[i]*2,
                Height = radiuses[i]*2
            };

            ellipse.MouseDown += ellipse_MouseDown;
            ellipse.MouseMove += ellipse_MouseMove;
            ellipse.MouseUp += ellipse_MouseUp;

            return ellipse;
        }

        void ellipse_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var ellipse = (Ellipse) sender;
            if (e.LeftButton != MouseButtonState.Pressed)
                return;
            ellipse.CaptureMouse();
            ellipse.RenderTransform = new ScaleTransform(1.25, 1.25, ellipse.Width / 2, ellipse.Height / 2);
            ellipse.Opacity = 0.75;
        }

        void ellipse_MouseMove(object sender, MouseEventArgs e)
        {
            /*// TODO move all overlapping points!
            // find close points
            int tolerance = 2;
            foreach (var child in canvas.Children)
            {
                var element = child as Ellipse;
                if (element != null)
                {
                    var x1 = Canvas.GetLeft(ellipse);
                    var y1 = Canvas.GetTop(ellipse);
                    var x2 = Canvas.GetLeft(element);
                    var y2 = Canvas.GetTop(element);

                    if (x2 >= x1 - tolerance && x2 <= x1 + tolerance && y2 >= y1 - tolerance && y2 <= y1 + tolerance)
                    {
                        ellipse_MouseMove(element, e);
                        return;
                    }
                }
            }
*/
            var ellipse = (Ellipse)sender;

            if (e.LeftButton != MouseButtonState.Pressed || !ellipse.IsMouseCaptured)
                return;
            var pos = e.GetPosition(canvas);
            Canvas.SetLeft(ellipse, pos.X - ellipse.Width * 0.5);
            Canvas.SetTop(ellipse, pos.Y - ellipse.Height * 0.5);

            DrawCurve();
        }

        void ellipse_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var ellipse = (Ellipse)sender;
            if (!ellipse.IsMouseCaptured)
                return;
            ellipse.ReleaseMouseCapture();
            ellipse.RenderTransform = null;
            ellipse.Opacity = 1;
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
