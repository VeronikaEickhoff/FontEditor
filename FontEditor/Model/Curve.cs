using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows;

namespace FontEditor.Model
{
    class Curve
    {
        private Point[] m_points; // Used to create the curve
        private double t1 = 1.0 / 3;
        private double t2 = 2.0 / 3;

        public Curve(Vector begin, Vector end)
        {
            m_points = new Point[4];
            m_points[0] = (Point)begin;
            m_points[3] = (Point)end;
            m_points[1] = (Point)((1 - t1) * begin + t1 * end + new Vector(50,1));
            m_points[2] = (Point)((1 - t2) * begin + t2 * end + new Vector(-50, 1));
        }
        
        public void translate(int idx, Vector dv)
        {
            switch (idx)
            {
                case 0:
                    m_points[0] += dv;
                    m_points[1] += (1 - t1) * dv;
                    m_points[2] += (1 - t2) * dv;
                    break;
                case 3:
                    m_points[3] += dv;
                    m_points[2] += t2 * dv;
                    m_points[1] += t1 * dv;
                    break;
                case 2:
                case 1:
                    m_points[idx] += dv;
                    break;
                case -1: // just translate all the points
                    for (int i = 0; i < 4; i++)
                        m_points[i] += dv;
                    break;
                default:
                    return;
            }

        }

        void setPointPosition(int idx, Vector pos)
        {
            if (idx >= 0 && idx <= 3)
            {
                translate(idx, pos - (Vector)m_points[idx]);
            }
        }

        public Point[] getPoints() 
        {
            return m_points;
        }
    }
}
