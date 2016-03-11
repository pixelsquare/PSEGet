/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocatorTemplate xmlns:vm="clr-namespace:PSEGet3.ViewModel"
                                   x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"
  
  OR (WPF only):
  
  xmlns:vm="clr-namespace:PSEGet3.ViewModel"
  DataContext="{Binding Source={x:Static vm:ViewModelLocatorTemplate.ViewModelNameStatic}}"
*/

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Practices.ServiceLocation;
using GalaSoft.MvvmLight.Ioc;
using PSEGetLib;
using PSEGetLib.Service;
using PSEGetLib.Data.Service;
using PSEGetLib.Interfaces;

namespace PSEGet3.ViewModel
{
    /// <summary>
    ///     This class contains static references to all the view models in the
    ///     application and provides an entry point for the bindings.
    ///     <para>
    ///         Use the <strong>mvvmlocatorproperty</strong> snippet to add ViewModels
    ///         to this locator.
    ///     </para>
    ///     <para>
    ///         In Silverlight and WPF, place the ViewModelLocatorTemplate in the App.xaml resources:
    ///     </para>
    ///     <code>
    /// &lt;Application.Resources&gt;
    ///     &lt;vm:ViewModelLocatorTemplate xmlns:vm="clr-namespace:PSEGet3.ViewModel"
    ///                                  x:Key="Locator" /&gt;
    /// &lt;/Application.Resources&gt;
    /// </code>
    ///     <para>
    ///         Then use:
    ///     </para>
    ///     <code>
    /// DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"
    /// </code>
    ///     <para>
    ///         You can also use Blend to do all this with the tool's support.
    ///     </para>
    ///     <para>
    ///         See http://www.galasoft.ch/mvvm/getstarted
    ///     </para>
    ///     <para>
    ///         In <strong>*WPF only*</strong> (and if databinding in Blend is not relevant), you can delete
    ///         the Main property and bind to the ViewModelNameStatic property instead:
    ///     </para>
    ///     <code>
    /// xmlns:vm="clr-namespace:PSEGet3.ViewModel"
    /// DataContext="{Binding Source={x:Static vm:ViewModelLocatorTemplate.ViewModelNameStatic}}"
    /// </code>
    /// </summary>
    public class ViewModelLocator
    {
        //private static MainViewModel _main;
        //private static ConverterViewModel _converterViewModel;
        //private static ConvertFromFileViewModel _convertFromFileViewModel;
        //private static DownloadAndConvertViewModel _downloadAndConvertViewModel;
        //private static DownloadHistoricalDataViewModel _downloadHistoricalViewModel;
        //private static OutputSettingsViewModel _outputSettingsViewModel;
        //private static MarketSummaryViewModel _marketSummaryViewModel;
        //private static MessageWindowViewModel _errorWindowViewModel;
        //private static MarketActivityViewModel _marketActivityViewModel;
        //private static ProgressDialogViewModel _progressDialogViewModel;

        /// <summary>
        ///     Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        static ViewModelLocator()
        {
            ////if (ViewModelBase.IsInDesignModeStatic)
            ////{
            ////    // Create design time view models
            ////}
            ////else
            ////{
            ////    // Create run time view models
            ////}
            //_sp = ServiceProviderBase.Instance; 

            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<IPdfService, PdfTextSharpService>();
            SimpleIoc.Default.Register<IPSEGetDataService, PSEGetDataService>();
            //SimpleIoc.Default.Register<IReportDownloader, ReportDownloader>();
            SimpleIoc.Default.Register<IAmibrokerService, AmibrokerService>();

            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<ConverterViewModel>();
            SimpleIoc.Default.Register<ConvertFromFileViewModel>();
            SimpleIoc.Default.Register<DownloadAndConvertViewModel>();
            //SimpleIoc.Default.Register<DownloadHistoricalDataViewModel>();
            SimpleIoc.Default.Register<OutputSettingsViewModel>();
            SimpleIoc.Default.Register<MarketSummaryViewModel>();
            SimpleIoc.Default.Register<MessageWindowViewModel>();
            SimpleIoc.Default.Register<MarketActivityViewModel>();
            SimpleIoc.Default.Register<ProgressDialogViewModel>();

            ViewModelLocator.DownloadAndConvertVMStatic.FromDate = DateTime.Today;
            ViewModelLocator.DownloadAndConvertVMStatic.ToDate = DateTime.Today;
            ViewModelLocator.ConverterVMStatic.DataConvertMethod = ViewModelLocator.OutputSettingsVMStatic.DataConvertMethod;
            ViewModelLocator.ConverterVMStatic.IsBusy = false;

            //CreateMain();
        }

