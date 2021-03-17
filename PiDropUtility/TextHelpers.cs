using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pluralize.NET;
using Topten.RichTextKit;

namespace PiDropUtility
{
    public static class TextHelpers
    {


        public static string PluralizeIfNeeded(this string toPluralizeIfNeeded, IList pluralizeIfMoreThanOne)
        {
            if (pluralizeIfMoreThanOne == null || pluralizeIfMoreThanOne.Count < 1) return toPluralizeIfNeeded;
            var toPlural = new Pluralizer();
            return toPlural.Pluralize(toPluralizeIfNeeded);
        }

        public static float AutoFitRichStringWidth(string stringContents, string fontFamily, int maxWidth, int maxHeight)
        {
            var fontSize = 1;

            var testString = new RichString()
                .FontFamily(fontFamily)
                .FontSize(fontSize)
                .Add(stringContents);

            while (testString.MeasuredWidth < maxWidth && testString.MeasuredHeight < maxHeight)
                testString = new RichString()
                    .FontFamily(fontFamily)
                    .FontSize(++fontSize)
                    .Add(stringContents);

            return fontSize;
        }
    }
}
