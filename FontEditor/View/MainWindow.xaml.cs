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
    /// 



    public partial class MainWindow : Window
    {
        enum Tabs
        {
            FONT_EDIT,
            TEXT_EDIT
        };
        private FontEditor.Model.Curve[] currentCurves;
        private Stack<Point> currentPoints; // all points on current frame; each sequential quad of them gives us a curve
                 // we nned to store points in order to be able to remove recently drawn
        private Tabs m_currentTab = Tabs.FONT_EDIT;


        public MainWindow()
        {
            InitializeComponent();
        }

        private void fontCanvas_MouseLeftButtonDown(object sender, System.Windows.Input.MouseEventArgs e)
        {
            System.Windows.MessageBox.Show("You have clicked canvas", "My Application");

            // understand whether user clicked on already existing point or he created new point
        }

        private void fontCanvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            System.Windows.MessageBox.Show("You are moving mouse", "My Application");

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
