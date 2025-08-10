using Sledge.Formats.Bsp.Lumps;

namespace Sledge.Formats.Bsp.Objects
{
    internal class Cliphull : Hull
    {
        private readonly Model _model;
        private readonly int _hullNum;
        private readonly Clipnodes _clipnodes;
        private readonly Planes _planes;

        public Cliphull(BspFile bsp, Model model, int hullNum)
        {
            _model = model;
            _hullNum = hullNum;
            _clipnodes = bsp.Clipnodes;
            _planes = bsp.Planes;
        }

        public override int GetRoot()
        {
            return _model.HeadNodes[_hullNum];
        }

        public override Plane GetPlane(int nodeNum)
        {
            var node = _clipnodes[nodeNum];
            return _planes[(int)node.Plane];
        }

        public override int GetChild(int nodeNum, int side)
        {
            var node = _clipnodes[nodeNum];
            return node.Children[side];
        }
    }
}