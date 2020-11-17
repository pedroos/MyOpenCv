using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;

namespace MyOpenCv
{
    using ImageProcessing;

    /// <summary>
    /// Provides useful functions to the command-line interface of MyOpenCv.
    /// </summary>
    public static class App
    {
        readonly static Dictionary<int, Filter> loadedFilters = new Dictionary<int, Filter>();
        
        /// <summary>
        /// Loads or reloads the current filters from the available filters Configuration section.
        /// Each filter is automatically assigned an increasing id (of integer type) to be used as reference 
        /// from the user input.
        /// </summary>
        /// <remarks>
        /// Loaded filter configuration is present in the file App.config.
        /// This method should be re-called every time the filter configuration changes.
        /// </remarks>
        internal static void LoadConfiguredFilters()
        {
            loadedFilters.Clear();
            int id = 0;
            string[] loadFilters = ConfigurationManager.AppSettings["loadFilters"].Split(',');
            foreach (string availableFilter in loadFilters)
            {
                var type = Type.GetType(availableFilter.Trim(), false, true);
                if (type == null) 
                    throw new ConfigurationErrorsException(string.Format("Filter not found: {0}",
                        availableFilter));
                if (!type.IsSubclassOf(typeof(Filter)))
                    throw new ConfigurationErrorsException(string.Format("'{0}' is not a subclass of 'Filter'",
                        availableFilter));
                var filter = (Filter)Activator.CreateInstance(type);
                loadedFilters.Add(++id, filter);
            }
        }

        /// <summary>
        /// Returns the filters which are loaded and are applicable to the specified image.
        /// </summary>
        /// <param name="bitmap">The target image</param>
        /// <returns>A list of applicable filters</returns>
        internal static IEnumerable<(int Id, Filter Filter)> FiltersForImage(Bitmap bitmap)
        {
            return loadedFilters.Where(f => f.Value.BitmapCondition(bitmap)).Select(f => (f.Key, f.Value));
        }

        /// <summary>
        /// Uses a loaded filter by id.
        /// </summary>
        /// <param name="id">The automatically generated filter id</param>
        /// <returns>The filter.</returns>
        internal static Filter Filter(int id)
        {
            if (!loadedFilters.ContainsKey(id))
                throw new ArgumentException(string.Format("Filter id not found: {0}", id));
            return loadedFilters[id];
        }

        /// <summary>
        /// Loads a bitmap image from a file path.
        /// </summary>
        /// <param name="fileName">The file path</param>
        /// <returns>A bitmap loaded with the image contents</returns>
        internal static Bitmap LoadBitmap(string fileName)
        {
            if (!File.Exists(fileName)) throw new FileNotFoundException();
            var fileInfo = new FileInfo(fileName);
            var fs = fileInfo.OpenRead();
            Bitmap bitmap = new Bitmap(fs);
            return bitmap;
        }

        /// <summary>
        /// Parses a string into a filter parameter value.
        /// </summary>
        /// <typeparam name="T">The type of the filter parameter</typeparam>
        /// <param name="input">The input string</param>
        /// <returns>The value in case of success; null (or the default value) in case of failure.</returns>
        public static object To<T>(this string input)
        {
            if (typeof(T).IsAssignableFrom(typeof(int)))
            {
                if (!int.TryParse(input, out int value)) return default;
                return value;
            }
            if (typeof(T).IsAssignableFrom(typeof(float)))
            {
                if (!float.TryParse(input, out float value)) return default;
                return value;
            }
            if (typeof(T) == typeof(RgbSplitFilter.ChannelOption))
            {
                if (!Enum.TryParse(input, out RgbSplitFilter.ChannelOption channelOption)) return default;
                return channelOption;
            }
            if (typeof(T).IsAssignableFrom(typeof(Color)))
            {
                string pattern = @"^(?:(?:^|,\s*)([01]?\d\d?|2[0-4]\d|25[0-5])){3}$";
                var match = Regex.Match(input, pattern);
                if (!match.Success) return default;
                return Color.FromArgb(
                    int.Parse(match.Groups[1].Captures[0].Value),
                    int.Parse(match.Groups[1].Captures[1].Value),
                    int.Parse(match.Groups[1].Captures[2].Value)
                );
            }
            return default;
        }
    }
}
