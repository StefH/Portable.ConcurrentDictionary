using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Silverlight50Application
{
    public partial class MainPage : UserControl
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            var con = new ConcurrentDictionary<string, int>();
            con.TryAdd("cat", 1);
            con.TryAdd("dog", 2);

            // Try to update if value is 4 (this fails).
            con.TryUpdate("cat", 200, 4);

            // Try to update if value is 1 (this works).
            con.TryUpdate("cat", 100, 1);
        }
    }
}
