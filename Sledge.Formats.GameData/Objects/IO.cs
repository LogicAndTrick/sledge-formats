namespace Sledge.Formats.GameData.Objects
{
    public class IO
    {
        public IOType IOType { get; set; }
        public VariableType VariableType { get; set; }
        public string SubType { get; set; }
        /// <summary>
        /// If the <see cref="VariableType"/> is <see cref="Objects.VariableType.Unknown"/>, this will contain the specified type of the IO instance. Otherwise, it will be null.
        /// </summary>
        public string UnknownVariableTypeName { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public GameDataDictionary Metadata { get; set; }

        public IO(IOType ioType, VariableType variableType, string subType, string name)
        {
            IOType = ioType;
            VariableType = variableType;
            SubType = subType;
            Name = name;
            Description = "";
            Metadata = new GameDataDictionary(string.Empty);
        }
    }
}
