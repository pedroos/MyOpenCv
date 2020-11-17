using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyOpenCv
{

    /// <summary>
    /// Denotes an error while defining or reading a filter parameter.
    /// </summary>
    public abstract class ParameterException : Exception { }

    /// <summary>
    /// Denotes a parameter name mismatch.
    /// </summary>
    public class ParameterNameException : ParameterException
    {
        /// <summary>
        /// The wrong name of the parameter utilized
        /// </summary>
        public string Name { get; }
        
        /// <param name="name">The wrong name of the parameter utilized</param>
        public ParameterNameException(string name)
        {
            Name = name;
        }
        public override string ToString()
        {
            return string.Format("Parameter '{0}' not found", Name);
        }
    }

    /// <summary>
    /// Denotes a parameter type mismatch.
    /// </summary>
    public class ParameterTypeException : ParameterException
    {
        /// <summary>
        /// The name of the filter
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The wrong type utilized
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// A correct type inferenced from the parameter definition
        /// </summary>
        public Type FoundType { get; }

        /// <param name="name">The name of the filter</param>
        /// <param name="type">The wrong type utilized</param>
        /// <param name="foundType">A correct type inferenced from the parameter definition</param>
        public ParameterTypeException(string name, Type type, Type foundType)
        {
            Name = name;
            Type = type;
            FoundType = foundType;
        }
        public override string ToString()
        {
            return string.Format("Parameter '{0}' type is not '{1}' ({2} found)", Name, Type.Name,
                FoundType.Name);
        }
    }
}
