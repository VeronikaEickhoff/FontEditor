using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Xml;
using FontEditor.View;
using FontEditor.Model;
using Path = System.Windows.Shapes.Path;

namespace FontEditor.Controller
{
    partial class SegmentController
    {
        public enum ControllerState
        {
            ADD,
            MOVE
        }
        private Canvas m_canvas;
		private Dictionary<Path, PathGeometry> m_pathGeometries;
        private LinkedList<DrawableCurve> m_curves;
        private Dictionary<Path, LinkedList<DrawableCurve>> m_pathsToCurves;
        private Dictionary<DrawableCurve, Path> m_curvesToPaths;
        private double m_touchRadius = 20;
        public ControllerState m_state = ControllerState.ADD;
        private DrawableCurve m_touchedCurve = null;
        private Path m_touchedPath = null;
        private int m_touchedPointIdx = -2;
        private Vector m_prevMousePos;
        private bool m_isMousePressed = false;

        private Path m_lastPath;
		private Stack<SimpleAction> m_actions;
		private Stack<int> m_firstPrevoiusAction;

		private Vector m_startMousePos; 

        public void setTouchedCurve(DrawableCurve c, int touchedPoint)
        {
            m_touchedCurve = c;
            m_touchedPointIdx = touchedPoint;
        }

        public SegmentController(Canvas canvas)
        {
            m_canvas = canvas;
			m_pathGeometries = new Dictionary<Path, PathGeometry>();

            m_curves = new LinkedList<DrawableCurve>();
            m_pathsToCurves = new Dictionary<Path, LinkedList<DrawableCurve>>();
            m_curvesToPaths = new Dictionary<DrawableCurve, Path>();
			m_actions = new Stack<SimpleAction>();
			m_firstPrevoiusAction = new Stack<int>();
        }

        public void onMouseDown(Point p)
        {
            Vector v = (Vector)p;
            m_isMousePressed = true;
			m_startMousePos = v;
            switch (m_state)
            {
                case ControllerState.ADD:
                    {
						m_firstPrevoiusAction.Push(m_actions.Count);

                        DrawableCurve closestCurve = null;
                        double closestDist = 1e305;
                        bool headIsCLoserThanTail = false;
                        foreach (DrawableCurve c in m_curves)
                        {
                            Vector tail = (Vector)c.getPoints()[3];
                            Vector head = (Vector)c.getPoints()[0];
                            double distToTail = (tail - v).Length;
                            double distToHead = (head - v).Length;
                            if (distToTail < closestDist && !c.hasNext())
                            {
                                closestDist = distToTail;
                                closestCurve = c;
                                headIsCLoserThanTail = false;
                            }
                            if (distToHead < closestDist && !c.hasPrev())
                            {
                                closestDist = distToHead;
                                closestCurve = c;
                                headIsCLoserThanTail = true;
                            }
                        }
                        if (closestDist < m_touchRadius && closestCurve != null)
                        {
                            m_touchedCurve = new DrawableCurve(new Curve(v, v), closestCurve, m_canvas, this, headIsCLoserThanTail);
							m_actions.Push(new AddCurveAction(m_touchedCurve, this));
                            m_pathsToCurves[m_curvesToPaths[closestCurve]].AddLast(m_touchedCurve);
                            m_curvesToPaths[m_touchedCurve] = m_curvesToPaths[closestCurve];
                            if (headIsCLoserThanTail)
                                m_touchedPointIdx = 0;
                            else
                                m_touchedPointIdx = 3;
                        }
                        else
                        {
                            PathGeometry pg = new PathGeometry();
                            pg.FillRule = FillRule.EvenOdd;
                            

                            System.Windows.Shapes.Path path = new Path();
                            path.Stroke = Brushes.Black;
							path.Data = pg;
                            path.StrokeThickness = 2;
                            path.MouseDown += onCurveMouseDown;
                            path.MouseEnter += onMouseOverPath;
                            path.MouseLeave += onMouseLeftPath;

							m_pathGeometries.Add(path, pg);
                            m_pathsToCurves.Add(path, new LinkedList<DrawableCurve>());
                            m_canvas.Children.Add(path); // Here
							PathFigure pf = new PathFigure();
							pg.Figures.Add(pf);
                            m_touchedCurve = new DrawableCurve(new Curve(v, v), pf, m_canvas, this); // Here curve adds itself to canvas -> Bootstrapping

                            m_pathsToCurves[path].AddLast(m_touchedCurve);
                            m_curvesToPaths[m_touchedCurve] = path;

                            m_touchedPointIdx = 3;
							m_actions.Push(new AddCurveAction(m_touchedCurve, this));

                        }
                        m_curves.AddLast(m_touchedCurve);

                        m_prevMousePos = v;
                    }
                    break;
                case ControllerState.MOVE: // this is processed by onCurveMouseDown()
                    {
                        DrawableCurve closestCurve = null;
                        double closestDist = 1e305;
                        int closestIdx = 0;
                        foreach (DrawableCurve c in m_curves)
                        {
                            Point[] points = c.getPoints();
                            for (int i = 0; i < 4; i++)
                            {
                                double dist = ((Vector)points[i] - v).Length;
                                if (closestDist > dist)
                                {
                                    closestDist = dist;
                                    closestIdx = i;
                                    closestCurve = c;
                                }
                            }
                        }
                        if (closestDist < m_touchRadius && closestCurve != null)
                        {
							m_firstPrevoiusAction.Push(m_actions.Count);
                            m_touchedCurve = closestCurve;
                            m_touchedPointIdx = closestIdx;
                            if (m_touchedPointIdx == 0 && m_touchedCurve.hasPrev())
                            {
                                m_touchedCurve = m_touchedCurve.getPrev();
                                m_touchedPointIdx = 3;
                            }
                        }
                    }
                    break;
            }
        }


