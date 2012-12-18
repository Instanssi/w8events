using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Data.Json;

namespace InstanssiEventsViewer
{
    public class ComboboxItem
    {
        public string Text { get; set; }
        public object Value { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }

    public sealed partial class MainPage : InstanssiEventsViewer.Common.LayoutAwarePage
    {
        private HttpClient httpClient;
        private int selectedIndex;

        public MainPage()
        {
            this.InitializeComponent();
            this.selectedIndex = -1;
            httpClient = new HttpClient();
            httpClient.MaxResponseContentBufferSize = 256000;
            httpClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");
        }

        private async void request_happenings()
        {
            this.selectInstanceBox.Items.Clear();
            try
            {
                // Get data
                HttpResponseMessage response = await httpClient.GetAsync("http://instanssi.org/api/happenings/");
                response.EnsureSuccessStatusCode();
                string responseText = await response.Content.ReadAsStringAsync();

                // Parse JSON
                JsonValue jv = JsonValue.Parse(responseText);
                JsonArray hplist = jv.GetObject().GetNamedArray("happenings");

                // HAndle values
                for (int i = 0; i < hplist.Count; i++)
                {
                    // Get happening information
                    string title = hplist[i].GetObject().GetNamedString("name");
                    double id = hplist[i].GetObject().GetNamedNumber("id");

                    // Save to combobox
                    ComboboxItem item = new ComboboxItem();
                    item.Text = title;
                    item.Value = id;
                    this.selectInstanceBox.Items.Add(item);
                }

                // If we got results, select first result automatically
                if(this.selectedIndex == -1 && hplist.Count() > 0) {
                    this.selectInstanceBox.SelectedIndex = 0;
                } 
                else if (hplist.Count >= this.selectedIndex)
                {
                    this.selectInstanceBox.SelectedIndex = this.selectedIndex;
                }

                // All done, enable button
                this.selectInstanceButton.IsEnabled = true;
            }
            catch (HttpRequestException hre)
            {
                ComboboxItem errorItem = new ComboboxItem();
                errorItem.Text = "Error while fetching data";
                errorItem.Value = -1;

                this.selectInstanceBox.Items.Add(errorItem);
                this.selectInstanceButton.IsEnabled = false;
            }
            catch (Exception ex)
            {
                ComboboxItem errorItem = new ComboboxItem();
                errorItem.Text = ex.ToString();
                errorItem.Value = -1;
                this.selectInstanceBox.Items.Add(errorItem);
                this.selectInstanceButton.IsEnabled = false;
            }
        }

        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            try
            {
                if (pageState.ContainsKey("index"))
                {
                    this.selectedIndex = (int)pageState["index"];
                }
            }
            catch (Exception e) { }
            request_happenings();
        }

        protected override void SaveState(Dictionary<String, Object> pageState)
        {
            try
            {
                pageState["index"] = this.selectedIndex;
            }
            catch (Exception e) { }
        }

        private void selectInstanceButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame != null)
            {
                ComboboxItem item = (ComboboxItem)this.selectInstanceBox.SelectedItem;
                this.selectedIndex = this.selectInstanceBox.SelectedIndex;
                this.Frame.Navigate(typeof(EventsPage), item);
            }
        }
    }
}
