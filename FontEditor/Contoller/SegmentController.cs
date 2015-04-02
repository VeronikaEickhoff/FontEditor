using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;

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
        private PathGeometry m_path;
        private LinkedList<DrawableCurve> m_curves;
        private double m_touchRadius = 20;
        public ControllerState m_state = ControllerState.ADD;
        private DrawableCurve m_touchedCurve = null;
        private int m_touchedPointIdx = -1;
        private Vector m_prevMousePos;
        private bool m_isMousePressed = false; 

        public SegmentController(Canvas canvas)
        {
            m_canvas = canvas;
            m_path = new PathGeometry();

            System.Windows.Shapes.Path p = new Path();
            p.Stroke = Brushes.Black;
            p.Data = m_path;

            canvas.Children.Add(p); // Here
            m_curves = new LinkedList<DrawableCurve>();
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

                foreach (DrawableCurve c in m_curves)
                {
                    if (!c.hasNext())
                    { 
                        Vector tail = (Vector)c.getPoints()[3];
                        double len = (tail - v).Length;
                        if (len < closestDist)
                        {
                            closestDist = len;
                            closestCurve = c;
                        }
                    }
                }
                if (closestDist < m_touchRadius && closestCurve != null)
                {
                    m_touchedCurve = new DrawableCurve(new Curve(v, v), closestCurve, m_canvas);  
                }
                else
                {
                    m_touchedCurve = new DrawableCurve(new Curve(v, v), m_path, m_canvas);
                }
                m_curves.AddLast(m_touchedCurve);
                m_touchedPointIdx = 3;
                m_prevMousePos = v;
                break;
            case ControllerState.MOVE:
                break;
            }
        }

        public void onMouseMove(Point p)
        {
            Vector v = (Vector)p;

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
            m_touchedPointIdx = -1;
        }

        public void addSegment()
        { 
        
        }
    }
}
