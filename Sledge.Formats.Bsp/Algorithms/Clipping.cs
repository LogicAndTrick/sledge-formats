using System.Numerics;
using Sledge.Formats.Bsp.Objects;

namespace Sledge.Formats.Bsp.Algorithms
{
    internal static class Clipping
    {
        public static TraceLine TraceLine(
            BspFile bsp,
            Model model,
            int hullNum,
            Vector3 start,
            Vector3 end
        )
        {
            var hull = hullNum == 0 ? new Hull0(bsp, model) : (Hull)new Cliphull(bsp, model, hullNum);
            return TraceLine(hull, start, end);
        }

        public static TraceLine TraceLine(
            Hull hull,
            Vector3 start,
            Vector3 end
        )
        {
            var tl = new TraceLine
            {
                StartPoint = start,
                StartContents = 0,
                EndPoint = end,
                EndContents = Contents.Solid,
                TargetPoint = end
            };
            tl.Success = TraceLineRecursive(hull, -1, -1, hull.GetRoot(), start, end, tl);
            if (tl.Success)
            {
                var len1 = (tl.EndPoint - tl.StartPoint).Length();
                var len2 = (end - start).Length();
                tl.EndFraction = len2 / len1;
            }
            return tl;
        }

        private static bool TraceLineRecursive(
            Hull hull,
            // we only pass the parent and side so we can set the intersection plane in the trace
            int parentNodeNum,
            int currentSide,
            int currentNodeNum,
            Vector3 start,
            Vector3 end,
            TraceLine trace)
        {
            // handle leaf nodes
            if (currentNodeNum < 0)
            {
                var contents = (Contents)currentNodeNum;

                // check the starting point, this will always be the first leaf node we hit
                if (trace.StartContents == 0)
                {
                    trace.StartContents = contents;
                }

                // set the end contents as the last node we hit will be the intersection node
                if (contents != Contents.Solid)
                {
                    trace.EndContents = contents;
                }

                // treat solid as empty if we haven't left a solid node yet
                if (contents == Contents.Solid && trace.StartContents == Contents.Solid && trace.EndContents == Contents.Solid)
                {
                    contents = Contents.Empty;
                }

                // if we are in a solid node then we have an intersection
                if (contents == Contents.Solid)
                {
                    var pln = hull.GetPlane(parentNodeNum);

                    // flip the intersection plane for the far side
                    var intersectionPlane = new System.Numerics.Plane(pln.Normal, pln.Distance);
                    if (currentSide == 1) intersectionPlane = intersectionPlane.Flip();

                    trace.EndPoint = start;
                    trace.EndPlane = intersectionPlane;
                    return true;
                }

                // empty node, no intersection
                return false;
            }

            // see which side of the plane each end is on
            var plane = hull.GetPlane(currentNodeNum);
            var t1 = plane.Evaluate(start);
            var t2 = plane.Evaluate(end);

            // if each end is on the same side, just traverse directly into that node
            if (t1 >= 0 && t2 >= 0) return TraceLineRecursive(hull, currentNodeNum, 0, hull.GetChild(currentNodeNum, 0), start, end, trace);
            if (t1 < 0 && t2 < 0) return TraceLineRecursive(hull, currentNodeNum, 0, hull.GetChild(currentNodeNum, 1), start, end, trace);

            // otherwise, each end is on a different side of the plane, so we need to traverse both children
            // we add the magic constant `distEpsilon` to make sure we maintain consistency with Quake logic
            const float distEpsilon = 0.03125f;
            float fraction;
            if (t1 < 0) fraction = (t1 + distEpsilon) / (t1 - t2);
            else fraction = (t1 - distEpsilon) / (t1 - t2);

            // clamp the fraction to 0..1
            if (fraction < 0) fraction = 0;
            if (fraction > 1) fraction = 1;

            var isect = start + fraction * (end - start);

            var side = t1 < 0 ? 1 : 0;
            var otherSide = side ^ 1;

            // check the nodes in order of closest to start point
            if (TraceLineRecursive(hull, currentNodeNum, side, hull.GetChild(currentNodeNum, side), start, isect, trace)) return true;
            if (TraceLineRecursive(hull, currentNodeNum, otherSide, hull.GetChild(currentNodeNum, otherSide), isect, end, trace)) return true;

            // no intersection
            return false;
        }
    }
}
