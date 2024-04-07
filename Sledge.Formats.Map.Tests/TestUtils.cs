using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sledge.Formats.Map.Objects;

namespace Sledge.Formats.Map.Tests;

public static class TestUtils
{
    public static bool AreEqualMap(MapFile a, MapFile b)
    {
        if (!AreEqualMapObject(a.Worldspawn, b.Worldspawn)) return false;
        // todo: visgroups, paths, cameras, additionalobjects, cordonbounds, backgroundimages
        return true;
    }

    public static bool AreEqualMapObject(MapObject a, MapObject b)
    {
        return a switch
        {
            Worldspawn aw => b is Worldspawn bw && AreEqualWorldspawn(aw, bw),
            Entity ae => b is Entity be && AreEqualEntity(ae, be),
            Group ag => b is Group bg && AreEqualGroup(ag, bg),
            Solid al => b is Solid bl && AreEqualSolid(al, bl),
            _ => false
        };
    }

    public static bool AreEqualBase(MapObject a, MapObject b)
    {
        // color
        if (!a.Color.Equals(b.Color)) return false;

        // visgroups
        if (a.Visgroups.Count != b.Visgroups.Count) return false;
        var diff = a.Visgroups.ToHashSet();
        diff.SymmetricExceptWith(b.Visgroups);
        if (diff.Count > 0) return false;

        // children
        if (a.Children.Count != b.Children.Count) return false;
        var bChildren = b.Children.ToList();
        foreach (var ac in a.Children)
        {
            var matching = b.Children.FirstOrDefault(bc => AreEqualMapObject(ac, bc));
            if (matching == null) return false;
            bChildren.Remove(matching);
        }

        return bChildren.Count == 0;
    }

    public static bool AreEqualWorldspawn(Worldspawn a, Worldspawn b)
    {
        return AreEqualEntity(a, b);
    }

    public static bool AreEqualEntity(Entity a, Entity b)
    {
        if (!AreEqualBase(a, b)) return false;
        if (a.ClassName != b.ClassName) return false;
        if (a.SpawnFlags != b.SpawnFlags) return false;
        if (a.Properties.Count != b.Properties.Count) return false;
        var keys = b.Properties.Keys.ToList();
        foreach (var k in a.Properties.Keys)
        {
            var av = a.Properties.TryGetValue(k, out var avo) ? avo ?? "" : "";
            var bv = b.Properties.TryGetValue(k, out var bvo) ? bvo ?? "" : "";
            keys.Remove(k);
            if (av != bv) return false;
        }

        return keys.Count == 0;
    }

    public static bool AreEqualGroup(Group a, Group b)
    {
        return AreEqualBase(a, b);
    }

    public static bool AreEqualSolid(Solid a, Solid b)
    {
        if (!AreEqualBase(a, b)) return false;

        if (a.Faces.Count != b.Faces.Count) return false;
        var bFaces = b.Faces.ToList();
        foreach (var af in a.Faces)
        {
            var matching = b.Faces.FirstOrDefault(bf => AreEqualFace(af, bf));
            if (matching == null) return false;
            bFaces.Remove(matching);
        }

        if (a.Meshes.Count != b.Meshes.Count) return false;
        var bMeshes = b.Meshes.ToList();
        foreach (var am in a.Meshes)
        {
            var matching = b.Meshes.FirstOrDefault(bm => AreEqualMesh(am, bm));
            if (matching == null) return false;
            bMeshes.Remove(matching);
        }

        return bFaces.Count == 0 && bMeshes.Count == 0;
    }

    public static bool AreEqualSurface(Surface a, Surface b)
    {
        return a.TextureName == b.TextureName
               && a.UAxis == b.UAxis
               && a.VAxis == b.VAxis
               && Math.Abs(a.XScale - b.XScale) < float.Epsilon
               && Math.Abs(a.YScale - b.YScale) < float.Epsilon
               && Math.Abs(a.XShift - b.XShift) < float.Epsilon
               && Math.Abs(a.YShift - b.YShift) < float.Epsilon
               && Math.Abs(a.Rotation - b.Rotation) < float.Epsilon
               && a.ContentFlags == b.ContentFlags
               && a.SurfaceFlags == b.SurfaceFlags
               && Math.Abs(a.Value - b.Value) < float.Epsilon
               && Math.Abs(a.LightmapScale - b.LightmapScale) < float.Epsilon
               && a.SmoothingGroups == b.SmoothingGroups;
    }

    public static bool AreEqualFace(Face a, Face b)
    {
        if (!AreEqualSurface(a, b)) return false;
        if (a.Plane != b.Plane) return false;
        if (a.Vertices.Count != b.Vertices.Count) return false;

        var bIdx = b.Vertices.IndexOf(a.Vertices[0]);
        if (bIdx < 0) return false;
        for (var i = 0; i < a.Vertices.Count; i++)
        {
            var av = a.Vertices[i];
            var bv = b.Vertices[(i + bIdx) % a.Vertices.Count];
            if (av != bv) return false;
        }
        return true;
    }

    public static bool AreEqualMesh(Mesh a, Mesh b)
    {
        if (!AreEqualSurface(a, b)) return false;
        if (a.Width != b.Width) return false;
        if (a.Height != b.Height) return false;
        if (a.Points.Count != b.Points.Count) return false;

        for (var i = 0; i < a.Points.Count; i++)
        {
            var ap = a.Points[i];
            var bp = b.Points.FirstOrDefault(x => x.X == ap.X && x.Y == ap.Y);
            if (bp == null) return false;
            if (!AreEqualMeshPoint(ap, bp)) return false;
        }

        return true;
    }

    public static bool AreEqualMeshPoint(MeshPoint a, MeshPoint b)
    {
        return a.X == b.X
               && a.Y == b.Y
               && a.Position == b.Position
               && a.Normal == b.Normal
               && a.Texture == b.Texture;
    }
}