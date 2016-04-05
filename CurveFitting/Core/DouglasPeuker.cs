using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurveFitting.Core
{
    public class DouglasPeuker
    {
        private void SegmentReduce(List<Point2d> points, int first, int last, double tolerance, List<int> pointIndexes)
        {
            double maxD = 0;
            var maxDistIndex = 0;

            for (int i = first; i <= last; i++)
            {
                var distance = points[i].Distance(points[first], points[last]);
                if (distance > maxD)
                {
                    maxD = distance;
                    maxDistIndex = i;
                }
            }

            if (maxD > tolerance && maxDistIndex != 1)
            {
                pointIndexes.Add(maxDistIndex);
                SegmentReduce(points, first, maxDistIndex, tolerance, pointIndexes);
                SegmentReduce(points, maxDistIndex, last, tolerance, pointIndexes);
            }
        }

        public List<Point2d>  DPReduce(List<Point2d> points, double tolerance) 
        {
            if (points == null || points.Count < 3) return points;

            var fPoint = 0;
            var lPoint = points.Count - 1;

            var indexes = new List<int>();
            indexes.Add(fPoint); 
            indexes.Add(lPoint);

            while ((points[fPoint].X == points[lPoint].X) && (points[fPoint].Y == points[lPoint].Y))
            {
                lPoint = lPoint - 1;
            }

            SegmentReduce(points, fPoint, lPoint, tolerance, indexes);

            for (int i = 0; i < indexes.Count; i++)
            {
                for (int j = indexes.Count - 1; j > i; j--)
                {
                    if (indexes[i] > indexes[j])
                    {
                        int temp = indexes[i];
                        indexes[i] = indexes[j];
                        indexes[j] = temp;
                    }
                }
            }

            var result = new List<Point2d>();

            for (int i = 0; i < indexes.Count; i++)
            {
                result.Add(points[indexes[i]]);
            }

            return result;

        }
    }
}
