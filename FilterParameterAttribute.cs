using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyOpenCv
{
    /// <summary>
    /// Declares a parameter for a filter.
    /// </summary>
    /// <remarks>
    /// 'FilterParameter' attributes should be attached to Filter subclasses to describe the parameters accepted 
    /// by the filter.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class FilterParameterAttribute : Attribute
    {
        public Type Type { get; }
        
        public string Name { get; }
        public object DefaultValue { get; }

        /// <param name="type">The type of the parameter value to accept</param>
        /// <param name="name">The name of the parameter</param>
        public FilterParameterAttribute(Type type, string name, object defaultValue = default)
        {
            Type = type;
            Name = name;
            DefaultValue = defaultValue;
        }
    }
}
