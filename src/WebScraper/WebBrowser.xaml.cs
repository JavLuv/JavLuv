﻿using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace WebScraper
{
    public partial class WebBrowser : UserControl
    {
        #region Constructors

        public WebBrowser()
        {
            InitializeComponent();
        }

        #endregion

        #region Dependency Properties

        // Root site
        public string RootSite
        {
            get { return (string)GetValue(RootSiteProperty); }
            set { SetCurrentValue(RootSiteProperty, value); }
        }
        public static readonly DependencyProperty RootSiteProperty =
            DependencyProperty.Register("RootSite", typeof(string), typeof(WebBrowser), new PropertyMetadata(""));

        // Web address
        public string Address
        {
            get { return (string)GetValue(AddressProperty); }
            set { SetCurrentValue(AddressProperty, value); }
        }
        public static readonly DependencyProperty AddressProperty =
            DependencyProperty.Register("Address", typeof(string), typeof(WebBrowser), new PropertyMetadata(""));

        // Resulting IDocument
        public IHtmlDocument HtmlDocument
        {
            get { return (IHtmlDocument)GetValue(HtmlDocumentProperty); }
            set { SetCurrentValue(HtmlDocumentProperty, value); }
        }
        public static readonly DependencyProperty HtmlDocumentProperty =
            DependencyProperty.Register("HtmlDocument", typeof(IHtmlDocument), typeof(WebBrowser), new PropertyMetadata(null));

        #endregion

        #region Events

        public event EventHandler<EventArgs> ParsingCompleted;

        #endregion

        #region Public Functions

        async public Task LoadSite()
        {
            if (m_initialized == false)
            {
                await webView.EnsureCoreWebView2Async();
                webView.CoreWebView2.NewWindowRequested += webView_NewWindowRequested;
                webView.NavigationCompleted += WebView_NavigationCompleted;
                m_initialized = true;
            }
            var uri = new Uri(Address);
            if (uri == webView.Source)
                webView.Reload();
            else
                webView.Source = new Uri(Address);
        }

        async public Task ParseSite()
        {
            HtmlDocument = null;
            string html = await webView.CoreWebView2.ExecuteScriptAsync("document.body.outerHTML");
            html = Regex.Unescape(html);
            html = html.Remove(0, 1);
            html = html.Remove(html.Length - 1, 1);
            HtmlParser parser = new HtmlParser();
            HtmlDocument = parser.ParseDocument(html);
        }

        #endregion

        #region Handlers

        private void webView_NewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            // Block window if it's a different domain than expected
            if (e.Uri.Contains(RootSite))
            {
                e.NewWindow = (CoreWebView2)sender;
            }
            e.Handled = true;
        }

        private void WebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (ParsingCompleted != null)
                ParsingCompleted(this, new EventArgs());
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            bool designTime = System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject());
            if (designTime || m_loaded)
                return;
            m_loaded = true;
        }

        #endregion

        #region Private Members

        private bool m_loaded = false;
        private bool m_initialized = false;

        #endregion
    }
}
