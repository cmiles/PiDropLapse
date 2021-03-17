using System;
using System.Collections.Generic;
using System.Linq;
using Microcharts;
using PiDropUtility;
using SkiaSharp;
using Topten.RichTextKit;

namespace PiDropSimpleSensorReport
{
    public static class ChartImages
    {
        public static SKImage SensorDataChart(List<SensorReading> readings, SKColor readingColor)
        {
            var readingsToUse = readings.OrderByDescending(x => x.ReadingDateTime).Take(14)
                .OrderBy(x => x.ReadingDateTime).ToList();

            var chartData = readingsToUse
                .Select(x => new ChartEntry((float) Math.Round(x.ReadingValue, 0.0))
                {
                    Color = readingColor,
                    Label = $"{x.ReadingDateTime:M/dd HH}",
                    TextColor = readingColor,
                    ValueLabel = $"{x.ReadingValue:0.0}",
                    ValueLabelColor = readingColor
                }).ToList();

            var dataChart = new LineChart
            {
                Entries = chartData,
                IsAnimated = false,
                AnimationDuration = new TimeSpan(0),
                LabelTextSize = 28
            };

            var dataChartImage = new SKImageInfo(400, 600);
            var dataChartSurface = SKSurface.Create(dataChartImage);
            var dataChartCanvas = dataChartSurface.Canvas;
            dataChartCanvas.Clear();
            dataChartCanvas.RotateDegrees(270, 300, 300);

            dataChart.DrawContent(dataChartCanvas, 600, 400);
            dataChartCanvas.Save();

            return dataChartSurface.Snapshot();
        }

        public static SKImage SensorTitle(List<SensorReading> readings, SKColor readingColor)
        {
            var latestReading = readings.OrderByDescending(x => x.ReadingDateTime).First();

            var fitSize =
                TextHelpers.AutoFitRichStringWidth(
                    $"{latestReading.ReadingValue:0.0} - {latestReading.ReadingDateTime:M/dd HH}",
                    "Arial", 360, 40);

            var titleRichString = new RichString()
                .FontFamily("Arial")
                .TextColor(SKColors.White)
                .FontSize(fitSize)
                .TextColor(readingColor)
                .Add($"{latestReading.ReadingValue:0.0}")
                .TextColor(SKColors.Gray)
                .Add(" - ")
                .TextColor(SKColors.White)
                .Add($"{latestReading.ReadingTag}");

            var titleChartImage = new SKImageInfo(400, 50);
            var titleChartSurface = SKSurface.Create(titleChartImage);
            var titleChartCanvas = titleChartSurface.Canvas;

            var titleLeftStart = (400 - titleRichString.MeasuredWidth) / 2;
            titleRichString.Paint(titleChartCanvas, new SKPoint(titleLeftStart, 10));

            titleChartCanvas.Save();

            return titleChartSurface.Snapshot();
        }
    }
}