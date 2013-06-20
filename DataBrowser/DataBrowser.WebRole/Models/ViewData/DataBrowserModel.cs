using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Web.Mvc;
using System.Web.UI.DataVisualization.Charting;

namespace Ogdi.InteractiveSdk.Mvc.Models
{
	public class DataBrowserModel
	{
		private char isBookmarked;

		public char IsBookmarked
		{
			get { return isBookmarked; }
			set { isBookmarked = value; }
		}

		private short visibleView;

		public short VisibleView
		{
			get { return visibleView; }
			set { visibleView = value; }
		}

		private string sampleCodeDataView;

		public string SampleCodeDataView
		{
			get { return sampleCodeDataView; }
			set { sampleCodeDataView = value; }
		}

		private string filterText;

		public string FilterText
		{
			get { return filterText; }
			set { filterText = value; }
		}

		private string sampleCodeMapView;

		public string SampleCodeMapView
		{
			get { return sampleCodeMapView; }
			set { sampleCodeMapView = value; }
		}

		private string sampleCodeBarChartView;

		public string SampleCodeBarChartView
		{
			get { return sampleCodeBarChartView; }
			set { sampleCodeBarChartView = value; }
		}

		private string sampleCodePieChartView;

		public string SampleCodePieChartView
		{
			get { return sampleCodePieChartView; }
			set { sampleCodePieChartView = value; }
		}

		private short visibleTag;

		public short VisibleTag
		{
			get { return visibleTag; }
			set { visibleTag = value; }
		}

		private int zoomLevel;

		public int ZoomLevel
		{
			get { return zoomLevel; }
			set { zoomLevel = value; }
		}

		private string container;

		public string Container
		{
			get { return container; }
			set { container = value; }
		}

		private string entitySetName;

		public string EntitySetName
		{
			get { return entitySetName; }
			set { entitySetName = value; }
		}

		public EntitySetDetails EntitySetDetails { get; set; }

		public EntitySetWrapper EntitySetWrapper { get; set; }

		private string dBErrorLine1;

		public string DBErrorLine1
		{
			get { return dBErrorLine1; }
			set { dBErrorLine1 = value; }
		}

		private string dBErrorLine2;

		public string DBErrorLine2
		{
			get { return dBErrorLine2; }
			set { dBErrorLine2 = value; }
		}

		private string baseQueryName;

		public string BaseQueryName
		{
			get { return baseQueryName; }
			set { baseQueryName = value; }
		}
		
		private string filteredQueryName;

		public string FilteredQueryName
		{
			get { return filteredQueryName; }
			set { filteredQueryName = value; }
		}

		private bool nextEnable;

		public bool NextEnable
		{
			get { return nextEnable; }
			set { nextEnable = value; }
		}
		
		private bool prevEnable;

		public bool PrevEnable
		{
			get { return prevEnable; }
			set { prevEnable = value; }
		}
			
		private DataTable tableBrowserData;

		public DataTable TableBrowserData
		{
			get { return tableBrowserData; }
			set { tableBrowserData = value; }
		}

		private SelectList dataViewLanguages;

		public SelectList DataViewLanguages
		{
			get { return dataViewLanguages; }
			set { dataViewLanguages = value; }
		}

		private double longitude;

		public double Longitude
		{
			get { return longitude; }
			set { longitude = value; }
		}

		private double latitude;

		public double Latitude
		{
			get { return latitude; }
			set { latitude = value; }
		}

		private string mapStyle;

		public string MapStyle
		{
			get { return mapStyle; }
			set { mapStyle = value; }
		}

		private int mapMode;

		public int MapMode
		{
			get { return mapMode; }
			set { mapMode = value; }
		}

		private long sceneId;

		public long SceneId
		{
			get { return sceneId; }
			set { sceneId = value; }
		}

		private string birdseyeSceneOrientation;

		public string BirdseyeSceneOrientation
		{
			get { return birdseyeSceneOrientation; }
			set { birdseyeSceneOrientation = value; }
		}

		private SelectList mapViewLanguages;

		public SelectList MapViewLanguages
		{
			get { return mapViewLanguages; }
			set { mapViewLanguages = value; }
		}

		private Dictionary<string, double> chart;

		public Dictionary<string, double> Chart
		{
			get { return chart; }
			set { chart = value; }
		}

		private SelectList barHorizontal;