        public void onMouseMove(Point p)
        {
            Vector v = (Vector)p;

            if (m_touchedPath != null)
            {
                foreach (DrawableCurve c in m_pathsToCurves[m_touchedPath])
                {
                    c.translate(-1, v - m_prevMousePos);
                }
                DrawableCurve firstCurve = m_pathsToCurves[m_touchedPath].First.Value;
                DrawableCurve lastCurve = m_pathsToCurves[m_touchedPath].Last.Value;

                int closestIdx = 0;
                double closestDistToHead = 0;
                double closestDistToTail = 0;
                DrawableCurve closestCurveToTail = null;
                DrawableCurve closestCurveToHead = null;
                int closestOtherIdxToHead = 0;
                int closestOtherIdxToTail = 0;
                Path newPath = null;

                if (!firstCurve.hasPrev())
                    findClosestCurve(firstCurve, 0, out closestDistToHead, out closestCurveToHead, out closestOtherIdxToHead);
                if (!lastCurve.hasNext())
                    findClosestCurve(lastCurve, 3, out closestDistToTail, out closestCurveToTail, out closestOtherIdxToTail);

				if (Math.Min(closestDistToHead, closestDistToTail) > m_touchRadius)
					goto EXIT;

				if (closestDistToHead < closestDistToTail && closestCurveToHead != null)
                {
					attachCurves(lastCurve, closestCurveToHead, 0, closestOtherIdxToHead);
					m_touchedPath = null;
                }
				else if (closestDistToTail < closestDistToHead && closestCurveToTail != null)
                {
					attachCurves(lastCurve, closestCurveToTail, 3, closestOtherIdxToTail);
					m_touchedPath = null;
                }

				if (m_touchedPath == null)
					m_actions.Push(new MoveAction(-1, lastCurve, m_startMousePos, v, this));

				
            }
            else
            {
                if (m_isMousePressed && m_touchedCurve != null)
                {
                    m_touchedCurve.translate(m_touchedPointIdx, v - m_prevMousePos);

                    if (m_touchedPointIdx != 0 && m_touchedPointIdx != 3)
                        goto EXIT;

                    double closestDist = 0;
                    DrawableCurve closestCurve = null;
                    int closestOtherIdx = 0;
                    Path newPath = null;

                    if (m_touchedCurve.hasPrev() && m_touchedPointIdx == 0)
                    {
                        goto EXIT;
                    }
                    else if (m_touchedCurve.hasNext() && m_touchedPointIdx == 3)
                    {
                        goto EXIT;
                    }

                    findClosestCurve(m_touchedCurve, m_touchedPointIdx, out closestDist, out closestCurve, out closestOtherIdx);
                    if (closestDist < m_touchRadius && closestCurve != null)
                    {
                        attachCurves(m_touchedCurve, closestCurve, m_touchedPointIdx, closestOtherIdx);
                        if (m_touchedPointIdx == 0)
                        {
                            m_touchedPointIdx = 3;
                            if (closestOtherIdx == 3)
                                m_touchedCurve = closestCurve;
                        }
                    }

                }
                else
                {
                    // highlight points over which the mouse is
                }
            }
        EXIT:
            m_prevMousePos = v;
            m_canvas.UpdateLayout();
        }

