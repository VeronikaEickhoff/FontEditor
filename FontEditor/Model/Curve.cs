using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            m_points[1] = (Point)((1 - t1) * begin + t1 * end + new Vector(10,1));
            m_points[2] = (Point)((1 - t2) * begin + t2 * end + new Vector(-10, 1));
        }

		private Curve(Curve rhs)
		{
			m_points = new Point[4];
			for (int i = 0; i < 4; i++)
			{
				m_points[i] = new Point(rhs.m_points[i].X, rhs.m_points[i].Y);
			}
		}

		public Curve(Point[] p, float a, float b)
		{
			m_points = new Point[4];
			for (int i = 0; i < 4; i++)
			{
				m_points[i] = new Point(p[i].X, p[i].Y);
			}
			t1 = a;
			t2 = b;
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

        private void swapPoints(int ind1, int ind2)
        {
            Point temp = m_points[ind1];
            m_points[ind1] = m_points[ind2];
            m_points[ind2] = temp;
        }

        public void changeOrientation()
        {
            swapPoints(0, 3);
            swapPoints(1, 2);
        }

		public Curve getCopy() 
		{
			return (new Curve(this));
		}
		public override string ToString()
		{
			string s = "";
			for (int i = 0; i < 4; i++)
				s += m_points[i].ToString() + "*";
			s += t1.ToString() + "*" + t2.ToString();

			return s;
		}

		public void smoothifyWithNext(Curve next)
		{
			Vector v0 = (Vector)next.m_points[0];
			Vector v1 = (Vector)next.m_points[1];

			Vector u2 = (Vector)m_points[2];
			Vector u3 = (Vector)m_points[3];

			Vector dir = v0 - v1;
			Vector prevDir = u2 - u3;
			m_points[2] = (Point)(u3 + dir * prevDir.Length / dir.Length);
		}

		public void smoothifyWithPrev(Curve prev)
		{
			Vector v0 = (Vector)m_points[0];
			Vector v1 = (Vector)m_points[1];

			Vector u2 = (Vector)prev.m_points[2];
			Vector u3 = (Vector)prev.m_points[3];

			Vector dir = u3 - u2;
			Vector prevDir = v1 - v0;
			m_points[1] = (Point)(v0 + dir * prevDir.Length / dir.Length);
		}
    }
}
