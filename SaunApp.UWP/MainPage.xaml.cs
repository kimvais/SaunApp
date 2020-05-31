using Windows.Foundation;
using Windows.UI.ViewManagement;

namespace SaunApp.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();
            LoadApplication(new SaunApp.App());
        }
    }
}
