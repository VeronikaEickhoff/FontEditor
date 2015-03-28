using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Collections;
using System.Windows.Media.Animation;
using FontEditor.Model;

namespace FontEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 



    public partial class MainWindow : Window
    {
        enum Tabs
        {
            FONT_EDIT,
            TEXT_EDIT
        };
        
        private Tabs m_currentTab = Tabs.FONT_EDIT;

        private Curves _Curves;

        private Stack<Point> _CurrentPoints = new Stack<Point>();  // used to add and to remove points while drawing

        public MainWindow()
        {
            InitializeComponent();

            //CreateBezierSegment();

        }

        private void CreateBezierSegment()
        {
            var bezierSegment = new BezierSegment(new Point(100, 200), new Point(400, 200), new Point(400, 300), true);
            var pathSegmentCollection = new PathSegmentCollection {bezierSegment};

            var pathFigure = new PathFigure {StartPoint = new Point(100, 300), Segments = pathSegmentCollection};
            var pathFigureCollection = new PathFigureCollection {pathFigure};

            var pathGeometry = new PathGeometry {Figures = pathFigureCollection};

            var path = new Path {Stroke = new SolidColorBrush(Colors.Black), Data = pathGeometry};

            fontCanvas.Children.Add(path);
        }


        private void fontCanvas_MouseLeftButtonDown(object sender, System.Windows.Input.MouseEventArgs e)
        {
            // understand whether user clicked on already existing point or he created new point
            
            _CurrentPoints.Push(e.GetPosition(fontCanvas));
            DrawPoint();

            if (_CurrentPoints.Count == 4)
            {
                _Curves.AddCurve(_CurrentPoints.ToList());
                DrawCurve(_CurrentPoints.ToList());
            }
        }

        private void DrawCurve(List<Point> points)
        {
             // draw Bezier segment
        }

        private void DrawPoint()
        {
                // draw point on canvas
            var point = new Ellipse();



        }


        private void fontCanvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
           // System.Windows.MessageBox.Show("You are moving mouse", "My Application");

            // if point was clicked or just has been created, we pull this point
        }

        private void tabControl1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Contains(fontTab))
                m_currentTab = Tabs.FONT_EDIT;
            else
                m_currentTab = Tabs.TEXT_EDIT;

        }

        private void tabControl1_Selected(Object sender, TabControlEventArgs e)
        {
            System.Windows.MessageBox.Show("You are moving mouse", "My Application");
        }


        
    }

    
}
