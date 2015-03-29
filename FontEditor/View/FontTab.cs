using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

// Veronika codes here
namespace FontEditor.View
{
    public partial class MainWindow : Window
    {
        private void initFontTab()
        { 
            // write here what should be done on window creation
        }

        private void onFontTabSelected()
        {
            // Veronika writes here ...
            // what should happen when user clicks her tab
        }

        private void onFontTabUnselected()
        { 
            // what should happen on unselection of your tab
        }

        private void fontCanvas_MouseLeftButtonDown(object sender, System.Windows.Input.MouseEventArgs e)
        {

            // understand whether user clicked on already existing point or he created new point
        }

        private void fontCanvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            //System.Windows.MessageBox.Show("You are moving mouse", "My Application");

            // if point was clicked or just has been created, we pull this point
        }
    }
}
