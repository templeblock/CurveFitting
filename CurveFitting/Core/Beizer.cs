using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CurveFitting.Core
{
    /// <summary>
    /// referenced from http://stackoverflow.com/questions/5525665/smoothing-a-hand-drawn-curve
    /// </summary>
    public class Beizer
    {
        private Point2d _p1, _p2, _p3, _p4;
        private int _granularity;

        public List<Point2d> Points
        {
            get
            {
                return (new Point2d[] { _p1, _p2, _p3, _p4 }).ToList();
            }
        }

        public Point2d this[int i]
        {
            get
            {
                if (i == 0)
                    return _p1;
                else if (i == 1)
                    return _p2;
                else if (i == 2)
                    return _p3;
                else if (i == 3)
                    return _p4;
                else
                    return null;
            }
        }

        public Beizer(Point2d p1, Point2d p2, Point2d p3, Point2d p4, int granularity)
        {
            _p1 = p1; _p2 = p2; _p3 = p3; _p4 = p4;
            _granularity = granularity;
        }

        

        public List<Point2d> GetSegments()
        {
            double inc = 1.0 / _granularity;
            double t = 0; double t1 = 0;

            var bSegments = new List<Point2d>();

            for (int i = 0; i < _granularity; i++)
            {
                t1 = 1 - t;
                double t1_3 = t1 * t1 * t1;
                double t1_3a = (3 * t) * t1 * t1;
                double t1_3b = (3 * t * t) * t1;
                double t1_3c = t * t * t;

                double x = t1_3 * _p1.X + t1_3a * _p2.X + t1_3b * _p3.X + t1_3c * _p4.X;
                double y = t1_3 * _p1.Y + t1_3a * _p2.Y + t1_3b * _p3.Y + t1_3c * _p4.Y;

                bSegments[i] = new Point2d(x, y);
                t = t + inc;
            }

            return bSegments;
        }

        public static Point2d ComputeParam(int degree, List<Point2d> points, double t)
        {
            var tmp = new Point2d[degree];

            for (int i = 0; i <= degree; i++)
            {
                tmp[i] = new Point2d(points[i].X, points[i].Y);
            }

            for (int i = 1; i < degree + 1; i++)
            {
                for (int j = 0; j < degree; j++)
                {
                    tmp[j].X = (1 - t) * tmp[j].X + t * tmp[j + 1].X;       
                    tmp[j].Y = (1 - t) * tmp[j].Y + t * tmp[j + 1].Y;
                }
            }

            return tmp[0];
        }

        /// <summary>
        /// -- to solve to matrix equation
        ///    -- | C11 C12 | |A1| = |X1|
        ///    -- | C21 C22 | |A2|   |X2|
        ///    -- A1, A2 are alpha1, alpha2
        /// </summary>
        /// <param name="points"></param>
        /// <param name="first"></param>
        /// <param name="last"></param>
        /// <param name="uPrime"></param>
        /// <param name="tHat1"></param>
        /// <param name="tHat2"></param>
        /// <returns></returns>
        public static Beizer Generate(List<Point2d> points, int first, int last, List<double> uPrime, Vector2d tHat1, Vector2d tHat2)
        {   
            //Vector2d[,] A = new Vector2d[MAXPOINTS, 2];/* Precomputed rhs for eqn    */
            List<Vector2d[]> A = new List<Vector2d[]>();   
          
            // matrix C
            double[,] C = new double[2, 2];
            // matrix X
            double[] X = new double[2];
            // beisier points
            Point2d[] bezCurvePoints = new Point2d[4];    

            var nPts = last - first + 1; 

            // compute A's (alpha left and alpha right)
            for (var i = 0; i < nPts; i++)
            {   
                var v1 = tHat1;
                var v2 = tHat2;
                v1 = v1 * (3 * uPrime[i] * (1 - uPrime[i]) * (1 - uPrime[i]));
                v2 = v2 * (3 * uPrime[i] * uPrime[i] * (1 - uPrime[i]));

                A[i] = new Vector2d[2];
                A[i][0] = v1;
                A[i][1] = v2;
            }

            /* Create the C and X matrices  */
            C[0, 0] = 0.0;
            C[0, 1] = 0.0;
            C[1, 0] = 0.0;
            C[1, 1] = 0.0;
            X[0] = 0.0;
            X[1] = 0.0;

            for (var i = 0; i < nPts; i++)
            {   
                C[0, 0] += A[i][0].Dot(A[i][0]);
                C[0, 1] += A[i][0].Dot(A[i][1]);

                C[1, 0] = C[0, 1];
                C[1, 1] += A[i][1].Dot(A[i][1]);

                var vfi = new Vector2d(points[first + i]);
                var vf = new Vector2d(points[first]);
                var vl = new Vector2d(points[last]);

                var op1 = vf * ((1 - uPrime[i]) * (1 - uPrime[i]) * (1 - uPrime[i]));
                var op2 = vf * (3 * uPrime[i] * (1 - uPrime[i]) * (1 - uPrime[i]));
                var op3 = vl * (3 * uPrime[i] * uPrime[i] * (1 - uPrime[i]));
                var op4 = vl * uPrime[i] * uPrime[i] * uPrime[i];

                var vsum = op1 + op2 + op3 + op4;

                X[0] += A[i][0].Dot(vsum);
                X[1] += A[i][1].Dot(vsum);
            }

            /* Compute the determinants of C and X  */
            double detC0C1 = C[0, 0] * C[1, 1] - C[1, 0] * C[0, 1];
            double detC0X = C[0, 0] * X[1] - C[1, 0] * X[0];
            double detXC1 = X[0] * C[1, 1] - X[1] * C[0, 1];

            /* Finally, derive alpha values */
            double alpha_l = (detC0C1 == 0) ? 0.0 : detXC1 / detC0C1;
            double alpha_r = (detC0C1 == 0) ? 0.0 : detC0X / detC0C1;

            /* If alpha negative, use the Wu/Barsky heuristic (see text) */
            /* (if alpha is 0, you get coincident control points that lead to
             * divide by zero in any subsequent NewtonRaphsonRootFind() call. */
            double segLength = new Vector2d(points[first], points[last]).Length;
            double epsilon = 0.000001 * segLength;
            if (alpha_l < epsilon || alpha_r < epsilon)
            {
                /* fall back on standard (probably inaccurate) formula, and subdivide further if needed. */
                double dist = segLength / 3.0;
                bezCurvePoints[0] = points[first];
                bezCurvePoints[3] = points[last];
                bezCurvePoints[1] = ((dist * tHat1) + bezCurvePoints[0]).GetPoint();
                bezCurvePoints[2] = ((dist * tHat2) + bezCurvePoints[3]).GetPoint();
                return (new Beizer(bezCurvePoints[0], bezCurvePoints[1], bezCurvePoints[2], bezCurvePoints[3], 4));
            }

            /*  First and last control points of the Bezier curve are */
            /*  positioned exactly at the first and last data points */
            /*  Control points 1 and 2 are positioned an alpha distance out */
            /*  on the tangent vectors, left and right, respectively */
            bezCurvePoints[0] = points[first];
            bezCurvePoints[3] = points[last];
            bezCurvePoints[1] = ((tHat1 * alpha_l) + bezCurvePoints[0]).GetPoint();
            bezCurvePoints[2] = ((tHat2 * alpha_r) + bezCurvePoints[3]).GetPoint();
            return (new Beizer(bezCurvePoints[0], bezCurvePoints[1], bezCurvePoints[2], bezCurvePoints[3], 4));
        }
    }
}
