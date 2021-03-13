using System;
using System.Drawing;

namespace PiDropLapse
{
    public static class ImageHelpers
    {
        /// <summary>
        ///     Returns a font (inside the specified range) that is the largest font
        ///     that will fit inside the given width.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="stringToDraw"></param>
        /// <param name="referenceFont"></param>
        /// <param name="maxWidth"></param>
        /// <param name="minimumFontSize"></param>
        /// <param name="maximumFontSize"></param>
        /// <returns></returns>
        public static Font TryAdjustFontSizeToFitWidth(Graphics g, string stringToDraw, Font referenceFont,
            int maxWidth, int minimumFontSize, int maximumFontSize)
        {
            // ReSharper disable once CommentTypo
            //Based on https://docs.microsoft.com/en-us/previous-versions/bb986765(v=msdn.10)?redirectedfrom=MSDN 
            //with thanks to https://stackoverflow.com/questions/15571715/auto-resize-font-to-fit-rectangle/30567857 for the link;

            Font testFont = new(referenceFont.Name, referenceFont.Size, referenceFont.Style);

            for (var testSize = maximumFontSize; testSize >= minimumFontSize; testSize--)
            {
                testFont = new Font(referenceFont.Name, testSize, referenceFont.Style);

                // Test the string with the new size
                var adjustedSizeNew = g.MeasureString(stringToDraw, testFont);

                if (maxWidth > Convert.ToInt32(adjustedSizeNew.Width))
                    // First font to fit - return it
                    return testFont;
            }

            return testFont;
        }
    }
}