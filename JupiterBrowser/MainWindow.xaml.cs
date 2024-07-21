using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace JupiterBrowser
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<TabItem> Tabs { get; set; }
        private TabItem _draggedItem;
        private Point _startPoint;

        public int id = 1;

        public MainWindow()
        {
            InitializeComponent();
            Tabs = new ObservableCollection<TabItem>();
            TabListBox.ItemsSource = Tabs;
            this.DataContext = this;
        }

        private void NewTabButton_Click(object sender, RoutedEventArgs e)
        {
            var newTab = new TabItem { TabName = "New Tab " + id };
            Tabs.Add(newTab);
            id += 1;

            var webView = new WebView2();
            webView.Source = new System.Uri("https://www.google.com");
            newTab.WebView = webView;
            webView.NavigationCompleted += WebView_NavigationCompleted;

            if (TabListBox.SelectedItem is TabItem selectedTab)
            {
                ContentArea.Content = selectedTab.WebView;
            }
        }

        private async void WebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (sender is WebView2 webView)
            {
                var title = await webView.CoreWebView2.ExecuteScriptAsync("document.title");
                title = title.Trim('"'); // Remove the surrounding quotes

                var tabItem = Tabs.FirstOrDefault(tab => tab.WebView == webView);
                if (tabItem != null)
                {
                    tabItem.TabName = title;
                }

                // Refresh the ListBox to update the displayed tab name
                var selectedIndex = TabListBox.SelectedIndex;
                TabListBox.ItemsSource = null;
                TabListBox.ItemsSource = Tabs;
                TabListBox.SelectedIndex = selectedIndex;
            }
        }

        private void TabListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TabListBox.SelectedItem is TabItem selectedTab)
            {
                ContentArea.Content = selectedTab.WebView;
            }
        }

        private void CloseTabButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var tabItem = button.DataContext as TabItem;

            if (tabItem != null)
            {
                // Dispose of the WebView2 control
                tabItem.WebView?.Dispose();

                Tabs.Remove(tabItem);
            }
        }

        private void TabListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);
        }

        private void TabListBox_MouseMove(object sender, MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(null);
            Vector diff = _startPoint - mousePos;

            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                var listBoxItem = VisualUpwardSearch(e.OriginalSource as DependencyObject) as ListBoxItem;

                if (listBoxItem != null)
                {
                    _draggedItem = listBoxItem.DataContext as TabItem;
                    if (_draggedItem != null)
                    {
                        DragDrop.DoDragDrop(listBoxItem, _draggedItem, DragDropEffects.Move);
                    }
                }
            }
        }

        private void TabListBox_DragOver(object sender, DragEventArgs e)
        {
            if (_draggedItem != null)
            {
                e.Effects = DragDropEffects.Move;
            }
        }

        private void TabListBox_Drop(object sender, DragEventArgs e)
        {
            if (_draggedItem != null)
            {
                var listBox = sender as ListBox;
                var targetItem = VisualUpwardSearch(e.OriginalSource as DependencyObject) as ListBoxItem;
                var targetTab = targetItem?.DataContext as TabItem;

                if (targetTab != null && targetTab != _draggedItem)
                {
                    int oldIndex = Tabs.IndexOf(_draggedItem);
                    int newIndex = Tabs.IndexOf(targetTab);

                    if (oldIndex != -1 && newIndex != -1)
                    {
                        Tabs.Move(oldIndex, newIndex);
                    }
                }

                _draggedItem = null;
            }
        }

        private DependencyObject VisualUpwardSearch(DependencyObject source)
        {
            while (source != null && !(source is ListBoxItem))
            {
                source = VisualTreeHelper.GetParent(source);
            }
            return source;
        }
    }

    public class TabItem
    {
        public string TabName { get; set; }
        public WebView2 WebView { get; set; }
    }
}
