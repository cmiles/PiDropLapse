﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microcharts;
using Pluralize.NET;
using SkiaSharp;
using Topten.RichTextKit;

namespace PiDropSimpleSensorReport
{
    public static class Helpers
    {
        public static float AutoFitRichStringWidth(string richString, string fontFamily, int maxWidth, int maxHeight)
        {
            var fontSize = 1;

            var testString = new RichString()
                .FontFamily(fontFamily)
                .FontSize(fontSize)
                .Add(richString);

            while (testString.MeasuredWidth < maxWidth && testString.MeasuredHeight < maxHeight)
                testString = new RichString()
                    .FontFamily(fontFamily)
                    .FontSize(++fontSize)
                    .Add(richString);

            return fontSize;
        }

        public static string PluralizeIfNeeded(this string toPluralizeIfNeeded, IList pluralizeIfMoreThanOne)
        {
            if (pluralizeIfMoreThanOne == null || pluralizeIfMoreThanOne.Count < 1) return toPluralizeIfNeeded;
            var toPlural = new Pluralizer();
            return toPlural.Pluralize(toPluralizeIfNeeded);
        }

        public static SKImage SensorDataChart(List<SensorReading> readings, SKColor readingColor)
        {
            var readingsToUse = readings.OrderBy(x => x.ReadingDateTime).Take(14).ToList();
            var chartData = readingsToUse
                .Select(x => new ChartEntry((float) x.ReadingValue)
                {
                    Color = SKColors.Red,
                    Label = $"{x.ReadingDateTime:M/dd HH}",
                    ValueLabel = $"{x.ReadingValue:0.0}",
                    ValueLabelColor = readingColor
                }).ToList();

            var dataChart = new LineChart
            {
                Entries = chartData,
                IsAnimated = false,
                LabelTextSize = 28
            };

            var dataChartImage = new SKImageInfo(400, 600);
            var dataChartSurface = SKSurface.Create(dataChartImage);
            var dataChartCanvas = dataChartSurface.Canvas;
            dataChartCanvas.RotateDegrees(270, 300, 300);

            dataChart.DrawContent(dataChartCanvas, 600, 400);
            dataChartCanvas.Save();

            return dataChartSurface.Snapshot();
        }

        public static SKImage SensorTitle(List<SensorReading> readings, SKColor readingColor)
        {
            var readingsToUse = readings.OrderBy(x => x.ReadingDateTime).Take(12).ToList();
            var latestReading = readingsToUse.First();

            var fitSize =
                AutoFitRichStringWidth($"{latestReading.ReadingValue:0.0} - {latestReading.ReadingDateTime:M/dd HH}",
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