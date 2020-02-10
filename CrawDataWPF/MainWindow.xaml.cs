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
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.Net;
using System.Runtime.InteropServices;
using System.Reflection;

namespace CrawDataWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        #region properties

        ObservableCollection<MenuTreeView> item;

        string homePage = "https://howkteam.com/";

        HttpClient craw;
        HttpClientHandler clienHandler;
        CookieContainer cookie;
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            InitHttpClient();

            item = new ObservableCollection<MenuTreeView>();

            crawlTreeView.ItemsSource = item;
        }

        #region methods

        void InitHttpClient()
        {
            // HttpClient send/receive API use BaseAddress
            //clienHandler = new HttpClientHandler
            //{
            //    CookieContainer = cookie, // khi tao se tu luu vao cookie
            //    ClientCertificateOptions = ClientCertificateOption.Automatic,
            //    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            //    AllowAutoRedirect = true,
            //    UseDefaultCredentials = false
            //};

            craw = new HttpClient(/*clienHandler*/); // co the su dung APi


            // xem request header dk gui len , gia lập gói tin để gửi lên, cookie
            // craw.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0.3945.130 Safari/537.36")
            /*
             * Origin
             * Host
             * Referer
             * Scam
             * scheme
             * accept
             * Accept-Encoding
             * Accept-Language
             * UserAgent
             * 
             * 
             * Use HttpClient
             * 
             */

            craw.BaseAddress = new Uri(homePage);
        }

        void AddItemToTreeView(ObservableCollection<MenuTreeView> root, MenuTreeView node)
        {
            root.Add(node);
        }

        string CrawlDataFromURL(string url)
        {
            string html = "";

            
                //var messenger = await craw.GetAsync(url);
            html = WebUtility.HtmlDecode(craw.GetStringAsync(url).Result);
            
            return html;
        }

        void Craw(string url)
        {
            string htmlURL = CrawlDataFromURL(url);
            var courseURL = Regex.Matches(htmlURL, @"<a href=""\/course[\s\S]*?<\/h4>", RegexOptions.Singleline);

            foreach(var eachCourse in courseURL)
            {
                string eachName = Regex.Match(eachCourse.ToString(), @"(?<=title="")(.*?)(?="">)", RegexOptions.Singleline).Value;
                string eachURL = Regex.Match(eachCourse.ToString(), @"(?<=href="")(.*?)(?="">)").Value;

                MenuTreeView newMenu = new MenuTreeView
                {
                    Name = eachName,
                    URL = eachURL
                };

                AddItemToTreeView(item, newMenu);

                string nextHtmlURL = CrawlDataFromURL(eachURL);
                string nextCourseURL = Regex.Match(nextHtmlURL, @"<div class=""asyncPartial""(.*?)>", RegexOptions.Singleline).Value;
                nextCourseURL = Regex.Match(nextCourseURL, @"(?<=data-url="")(.*?)(?="")", RegexOptions.Singleline).Value;
                nextHtmlURL = CrawlDataFromURL(nextCourseURL);
                var nextPerCourseURL = Regex.Matches(nextHtmlURL, @"<a[\s\S]*?<\/a>");

                foreach (var nextPerCourse in nextPerCourseURL)
                {
                    string eachNextCourseName = Regex.Match(nextPerCourse.ToString(), @"(?<=\/span>)[\s\S]*?(?=<\/a>)").Value;

                    string eachNextCoureURL = Regex.Match(nextPerCourse.ToString(), @"(?<=href="")(.*?)(?="">)").Value;

                    MenuTreeView subItem = new MenuTreeView
                    {
                        Name = eachNextCourseName,
                        URL = eachNextCoureURL
                    };

                    //AddItemToTreeView(newMenu.Items, subItem);
                    newMenu.Items.Add(subItem);
                }

            }


        }
        public static void SetSilent(WebBrowser browser, bool silent)
        {
            if (browser == null)
                throw new ArgumentNullException("browser");

            // get an IWebBrowser2 from the document
            IOleServiceProvider sp = browser.Document as IOleServiceProvider;
            if (sp != null)
            {
                Guid IID_IWebBrowserApp = new Guid("0002DF05-0000-0000-C000-000000000046");
                Guid IID_IWebBrowser2 = new Guid("D30C1661-CDAF-11d0-8A3E-00C04FC9E26E");

                object webBrowser;
                sp.QueryService(ref IID_IWebBrowserApp, ref IID_IWebBrowser2, out webBrowser);
                if (webBrowser != null)
                {
                    webBrowser.GetType().InvokeMember("Silent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.PutDispProperty, null, webBrowser, new object[] { silent });
                }
            }
        }

        [ComImport, Guid("6D5140C1-7436-11CE-8034-00AA006009FA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IOleServiceProvider
        {
            [PreserveSig]
            int QueryService([In] ref Guid guidService, [In] ref Guid riid, [MarshalAs(UnmanagedType.IDispatch)] out object ppvObject);
        }
        #endregion

        private void loadData_Click(object sender, RoutedEventArgs e)
        {
            crawlTreeView.Dispatcher.Invoke(new Action(() => Craw("learn/lap-trinh/lap-trinh-c-7-5")));

            //Task t = new Task(() => { Craw("learn/lap-trinh/lap-trinh-c-7-5"); });
            //t.Start();

        }

        //private void Button_Click(object sender, RoutedEventArgs e)
        //{
        //    string url = (sender as Button).Tag.ToString();

        //    //txtBrowser.Navigate(url);
        //    Process.Start(url);
        //}

        private void TextBlock_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            string url = homePage + (sender as TextBlock).Tag.ToString();
            txtBrowser.Navigate(url);
        }

        private void TxtBrowser_Navigated(object sender, NavigationEventArgs e)
        {
            SetSilent(txtBrowser, true);
        }


        //StringBuilder eventstr = new StringBuilder();
        //void HandleButton(object sender, RoutedEventArgs args)
        //{
        //    // Get the element that handled the event.
        //    FrameworkElement fe = (FrameworkElement)sender;
        //    eventstr.Append("Event handled by element named ");
        //    eventstr.Append(fe.Name);
        //    eventstr.Append("\n");

        //    // Get the element that raised the event. 
        //    FrameworkElement fe2 = (FrameworkElement)args.Source;
        //    eventstr.Append("Event originated from source element of type ");
        //    eventstr.Append(args.Source.GetType().ToString());
        //    eventstr.Append(" with Name ");
        //    eventstr.Append(fe2.Name);
        //    eventstr.Append("\n");

        //    // Get the routing strategy.
        //    eventstr.Append("Event used routing strategy ");
        //    eventstr.Append(args.RoutedEvent.RoutingStrategy);
        //    eventstr.Append("\n");

        //    results.Text = eventstr.ToString();
    }
    
}
