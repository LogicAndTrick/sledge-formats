using Sledge.Formats.Bsp.Lumps;

namespace Sledge.Formats.Bsp.Objects
{
    internal class Hull0 : Hull
    {
        private readonly Model _model;
        private readonly Nodes _nodes;
        private readonly Planes _planes;
        private readonly Leaves _leaves;

        public Hull0(BspFile bsp, Model model)
        {
            _model = model;
            _nodes = bsp.Nodes;
            _planes = bsp.Planes;
            _leaves = bsp.Leaves;
        }

        public override int GetRoot()
        {
            return _model.HeadNodes[0];
        }

        public override Plane GetPlane(int nodeNum)
        {
            var node = _nodes[nodeNum];
            return _planes[(int)node.Plane];
        }

        public override int GetChild(int nodeNum, int side)
        {
            var node = _nodes[nodeNum];
            var c = node.Children[side];
            if (c < 0) return (int)_leaves[~c].Contents;
            return c;
        }
    }
}