using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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
        }

        private void NewTabButton_Click(object sender, RoutedEventArgs e)
        {
            Tabs.Add(new TabItem { TabName = "New Tab " + id });
            id += 1;
        }

        private void TabListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TabListBox.SelectedItem is TabItem selectedTab)
            {
                ContentArea.Content = new TextBlock { Text = $"Content for {selectedTab.TabName}" };
            }
        }

        private void CloseTabButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var tabItem = button.DataContext as TabItem;

            if (tabItem != null)
            {
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
    }
}
