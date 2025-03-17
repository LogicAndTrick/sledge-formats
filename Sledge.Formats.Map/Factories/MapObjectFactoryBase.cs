using System;
using System.Collections.Generic;
using System.Linq;
using Sledge.Formats.Geometric;
using Sledge.Formats.Map.Objects;

namespace Sledge.Formats.Map.Factories
{
    /// <summary>
    /// Abstract base class that provides automatic property get/set information using reflection.
    /// </summary>
    public abstract class MapObjectFactoryBase : IMapObjectFactory
    {
        private readonly Lazy<Dictionary<string, ReflectionMapObjectFactoryProperty>> _properties;

        protected MapObjectFactoryBase()
        {
            _properties = new Lazy<Dictionary<string, ReflectionMapObjectFactoryProperty>>(() => GetType().GetProperties().Where(x => x.CanRead && x.CanWrite).ToDictionary(x => x.Name, x => new ReflectionMapObjectFactoryProperty(x)));
        }

        /// <inheritdoc cref="IMapObjectFactory.Name"/>
        public abstract string Name { get; }

        /// <inheritdoc cref="IMapObjectFactory.Create"/>
        public abstract IEnumerable<MapObject> Create(Box box);

        /// <inheritdoc cref="IMapObjectFactory.GetProperties"/>
        public IEnumerable<MapObjectFactoryProperty> GetProperties()
        {
            return _properties.Value.Values;
        }

        /// <inheritdoc cref="IMapObjectFactory.GetProperty"/>
        public object GetProperty(string name)
        {
            if (!_properties.Value.ContainsKey(name)) throw new ArgumentException($"Property with name '{name}' not found", nameof(name));
            return _properties.Value[name].Property.GetValue(this);
        }

        /// <inheritdoc cref="IMapObjectFactory.SetProperty"/>
        public void SetProperty(string name, object value)
        {
            if (!_properties.Value.ContainsKey(name)) throw new ArgumentException($"Property with name '{name}' not found", nameof(name));
            _properties.Value[name].Property.SetValue(this, value);
        }
    }
}