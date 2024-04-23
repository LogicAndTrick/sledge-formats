using System.Globalization;
using Sledge.Formats.Map.Objects;
using Sledge.Formats.Valve;
using System.Numerics;
using Sledge.Formats.Geometric.Precision;

namespace Sledge.Formats.Map.Formats.VmfObjects
{
    internal class VmfSide
    {
        public long ID { get; set; }
        public Face Face { get; set; }

        public VmfSide(SerialisedObject obj)
        {
            ID = obj.Get("ID", 0L);
            Face = new Face
            {
                TextureName = obj.Get("material", ""),
                Rotation = obj.Get("rotation", 0f),
                LightmapScale = obj.Get("lightmapscale", 0),
                SmoothingGroups = obj.Get("smoothing_groups", "")
            };
            if (Util.ParseDoubleArray(obj.Get("plane", ""), new[] { ' ', '(', ')' }, 9, out var pl))
            {
                // Converting VMF clockwise ordering into counter-clockwise
                Face.OriginalPlaneVertices = new[]
                {
                    new Vector3d(pl[6], pl[7], pl[8]),
                    new Vector3d(pl[3], pl[4], pl[5]),
                    new Vector3d(pl[0], pl[1], pl[2])
                };
                Face.Plane = Plane.CreateFromVertices(
                    Face.OriginalPlaneVertices[0].Round().ToVector3(),
                    Face.OriginalPlaneVertices[1].Round().ToVector3(),
                    Face.OriginalPlaneVertices[2].Round().ToVector3()
                );
            }
            else
            {
                Face.Plane = new Plane(Vector3.UnitZ, 0);
            }
            if (Util.ParseFloatArray(obj.Get("uaxis", ""), new[] { ' ', '[', ']' }, 5, out float[] ua))
            {
                Face.UAxis = new Vector3(ua[0], ua[1], ua[2]);
                Face.XShift = ua[3];
                Face.XScale = ua[4];
            }
            if (Util.ParseFloatArray(obj.Get("vaxis", ""), new[] { ' ', '[', ']' }, 5, out float[] va))
            {
                Face.VAxis = new Vector3(va[0], va[1], va[2]);
                Face.YShift = va[3];
                Face.YScale = va[4];
            }
        }

        public VmfSide(Face face, int id)
        {
            ID = id;
            Face = face;
        }

        public Face ToFace()
        {
            return Face;
        }

        public SerialisedObject ToSerialisedObject()
        {
            var so = new SerialisedObject("side");

            so.Set("id", ID);
            // reverse the vertex order for vmf
            so.Set("plane", $"({FormatVector3(Face.Vertices[2])}) ({FormatVector3(Face.Vertices[1])}) ({FormatVector3(Face.Vertices[0])})");
            so.Set("material", Face.TextureName);
            so.Set("uaxis", $"[{FormatVector3(Face.UAxis)} {FormatDecimal(Face.XShift)}] {FormatDecimal(Face.XScale)}");
            so.Set("vaxis", $"[{FormatVector3(Face.VAxis)} {FormatDecimal(Face.YShift)}] {FormatDecimal(Face.YScale)}");
            so.Set("rotation", Face.Rotation);
            so.Set("lightmapscale", Face.LightmapScale);
            so.Set("smoothing_groups", Face.SmoothingGroups);

            return so;
        }

        private static string FormatVector3(Vector3 c)
        {
            return $"{FormatDecimal(c.X)} {FormatDecimal(c.Y)} {FormatDecimal(c.Z)}";
        }

        private static string FormatDecimal(float d)
        {
            return d.ToString("0.00####", CultureInfo.InvariantCulture);
        }
    }
}