        private void attachCurves(DrawableCurve child, DrawableCurve parent, int childIdx, int parentIdx)
        {
			Vector dv;
            child.attach(parent, childIdx, parentIdx, out dv);
			m_actions.Push(new AttachCurveAction(child, parent, parentIdx, childIdx, dv, this));
            Path newPath = m_curvesToPaths[parent];

            Path currentPath = m_curvesToPaths[child];

            if (newPath != null && newPath != currentPath)
            {
                foreach (DrawableCurve dc in m_pathsToCurves[currentPath])
                {
                    m_pathsToCurves[newPath].AddLast(dc);
                    m_curvesToPaths[dc] = newPath;
                }
                m_pathsToCurves.Remove(currentPath);
                m_canvas.Children.Remove(currentPath); // Here
				m_pathGeometries.Remove(currentPath);
            }
        }

        private void findClosestCurve(DrawableCurve testedCurve, int testedCurveIdx, out double closestDist, out DrawableCurve closestCurve, out int closestOtherIdx)
        {
            closestCurve = null;
            closestDist = 1e305;
            closestOtherIdx = 0;
            int bestIndex = 0;

            Vector v = (Vector)testedCurve.getPoints()[testedCurveIdx];
            foreach (DrawableCurve c in m_curves)
            {
                if (c.Equals(testedCurve))
                    continue;
                Vector tail = (Vector)c.getPoints()[3];
                Vector head = (Vector)c.getPoints()[0];
                double distToTail = (tail - v).Length;
                double distToHead = (head - v).Length;
                if (distToTail < closestDist && !c.hasNext())
                {
                    closestDist = distToTail;
                    closestCurve = c;
                    closestOtherIdx = 3;
                }
                if (distToHead < closestDist && !c.hasPrev())
                {
                    closestDist = distToHead;
                    closestCurve = c;
                    closestOtherIdx = 0;
                }
            }
        }

        public void onMouseUp(Point p)
        {
			if (null != m_touchedPath)
			{
				m_touchedPath.Stroke = new SolidColorBrush(Colors.Black);
				m_actions.Push(new MoveAction(-1, m_pathsToCurves[m_touchedPath].First.Value, m_startMousePos, (Vector)p, this));
				m_touchedPath = null;
			}
			else
			{
				m_actions.Push(new MoveAction(m_touchedPointIdx, m_touchedCurve, m_startMousePos, (Vector)p, this));
			}
			m_isMousePressed = false;
			m_touchedCurve = null;
			m_touchedPointIdx = -2;
        }

        private void onCurveMouseDown(object sender, MouseButtonEventArgs e)
        {
            var path = (Path)sender;

            path.Stroke = new SolidColorBrush(Colors.DeepPink);
            m_prevMousePos = (Vector)e.GetPosition(m_canvas);
            m_touchedPath = path;
        }

        private void onMouseOverPath(object sender, MouseEventArgs e)
        {
            if (!m_isMousePressed)
            {
                Path path = (Path)sender;
                path.Stroke = new SolidColorBrush(Colors.DarkOliveGreen);
            }
        }

        private void onMouseLeftPath(object sender, MouseEventArgs e)
        {
            if (!m_isMousePressed)
            {
                Path path = (Path)sender;
                path.Stroke = new SolidColorBrush(Colors.Black);
            }
        }

        public void addSegment()
        {

        }

        // Letter drawing is done, moving segments is still possible
        public void showPreview(Grid previewCanvas)
        {
            // Combine all created PathGeometries into one path and show it in the preview canvas
            var geometryGroup = new GeometryGroup { Children = new GeometryCollection(m_pathGeometries.Values) };
            var combinedPath = new Path
            {
                Data = geometryGroup,
                Stroke = new SolidColorBrush(Colors.Black),
                StrokeThickness = 2,
                Stretch = Stretch.Fill,
                Fill = new SolidColorBrush(Colors.Black)
            };
            previewCanvas.Children.Add(combinedPath);
        }

		public void undo()
		{
			int prevActionidx = m_firstPrevoiusAction.Pop();
			int undoActionsNumber = m_actions.Count - prevActionidx;
			for (int i = 0; i < undoActionsNumber; i++)
			{
				if (m_actions.Count != 0)
				{
					SimpleAction act = m_actions.Pop();
					act.undo();
				}
			}
		}

		public void redo()
		{ 
		
		}

        // Letter editing is completely done - before saving
        public Path CreateLetterPath()
        {
            var geometryGroup = new GeometryGroup { Children = new GeometryCollection(m_pathGeometries.Values) };
            var combinedPath = new Path
            {
                Data = geometryGroup,
                Stroke = new SolidColorBrush(Colors.Black),
                StrokeThickness = 2,
                Stretch = Stretch.Fill,
                Fill = new SolidColorBrush(Colors.Black)
            };
            return combinedPath;
        }

		public bool hasEmptyActions()
		{
			return (m_actions.Count == 0);
		}
    }
}
