namespace Sledge.Formats.Bsp
{
    public class BspFileOptions
    {
        public static readonly BspFileOptions Default = new BspFileOptions
        {
            AutodetectBlueShiftFormat = true,
            UseBlueShiftFormat = false
        };

        /// <summary>
        /// Set to true to automatically detect when a map is using the blue shift bsp format
        /// and mark the file to read and write as that format.
        /// </summary>
        public bool AutodetectBlueShiftFormat { get; set; } = true;

        /// <summary>
        /// Flag to read or write the bsp in the blue shift format.
        /// During read, when <see cref="AutodetectBlueShiftFormat"/> is true, this flag is ignored.
        /// During write, this flag is used.
        /// </summary>
        public bool UseBlueShiftFormat { get; set; } = false;

        public BspFileOptions Copy()
        {
            return new BspFileOptions
            {
                AutodetectBlueShiftFormat = AutodetectBlueShiftFormat,
                UseBlueShiftFormat = UseBlueShiftFormat
            };
        }
    }
}
