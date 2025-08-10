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
                TargetPoint = end,
                PassedThroughNonSolid = false
            };
            tl.Success = !TraceLineRecursive(hull, hull.GetRoot(), 0, 1, start, end, tl);
            return tl;
        }

        private static bool TraceLineRecursive(
            Hull hull,
            int currentNodeNum,
            float startFrac,
            float endFrac,
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

                // see if we've passed through a non-solid leaf
                if (contents != Contents.Solid) trace.PassedThroughNonSolid = true;

                return true;
            }

            // see which side of the plane each end is on
            var plane = hull.GetPlane(currentNodeNum);
            var t1 = plane.Evaluate(start);
            var t2 = plane.Evaluate(end);

            // if each end is on the same side, just traverse directly into that node
            if (t1 >= 0 && t2 >= 0) return TraceLineRecursive(hull, hull.GetChild(currentNodeNum, 0), startFrac, endFrac, start, end, trace);
            if (t1 < 0 && t2 < 0) return TraceLineRecursive(hull, hull.GetChild(currentNodeNum, 1), startFrac, endFrac, start, end, trace);

            // otherwise, each end is on a different side of the plane, so we need to traverse both children
            // we add the magic constant `distEpsilon` to make sure we maintain consistency with Quake logic
            const float distEpsilon = 0.03125f;
            float fraction;
            if (t1 < 0) fraction = (t1 + distEpsilon) / (t1 - t2);
            else fraction = (t1 - distEpsilon) / (t1 - t2);

            // clamp the fraction to 0..1
            if (fraction < 0) fraction = 0;
            if (fraction > 1) fraction = 1;

            var midFrac = startFrac + (endFrac - startFrac) * fraction;
            var mid = start + fraction * (end - start);

            var side = t1 < 0 ? 1 : 0;
            var otherSide = 1 - side;

            if (!TraceLineRecursive(hull, hull.GetChild(currentNodeNum, side), startFrac, midFrac, start, mid, trace)) return false;

            if (hull.GetContents(hull.GetChild(currentNodeNum, otherSide), mid) != Contents.Solid)
            {
                return TraceLineRecursive(hull, hull.GetChild(currentNodeNum, otherSide), midFrac, endFrac, mid, end, trace);
            }

            if (!trace.PassedThroughNonSolid) return false;

            if (side == 0) trace.EndPlane = new System.Numerics.Plane(plane.Normal, -plane.Distance);
            else trace.EndPlane = new System.Numerics.Plane(-plane.Normal, plane.Distance);

            // this will only run 10x maximum since fraction is between 0 and 1
            while (fraction >= 0.1f && hull.GetContents(hull.GetRoot(), mid) == Contents.Solid)
            {
                fraction -= 0.1f;
                midFrac = startFrac + (endFrac - startFrac) * fraction;
                mid = start + fraction * (end - start);
            }

            trace.EndFraction = midFrac;
            trace.EndPoint = mid;
            return false;
        }
    }
}
