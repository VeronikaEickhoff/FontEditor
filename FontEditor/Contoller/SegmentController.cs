using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using System.Windows.Input;
using FontEditor.View;
using FontEditor.Model;

namespace FontEditor.Contoller
{
    class SegmentController
    {
        internal enum ControllerState
        { 
            ADD,
            MOVE
        }
        private Canvas m_canvas;
        private List<PathGeometry> m_pathGeometries;
        private List<Path> m_paths;
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

        public void setTouchedCurve(DrawableCurve c, int touchedPoint)
        {
            m_touchedCurve = c;
            m_touchedPointIdx = touchedPoint;
        }

        public SegmentController(Canvas canvas)
        {
            m_canvas = canvas;
            m_pathGeometries = new List<PathGeometry>();
            m_paths = new List<Path>();
            
            m_curves = new LinkedList<DrawableCurve>();
            m_pathsToCurves = new Dictionary<Path, LinkedList<DrawableCurve>>();
            m_curvesToPaths = new Dictionary<DrawableCurve, Path>();
        }

        public void onMouseDown(Point p) 
        {
            Vector v = (Vector)p;
            m_isMousePressed = true;

            switch (m_state)
            {
            case ControllerState.ADD:
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
                    m_pathsToCurves[m_curvesToPaths[closestCurve]].AddLast(m_touchedCurve);
                    m_curvesToPaths[m_touchedCurve] = m_curvesToPaths[closestCurve];
                }
                else
                {
                    PathGeometry pg = new PathGeometry();
                    m_pathGeometries.Add(pg);

                    System.Windows.Shapes.Path path = new Path();
                    path.Stroke = Brushes.Black;
                    path.Data = m_pathGeometries.Last();
                    path.MouseDown += onCurveMouseDown;

                    m_paths.Add(path);
                    m_pathsToCurves.Add(path, new LinkedList<DrawableCurve>());
                    m_canvas.Children.Add(path); // Here
                    m_touchedCurve = new DrawableCurve(new Curve(v, v), pg, m_canvas, this);
                    m_pathsToCurves[path].AddLast(m_touchedCurve);
                    m_curvesToPaths[m_touchedCurve] = path;
                }
                m_curves.AddLast(m_touchedCurve);
                m_touchedPointIdx = 3;
                m_prevMousePos = v;
                break;
            case ControllerState.MOVE: // this is processed by onCurveMouseDown()

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
            }

            if (m_isMousePressed && m_touchedCurve != null)
            {
                m_touchedCurve.translate(m_touchedPointIdx, v - m_prevMousePos);
            }
            else
            { 
                // highlight points over which the mouse is
            }
            m_prevMousePos = v;
            m_canvas.UpdateLayout();
        }

        public void onMouseUp(Point p)
        {
            m_isMousePressed = false;
            m_touchedCurve = null;
            m_touchedPointIdx = -2;
            
            if (null != m_touchedPath)
            {
                m_touchedPath.Stroke = new SolidColorBrush(Colors.Black);
                m_touchedPath = null;
            }
        }

        private void onCurveMouseDown(object sender, MouseButtonEventArgs e)
        {
            var path = (Path)sender;
            path.Stroke = new SolidColorBrush(Colors.DeepPink);
            m_prevMousePos = (Vector)e.GetPosition(m_canvas);
            m_touchedPath = path;
        }

        public void addSegment()
        { 
        
        }
    }
}