        /// <summary>
        ///     Gets the Main property.
        /// </summary>
        public static MainViewModel MainStatic
        {
            get
            {
                //if (_main == null)
                //{
                //    CreateMain();
                //}

                return ServiceLocator.Current.GetInstance<MainViewModel>();
            }
        }

        public static ConverterViewModel ConverterVMStatic
        {
            get
            {
                //if (_converterViewModel == null)
                //{
                //    CreateConverterViewModel();
                //}

                //return _converterViewModel;
                return ServiceLocator.Current.GetInstance<ConverterViewModel>();
            }
        }

        public static ConvertFromFileViewModel ConvertFromFileVMStatic
        {
            get
            {
                //if (_convertFromFileViewModel == null)
                //{
                //    CreateConvertFromFileViewModel();
                //}

                //return _convertFromFileViewModel;

                return ServiceLocator.Current.GetInstance<ConvertFromFileViewModel>();
            }
        }

        public static DownloadAndConvertViewModel DownloadAndConvertVMStatic
        {
            get
            {
                //if (_downloadAndConvertViewModel == null)
                //{
                //    CreateDownloadAndConvertViewModel();
                //}
                //return _downloadAndConvertViewModel;
                return ServiceLocator.Current.GetInstance<DownloadAndConvertViewModel>();
            }
        }

        //public static DownloadHistoricalDataViewModel DownloadHistoricalDataVMStatic
        //{
        //    get
        //    {
        //        //if (_downloadHistoricalViewModel == null)
        //        //{
        //        //    CreateDownloadHistoricalDataViewModel();
        //        //}
        //        //return _downloadHistoricalViewModel;
        //        return ServiceLocator.Current.GetInstance<DownloadHi>
        //    }
        //}

        public static OutputSettingsViewModel OutputSettingsVMStatic
        {
            get
            {
                //if (_outputSettingsViewModel == null)
                //{
                //    CreateOutputSettingsViewModel();
                //}
                //return _outputSettingsViewModel;
                return ServiceLocator.Current.GetInstance<OutputSettingsViewModel>();
            }
        }

        public static MarketSummaryViewModel MarketSummaryVMStatic
        {
            get
            {
                //if (_marketSummaryViewModel == null)
                //{
                //    CreateMarketSummaryViewModel();
                //}
                //return _marketSummaryViewModel;
                return ServiceLocator.Current.GetInstance<MarketSummaryViewModel>();
            }
        }

        public static MessageWindowViewModel ErrorWindowVMStatic
        {
            get
            {
                //if (_errorWindowViewModel == null)
                //{
                //    CreateErrorWindowViewModel();
                //}
                //return _errorWindowViewModel;
                return ServiceLocator.Current.GetInstance<MessageWindowViewModel>();
            }
        }

        public static MarketActivityViewModel MarketActivityVMStatic
        {
            get
            {
                //if (_marketActivityViewModel == null)
                //{
                //    CreateMarketActivityViewModel();
                //}
                //return _marketActivityViewModel;
                return ServiceLocator.Current.GetInstance<MarketActivityViewModel>();
            }
        }

        public static ProgressDialogViewModel ProgressDialogVMStatic
        {
            get
            {
                //if (_progressDialogViewModel == null)
                //{
                //    CreateProgressDialogViewModel();
                //}
                //return _progressDialogViewModel;
                return ServiceLocator.Current.GetInstance<ProgressDialogViewModel>();
            }
        }


