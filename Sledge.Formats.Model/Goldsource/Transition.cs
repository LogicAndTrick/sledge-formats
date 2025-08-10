namespace Sledge.Formats.Model.Goldsource
{
    public class Transition
    {
        public int FromNode { get; set; }
        public int ToNode { get; set; }
        public byte ViaNode { get; set; }
    }
}