		public SelectList BarHorizontal
		{
			get { return barHorizontal; }
			set { barHorizontal = value; }
		}

		private SelectList barVertical;

		public SelectList BarVertical
		{
			get { return barVertical; }
			set { barVertical = value; }
		}

		private SelectList barDateRange;

		public SelectList BarDateRange
		{
			get { return barDateRange; }
			set { barDateRange = value; }
		}

		private string barYOption;

		public string BarYOption
		{
			get { return barYOption; }
			set { barYOption = value; }
		}

		private string barYColOption;

		public string BarYColOption
		{
			get { return barYColOption; }
			set { barYColOption = value; }
		}

		private bool isSelectOne;

		public bool IsSelectOne
		{
			get { return isSelectOne; }
			set { isSelectOne = value; }
		}
		
		private SelectList barChartViewLanguages;

		public SelectList BarChartViewLanguages
		{
			get { return barChartViewLanguages; }
			set { barChartViewLanguages = value; }
		}
		
		private SelectList pieHorizontal;

		public SelectList PieHorizontal
		{
			get { return pieHorizontal; }
			set { pieHorizontal = value; }
		}
				
		private SelectList pieVertical;

		public SelectList PieVertical
		{
			get { return pieVertical; }
			set { pieVertical = value; }
		}
		
		private SelectList pieDateRange;

		public SelectList PieDateRange
		{
			get { return pieDateRange; }
			set { pieDateRange = value; }
		}
		
		private string pieYOption;

		public string PieYOption
		{
			get { return pieYOption; }
			set { pieYOption = value; }
		}

		private string pieYColOption;

		public string PieYColOption
		{
			get { return pieYColOption; }
			set { pieYColOption = value; }
		}
		
		private SelectList pieChartViewLanguages;

		public SelectList PieChartViewLanguages
		{
			get { return pieChartViewLanguages; }
			set { pieChartViewLanguages = value; }
		}

		private string xColName;

		public string XColName
		{
			get { return xColName; }
			set { xColName = value; }
		}
		
		private string yColName;

		public string YColName
		{
			get { return yColName; }
			set { yColName = value; }
		}
		
		private int xCount;

		public int XCount
		{
			get { return xCount; }
			set { xCount = value; }
		}

		public bool IsPlanned
		{
			get { return EntitySetWrapper.EntitySet.IsEmpty; }
		}

