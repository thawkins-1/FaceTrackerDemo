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

namespace TLHFaceTrack
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CamModel _camModel = null;

        public MainWindow()
        {
             InitializeComponent();

            Button_Start.IsEnabled = true;
            Button_Stop.IsEnabled = false;
        }

        #region ui event haandlers
        private async void Button_Start_Click(object sender, RoutedEventArgs e)
        {
            Button_Start.IsEnabled = false;
            Button_Stop.IsEnabled = false;

            // initialize the cam model if needed
            if (_camModel == null)
                _camModel = new CamModel(Image_Output);

            try
            {
                // starting the camera
                await _camModel.Start();
                Button_Start.IsEnabled = false;
                Button_Stop.IsEnabled = true;
            }
            catch (Exception)
            {
                MessageBox.Show(Properties.Resources.Res_Error_Msg, Properties.Resources.Res_Error_Title, MessageBoxButton.OK, MessageBoxImage.Error);
                Button_Start.IsEnabled = true;
                Button_Stop.IsEnabled = false;
            }
        }

        private async void Button_Stop_Click(object sender, RoutedEventArgs e)
        {
            Button_Start.IsEnabled = false;
            Button_Stop.IsEnabled = false;

            try
            {
                // stopping the camera
                await _camModel.Stop();
            }
            catch (Exception)
            {
                MessageBox.Show(Properties.Resources.Res_Error_Msg, Properties.Resources.Res_Error_Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            Button_Start.IsEnabled = true;
            Button_Stop.IsEnabled = false;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _camModel?.Dispose();
        }
        #endregion
   }
}
