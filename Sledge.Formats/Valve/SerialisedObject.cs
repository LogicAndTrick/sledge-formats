using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Sledge.Formats.Valve
{
    /// <summary>
    /// Represents a serialised object with basic features similar to XML.
    /// </summary>
    [Serializable]
    public class SerialisedObject : ISerializable
    {
        /// <summary>
        /// The name of the object
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The properties (or attributes) of the object
        /// </summary>
        public List<KeyValuePair<string, string>> Properties { get; set; }

        /// <summary>
        /// A list of child objects
        /// </summary>
        public List<SerialisedObject> Children { get; set; }

        /// <summary>
        /// Construct a blank <see cref="SerialisedObject"/> with the provided name.
        /// </summary>
        /// <param name="name">The name of the object</param>
        public SerialisedObject(string name)
        {
            Name = name;
            Properties = new List<KeyValuePair<string, string>>();
            Children = new List<SerialisedObject>();
        }

        /// <summary>
        /// Construct a <see cref="SerialisedObject"/> from a stream.
        /// If the stream does not contain exactly one serialised object, an exception will be thrown.
        /// </summary>
        /// <param name="stream">The stream to read exactly one <see cref="SerialisedObject"/> from</param>
        /// <exception cref="InvalidOperationException">If the stream does not contain exactly one serialised object</exception>
        public SerialisedObject(Stream stream)
        {
            var objects = SerialisedObjectFormatter.Instance.Deserialize(stream).ToList();
            if (objects.Count == 0) throw new InvalidOperationException("Stream contains no objects, use SerialisedObjectFormatter.Deserialise instead.");
            if (objects.Count > 1) throw new InvalidOperationException("Stream contains more than one object, use SerialisedObjectFormatter.Deserialise instead.");
            Name = objects[0].Name;
            Properties = objects[0].Properties;
            Children = objects[0].Children;
        }

        /// <summary>
        /// Constructor for <see cref="ISerializable"/> creation.
        /// </summary>
        protected SerialisedObject(SerializationInfo info, StreamingContext context)
        {
            Name = info.GetString("Name");
            Properties = (List<KeyValuePair<string, string>>) info.GetValue("Properties", typeof(List<KeyValuePair<string, string>>));
            Children = (List<SerialisedObject>) info.GetValue("Children", typeof(List<SerialisedObject>));
        }

        /// <inheritdoc cref="ISerializable.GetObjectData"/>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Name", Name);
            info.AddValue("Properties", Properties);
            info.AddValue("Children", Children);
        }

        /// <summary>
        /// Serialise this object to the provided stream.
        /// </summary>
        /// <param name="stream">The stream to write to</param>
        public void WriteTo(Stream stream)
        {
            SerialisedObjectFormatter.Instance.Serialize(stream, this);
        }

        /// <summary>
        /// Convert this object to a serialised object string
        /// </summary>
        public override string ToString()
        {
            var stream = new MemoryStream();
            WriteTo(stream);
            return Encoding.UTF8.GetString(stream.ToArray());
        }
    }
}