        public Chart BarChart
        {
            get
            {
                const int ChartMaxWidth = 860;
                const int ChartMaxHeight = 500;

                var fontFamily = new FontFamily("Calibri");

                //  Create a new instance of the Chart class at run time
                //  However, for simplicity, it is recommended that 
                //  you create a Chart instance at design time.
                //  This is the root object of the Chart control.
                var OgdiBarChart = new Chart();

                //  Number of elements on X-axis
                int calculatedWidth = Convert.ToInt32(this.XCount) * 15;

                //  Set Bar Chart width to Calculated width
                OgdiBarChart.Width = (calculatedWidth > ChartMaxWidth) ? calculatedWidth : ChartMaxWidth;
                OgdiBarChart.Height = ChartMaxHeight;

                //  Set Unique ID for Bar Chart
                OgdiBarChart.ID = "imgBarChart";

                //  Specify how an image of the chart will be rendered. 
                //  BinaryStreaming --> Chart is streamed directly to the client. 
                //  ImageMap --> Chart is rendered as an image map. 
                //  ImageTag --> Chart is rendered using an HTML image tag. 
                OgdiBarChart.RenderType = RenderType.ImageTag;

                //  Set the chart rendering type.
                //  This property defines the type of storage used to render chart images.
                OgdiBarChart.ImageStorageMode = ImageStorageMode.UseHttpHandler;

                //  Set the palette for the Chart control. 
                //  Berry --> utilizes blues and purples. 
                //  Bright  --> utilizes bright colors. 
                //  BrightPastel  --> utilizes bright pastel colors. 
                //  Chocolate  --> utilizes shades of brown. 
                //  EarthTones  --> utilizes earth tone colors such as
                //                  green and brown. 
                //  Excel  --> utilizes Excel-style colors. 
                //  Fire  --> utilizes red, orange and yellow colors. 
                //  Grayscale  --> utilizes grayscale colors, that is, 
                //              shades of black and white. 
                //  Light --> utilizes light colors. 
                //  None --> No palette is used.  
                //  Pastel --> utilizes pastel colors. 
                //  SeaGreen --> utilizes colors that range from green to blue. 
                //  SemiTransparent --> utilizes semi-transparent colors. 
                OgdiBarChart.Palette = ChartColorPalette.BrightPastel;

                //  Add a Title object to the end of the title collection. 
                OgdiBarChart.Titles.Add(new Title(this.EntitySetName,
                                        Docking.Top,
                                        new Font(fontFamily, 14, FontStyle.Bold), Color.FromArgb(26, 59, 105)));

                //  Create a ChartArea class object which represents a 
                // chart area on the chart image. 
                var chartArea = new ChartArea("ColumnChartArea");

                //  Create a Series class object which stores data points 
                // and series attributes. 
                var series = new Series("Series 1");

                //  Set an Axis object that represents the primary X-axis.

                Axis XAxis = chartArea.AxisX;

                XAxis.Title = this.XColName;

                XAxis.TitleFont = new Font(fontFamily, 12, FontStyle.Bold);
                XAxis.LabelStyle.Font = new Font(fontFamily, 9);

                XAxis.IntervalType = DateTimeIntervalType.Auto;
                XAxis.IsLabelAutoFit = true;
                XAxis.Interval = 1;

                //  Set an Axis object that represents the primary Y-axis.

                Axis YAxis = chartArea.AxisY;

                YAxis.Title = this.YColName;
                YAxis.TitleFont = new Font(fontFamily, 12, FontStyle.Bold);
                YAxis.LabelStyle.Font = new Font(fontFamily, 9);
                YAxis.IsLabelAutoFit = true;

                //  This "Position" property defines the position of a ChartArea 
                // object within the Chart.    

                ElementPosition position = chartArea.Position;

                position.Width = 98;   //  this value is in percent 
                position.Height = 90;     //  this value is in percent
                position.Y = 8;        //  this value is in percent

                //  Add a ChartArea object to the  ChartAreas collection. 
                OgdiBarChart.ChartAreas.Add(chartArea);

                //  Set the chart type of a series. 
                series.ChartType = SeriesChartType.Column;

                //  Set the value type plotted along the X-axis to String
                series.XValueType = ChartValueType.String;

                //  Set the value type plotted along the Y-axis to Double
                series.YValueType = ChartValueType.Double;

                //  Set the font of the data point. 
                series.Font = new Font(fontFamily, 8, FontStyle.Bold);

                //  Set a flag that indicates whether to show the value of 
                //  the data point on the label.
                series.IsValueShownAsLabel = true;

                series.Label = "#VALY{#,0}";

                //  Set the tooltip
                series.ToolTip = "'#VALX' with value '#VALY{#,0}'";

                //  Add a Series object to the  Series collection. 
                OgdiBarChart.Series.Add(series);

                //  If there is data for representing chart in Viewdata then ...
                if (this.Chart != null)
                {
                    //  For each XY value pair do ...   
                    foreach (KeyValuePair<string, double> XyPair in this.Chart)
                    {
                        //  Add a DataPoint object to the end of the collection,
                        //  with the specified X-value and Y-value. 
                        OgdiBarChart.Series[0].Points.AddXY(

                            //  If key is empty then put xValue = <<BLANK>>...
                            XyPair.Key.Trim().Equals(string.Empty) ? "<<BLANK>>" :

                            //  else if Key length is greater than 20 then xValue becomes 
                            //  first 18 characters appeneded with two dots
                            //  else if Key length is less than or equal to 20 then 
                            //  xValue will be considered as entire value of key
                            XyPair.Key.Length > 20 ?
                                XyPair.Key.Substring(0, 18) + ".." : XyPair.Key,
                            //  yValue is assigned to value in Xypair
                                XyPair.Value);
                    }
                }

                /*
                //  Set a BorderSkin object, which provides border skin
                // functionality for the Chart control.
                OgdiBarChart.BorderSkin.SkinStyle = BorderSkinStyle.Emboss;

                //  Set BorderColor
                OgdiBarChart.BorderColor = Color.FromArgb(26, 59, 105);

                //  Set the style of the entire chart image border line.
                OgdiBarChart.BorderlineDashStyle = ChartDashStyle.Solid;

                //  Set BorderWidth
                OgdiBarChart.BorderWidth = 2;
                 * */

                return OgdiBarChart;
            }
        }

