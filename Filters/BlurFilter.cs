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
    /// Applies a blur filter to a image.
    /// </summary>
    [FilterParameter(typeof(int), "radius", defaultValue: 1)]
    [FilterParameter(typeof(int), "weight", defaultValue: 1)]
    public class BlurFilter : CopyFilter
    {
        /// <summary>
        /// The name of the filter.
        /// </summary>
        public override string Name { get { return "Blur"; } }

        /// <param name="bitmap">The target image</param>
        /// <returns>True if the image is useable with this filter; False otherwise</returns>
        public override bool BitmapCondition(Bitmap bitmap)
        {
            return true;
        }

        /// <param name="bitmap">The input bitmap</param>
        /// <param name="outBitmap">The output bitmap</param>
        public override void Process(Bitmap bitmap, out Bitmap outBitmap)
        {
            var radius = GetParameter<int>("radius");
            var weight = GetParameter<int>("weight");

            var newBitmap = new Bitmap(bitmap.Width, bitmap.Height);
            int side = radius * 2 + 1;
            int middle = (int)Math.Ceiling((float)side / 2);
            Color[,] avg = new Color[side, side];
            int channelMean(Color channel)
            {
                int num = 0, den = 0;
                for (int y = 0; y < side; ++y)
                {
                    for (int x = 0; x < side; ++x)
                    {
                        bool isMiddle = x == middle && y == middle;
                        var color = avg[x, y];
                        byte atChannel = channel == Color.Red ? color.R : channel == Color.Blue ? color.B : 
                            color.G;
                        num += isMiddle ? atChannel * weight : atChannel;
                        den += isMiddle ? weight : 1;
                    }
                }
                return (int)Math.Round((float)num / den);
            }
            for (int y = 0; y < bitmap.Height; ++y)
            {
                for (int x = 0; x < bitmap.Height; ++x)
                {
                    for (int y2 = radius * -1; y2 <= radius; ++y2)
                    {
                        for (int x2 = radius * -1; x2 <= radius; ++x2)
                        {
                            int xOff = x + x2, yOff = y + y2;
                            bool outOfBounds = xOff < 0 || xOff > bitmap.Width - 1 || yOff < 0 || 
                                yOff > bitmap.Height - 1;
                            Color value = !outOfBounds ? bitmap.GetPixel(x + x2, y + y2) : default;
                            avg[y2 + radius, x2 + radius] = value;
                        }
                    }
                    var newColor = Color.FromArgb(channelMean(Color.Red), channelMean(Color.Green), 
                        channelMean(Color.Blue));
                    newBitmap.SetPixel(x, y, newColor);
                }
            }
            outBitmap = newBitmap;
        }
    }
}
