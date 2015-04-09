using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontEditor.View;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;

namespace FontEditor.Controller
{
	partial class SegmentController
	{
		abstract class SimpleAction
		{
			protected SegmentController m_controller;

			public SimpleAction(SegmentController controller)
			{
				m_controller = controller;
			}
			public abstract void undo();
			public abstract void redo();
		}

		class AddCurveAction : SimpleAction
		{
			private DrawableCurve m_addedCurve;

			public AddCurveAction(DrawableCurve c, SegmentController controller) : base(controller)
			{
				m_addedCurve = c;
			}

			public override void undo()
			{
				if (!m_controller.m_curvesToPaths.ContainsKey(m_addedCurve))
					return;

				Path p = m_controller.m_curvesToPaths[m_addedCurve];

				m_controller.m_curvesToPaths.Remove(m_addedCurve);
				m_controller.m_pathsToCurves[p].Remove(m_addedCurve);
				if (m_controller.m_pathsToCurves[p].Count == 0)
					m_controller.m_pathsToCurves.Remove(p);
				m_addedCurve.removeFromCanvas();
				m_controller.m_pathGeometries.Remove(p);
				m_controller.m_canvas.Children.Remove(p);
				m_controller.m_curves.Remove(m_addedCurve);
			}

			public override void redo()
			{
				//m_controller.redo(2);
			}
		}

		class AttachCurveAction : SimpleAction
		{
			private DrawableCurve m_attachedWho;
			private DrawableCurve m_attachedTo;
			private int m_toIdx;
			private int m_whoIdx;
			private Vector dv;

			public AttachCurveAction(DrawableCurve who, DrawableCurve to, int toIdx, int whoIdx, Vector translation, SegmentController controller)
				: base(controller)
			{
				m_attachedWho = who;
				m_attachedTo = to;
				m_toIdx = toIdx;
				m_whoIdx = whoIdx;
				dv = translation;
			}

			public override void undo()
			{
				if (!m_controller.m_curvesToPaths.ContainsKey(m_attachedWho) || !m_controller.m_curvesToPaths.ContainsKey(m_attachedTo))
					return;

				m_attachedWho.detach(m_attachedTo, m_whoIdx, m_toIdx, dv);

				PathFigure pf = m_attachedWho.getMyFigure();
				if (!pf.Equals(m_attachedTo.getMyFigure()))
				{
					PathGeometry pg = new PathGeometry();
					pg.FillRule = FillRule.EvenOdd;


					System.Windows.Shapes.Path path = new Path();
					path.Stroke = Brushes.Black;
					path.Data = pg;
					path.StrokeThickness = 2;
					path.MouseDown += m_controller.onCurveMouseDown;
					path.MouseEnter += m_controller.onMouseOverPath;
					path.MouseLeave += m_controller.onMouseLeftPath;

					m_controller.m_pathGeometries.Add(path, pg);
					m_controller.m_pathsToCurves.Add(path, new LinkedList<DrawableCurve>());
					m_controller.m_canvas.Children.Add(path); // Here
					pg.Figures.Add(pf);

					LinkedList<DrawableCurve> curves = m_attachedWho.getCurvesFromMyFigure();

					Path prevPath = m_controller.m_curvesToPaths[m_attachedTo];
					foreach (DrawableCurve dc in curves)
					{
						m_controller.m_pathsToCurves[prevPath].Remove(dc);
						m_controller.m_pathsToCurves[path].AddLast(dc);
						m_controller.m_curvesToPaths[dc] = path;
					}
				}
			}

			public override void redo()
			{
				throw new NotImplementedException();
			}
		}

		class MoveAction : SimpleAction
		{
			private int m_idx;
			private DrawableCurve m_curve;
			private Vector m_startPoint;
			private Vector m_endPoint;

			public MoveAction(int moveIdx, DrawableCurve dc, Vector startPoint, Vector endPoint, SegmentController c)
				: base(c)
			{
				m_idx = moveIdx;
				m_curve = dc;
				m_startPoint = startPoint;
				m_endPoint = endPoint;
			}

			public override void undo()
			{
				if (m_idx == -1)
				{
					LinkedList<DrawableCurve> sameFigureCurves = m_curve.getCurvesFromMyFigure();
					foreach (DrawableCurve c in sameFigureCurves)
					{
						c.translate(-1, m_startPoint - m_endPoint);
					}
				}
				else
					m_curve.translate(m_idx, m_startPoint - m_endPoint);
			}

			public override void redo()
			{
				throw new NotImplementedException();
			}
		}
	}
}
