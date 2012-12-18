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
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class EventsPage : InstanssiEventsViewer.Common.LayoutAwarePage
    {
        // Selected ID for event
        private double selectedId;
        private HttpClient httpClient;

        public EventsPage()
        {
            this.InitializeComponent();
            httpClient = new HttpClient();
            httpClient.MaxResponseContentBufferSize = 256000;
            httpClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ComboboxItem item = e.Parameter as ComboboxItem;
            this.selectedId = (double)item.Value;
            this.eventNameText.Text = item.Text;

            request_events();
        }

        private async void request_events()
        {
            this.eventsListView.Items.Clear();
            this.eventsListView.CanReorderItems = false;
            this.eventsListView.CanDragItems = false;
            this.eventsListView.IsItemClickEnabled = false;

            try
            {
                // Get data
                HttpResponseMessage response = await httpClient.GetAsync("http://instanssi.org/api/events/"+this.selectedId+"/");
                response.EnsureSuccessStatusCode();
                string responseText = await response.Content.ReadAsStringAsync();

                // Parse JSON
                JsonValue jv = JsonValue.Parse(responseText);
                JsonArray hplist = jv.GetObject().GetNamedArray("events");

                // Handle values
                if (hplist.Count() > 0)
                {
                    for (int i = 0; i < hplist.Count; i++)
                    {
                        // Get happening information
                        string title = hplist[i].GetObject().GetNamedString("title");
                        string date = hplist[i].GetObject().GetNamedString("date");

                        TextBlock vitem = new TextBlock();
                        vitem.Text = date + "\n" + title;
                        this.eventsListView.Items.Add(vitem);
                    }
                }
                else
                {
                    TextBlock vitem = new TextBlock();
                    vitem.Text = "This happening has no public events.";
                    this.eventsListView.Items.Add(vitem);
                }
                this.eventsListView.UpdateLayout();
            }
            catch (HttpRequestException hre)
            {
                TextBlock vitem = new TextBlock();
                vitem.Text = "Error while fetching data from instanssi.org!";
                this.eventsListView.Items.Add(vitem);
            }
            catch (Exception ex)
            {
                TextBlock vitem = new TextBlock();
                vitem.Text = ex.ToString();
                this.eventsListView.Items.Add(vitem);
            }
        }

        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            try
            {
                if (pageState.ContainsKey("id"))
                {
                    this.selectedId = (double)pageState["id"];
                    request_events();
                }
            } 
            catch(Exception e) {}
        }

        protected override void SaveState(Dictionary<String, Object> pageState)
        {
            try
            {
                pageState["id"] = this.selectedId;
            }
            catch (Exception e) { }
        }
    }
}
