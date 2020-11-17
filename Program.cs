using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Configuration;

namespace MyOpenCv
{
    using ImageProcessing;

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // Load the specified image
            if (!(args.Length > 0))
            {
                Console.WriteLine("Please specify an image argument");
                Console.ReadKey();
                return;
            }
            string imageFile = args.First();
            if (!Path.IsPathRooted(imageFile)) 
                imageFile = Path.Combine(Directory.GetCurrentDirectory(), imageFile);

            if (!File.Exists(imageFile))
            {
                Console.WriteLine("File not found: " + imageFile);
                Console.ReadKey();
                return;
            }

            var bitmap = App.LoadBitmap(imageFile);

            while (true)
            {
                Console.WriteLine(Environment.NewLine + Environment.NewLine + "Image: {0}" + Environment.NewLine, 
                    imageFile);

                // Load the configured filters
                try
                {
                    App.LoadConfiguredFilters();
                }
                catch (ConfigurationErrorsException ex)
                {
                    Console.WriteLine("Error loading available filters:" + Environment.NewLine);
                    Console.WriteLine(ex.Message);
                    Console.ReadKey();
                    return;
                }

                // Check applicable filters for the image kind
                var filtersForImage = App.FiltersForImage(bitmap);

                // Checks whether a filter char corresponds to a valid filter id and the filter is applicable to the 
                // current image
                bool validFilterStr(string arg, out Filter f)
                {
                    f = default;
                    if (!int.TryParse(arg, out int id)) return false;
                    try
                    {
                        f = App.Filter(id);
                    }
                    catch (ArgumentException ex)
                    {
                        Console.WriteLine(ex.Message);
                        return false;
                    }
                    if (!filtersForImage.Any(ffi => ffi.Id == id))
                    {
                        Console.WriteLine("The filter is not valid for this image type.");
                        return false;
                    }
                    return true;
                }

                // Checks input filter chars until a match is made
                if (!(args.Length > 1) || !validFilterStr(args[1], out Filter filter))
                {
                    Console.WriteLine("Available filters:" + Environment.NewLine);
                    while (true)
                    {
                        Console.WriteLine(string.Join(Environment.NewLine, filtersForImage.Select(ffi =>
                            string.Format(" {0} {1}", ffi.Id, ffi.Filter))) + Environment.NewLine);
                        char key = Console.ReadKey().KeyChar;
                        Console.Write(Environment.NewLine + Environment.NewLine);
                        if (char.IsNumber(key) && validFilterStr(key.ToString(), out filter))
                            break;
                        Console.WriteLine("Please select a filter number:" + Environment.NewLine);
                    }
                }

                Console.WriteLine("Selected filter: {0}" + Environment.NewLine, filter.Name);

                string example(Type parameterType)
                {
                    return
                        parameterType.IsAssignableFrom(typeof(int)) ? "1" :
                        parameterType.IsAssignableFrom(typeof(float)) ? "1.0" :
                        parameterType.IsAssignableFrom(typeof(Color)) ? "100,200,100" :
                        parameterType.IsAssignableFrom(typeof(RgbSplitFilter.ChannelOption)) ? "Red" :
                        "";
                }

                object parsed(Type parameterType, string input)
                {
                    return
                        parameterType.IsAssignableFrom(typeof(int)) ? input.To<int>() :
                        parameterType.IsAssignableFrom(typeof(float)) ? input.To<float>() :
                        parameterType.IsAssignableFrom(typeof(Color)) ? input.To<Color>() :
                        parameterType.IsAssignableFrom(typeof(RgbSplitFilter.ChannelOption)) ? 
                            input.To<RgbSplitFilter.ChannelOption>() :
                        default;
                }

                // Ask for filter parameter values
                foreach (var parameter in filter.Parameters)
                {
                    object value;
                    while (true)
                    {
                        Console.WriteLine("   Type the '{0}' value ({1}) and press return. Insert a blank value to " + 
                            "not choose a value for this parameter.", parameter.Name,
                            parameter.Type.Name);
                        Console.WriteLine("   Example: {0}" + Environment.NewLine, example(parameter.Type));
                        string input = Console.ReadLine();
                        if (!(input.Trim().Length > 0))
                        {
                            value = default;
                            break;
                        }
                        value = parsed(parameter.Type, input);
                        if (value != default) break;
                        Console.WriteLine("The value '{0}' couldn't be parsed." + Environment.NewLine, input);
                    }
                    if (value != default)
                    {
                        Console.WriteLine("Parsed value is: {0}" + Environment.NewLine, value);
                        filter.SetParameter(parameter.Name, value);
                    }
                    else
                    {
                        Console.WriteLine("No value informed, will use default value if available." + 
                            Environment.NewLine);
                    }
                }

                Console.WriteLine("Processing image...");

                // Process the image
                Bitmap outBitmap;
                try
                {
                    if (filter is CopyFilter)
                    {
                        (filter as CopyFilter).Process(bitmap, out outBitmap);
                    }
                    else
                    {
                        outBitmap = new Bitmap(bitmap);
                        (filter as InPlaceFilter).Process(outBitmap);
                    }
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine("Wrong argument error: {0}", ex.Message);
                    continue;
                }

                // Save the result
                Guid guid = Guid.NewGuid();
                string outDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "MyOpenCv");
                if (!Directory.Exists(outDir))
                {
                    try
                    {
                        Directory.CreateDirectory(outDir);
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        Console.WriteLine("Directory {0} can't be created; output file not written." + 
                            Environment.NewLine + ex.Message, outDir);
                        continue;
                    }
                }
                string filePath = Path.Combine(outDir, guid + ".png");
                outBitmap.Save(filePath);
                Console.WriteLine("Output saved at {0}", filePath);
                System.Diagnostics.Process.Start(filePath);
            }
        }
    }
}
