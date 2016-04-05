using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurveFitting.Core
{
    public class PJSchneider
    {
        private Vector2d ComputeLeftTangent(List<Point2d> points, int firstIndex)
        {
            var tHat = new Vector2d(points[firstIndex + 1]);
            var tmp = new Vector2d(points[firstIndex]);
            return (tHat - tmp).Unit;
        }

        private Vector2d ComputeRightTangent(List<Point2d> points, int endIndex)
        {
            var tHat = new Vector2d(points[endIndex - 1]);
            var tmp = new Vector2d(points[endIndex]);
            return (tHat + tmp).Unit;
        }

        private Vector2d ComputeCenterTangent(List<Point2d> points, int centerIndex)
        {
            var v1  = new Vector2d(points[centerIndex - 1]);
            v1 = v1 - new Vector2d(points[centerIndex]);

            var v2 = new Vector2d(points[centerIndex]);
            v2 = v2 - new Vector2d(points[centerIndex + 1]);

            var vCenter = (v1 + v2) * 0.5;
            return vCenter.Unit;
        }

        private List<double> ChordLengths(List<Point2d> points, int first, int last)
        {
            var result = new List<double>();
            result.Add(0);

            for (int i = first + 1; i < last; i++)
            {
                var v = new Vector2d(points[i - 1]);
                v = v - points[i];
                var r = result[i - 1] + v.Length;
                result.Add(r);
            }

            for (int i = first  + 1; i < last; i++)
            {
                var r = result[i - 1] / result[last - first - 1];
            }

            return result;
        }

        public double ComputeMaxError(List<Point2d> points, int first, int last, List<Point2d> curve, List<double> u, out int splitPoint)
        {
            double maxDist = 0;
	        double dist = 0;
	        Point2d p = null;
            Vector2d v = null;

            splitPoint = ((last - first + 1) - ((last - first + 1) % 2)) / 2;

            for (int i = first + 1; i < last - 1; i++)
            {
                p = Beizer.ComputeParam(3, curve, u[i - first]);
                v = new Vector2d(p.X - points[i].X, p.Y - points[i].Y);
                dist = v.Length;

                if (dist > maxDist)
                {
                    maxDist = dist;
                    splitPoint = i;
                }
            }

            return maxDist;
        }

        private double RootFind(List<Point2d> points, Point2d p, double u)
        {
            double numerator, denominator;
            // control points for Q' and Q''
            var cq1 = new Point2d[3];
            var cq2 = new Point2d[3];
            

            // generate control points for Q'
            for (int i = 0; i < 3; i++)
            {
                cq1[i] = (new Vector2d(points[i], points[i + 1]) * 3).GetPoint();
            }

            // generate control points for Q''
            for (int i = 0; i < 3; i++)
            {
                cq2[i] = (new Vector2d(cq1[i], cq1[i + 1]) * 2).GetPoint();
            }

            // Q(u)
            var qu = Beizer.ComputeParam(3, points, u);
            // Q'(u)
            var qu1 = Beizer.ComputeParam(3, cq1.ToList(), u);
            // Q''(u)
            var qu2 = Beizer.ComputeParam(3, cq2.ToList(), u);

            // compute f(u) / f'(u)
            numerator = (qu.X - p.X) * qu1.X + (qu.Y - p.Y) * qu1.Y;
            denominator = qu1.X * qu1.X + qu1.Y * qu1.Y + (qu.X - p.X) * qu2.X + (qu.Y - p.Y) * qu2.Y;


            if (denominator == 0) return u; // root found 
            
            // else:
            // Newton: t <- t - f(x) / f'(x)
            double result = u - (numerator / denominator);
	        return result;
        }

        private List<double> ReParam(List<Point2d> points, int first, int last, List<Point2d> curve, List<double> u)
        {
            var nPts = last - first + 1;
            var result = new List<double>();

            for (int i = first; i <= last; i++)
            {   
                var r = RootFind(curve, points[i], u[i - first]);
                result.Add(r);
            }

            return result;
        }

        private void FitCubic(List<Point2d> points, int first, int last, Vector2d tHat1, Vector2d tHat2, double error, List<Point2d> result)
        {
            var maxIteration = 4;
            var iteError = error * error;
            var nPts = last - first + 1;

            if (nPts == 2)
            {
                double dist = new Vector2d(points[first], points[last]).Length / 3.0;
                var b1 = points[first];
                var b4 = points[last];
                var b2 = (dist * tHat1 + b1).GetPoint();
                var b3 = (dist * tHat2 + b4).GetPoint();

                result.Add(b1);
                result.Add(b2);
                result.Add(b3);

                return;
            }

            //  Parameterize points, and attempt to fit curve 
            var u = ChordLengths(points, first, last);
            var splitPoint = 0;
            var bz = Beizer.Generate(points, first, last, u, tHat1, tHat2);

            //  wFind max deviation of points to fitted curve 
            var maxError = ComputeMaxError(points, first, last, bz.Points, u, out splitPoint);
            if (maxError < error)
            {
                result.Add(bz[1]);
                result.Add(bz[2]);
                result.Add(bz[3]);
                return;
            }

            // If error not too large, try some reparameterization  
            // and iteration 
            if (maxError < iteError)
            {
                for (var i = 0; i < maxIteration; i++)
                {
                    var uPrime = ReParam(points, first, last, bz.Points, u);
                    bz = Beizer.Generate(points, first, last, uPrime, tHat1, tHat2);
                    maxError = ComputeMaxError(points, first, last, bz.Points, uPrime, out splitPoint);

                    if (maxError < error)
                    {
                        result.Add(bz[1]);
                        result.Add(bz[2]);
                        result.Add(bz[3]);
                        return;
                    }
                    u = uPrime;
                }
            }

            // Fitting failed -- split at max error point and fit recursively 
            var tHatCenter = ComputeCenterTangent(points, splitPoint);
            FitCubic(points, first, splitPoint, tHat1, tHatCenter, error, result);
            tHatCenter = -tHatCenter;
            FitCubic(points, splitPoint, last, tHatCenter, tHat2, error, result);
        }

        public List<Point2d> FitCurve(List<Point2d> points, double error)
        {
            Vector2d tHat1, tHat2;    /*  Unit tangent vectors at endpoints */

            tHat1 = ComputeLeftTangent(points, 0);
            tHat2 = ComputeRightTangent(points, points.Count - 1);
            List<Point2d> result = new List<Point2d>();
            FitCubic(points, 0, points.Count - 1, tHat1, tHat2, error, result);
            return result;
        }
    }
}