        public Chart PieChart
        {
            get
            {
                var OgdiPieChart = new Chart();
                int calculatedHeight = 650;
                int count = Convert.ToInt32(this.XCount);

                if (count >= 0 && count <= 30)
                {
                    OgdiPieChart.Height = 650;
                }
                else if (count > 30 && count <= 60)
                {
                    calculatedHeight = count * 16;
                    OgdiPieChart.Height = calculatedHeight;
                }
                else if (count > 60 && count <= 90)
                {
                    calculatedHeight = count * 8;
                    OgdiPieChart.Height = calculatedHeight;
                }
                else if (count > 90)
                {
                    calculatedHeight = count * 7;
                    OgdiPieChart.Height = calculatedHeight;
                }

                calculatedHeight += 150;
                OgdiPieChart.Width = 880;

                //  Set Unique ID for Pie Chart
                OgdiPieChart.ID = "imgPieChart";

                //  Specify how an image of the chart will be rendered. 
                //  BinaryStreaming --> Chart is streamed directly to the client. 
                //  ImageMap --> Chart is rendered as an image map. 
                //  ImageTag --> Chart is rendered using an HTML image tag. 
                OgdiPieChart.RenderType = RenderType.ImageTag;

                //  Set the chart rendering type.
                //  This property defines the type of storage used to render chart images.
                OgdiPieChart.ImageStorageMode = ImageStorageMode.UseHttpHandler;

                var t = new Title(this.EntitySetName,
                    Docking.Top, new Font("Calibri", 14,
                        FontStyle.Bold),
                        Color.FromArgb(26, 59, 105));

                //  Add a Title object to the end of the title collection. 
                OgdiPieChart.Titles.Add(t);

                //  Create a ChartArea class object which represents a 
                //  chart area on the chart image. 
                var chartArea = new ChartArea("ColumnChartArea");

                //  Create a Series class object which stores data points 
                //  and series attributes. 
                var series = new Series("Series 1");

                //  Add a ChartArea object to the  ChartAreas collection. 
                OgdiPieChart.ChartAreas.Add(chartArea);

                //  Set the chart type of a series. 
                series.ChartType = SeriesChartType.Pie;

                //  Set the value type plotted along the Y-axis to Double
                series.YValueType = ChartValueType.Double;

                //  Set the tooltip             
                series.ToolTip = "'#VALX' : '#VALY'";

                series.Label = "#PERCENT{P2}";
                series.LabelToolTip = "'#VALX' : '#VALY'";
                series.LegendText = "'#VALX' : #PERCENT{P2}";
                series.LegendToolTip = "'#VALX' : '#VALY'";
                series["PieLabelStyle"] = "Disabled";

                //  Add a Series object to the  Series collection. 
                OgdiPieChart.Series.Add(series);

                if (this.Chart != null)
                {
                    foreach (KeyValuePair<string, double> XyPair in this.Chart)
                    {
                        OgdiPieChart.Series[0].Points.AddXY(XyPair.Key.Trim().
                            Equals(string.Empty) ? "<<BLANK>>" :
                            XyPair.Key.Length > 30 ? XyPair.Key.Substring(0, 28) + ".."
                            : XyPair.Key, XyPair.Value);
                    }
                }

                /*
                //  Set a BorderSkin object, which provides border skin
                // functionality for the Chart control.
                OgdiPieChart.BorderSkin.SkinStyle = BorderSkinStyle.Emboss;

                //  Set BorderColor
                OgdiPieChart.BorderColor = Color.FromArgb(26, 59, 105);

                //  Set the style of the entire chart image border line.
                OgdiPieChart.BorderlineDashStyle = ChartDashStyle.Solid;

                //  Set BorderWidth
                OgdiPieChart.BorderWidth = 2;
                 * */

                OgdiPieChart.Legends.Add("Legend1");
                OgdiPieChart.Legends[0].Docking = Docking.Bottom;
                OgdiPieChart.Legends[0].Alignment = StringAlignment.Center;
                OgdiPieChart.Legends[0].LegendStyle = LegendStyle.Table;
                OgdiPieChart.Legends[0].IsTextAutoFit = false;
                OgdiPieChart.Legends[0].TableStyle = LegendTableStyle.Wide;
                OgdiPieChart.Legends[0].TextWrapThreshold = 80;

                return OgdiPieChart;
            }
        }
	}
}
