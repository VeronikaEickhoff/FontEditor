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

using FontEditor.Contoller;

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
        private List<Ellipse> m_points;

        private Canvas m_canvas;
        private BezierSegment m_segment;
        private PathFigure m_figure;

        private DrawableCurve m_next;
        private DrawableCurve m_prev;
        private static double outerPointRadius = 6;
        private static double innerPointRadius = 4;
        private static double[] radiuses = { outerPointRadius, innerPointRadius, innerPointRadius, outerPointRadius };
        private static Brush[] untouchedBrushes = {Brushes.Black, Brushes.Gray, Brushes.Gray, Brushes.Black};
        
        private bool m_isTouched = false;
        private Point m_lastTouchPos;
        private SegmentController m_controller;
        

        public DrawableCurve(Curve curve, PathGeometry pathGeometry, Canvas myCanvas, SegmentController controller)
        {
            m_curve = curve;
            m_figure = new PathFigure();
            m_canvas = myCanvas;

            Point[] p = curve.getPoints();
            m_figure.StartPoint = p[0];
            m_figure.IsClosed = false;

            m_segment = new BezierSegment(p[1], p[2], p[3], true);
            m_figure.Segments.Add(m_segment);
            pathGeometry.Figures.Add(m_figure);

            m_points = new List<Ellipse>(4);
            for (int i = 0; i < 4; i++)
            {
                m_points.Add(CreateEllipse(i));
                m_canvas.Children.Add(m_points[i]);
            }
            m_controller = controller;

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
            var ellipse = (Ellipse)sender;
            int idx = m_points.IndexOf(ellipse);
            if (idx == 3 && m_next != null)
                return;

            if (e.LeftButton != MouseButtonState.Pressed || !ellipse.IsMouseCaptured)
                return;
            var pos = e.GetPosition(m_canvas);
            Canvas.SetLeft(ellipse, pos.X - ellipse.Width * 0.5);
            Canvas.SetTop(ellipse, pos.Y - ellipse.Height * 0.5);

            
            
            translate(idx, (Vector)pos - (Vector)m_curve.getPoints()[idx]);
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

        public DrawableCurve(Curve curve, DrawableCurve prev, Canvas canvas, SegmentController controller, bool connectToHead)
        {
            m_curve = curve;
            m_figure = prev.m_figure;

            m_canvas = canvas;
            Point[] p = curve.getPoints();
            m_segment = new BezierSegment(p[1], p[2], p[3], true);
            m_figure.Segments.Add(m_segment);
            m_next = null;
            m_prev = prev;
            prev.m_next = this;

            if (!connectToHead)
                m_figure.Segments.Add(m_segment);
            else
            {
                m_figure.Segments.Insert(0, m_segment);
                m_figure.StartPoint = p[0];
            }

            m_points = new List<Ellipse>(4);
            for (int i = 0; i < 4; i++)
            {
                m_points.Add(CreateEllipse(i));
                canvas.Children.Add(m_points[i]);
            }

            m_controller = controller;
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


            if (m_prev == null)
            {
                m_figure.StartPoint = p[0];
            }
        }

        public bool hasNext()
        {
            return m_next != null;
        }

        public bool hasPrev()
        {
            return m_prev != null;
        }

        public Point[] getPoints()
        {
            return m_curve.getPoints();
        }
    }
}
