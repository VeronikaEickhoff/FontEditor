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

namespace FontEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        FontEditor.Model.Curve[] currentCurves;
        Stack<Point> currentPoints; // all points on current frame; each sequential quad of them gives us a curve
                 // we nned to store points in order to be able to remove recently drawn

        public MainWindow()
        {
            InitializeComponent();
        }

        private void letterCanvasMD(object sender, System.Windows.Input.MouseEventArgs e)
        {
            System.Windows.MessageBox.Show("You have clicked canvas", "My Application");

            // understand whether user clicked on already existing point or he created new point
        }

        private void letterCanvasMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            System.Windows.MessageBox.Show("You are moving mouse", "My Application");

            // if point was clicked or just has been created, we pull this point
        }

        
    }

    
}
