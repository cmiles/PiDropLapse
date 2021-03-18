<Query Kind="Statements">
  <Connection>
    <ID>7412b080-efbd-480c-ba71-3da81a49efec</ID>
    <NamingServiceVersion>2</NamingServiceVersion>
    <Persist>true</Persist>
    <Driver Assembly="(internal)" PublicKeyToken="no-strong-name">LINQPad.Drivers.EFCore.DynamicDriver</Driver>
    <Database>PiDropLapse.db</Database>
    <DisplayName>PiDropGraphTest</DisplayName>
    <AttachFileName>C:\Code\PiDropLapse02\TestFiles\PiDropLapse.db</AttachFileName>
    <DriverData>
      <EFProvider>Microsoft.EntityFrameworkCore.Sqlite</EFProvider>
    </DriverData>
  </Connection>
  <NuGetReference>Microcharts</NuGetReference>
  <NuGetReference>SkiaSharp</NuGetReference>
  <NuGetReference>Topten.RichTextKit</NuGetReference>
  <Namespace>SkiaSharp</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>Topten.RichTextKit</Namespace>
  <Namespace>Microcharts</Namespace>
</Query>

Directory.SetCurrentDirectory (Path.GetDirectoryName (Util.CurrentQueryPath));

var groupedReadings = SensorReadings.ToList().GroupBy(x => x.ReadingTag).ToList();

var temperatureReadings = groupedReadings.Single(x => x.Key == "Pressure in mb");

var readingsToUse = temperatureReadings.OrderByDescending(x => x.ReadingDateTime).Take(14).OrderBy(x => x.ReadingDateTime).ToList();

var temperatureMicroChartData = readingsToUse
	.Select(loopEntries => new ChartEntry((float)loopEntries.ReadingValue)
	{
		Color = SKColors.Red,
		Label = $"{DateTime.Parse(loopEntries.ReadingDateTime):MM/dd HH}",
		TextColor = SKColors.Red,
		ValueLabel = $"{loopEntries.ReadingValue:0.0}",
		ValueLabelColor = SKColors.Red
	}).ToList();

var microChart = new Microcharts.PointChart()
{
	Entries = temperatureMicroChartData,
	IsAnimated = false,
	LabelTextSize = 28,
};

SKImageInfo mainTestImage = new SKImageInfo(400, 800);
using SKSurface mainTestSurface = SKSurface.Create(mainTestImage);
SKCanvas mainTestCanvas = mainTestSurface.Canvas;
mainTestCanvas.Clear(SKColors.Black);

SKImageInfo chartTestImage = new SKImageInfo(400, 600);
using SKSurface chartTestSurface = SKSurface.Create(chartTestImage);
SKCanvas chartTestCanvas = chartTestSurface.Canvas;
chartTestCanvas.RotateDegrees(270, 300, 300);

microChart.DrawContent(chartTestCanvas, 600, 400);

chartTestCanvas.Save();

float AutoFitRichStringWidth(string richString, string fontFamily, int maxWidth, int maxHeight)
{
	var fontSize = 1;
	
	var testString = new RichString()
		.FontFamily(fontFamily)
		.FontSize(fontSize)
		.Add(richString);

	while (testString.MeasuredWidth < maxWidth && testString.MeasuredHeight < maxHeight)
	{
		testString = new RichString()
			.FontFamily(fontFamily)
			.FontSize(++fontSize)
			.Add(richString);
	}
	
	return fontSize;
}

var fitSize = AutoFitRichStringWidth("20.0 - Temperature in F", "Arial", 360, 40);

var rs = new RichString()
	.TextColor(SKColors.White)
	.FontFamily("Arial")
	.FontSize(fitSize)
	.TextColor(SKColors.Red)
	.Add("20.0")
	.TextColor(SKColors.White)
	.Add(" - Temperature in F");

var titleLeftStart = (400 - rs.MeasuredWidth) / 2;
rs.Paint(mainTestCanvas, new SKPoint(titleLeftStart, 10));

mainTestCanvas.DrawImage(chartTestSurface.Snapshot(), 0, 50);
mainTestCanvas.Save();

var tempFile = Path.Combine(Path.GetDirectoryName (Util.CurrentQueryPath), $"imagetemp-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.jpg");

using var imageData = mainTestSurface.Snapshot().Encode(SKEncodedImageFormat.Jpeg, 80);
await using var imageStream = 
	File.OpenWrite(tempFile);
imageData.SaveTo(imageStream);

LINQPad.Extensions.Dump(Util.Image(tempFile));
