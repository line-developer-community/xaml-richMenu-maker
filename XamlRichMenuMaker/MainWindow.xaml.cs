using Line.Messaging;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Markup;


namespace XamlRichMenuMaker
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {

        private MainWindowViewModel VM { get => DataContext as MainWindowViewModel; }

        public MainWindow()
        {
            InitializeComponent();
            var accessToken = App.Settings.ChannelAccessToken;
            var userId = App.Settings.DebugUserId;
            DataContext = new MainWindowViewModel(accessToken, userId);
        }


        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (VM == null) { return; }

            var richMenus = new List<ResponseRichMenu>();
            foreach (var file in Directory.EnumerateFiles("RichMenuDefs", "*.xaml"))
            {
                if (Path.GetFileName(file) == "__template.xaml") { continue; }
                using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read))
                {

                    var ctrl = XamlReader.Load(stream) as RichMenuDefsControl;
                    richMenuDefs.Children.Clear();
                    richMenuDefs.Children.Add(ctrl);
                    ctrl.UpdateLayout();
                    var (newRichMenu, renderBmp) = ctrl.RenderingRichMenu();
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    richMenus.Add(new ResponseRichMenu(fileName, newRichMenu));
                    VM.SaveRichMenuImage(renderBmp, fileName);

                }
            }
            await VM.GetRichMenuListAsync(richMenus, null);
            richMenuDefs.Visibility = Visibility.Collapsed;
        }

        private async void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            await VM?.LoadSelectedMenuImage();
        }


    }
}
