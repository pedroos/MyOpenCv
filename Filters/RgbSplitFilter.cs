using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace MyOpenCv
{
    using ImageProcessing;

    /// <summary>
    /// Decomposes an image's colors into a single color channel.
    /// </summary>
    [FilterParameter(typeof(ChannelOption), "channel")]
    public class RgbSplitFilter : InPlaceFilter
    {
        /// <summary>
        /// Supported single-color channels for the RgbSplitFilter.
        /// </summary>
        public enum ChannelOption : byte
        {
            Red, Green, Blue
        }

        /// <summary>
        /// The name of the filter.
        /// </summary>
        public override string Name { get { return "Rgb split"; } }

        /// <param name="bitmap">The target image</param>
        /// <returns>True if the image is useable with this filter; False otherwise</returns>
        public override bool BitmapCondition(Bitmap bitmap)
        {
            return !((bitmap.Flags & (int)ImageFlags.HasTranslucent) == (int)ImageFlags.HasTranslucent);
        }

        /// <param name="bitmap">The input bitmap</param>
        /// <param name="outBitmap">The output bitmap</param>
        public override void Process(Bitmap bitmap)
        {
            var channel = GetParameter<ChannelOption>("channel");
            
            for (int y = 0; y < bitmap.Height; ++y)
            {
                for (int x = 0; x < bitmap.Width; ++x)
                {
                    var color = bitmap.GetPixel(x, y);
                    var newColor =
                        channel == ChannelOption.Blue ? Color.FromArgb(0, 0, color.B) :
                        channel == ChannelOption.Red ? Color.FromArgb(color.R, 0, 0) :
                        Color.FromArgb(0, color.G, 0);
                    bitmap.SetPixel(x, y, newColor);
                }
            }
        }
    }
}