        /// <summary>
        ///     Gets the Main property.
        /// </summary>
        [SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public MainViewModel Main
        {
            get { return MainStatic; }
        }

        public MarketSummaryViewModel MarketSummaryVM
        {
            get { return MarketSummaryVMStatic; }
        }

        public MarketActivityViewModel MarketActivityVM
        {
            get { return MarketActivityVMStatic; }
        }

        public ConverterViewModel ConverterVM
        {
            get { return ConverterVMStatic; }
        }

        public ConvertFromFileViewModel ConvertFromFileVM
        {
            get { return ConvertFromFileVMStatic; }
        }

        public DownloadAndConvertViewModel DownloadAndConvertVM
        {
            get { return DownloadAndConvertVMStatic; }
        }

        //public DownloadHistoricalDataViewModel DownloadHistoricalDataVM
        //{
        //    get { return DownloadHistoricalDataVMStatic; }
        //}

        public OutputSettingsViewModel OutputSettingsVM
        {
            get { return OutputSettingsVMStatic; }
        }

        public MessageWindowViewModel ErrorWindowVM
        {
            get { return ErrorWindowVMStatic; }
        }

        public ProgressDialogViewModel ProgressDialogVM
        {
            get { return ProgressDialogVMStatic; }
        }

        //public static void CreateProgressDialogViewModel()
        //{
        //    if (_progressDialogViewModel == null)
        //    {
        //        _progressDialogViewModel = new ProgressDialogViewModel();
        //    }
        //}

        //public static void CreateMarketActivityViewModel()
        //{
        //    if (_marketActivityViewModel == null)
        //    {
        //        _marketActivityViewModel = new MarketActivityViewModel();
        //    }
        //}

        //public static void CreateErrorWindowViewModel()
        //{
        //    if (_errorWindowViewModel == null)
        //    {
        //        _errorWindowViewModel = new MessageWindowViewModel();
        //    }
        //}

        //public static void CreateMarketSummaryViewModel()
        //{
        //    if (_marketSummaryViewModel == null)
        //    {
        //        _marketSummaryViewModel = new MarketSummaryViewModel();
        //    }
        //}

        //public static void CreateOutputSettingsViewModel()
        //{
        //    if (_outputSettingsViewModel == null)
        //    {
        //        _outputSettingsViewModel = new OutputSettingsViewModel();
        //    }
        //}

        //public static void CreateDownloadHistoricalDataViewModel()
        //{
        //    if (_downloadHistoricalViewModel == null)
        //    {
        //        _downloadHistoricalViewModel = new DownloadHistoricalDataViewModel();
        //    }
        //}

        //public static void CreateDownloadAndConvertViewModel()
        //{
        //    if (_downloadAndConvertViewModel == null)
        //    {
        //        _downloadAndConvertViewModel = new DownloadAndConvertViewModel();
        //        _downloadAndConvertViewModel.FromDate = DateTime.Today;
        //        _downloadAndConvertViewModel.ToDate = DateTime.Today;
        //    }
        //}

        //public static void CreateConvertFromFileViewModel()
        //{
        //    if (_convertFromFileViewModel == null)
        //    {
        //        _convertFromFileViewModel = new ConvertFromFileViewModel();
        //    }
        //}

        //public static void CreateConverterViewModel()
        //{
        //    if (_converterViewModel == null)
        //    {
        //        _converterViewModel = new ConverterViewModel();
        //        _converterViewModel.DataConvertMethod = OutputSettingsVMStatic.DataConvertMethod;
        //            // ConvertMethod.DownloadAndConvert;
        //        _converterViewModel.IsBusy = false;
        //    }
        //}

        /// <summary>
        ///     Provides a deterministic way to delete the Main property.
        /// </summary>
        public static void ClearMain()
        {
            //_main.Cleanup();
            //_main = null;
        }

        /// <summary>
        ///     Provides a deterministic way to create the Main property.
        /// </summary>
        //public static void CreateMain()
        //{
        //    if (_main == null)
        //    {
        //        _main = new MainViewModel();
        //    }
        //}

        /// <summary>
        ///     Cleans up all the resources.
        /// </summary>
        public static void Cleanup()
        {
            ClearMain();
        }
    }
}