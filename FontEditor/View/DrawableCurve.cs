using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontEditor.Model;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Windows.Input;

using FontEditor.Controller;

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
        private Ellipse[] m_points;
		private PathGeometry m_pathGeometry;

        private Canvas m_canvas;
        private BezierSegment m_segment;
        private PathFigure m_figure;

        private DrawableCurve m_next;
        private DrawableCurve m_prev;
        private static double outerPointRadius = 4;
        private static double innerPointRadius = 2;
        private static double[] radiuses = { outerPointRadius, innerPointRadius, innerPointRadius, outerPointRadius };
        private static Brush[] untouchedBrushes = {Brushes.Black, Brushes.Gray, Brushes.Gray, Brushes.Black};
		private static Brush overBrush = Brushes.DodgerBlue;
		private static Brush touchedBrush = Brushes.LimeGreen;

        private SegmentController m_controller;
        private bool m_pathIsClosed = false;
		private bool m_isStartCurve = false;
		private Line[] m_middleLines = null;
		private static float defaultMiddleLineLength = 50;

		public static bool smooth = false; 

        public DrawableCurve(Curve curve, PathFigure pathFigure, Canvas myCanvas, SegmentController controller)
        {
            m_curve = curve;
			m_figure = pathFigure;
            m_canvas = myCanvas;

            Point[] p = curve.getPoints();
            m_figure.StartPoint = p[0];
            m_figure.IsClosed = false;

            m_segment = new BezierSegment(p[1], p[2], p[3], true);
            m_figure.Segments.Add(m_segment);
			m_middleLines = new Line[2];

			for (int i = 0; i < 2; i++)
			{
				m_middleLines[i] = new Line();
				m_middleLines[i].Stroke = Brushes.Red;
                m_middleLines[i].StrokeThickness = 1;
                m_middleLines[i].StrokeDashArray = new DoubleCollection(new List<double>(){10, 5});
				m_canvas.Children.Add(m_middleLines[i]);
			}
			
			recountMiddleLines();

			m_points = new Ellipse[4];
            for (int i = 0; i < 4; i++)
            {
                Ellipse el = CreateEllipse(i);
				m_points[i] = el;
                m_canvas.Children.Add(m_points[i]);
                Canvas.SetLeft(el, p[i].X - radiuses[i]);
                Canvas.SetTop(el, p[i].Y - radiuses[i]);
            }
            m_controller = controller;

            m_next = null;
			m_prev = null;
			m_isStartCurve = true;
        }

		private void recountMiddleLines()
		{
			Point[] p = getPoints();

			for (int i = 0; i < 2; i++)
			{
				m_middleLines[i].X2 = p[i + 1].X;
				m_middleLines[i].Y2 = p[i + 1].Y;
				m_middleLines[i].X1 = p[3 * i].X;
				m_middleLines[i].Y1 = p[3 * i].Y;
			}
		}


		public Curve getMyCurve()
		{
			return m_curve.getCopy();
		}

		public bool isStartCurve()
		{
			return m_isStartCurve;
		}

		public bool isPathClosed()
		{
			return m_pathIsClosed;
		}

        private Ellipse CreateEllipse(int i)
        {
            // First check if the point or a close point already exists, if yes, don't create a new one - two curves will share one point
            var ellipse = new Ellipse
            {
                Stroke = untouchedBrushes[i],
                Fill = untouchedBrushes[i],
                Width = radiuses[i]*2,
                Height = radiuses[i]*2,
            };
			
            ellipse.MouseDown += ellipse_MouseDown;
            ellipse.MouseEnter += ellipse_MouseMove;
            ellipse.MouseUp += ellipse_MouseUp;
			ellipse.MouseLeave += ellipse_MouseLeave;
            return ellipse;
        }

        void ellipse_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var ellipse = (Ellipse) sender;
            if (e.LeftButton != MouseButtonState.Pressed)
                return;
            ellipse.CaptureMouse();
            ellipse.RenderTransform = new ScaleTransform(1.25, 1.25, ellipse.Width / 2, ellipse.Height / 2);
            //ellipse.Opacity = 0.75;
			ellipse.Stroke = touchedBrush;
			ellipse.Fill = touchedBrush;
        }

		void ellipse_MouseMove(object sender, MouseEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed)
				return;
			Ellipse ellipse = (Ellipse)sender;

			ellipse.Stroke = overBrush;
			ellipse.Fill = overBrush;
		}

		void ellipse_MouseLeave(object sender, MouseEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed)
				return;
			Ellipse ellipse = (Ellipse)sender;
			if (ellipse.Width == 2 * radiuses[0])
			{
				ellipse.Fill = untouchedBrushes[0];
				ellipse.Stroke = untouchedBrushes[0];
			}
			else
			{
				ellipse.Fill = untouchedBrushes[1];
				ellipse.Stroke = untouchedBrushes[1];
			}
		}

        void ellipse_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var ellipse = (Ellipse)sender;
            if (!ellipse.IsMouseCaptured)
                return;
            ellipse.ReleaseMouseCapture();
            ellipse.RenderTransform = null;
            ellipse.Opacity = 1;
			if (ellipse.Width == 2 * radiuses[0])
			{
				ellipse.Fill = untouchedBrushes[0];
				ellipse.Stroke = untouchedBrushes[0];
			}
			else
			{
				ellipse.Fill = untouchedBrushes[1];
				ellipse.Stroke = untouchedBrushes[1];
			}
        }

		private void changeOrientation()
		{
			m_curve.changeOrientation();
			for (int i = 0; i < 2; i++)
			{
				Ellipse tmp = m_points[i];
				m_points[i] = m_points[3 - i];
				m_points[3 - i] = tmp;
			}

			recountMiddleLines();
		}

        public DrawableCurve(Curve curve, DrawableCurve prev, Canvas canvas, SegmentController controller, bool connectToHead)
        {
            m_curve = curve;
            m_figure = null;

            m_canvas = canvas;
            Point[] p = curve.getPoints();
            m_segment = new BezierSegment(p[1], p[2], p[3], true);

			m_points = new Ellipse[4];
            for (int i = 0; i < 4; i++)
            {
                Ellipse el = CreateEllipse(i);
				m_points[i] = el;
                canvas.Children.Add(m_points[i]);
                Canvas.SetLeft(el, p[i].X - radiuses[i]);
                Canvas.SetTop(el, p[i].Y - radiuses[i]);
            }

			m_middleLines = new Line[2];
			for (int i = 0; i < 2; i++)
			{
				m_middleLines[i] = new Line();
                m_middleLines[i].Stroke = Brushes.Red;
                m_middleLines[i].StrokeThickness = 1;
                m_middleLines[i].StrokeDashArray = new DoubleCollection(new List<double>() { 10, 5 });
				m_canvas.Children.Add(m_middleLines[i]);
			}

			recountMiddleLines();

			Vector dv;
            if (!connectToHead)
                attach(prev, 0, 3, out dv);
            else
                attach(prev, 0, 0, out dv);

            m_controller = controller;
        }

		public void removeFromCanvas()
		{
			if (m_next != null)
				m_next.m_prev = null;
			if (m_prev != null)
				m_prev.m_next = null;
			erase();
		}

		private void erase()
		{
			for (int i = 0; i < 4; i++)
				m_canvas.Children.Remove(m_points[i]);
			for (int i = 0; i < 2; i++)
				m_canvas.Children.Remove(m_middleLines[i]);
		}

        public void translate(int idx, Vector dv, bool needSmooth = true)
        {
			System.Diagnostics.Debug.WriteLine(idx.ToString());
            m_curve.translate(idx, dv);
			if (idx == 3 && m_next != null)
            {
                m_next.translate(0, dv, false);
            }
			if (smooth && needSmooth)
			{
				if (null != m_prev && idx <= 1)
				{
					m_prev.m_curve.smoothifyWithNext(m_curve);
					m_prev.redraw();
				}
				if (null != m_next && idx >= 2)
				{
					m_next.m_curve.smoothifyWithPrev(m_curve);
					m_next.redraw();
				}
			}

			redraw();
        }

		private void redraw()
		{
			Point[] p = m_curve.getPoints();
			m_segment.Point1 = p[1];
			m_segment.Point2 = p[2];
			m_segment.Point3 = p[3];

			for (int i = 0; i < 4; i++)
			{
				Canvas.SetLeft(m_points[i], p[i].X - radiuses[i]);
				Canvas.SetTop(m_points[i], p[i].Y - radiuses[i]);
			}

			if (m_isStartCurve && m_figure != null)
			{
				m_figure.StartPoint = p[0];
			}

			recountMiddleLines();
		}

		public PathFigure getMyFigure()
		{
			return m_figure;
		}

		public LinkedList<DrawableCurve> getCurvesFromMyFigure()
		{
			LinkedList<DrawableCurve> ret = new LinkedList<DrawableCurve>();

			ret.AddLast(this);
			DrawableCurve cur = this.m_prev;
			while (cur != null && cur != this)
			{
				ret.AddLast(cur);
				cur = cur.m_prev;
			}

			if (cur == null)
			{
				cur = this.m_next;
				while (cur != null)
				{
					ret.AddLast(cur);
					cur = cur.m_next;
				}
			}

			return ret;
		}

        public bool attach(DrawableCurve closestCurve, int closestIdx, int closestOtherIdx, out Vector dv)
        {
			dv = new Vector(0, 0);
            if (closestCurve == null || (closestIdx != 3 && closestIdx != 0) || (closestOtherIdx != 0 && closestOtherIdx != 3))
                return false;

            Point[] closestCurvePoints = closestCurve.getPoints();
            dv = (Vector)closestCurvePoints[closestOtherIdx] - (Vector)m_curve.getPoints()[closestIdx];
			if (m_figure != null)
			{
				if (m_figure.Equals(closestCurve.m_figure))
				{
					m_pathIsClosed = true;
					m_figure.IsClosed = true;
					translate(closestIdx, dv);
				}	
			}
			if (!m_pathIsClosed)
				translate(closestIdx, dv);

            DrawableCurve cur = this;

            if (closestIdx == 0 && closestOtherIdx == 3)
			{
				if (!m_pathIsClosed)
					m_isStartCurve = false;
                while (cur != null)
                {
                    cur.m_figure = closestCurve.m_figure;
					if (m_pathIsClosed)
						m_figure.Segments.Remove(cur.m_segment);
                    closestCurve.m_figure.Segments.Add(cur.m_segment);
                    cur = cur.m_next;
                }
                
                m_prev = closestCurve;
                closestCurve.m_next = this;
            }
            else if (closestIdx == 0 && closestOtherIdx == 0)
            {
				m_isStartCurve = false;
                while (cur != null)
                {
                    cur.m_figure = closestCurve.m_figure;
					cur.changeOrientation();


                    Point[] p = cur.getPoints();
                    m_figure.StartPoint = p[0];

                    cur.m_segment = new BezierSegment(p[1], p[2], p[3], true);
                    m_figure.Segments.Insert(0, cur.m_segment);
                    DrawableCurve next = cur.m_next;
                    cur.m_next = cur.m_prev;
                    cur.m_prev = next;
					if (next == null)
						cur.m_isStartCurve = true;
                    cur = next;
                }

                m_next = closestCurve;
				closestCurve.m_isStartCurve = false;
                closestCurve.m_prev = this;
            }
            else if (closestIdx == 3 && closestOtherIdx == 0)
            {
                while (cur != null)
                {
                    cur.m_figure = closestCurve.m_figure;
                    Point[] p = cur.getPoints();
                    m_figure.StartPoint = p[0];
					if (m_pathIsClosed)
						m_figure.Segments.Remove(cur.m_segment);

                    m_figure.Segments.Insert(0, cur.m_segment);
                    DrawableCurve next = cur.m_next;
                    cur = cur.m_prev;
                }

                m_next = closestCurve;
				if (!m_pathIsClosed)
					closestCurve.m_isStartCurve = false;
                closestCurve.m_prev = this;
            }
            else // 3 to 3
            {
                while (cur != null)
                {
                    cur.m_figure = closestCurve.m_figure;
                    cur.changeOrientation();

                    Point[] p = cur.getPoints();

                    cur.m_segment = new BezierSegment(p[1], p[2], p[3], true);
                    m_figure.Segments.Add(cur.m_segment);
                    DrawableCurve prev = cur.m_prev;
                    cur.m_prev = cur.m_next;
                    cur.m_next = prev;
					if (prev == null)
						cur.m_isStartCurve = false;
                    cur = prev;
                }

                m_prev = closestCurve;
                closestCurve.m_next = this;
            }
			return true;
        }

		// pf is used if the previous state consisted of two not connected chains
		public bool detach(DrawableCurve closestCurve, int closestIdx, int closestOtherIdx, Vector dv)
		{
			if (closestCurve == null || (closestIdx != 3 && closestIdx != 0) || (closestOtherIdx != 0 && closestOtherIdx != 3))
				return false;

			Point[] closestCurvePoints = closestCurve.getPoints();
			bool pathWasClosed = m_pathIsClosed;
			DrawableCurve cur = this;

			if (closestIdx == 0 && closestOtherIdx == 3)
			{
				m_isStartCurve = true;

				if (!m_pathIsClosed)
				{
					m_figure = new PathFigure();
					while (cur != null)
					{
						cur.m_figure = m_figure;
						closestCurve.m_figure.Segments.Remove(cur.m_segment);
						m_figure.Segments.Add(cur.m_segment);
						cur = cur.m_next;
					}
					m_figure.StartPoint = getPoints()[0];
				}
				m_pathIsClosed = false;

				closestCurve.m_next = null;
				m_prev = null;

			}
			else if (closestIdx == 0 && closestOtherIdx == 0)
			{
				m_figure = new PathFigure();
				while (cur != null)
				{
					cur.m_figure = m_figure;
					cur.changeOrientation(); 

					Point[] p = cur.getPoints();
					closestCurve.m_figure.Segments.Remove(cur.m_segment);
					cur.m_segment = new BezierSegment(p[1], p[2], p[3], true);
					m_figure.Segments.Add(cur.m_segment);

					DrawableCurve prev = cur.m_prev;
					cur.m_prev = cur.m_next;
					cur.m_next = prev;
					if (prev == null)
						cur.m_isStartCurve = false;
					cur = prev;
				}

				m_prev = null;
				closestCurve.m_isStartCurve = true;
				closestCurve.m_prev = null;
				m_isStartCurve = true;
				m_figure.StartPoint = getPoints()[0];
				closestCurve.m_figure.StartPoint = closestCurve.getPoints()[0];
			}
			else if (closestIdx == 3 && closestOtherIdx == 0)
			{
				if (!m_pathIsClosed)
				{
					m_figure = new PathFigure();
					while (cur != null)
					{
						cur.m_figure = m_figure;
						closestCurve.m_figure.Segments.Remove(cur.m_segment);
						m_figure.Segments.Insert(0, cur.m_segment);
						cur = cur.m_prev;
					}
				}

				closestCurve.m_prev = null;
				m_next = null;
				m_pathIsClosed = false;
				closestCurve.m_figure.StartPoint = closestCurve.getPoints()[0];
				closestCurve.m_isStartCurve = true;
				
			}
			else
			{
				m_figure = new PathFigure();
				while (cur != null)
				{
					cur.m_figure = m_figure;
					cur.changeOrientation();

					Point[] p = cur.getPoints();
					closestCurve.m_figure.Segments.Remove(cur.m_segment);
					cur.m_segment = new BezierSegment(p[1], p[2], p[3], true);

					m_figure.Segments.Insert(0, cur.m_segment);
					DrawableCurve next = cur.m_next;
					cur.m_next = cur.m_prev;
					cur.m_prev = next;
					if (next == null)
					{
						cur.m_isStartCurve = true;
						m_figure.StartPoint = p[0];
					}
					cur = next;
				}

				m_next = null;
				closestCurve.m_next = null;
				
			}
			if (!pathWasClosed)
				translate(closestIdx, -dv);

			m_figure.IsClosed = false;
			return true;
		}

        public bool hasNext()
        {
            return m_next != null;
        }

        public bool hasPrev()
        {
            return m_prev != null;
        }

        public DrawableCurve getPrev()
        {
            return m_prev;
        }

        public Point[] getPoints()
        {
            return m_curve.getPoints();
        }

		public void setAsStart()
		{
			m_isStartCurve = true;
			if (m_figure != null)
				m_figure.StartPoint = getPoints()[0];
		}

		public void findClosestPointTo(Point p, out float min_t, out float minDist)
		{
			min_t = 0;
			minDist = 1e25F;

			Vector[] v = new Vector[4];
			Vector u = (Vector)p;
			for (int i = 0; i < 4; i++ )
				v[i] = (Vector)getPoints()[i];

			for (float t = 0; t < 1; t += 0.001f)
			{
				Vector q = Math.Pow((1 - t), 3) * v[0] + 3 * (1 - t) * (1 - t) * t * v[1] + 3 * t * t * (1 - t) * v[2] + t * t * t * v[3];
				double dist = (q - u).Length;
				if (dist < minDist)
				{
					min_t = (float)t;
					minDist = (float)dist;
				}
			}
		}

		public void split(float t, out DrawableCurve dc1, out DrawableCurve dc2)
		{
			Vector[] v = new Vector[4];
			Point[] p = getPoints();

			for (int i = 0; i < 4; i++)
			{
				v[i] = (Vector)p[i];
			}

			Vector grad = 3*(v[1] - v[0]) * (1 - t) * (1 - t) + 6*t * (1 - t) * (v[2] - v[1]) + 3*t * t * (v[3] - v[2]);
			Vector q = Math.Pow((1 - t), 3) * v[0] + 3 * (1 - t) * (1 - t) * t * v[1] + 3 * t * t * (1 - t) * v[2] + t * t * t * v[3];

			grad.Normalize();
			Vector p2 = q - grad * defaultMiddleLineLength;
			Vector p1 = q + grad * defaultMiddleLineLength;

			DrawableCurve cur = this;
			while (!cur.m_isStartCurve)
				cur = cur.m_prev;
			
			Curve c1 = new Curve(p[0], p[1], (Point)p2, (Point)q);
			Curve c2 = new Curve((Point)q, (Point)p1, p[2], p[3]);
			dc1 = new DrawableCurve(c1, m_prev, m_canvas, m_controller, false);
			dc1.m_figure = m_figure;
			
			dc2 = new DrawableCurve(c2, dc1, m_canvas, m_controller, false);
			dc2.m_next = m_next;
			if (m_next != null)
				m_next.m_prev = dc2;

			if (cur == this)
				cur = dc1;
			if (m_isStartCurve)
			{
				dc1.setAsStart();
			}
			DrawableCurve start = cur;

			m_figure.Segments.Clear();
			do
			{
				m_figure.Segments.Add(cur.m_segment);
				cur = cur.m_next;
			} while (cur != start && cur != null);
			erase();
		}

		public void Replace(DrawableCurve dc1, DrawableCurve dc2)
		{
			if (dc1.m_prev != null)
				dc1.m_prev.m_next = this;
			m_prev = dc1.m_prev;

			if (dc2.m_next != null)
				dc1.m_next.m_prev = this;
			m_next = dc2.m_next;

			DrawableCurve cur = dc1;
			while (!cur.m_isStartCurve)
				cur = cur.m_prev;

			if (cur == dc1)
				cur = this;
			if (dc1.m_isStartCurve)
				setAsStart();

			DrawableCurve start = cur;

			m_figure.Segments.Clear();
			do
			{
				m_figure.Segments.Add(cur.m_segment);
				cur = cur.m_next;
			} while (cur != start && cur != null);
			for (int i = 0; i < 4; i++)
			{
				m_canvas.Children.Add(m_points[i]);
			}

			for (int i = 0; i < 2; i++)
			{
				m_canvas.Children.Add(m_middleLines[i]);
			}

			dc1.erase();
			dc2.erase();
		}

    }
}
