using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using WiresharkApp;

namespace NetworkMonitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //String fields used for formatting dynamic fields in "Connected Devices" tab
        private const string infoNamePrefix = "Name: ";
        private const string infoNameUnknown = "Unknown Connection";
        private const string infoTotalPacketsPrefix = "Total Packets: ";
        private const string infoTotalDataPrefix = "Total Data: ";
        private const string infoMacAddressPrefix = "Mac Address: ";
        private const string infoMacAddressUnknown = "Unknown Mac Address";
        private const string infoPlaceholderText = "Click a name or graph line for more info...";
        //String fields used for formatting dynamic fields in "Live Feed" tab
        private const string liveTotalPacketsPrefix = "Total Packets: ";
        private const string liveTotalDataPrefix = "Total Data Across Wire: ";
        private const string liveMostSeenSourcePrefix = "Most Seen Source IP: ";
        private const string liveMostSeenDestinationPrefix = "Most Seen Remote IP: ";
        //For "Connected Devices" panel, width of different display texts
        private const int devicesRankingWidth = 57;
        private const int devicesNameWidth = 42;
        private const int devicesMACWidth = 95;
        //For "Live Feed" panel, width of different display texts.
        private const int liveSourceIPWidth = 66;
        private const int liveDestIPWidth = 49;
        private const int liveLocalPortWidth = 69;
        private const int liveSizeWidth = 60;
        private const int liveProtocolWidth = 59;
        //Height of "Connected Devices" entry button
        private const int devicesEntryHeight = 25;
        //Colors for top 5 clients on connectedDevices tab
        private readonly Brush[] topClientBrushes = { Brushes.Red, Brushes.Green, Brushes.Blue, Brushes.Purple, Brushes.Orange };

        //How large top tick is for usage graph, and increment each tick represents
        private const int totalGraphPacketCount = 100;
        private const int graphPacketTickInterval = 2;
        //Total time period our "Graph of Devices" covers, and point interval. Both in minutes
        private const int totalGraphTime = 60;
        private const int graphPointInterval = 1;
        //Width of axis for our usage graph. In pixels.
        private const double graphAxisSize = 10;

        //Info on all clients read in from the database
        private ClientInfo[] clients;
        //Info on all packets for live feed
        private PacketInfo[] packets;
        //Button, and corresponding client for each button
        private Dictionary<Button, ClientInfo> connectedDevicesButtonClients = new Dictionary<Button, ClientInfo>();

        private WiresharkProcess wiresharkProcess = new WiresharkProcess();
        
        public MainWindow()
        {
            InitializeComponent();

            Thread wiresharkThread = new Thread(wiresharkProcess.StartWireShark);
            wiresharkThread.Start();

            //Setup initial refresh after window loads
            this.ContentRendered += OnContentRendered;
            this.SizeChanged += OnSizeChanged;
            if (refreshConnectedDevices != null)
                refreshConnectedDevices.Click += OnUpdateDatabase;
            if (refreshLiveFeed != null)
                refreshLiveFeed.Click += OnUpdateDatabase;
        }

        /// <summary>
        /// Occurrs after window content first loads
        /// </summary>
        private void OnContentRendered(object sender, EventArgs e)
        {
            UpdateDatabase();
        }

        /// <summary>
        /// Called when window size changes
        /// </summary>
        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            //Refresh graph when window size changes, so it scales up to our current window size
            RefreshUsageGraph();
            //Refresh live feed sizes so they all move apropriately
            RefreshLiveFeedMargins();
        }

        /// <summary>
        /// Calls UpdateDatabase(). Used for buttons
        /// </summary>
        public void OnUpdateDatabase(object sender, RoutedEventArgs e) { UpdateDatabase(); }

        /// <summary>
        /// Grabs latest data from database, refreshing all dynamic fields
        /// </summary>
        public void UpdateDatabase()
        {
            uint totalPackets;
            double totalData;
            string sourceIP, destinationIP;
            //Get data on all clients from database
            DatabaseParser.RefreshDatabase(out clients, out packets, out totalPackets, out totalData, out sourceIP, out destinationIP);

            RefreshConnectedDevices();
            RefreshLiveFeed();

            Live_UpdateTotalPackets(totalPackets);
            Live_UpdateTotalData(totalData);
            Live_UpdateMostSeenSourceAddress(sourceIP);
            Live_UpdateMostSeenDestinationAddress(destinationIP);
        }

        #region Connected Devices Tab

        /// <summary>
        /// Selects the indicated client in the "Connected Devices" tab.
        /// </summary>
        /// <param name="client">Client to select</param>
        public void Info_SelectClient(ClientInfo client)
        {
            if (infoName != null)
                infoName.Text = infoNamePrefix + ((!string.IsNullOrEmpty(client.Name)) ? client.Name : infoNameUnknown);
            if (infoTotalPackets != null)
                infoTotalPackets.Text = infoTotalPacketsPrefix + client.PacketCount.ToString("N0");
            if (infoTotalData != null)
                infoTotalData.Text = infoTotalDataPrefix + FormatDataText(client.TotalData);
            if (infoMacAddress != null)
                infoMacAddress.Text = infoMacAddressPrefix + ((!string.IsNullOrEmpty(client.MacAddress)) ? client.MacAddress : infoMacAddressUnknown);
            //Ensure our placeholder is cleared
            if (infoPlaceholder != null)
                infoPlaceholder.Text = "";
        }

        /// <summary>
        /// Selects no client in the "Connected Devices" tab, clearing all text fields.
        /// </summary>
        public void Info_DeselectClient()
        {
            //Clear all other fields
            if (infoName != null)
                infoName.Text = infoNamePrefix;
            if (infoTotalPackets != null)
                infoTotalPackets.Text = infoTotalPacketsPrefix;
            if (infoTotalData != null)
                infoTotalData.Text = infoTotalDataPrefix;
            if (infoMacAddress != null)
                infoMacAddress.Text = infoMacAddressPrefix;
            //Reset placeholder text
            if (infoPlaceholder != null)
                infoPlaceholder.Text = infoPlaceholderText;
        }

        /// <summary>
        /// Refreshes our "Connected Devices" tab, retrieving latest data from the database and displaying it in graph and list form.
        /// </summary>
        private void RefreshConnectedDevices()
        {
            //Ensure no client is selected
            Info_DeselectClient();

            if (clients == null || connectedDevicesStack == null)
            {
                Console.WriteLine("Error! Null parameter passed, unable to refresh connected devices.");
                return;
            }

            //Organize clients by number of packets sent
            List<ClientInfo> sortedClients = clients.OrderByDescending(o => o.PacketCount).ToList();

            //Populate connected devices panel based in ascending packet count order.
            connectedDevicesStack.Children.Clear();
            connectedDevicesButtonClients.Clear();
            for (int i = 0; i < sortedClients.Count; i++)
                CreateConnectedDevicesEntry((uint)(i + 1), sortedClients[i]);

            //Populate graph based on connected devices
            RefreshUsageGraph();
        }

        /// <summary>
        /// Refreshes our usage graph on the UI
        /// </summary>
        private void RefreshUsageGraph()
        {
            usageGraph.Children.Clear();

            //Organize clients by number of packets sent
            List<ClientInfo> sortedClients = new List<ClientInfo>();
            if (clients != null && clients.Length > 0)
                sortedClients = clients.OrderByDescending(o => o.PacketCount).ToList();

            //In window space, min/max coordinates for plot.
            double xmin = graphAxisSize,
                xmax = usageGraph.ActualWidth - graphAxisSize,
                ymin = graphAxisSize,
                ymax = usageGraph.ActualHeight - graphAxisSize;
            //Step interval in pixels for x and y axis. Number of ticks is based off interval.
            double xTickStep = (usageGraph.ActualWidth - xmin) / (totalGraphTime / graphPointInterval),
                yTickStep = (usageGraph.ActualHeight - ymin) / (totalGraphPacketCount / graphPacketTickInterval);

            //Draw "Time" axis
            GeometryGroup timeAxis = new GeometryGroup();
            timeAxis.Children.Add(new LineGeometry(new Point(0, ymax), new Point(usageGraph.ActualWidth, ymax)));
            //Add graph ticks. Number of ticks is based off our graph point interval
            double x = xmin;
            for (int i = 0; i < totalGraphTime / graphPointInterval; i++)
            {
                timeAxis.Children.Add(new LineGeometry(
                    new Point(x, ymax - graphAxisSize / 2),
                    new Point(x, ymax + graphAxisSize / 2)));
                x += xTickStep;
            }

            //Create line through ticks
            Path timePath = new Path();
            timePath.StrokeThickness = 1;
            timePath.Stroke = Brushes.Black;
            timePath.Data = timeAxis;

            //Add time axis to our usage graph
            usageGraph.Children.Add(timePath);

            //Draw "Packets" axis
            GeometryGroup packetAxis = new GeometryGroup();
            packetAxis.Children.Add(new LineGeometry(new Point(xmin, 0), new Point(xmin, usageGraph.ActualHeight)));
            double y = ymax;
            for (int i = 0; i < totalGraphPacketCount / graphPacketTickInterval; i++)
            {
                packetAxis.Children.Add(new LineGeometry(
                    new Point(xmin - graphAxisSize / 2, y),
                    new Point(xmin + graphAxisSize / 2, y)));
                y -= yTickStep;
            }

            //Create path for packets axis
            Path packetsPath = new Path();
            packetsPath.StrokeThickness = 1;
            packetsPath.Stroke = Brushes.Black;
            packetsPath.Data = packetAxis;

            //Add time axis to our usage graph
            usageGraph.Children.Add(packetsPath);

            //Draw top clients graph lines
            for (int c = 0; c < Math.Min(topClientBrushes.Length, sortedClients.Count); c++)
            {
                PointCollection graphPoints = new PointCollection();
                ClientInfo client = sortedClients[c];
                x = xmin;

                //Draw point for each recorded packet count for the last 60 minutes.
                for (int i = 0; i < totalGraphTime; i++)
                {
                    y = (i < client.PacketSamples.Length) ? client.PacketSamples[i] : 0;

                    //Scale packet sample based on our sizes
                    y = ymax - yTickStep * (y / graphPacketTickInterval);

                    //Create point
                    graphPoints.Add(new Point(x, y));
                    x += xTickStep;
                }

                Polyline clientLine = new Polyline();
                clientLine.StrokeThickness = 1;
                clientLine.Stroke = topClientBrushes[c];
                clientLine.Points = graphPoints;

                usageGraph.Children.Add(clientLine);
            }
        }


        /// <summary>
        /// Creates a UI element for the ConnectedDevices stack using the given ClientInfo object.
        /// </summary>
        /// <param name="ranking">Ranking of this client to use for text field.</param>
        /// <param name="info">Client who we are creating an entry for.</param>
        private void CreateConnectedDevicesEntry(uint ranking, ClientInfo info)
        {
            Button button = new Button();
            button.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            button.VerticalContentAlignment = VerticalAlignment.Top;
            button.Height = devicesEntryHeight;

            //Create grid, so text is formatted correctly.
            Grid buttonGrid = new Grid();

            //Rank text
            TextBlock rankTxt = new TextBlock();
            if (ranking <= topClientBrushes.Length)
                rankTxt.Foreground = topClientBrushes[ranking - 1];
            rankTxt.TextWrapping = TextWrapping.Wrap;
            rankTxt.Text = "#" + ranking;
            rankTxt.FontSize = 16;
            rankTxt.HorizontalAlignment = HorizontalAlignment.Left;
            rankTxt.VerticalAlignment = VerticalAlignment.Top;
            //Center us under "Ranking" text
            rankTxt.Margin = new Thickness((devicesRankingWidth / 2f) - (GetStringWidth(rankTxt.Text, rankTxt.FontSize) / 2f), 0, 0, 0);

            buttonGrid.Children.Add(rankTxt);

            //Name text
            TextBlock nameTxt = new TextBlock();
            if (ranking <= topClientBrushes.Length)
                nameTxt.Foreground = topClientBrushes[ranking - 1];
            nameTxt.TextWrapping = TextWrapping.Wrap;
            nameTxt.Text = info.Name;
            nameTxt.FontSize = 16;
            nameTxt.HorizontalAlignment = HorizontalAlignment.Center;
            nameTxt.VerticalAlignment = VerticalAlignment.Top;
            //Center us under "Name" text
            nameTxt.Margin = new Thickness(57 + (devicesNameWidth / 2f) - (GetStringWidth(nameTxt.Text, nameTxt.FontSize) / 2f), 0, 84, 0);

            buttonGrid.Children.Add(nameTxt);

            //MAC text
            TextBlock macTxt = new TextBlock();
            if (ranking <= topClientBrushes.Length)
                macTxt.Foreground = topClientBrushes[ranking - 1];
            macTxt.TextWrapping = TextWrapping.Wrap;
            macTxt.Text = info.MacAddress;
            macTxt.FontSize = 16;
            macTxt.HorizontalAlignment = HorizontalAlignment.Right;
            macTxt.VerticalAlignment = VerticalAlignment.Top;
            //Center us under "MAC Address" text
            macTxt.Margin = new Thickness((devicesMACWidth / 2f) - (GetStringWidth(macTxt.Text, macTxt.FontSize) / 2f), 0, 0, 0);

            buttonGrid.Children.Add(macTxt);

            //This button selects this client
            button.Click += Info_OnSelectClient;

            //Set button to have all three text fields as childrne
            button.Content = buttonGrid;
            //Add button to our connected devices scroll view.
            connectedDevicesStack.Children.Add(button);
            //Register this button for the given client
            connectedDevicesButtonClients.Add(button, info);
        }

        /// <summary>
        /// Occurs when one of our connected devices is clicked. Selects the corresponding client.
        /// </summary>
        /// <param name="sender">Object who sent this event</param>
        /// <param name="e">Args for event, if any.</param>
        private void Info_OnSelectClient(object sender, RoutedEventArgs e)
        {
            //If the clicked object was a button, and is in our connected devices list, select that device
            if (sender is Button && connectedDevicesButtonClients.ContainsKey((Button)sender))
                Info_SelectClient(connectedDevicesButtonClients[(Button)sender]);
        }

        #endregion

        #region Live Feed

        /// <summary>
        /// Updates our live feed with a new value for total number of packets.
        /// </summary>
        /// <param name="totalData">Total number of packets read from wireshark</param>
        private void Live_UpdateTotalPackets(uint packetCount)
        {
            //If input is valid, update text with packet count.
            if (liveTotalPackets != null)
                liveTotalPackets.Text = liveTotalPacketsPrefix + packetCount;
        }

        /// <summary>
        /// Updates our live feed with a new value for total amount of data sent across wire.
        /// </summary>
        /// <param name="totalDataKb">Total amount of data sent through network, in KB</param>
        private void Live_UpdateTotalData(double totalDataKb)
        {
            //If input is valid, update text rounded to 3 decimal places
            if (liveTotalData != null && totalDataKb > 0)
                liveTotalData.Text = liveTotalDataPrefix + FormatDataText(totalDataKb);
        }

        /// <summary>
        /// Updates our live feed with a new value for most seen remote address
        /// </summary>
        /// <param name="newIp">IP address which has been seen the most</param>
        private void Live_UpdateMostSeenSourceAddress(string newIp)
        {
            if (liveMostSeenSource != null)
                liveMostSeenSource.Text = liveMostSeenSourcePrefix + newIp;
        }

        /// <summary>
        /// Updates our live feed with a new value for most seen local device address
        /// </summary>
        /// <param name="newIp">IP address of local device which has been seen the most</param>
        private void Live_UpdateMostSeenDestinationAddress(string newIp)
        {
            if (liveMostSeenDestination != null)
                liveMostSeenDestination.Text = liveMostSeenDestinationPrefix + newIp;
        }

        /// <summary>
        /// Refreshes our live feed of traffic with latest packet data we have
        /// </summary>
        private void RefreshLiveFeed()
        {
            if (packets == null || liveFeedStack == null)
            {
                Console.WriteLine("Error! Null parameter passed, unable to refresh live feed");
                return;
            }

            //Populate connected devices panel based in ascending packet count order.
            liveFeedStack.Children.Clear();
            for (int i = 0; i < packets.Length; i++)
                CreateLiveFeedEntry(packets[i]);
        }

        /// <summary>
        /// Refreshes all elements for our live feed so text is centered properly.
        /// </summary>
        private void RefreshLiveFeedMargins()
        {
            if (liveSourceIP != null && liveDestIP != null)
                liveDestIP.Margin = new Thickness(((Width / 2f) - 15) / 2 + 20, 34, 0, 0);
            if (liveProtocol != null && liveSize != null)
                liveSize.Margin = new Thickness(0, 34, ((Width / 2f) - 15) / 2 + 20, 0);

            RefreshLiveFeed();
        }

        /// <summary>
        /// Creates a UI element for the LiveFeed stack using the given PacketInfo object.
        /// </summary>
        /// <param name="info">Packet which we are creating an entry for.</param>
        private void CreateLiveFeedEntry(PacketInfo info)
        {
            Button button = new Button();
            button.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            button.VerticalContentAlignment = VerticalAlignment.Top;
            button.Height = devicesEntryHeight;

            //Create grid, so text is formatted correctly.
            Grid buttonGrid = new Grid();

            //Source address text
            TextBlock sourceIpTxt = new TextBlock();
            sourceIpTxt.TextWrapping = TextWrapping.Wrap;
            sourceIpTxt.Text = info.SourceAddress;
            sourceIpTxt.FontSize = 16;
            sourceIpTxt.HorizontalAlignment = HorizontalAlignment.Left;
            sourceIpTxt.VerticalAlignment = VerticalAlignment.Top;
            //Center us under "Source Address" text
            sourceIpTxt.Margin = new Thickness((liveSourceIP.Margin.Left - 10) + (liveSourceIPWidth / 2f) - (GetStringWidth(sourceIpTxt.Text, sourceIpTxt.FontSize) / 2f), 0, 0, 0);

            buttonGrid.Children.Add(sourceIpTxt);

            //Dest address text
            TextBlock destIpTxt = new TextBlock();
            destIpTxt.TextWrapping = TextWrapping.Wrap;
            destIpTxt.Text = info.DestAddress;
            destIpTxt.FontSize = 16;
            destIpTxt.HorizontalAlignment = HorizontalAlignment.Left;
            destIpTxt.VerticalAlignment = VerticalAlignment.Top;
            //Center us under "Dest IP" text
            destIpTxt.Margin = new Thickness((liveDestIP.Margin.Left - 10) + (liveDestIPWidth / 2f) - (GetStringWidth(destIpTxt.Text, destIpTxt.FontSize) / 2f), 0, 0, 0);

            buttonGrid.Children.Add(destIpTxt);

            //Local port text
            TextBlock portTxt = new TextBlock();
            portTxt.TextWrapping = TextWrapping.Wrap;
            portTxt.Text = info.Port.ToString();
            portTxt.FontSize = 16;
            portTxt.HorizontalAlignment = HorizontalAlignment.Center;
            portTxt.VerticalAlignment = VerticalAlignment.Top;
            //Center us under "Local Port" text
            portTxt.Margin = new Thickness((liveLocalPortWidth / 2f) - (GetStringWidth(portTxt.Text, portTxt.FontSize) / 2f), 0, 0, 0);

            buttonGrid.Children.Add(portTxt);

            //Size text
            TextBlock sizeTxt = new TextBlock();
            sizeTxt.TextWrapping = TextWrapping.Wrap;
            sizeTxt.Text = String.Format("{0:n}", info.Size);
            sizeTxt.FontSize = 16;
            sizeTxt.HorizontalAlignment = HorizontalAlignment.Right;
            sizeTxt.VerticalAlignment = VerticalAlignment.Top;
            //Center us under "Size (KB)" text
            sizeTxt.Margin = new Thickness(0, 0, (liveSize.Margin.Right - 25) + (liveSizeWidth / 2f) - (GetStringWidth(sizeTxt.Text, sizeTxt.FontSize) / 2f), 0);

            buttonGrid.Children.Add(sizeTxt);

            //Protocol text
            TextBlock protocolTxt = new TextBlock();
            protocolTxt.TextWrapping = TextWrapping.Wrap;
            protocolTxt.Text = info.Protocol.ToString();
            protocolTxt.FontSize = 16;
            protocolTxt.HorizontalAlignment = HorizontalAlignment.Right;
            protocolTxt.VerticalAlignment = VerticalAlignment.Top;
            //Center us under "Protocol" text
            protocolTxt.Margin = new Thickness(0, 0, (liveProtocol.Margin.Right - 25) + (liveProtocolWidth / 2f) - (GetStringWidth(protocolTxt.Text, protocolTxt.FontSize) / 2f), 0);

            buttonGrid.Children.Add(protocolTxt);

            //Set button to have all three text fields as childrne
            button.Content = buttonGrid;
            //Add button to our connected devices scroll view.
            liveFeedStack.Children.Add(button);
        }

        #endregion
        
        /// <summary>
        /// Takes the given data in kilobytes, converts it to the best data unit, then outputs it as a formatted string with 2 decimal places.
        /// </summary>
        /// <param name="dataKb">Data measurement in KB</param>
        /// <returns>dataKb formatted properly</returns>
        private string FormatDataText(double dataKb)
        {
            string unit = "KB";
            if (dataKb >= 1000)
            {
                dataKb /= 1000;
                unit = "MB";
            }
            if (dataKb >= 1000)
            {
                dataKb /= 1000;
                unit = "GB";
            }

            return String.Format("{0:n} {1}", dataKb, unit);
        }

        /// <summary>
        /// Returns the width of the given string, using Segoe UI font
        /// </summary>
        /// <param name="str">String whose width we would like to calculate</param>
        /// <param name="fontSize">Fontsize to use for the string</param>
        /// <returns>Width the string will take on the UI using the Segoe UI font</returns>
        private double GetStringWidth(string str, double fontSize)
        {
            FontFamilyConverter converter = new FontFamilyConverter();


            FormattedText formattedText = new FormattedText(
                str,
                CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight,
                new Typeface((FontFamily)converter.ConvertFromString("Segoe UI"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
                fontSize, Brushes.Black);

            return formattedText.Width;
        }
    }
}
