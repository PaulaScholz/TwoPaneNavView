//***********************************************************************
//
// Copyright (c) 2020 Microsoft Corporation. All rights reserved.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//**********************************************************************​
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using MUXC = Microsoft.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TwoPaneNavView
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
#pragma warning disable CA2211 // Non-constant fields should not be visible
        /// <summary>
        /// This lets downlevel Pages or UserControls access MainPage public instance methods and 
        /// properties through this static instance variable, set in the MainPage constructor.
        /// </summary>
        public static MainPage Current = null;
#pragma warning restore CA2211 // Non-constant fields should not be visible

        // These are used to set Pane1Length and Pane2Length. Not used here.
        private GridLength OneStarGridLength = new GridLength(1, GridUnitType.Star);
        private GridLength ZeroStarGridLength = new GridLength(0, GridUnitType.Star);

        // the number of toggleButtons we use on Pane 1
        private double _numberOfButtons = 5;

        private bool _applicationIsSpanned = false;

        /// <summary>
        /// True if the application is spanned across two screens.  We may bind 
        /// this in the UI in future, so implement property change notification.
        /// </summary>
        public bool ApplicationIsSpanned
        {
            get { return _applicationIsSpanned; }
            set
            {
                Set(ref _applicationIsSpanned, value);
            }
        }

        private DisplayOrientations _currentDisplayOrientation = DisplayOrientations.Portrait;

        /// <summary>
        /// Portrait or Landscape
        /// </summary>
        public DisplayOrientations CurrentDisplayOrientation
        {
            get { return _currentDisplayOrientation; }
            set
            {
                Set(ref _currentDisplayOrientation, value);
            }
        }

        MUXC.TwoPaneViewMode _currentTPVMode = MUXC.TwoPaneViewMode.Wide;

        /// <summary>
        /// SinglePane, Wide, or Tall
        /// </summary>
        public MUXC.TwoPaneViewMode CurrentTPVMode
        {
            get { return _currentTPVMode; }
            set
            {
                Set(ref _currentTPVMode, value);
            }
        }

        private double _buttonWidth = 140;

        /// <summary>
        /// The width of the ToggleButtons, which changes depending on orientation.
        /// </summary>
        public double ButtonWidth
        {
            get { return _buttonWidth; }
            set
            {
                Set(ref _buttonWidth, value);
            }
        }


        public MainPage()
        {
            this.InitializeComponent();

            // point the static instance variable to this instance of the Page.
            Current = this;

            Loaded += MainPage_Loaded;

            // this is for the page to compute spanning status and orientation
            SizeChanged += MainPage_SizeChanged;

            // this is for the TwoPaneView to compute UI element size changes
            MainView.SizeChanged += MainView_SizeChanged;

        }

        /// <summary>
        /// Fired when the size of the TwoPaneView changes.  This is where we adjust
        /// UI element sizes within the TwoPaneView.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Debug.WriteLine("MainView_SizeChanged fired on TwoPaneView object.");

            // need to adjust by the enclosing Grid's padding and margin values
            double tpvWidth = MainView.ActualWidth - Pane1Grid.Padding.Left - Pane1Grid.Padding.Right
                - Pane1Grid.Margin.Left - Pane1Grid.Margin.Right;

            if (CurrentDisplayOrientation == DisplayOrientations.Portrait && !ApplicationIsSpanned)
            {
                ButtonWidth = tpvWidth / _numberOfButtons;

                Debug.WriteLine("Portrait and not spanned");
                Debug.WriteLine(string.Format("MainView.ActualWidth = {0}", MainView.ActualWidth));
                Debug.WriteLine(string.Format("MainView.ActualHeight = {0}", MainView.ActualHeight));
                Debug.WriteLine(string.Format("ButtonWidth = {0}", ButtonWidth));
            }
            else if (CurrentDisplayOrientation == DisplayOrientations.Landscape && !ApplicationIsSpanned)
            {
                ButtonWidth = tpvWidth / (_numberOfButtons * 2);

                Debug.WriteLine("Landscape and not spanned");
                Debug.WriteLine(string.Format("MainView.ActualWidth = {0}", MainView.ActualWidth));
                Debug.WriteLine(string.Format("MainView.ActualHeight = {0}", MainView.ActualHeight));
                Debug.WriteLine(string.Format("ButtonWidth = {0}", ButtonWidth));
            }
            else if (CurrentDisplayOrientation == DisplayOrientations.Portrait && ApplicationIsSpanned)
            {
                ButtonWidth = tpvWidth / (_numberOfButtons * 2);

                Debug.WriteLine("Portrait and spanned");
                Debug.WriteLine(string.Format("MainView.ActualWidth = {0}", MainView.ActualWidth));
                Debug.WriteLine(string.Format("MainView.ActualHeight = {0}", MainView.ActualHeight));
                Debug.WriteLine(string.Format("ButtonWidth = {0}", ButtonWidth));
            }
            else if (CurrentDisplayOrientation == DisplayOrientations.Landscape && ApplicationIsSpanned)
            {
                ButtonWidth = tpvWidth / _numberOfButtons;

                Debug.WriteLine("Landscape and spanned");
                Debug.WriteLine(string.Format("MainView.ActualWidth = {0}", MainView.ActualWidth));
                Debug.WriteLine(string.Format("MainView.ActualHeight = {0}", MainView.ActualHeight));
                Debug.WriteLine(string.Format("ButtonWidth = {0}", ButtonWidth));
            }

            Debug.WriteLine("--------------------------------------------------------------");
        }

        /// <summary>
        /// Here, we check if we are on a DualScreen device
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if(IsDualScreenDevice())
            {
                DeviceTypeText.Text = "This is running on DualScreen device";
            }
            else
            {
                DeviceTypeText.Text = "This is running on desktop Windows";
            }
        }

        /// <summary>
        /// Informal test to see if we're running on a DualScreen capable device
        /// or are running on desktop Windows. V1 SDK has no way to check this, so
        /// we resort to this informal method.
        /// </summary>
        /// <returns>True if on a DualScreen device</returns>
        public bool IsDualScreenDevice()
        {
            ApplicationView view = null;
            bool retValue = false;

            try
            {
                view = ApplicationView.GetForCurrentView();

                if (view != null)
                {
                    var dispRegions = view.WindowingEnvironment.GetDisplayRegions();

                    Debug.WriteLine(string.Format("There are {0} DisplayRegions reported in MainPage_Loaded", dispRegions.Count));

                    // DualScreen devices have exactly two display regions. If we don't have
                    // two regions, we're not on a DualScreen device.  Desktop PCs can also
                    // have two regions, so check their sizes also.  This is for V1 of the SDK!!!
                    if(dispRegions.Count == 2)
                    {
                        if( (dispRegions[1].WorkAreaOffset.X == 720 && dispRegions[1].WorkAreaSize.Height == 936) ||    // portrait
                            (dispRegions[1].WorkAreaOffset.Y == 720 && dispRegions[1].WorkAreaSize.Height == 696) )     // landscape
                        {
                            retValue = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception {0}", ex.ToString());
            }

            return retValue;
        }

        /// <summary>
        /// Fired when a rotation or spanning occurs. Dual-Screen experience windows
        /// are either maximized or minimized, there is no intermediate position.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Debug.WriteLine("MainPage_SizeChanged fired on Page object.");

            Debug.WriteLine(string.Format("Previous size: {0} width  {1} height", e.PreviousSize.Width, e.PreviousSize.Height));
            Debug.WriteLine(string.Format("New size: {0} width  {1} height", e.NewSize.Width, e.NewSize.Height));

            // determine orientation & spanning state without using ApplicationView object
            // through this little state machine

            // If the PreviousSize values are zero, then the NewSize
            // values are those at application launch.  If the app
            // is minimized and then maximized, the PreviousSize values
            // are whatever they were before minimized.  The app will
            // always be maximized at an unspanned state, regardless of
            // whether it was spanned before minimization.

            // this if clause determines the initial conditions
            if (e.PreviousSize.Width == 0 && e.PreviousSize.Height == 0)
            {
                // Right now, all we know is we started from application launch
                // and are unspanned. Let's determine whether or not we're 
                // Landscape or Portrait orientation.
                if (e.NewSize.Width < e.NewSize.Height)
                {
                    CurrentDisplayOrientation = DisplayOrientations.Portrait;
                }
                else
                {
                    CurrentDisplayOrientation = DisplayOrientations.Landscape;
                }

                // We always start out unspanned. Spanning is a result of user action.
                ApplicationIsSpanned = false;
            }
            else if (CurrentDisplayOrientation == DisplayOrientations.Portrait && !ApplicationIsSpanned)
            {
                // we're transitioning from Portrait-Unspanned to either Portrait-Spanned (spanning action)
                // or Landscape-Unspanned (rotation action)

                // If height does not change, we're going to Portrait-spanned
                if (e.PreviousSize.Height == e.NewSize.Height)
                {
                    ApplicationIsSpanned = true;
                }
                else
                {
                    // the height changed, we're now in Landscape unspanned
                    CurrentDisplayOrientation = DisplayOrientations.Landscape;
                }
            }
            else if (CurrentDisplayOrientation == DisplayOrientations.Landscape && !ApplicationIsSpanned)
            {
                // we're transitioning from Landscape-Unspanned to either Landscape-Spanned (spanning action)
                // or Portrait-Unspanned (rotation action)
                if (e.PreviousSize.Width == e.NewSize.Width)
                {
                    ApplicationIsSpanned = true;
                }
                else
                {
                    // the width changed, we're now in Portrait-Unspanned
                    CurrentDisplayOrientation = DisplayOrientations.Portrait;
                }
            }
            else if (CurrentDisplayOrientation == DisplayOrientations.Portrait && ApplicationIsSpanned)
            {
                // we're transitioning from Portrait-Spanned to either Portrait-Unspanned (spanning action)
                // or Landscape-Spanned (rotation action)
                if (e.PreviousSize.Height == e.NewSize.Height)
                {
                    ApplicationIsSpanned = false;
                }
                else
                {
                    // the height changed, we're now in Landscape-Spanned
                    CurrentDisplayOrientation = DisplayOrientations.Landscape;
                }
            }
            else if (CurrentDisplayOrientation == DisplayOrientations.Landscape && ApplicationIsSpanned)
            {
                // we're transitioning from Landscape-Spanned to either Landscape-Unspanned (spanning action)
                // or Portrait-Spanned (rotation action)
                if (e.PreviousSize.Width == e.NewSize.Width)
                {
                    ApplicationIsSpanned = false;
                }
                else
                {
                    // the width changed, we're now in Portrait-Spanned
                    CurrentDisplayOrientation = DisplayOrientations.Portrait;
                }
            }
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void Set<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return;
            }

            storage = value;
            OnPropertyChanged(propertyName);
        }

        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion
    }
}
