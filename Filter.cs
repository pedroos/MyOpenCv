using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Configuration;

namespace MyOpenCv.ImageProcessing
{
    /// <summary>
    /// Base class for all filters.
    /// </summary>
    /// <remarks>
    /// The Filter class shouldn't be subclassed directly; subclasses of InPlaceFilter or CopyFilter should be 
    /// created.
    /// </remarks>
    public abstract class Filter
    {
        /// <summary>
        /// The textual name of the filter.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// This method specified whether a given Bitmap image can be processed by this filter.
        /// </summary>
        /// <param name="bitmap">The bitmap to target.</param>
        /// <returns>True if the image can be processed by this filter; False otherwise.</returns>
        public abstract bool BitmapCondition(Bitmap bitmap);

        readonly FilterParameterAttribute[] parameters;
        readonly Dictionary<string, object> parameterValues;
        public Filter()
        {
            parameters = GetType().GetCustomAttributes(true).OfType<FilterParameterAttribute>().ToArray();
            parameterValues = new Dictionary<string, object>();
        }

        /// <summary>
        /// Returns a list of parameter name and type pairs attached to this filter.
        /// </summary>
        public IEnumerable<(string Name, Type Type)> Parameters
        {
            get { return parameters.Select(p => (p.Name, p.Type)); }
        }

        /// <summary>
        /// Reads the value of a filter parameter.
        /// </summary>
        /// <typeparam name="T">The type of the parameter</typeparam>
        /// <param name="name">The name of the parameter</param>
        /// <returns>The assigned parameter value</returns>
        protected T GetParameter<T>(string name)
        {
            var parameter = parameters.SingleOrDefault(p => p.Name == name);
            if (parameter == default)
                throw new ParameterNameException(name);
            if (parameter.Type != typeof(T))
                throw new ParameterTypeException(name, typeof(T), parameter.Type);

            if (!parameterValues.ContainsKey(name))
            {
                if (parameter.DefaultValue != default)
                {
                    if (parameter.DefaultValue.GetType() != parameter.Type)
                        throw new InvalidOperationException("Filter parameter '{0}' has a default value of a " + 
                            "wrong type");
                    return (T)parameter.DefaultValue;
                }
                throw new ArgumentException(string.Format("Filter parameter value '{0}' of type '{1}' not set",
                    name, parameter.Type.Name));
            }
            return (T)parameterValues[name];
        }

        /// <summary>
        /// Sets the value of a filter parameter.
        /// </summary>
        /// <param name="paramValues">A 2-element tuple containing names and values of parameters.</param>
        public void SetParameter(string name, object value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            // Check that the parameter name and type matches a parameter in the Filter subclass
            var parameter = parameters.Where(p => p.Name == name).FirstOrDefault();
            if (parameter == default)
                throw new ParameterNameException(name);
            if (parameter.Type != value.GetType())
                throw new ParameterTypeException(name, value.GetType(), parameter.Type);

            // Cache the parameter values in the class
            if (parameterValues.ContainsKey(name))
                parameterValues[name] = value;
            else 
                parameterValues.Add(name, value);
        }
    }

    /// <summary>
    /// Base class for filters which process in-place.
    /// </summary>
    public abstract class InPlaceFilter : Filter
    {
        public abstract void Process(Bitmap bitmap);
    }

    /// <summary>
    /// Base class for filters which process in a copy.
    /// </summary>
    public abstract class CopyFilter : Filter
    {
        public abstract void Process(Bitmap bitmap, out Bitmap outBitmap);
    }
}