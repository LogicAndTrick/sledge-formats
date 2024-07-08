using System;
using System.Collections.Generic;
using Sledge.Formats.Geometric;
using Sledge.Formats.Map.Objects;

namespace Sledge.Formats.Map.Factories
{
    /// <summary>
    /// A class that creates <see cref="MapObject"/> instances of any type
    /// </summary>
    public interface IMapObjectFactory
    {
        /// <summary>
        /// The name of this object factory
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Create the map objects from the given box
        /// </summary>
        /// <param name="box">The box to define the bounds of the created objects</param>
        /// <returns>0 or more MapObject instances</returns>
        IEnumerable<MapObject> Create(Box box);

        /// <summary>
        /// Get a list of properties for this factory.
        /// </summary>
        /// <returns>The list of properties</returns>
        IEnumerable<MapObjectFactoryProperty> GetProperties();

        /// <summary>
        /// Get a property value.
        /// </summary>
        /// <param name="name">The name of the property</param>
        /// <exception cref="ArgumentException">If the property of the given name doesn't exist</exception>
        object GetProperty(string name);

        /// <summary>
        /// Set a property value.
        /// </summary>
        /// <param name="name">The name of the property</param>
        /// <param name="value">The value of the property</param>
        /// <exception cref="ArgumentException">If the property of the given name doesn't exist</exception>
        /// <exception cref="InvalidCastException">If the property value could not be set due to an invalid type</exception>
        void SetProperty(string name, object value);
    }
}
