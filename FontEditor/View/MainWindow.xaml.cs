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
//using System.Timers;
using FontEditor.Model;

namespace FontEditor.View
{
    /// this is common part, PLEASE don't waste it unless it is extremeley needed (common logic, etc)
    /// 
    
    public partial class MainWindow : Window
    {
        enum Tabs
        {
            FONT_EDIT,
            TEXT_EDIT
        };

        private Tabs m_currentTab = Tabs.FONT_EDIT;
        
        public MainWindow()
        {
            InitializeComponent();
            initFontTab();
            initTextTab();
        }

        private void tabControl1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Contains(fontTab))
            {
                m_currentTab = Tabs.FONT_EDIT;
                onFontTabSelected();
                onEditTabUnselected();
            }
            else
            {
                m_currentTab = Tabs.TEXT_EDIT;
                onEditTabSelected();
                onFontTabUnselected();
            }

        }

        private void tabControl1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (m_currentTab == Tabs.TEXT_EDIT)
            {
                textTabMouseDown(sender, e);
            }
        }

        private void tabControl1_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (m_currentTab == Tabs.TEXT_EDIT)
            {
                textTabKeyDown(sender, e);
            }
        }

    }

    
}
