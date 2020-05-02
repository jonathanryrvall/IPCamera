using IPCamera.ViewModel;
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

namespace IPCamera.View
{
    /// <summary>
    /// Interaction logic for ViewportV.xaml
    /// </summary>
    public partial class ViewportV : UserControl
    {
        public ViewportV()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Drag viewport
        /// </summary>
        private void borPreview_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            (DataContext as MainVM).borPreview_PreviewMouseMove(sender, e);
        }


        /// <summary>
        /// Scroll event
        /// </summary>
        private void borPreview_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            (DataContext as MainVM).borPreview_PreviewMouseWheel(sender, e);
        }

       
    }
}
