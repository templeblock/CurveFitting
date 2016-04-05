using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurveFitting.Core
{
    public class Point2d
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Point2d(double x = 0, double y = 0)
        {
            X = x; Y = y;
        }

        public double SquareDistance(Point2d p)
        {
            return (this.X - p.X) * (this.X - p.X) + (this.Y - p.Y) * (this.Y - p.Y);
        }

        public double Distance(Point2d p)
        {
            return Math.Sqrt(this.SquareDistance(p));
        }

        public double Distance(Point2d p1, Point2d p2)
        {
            double area = Math.Abs(p1.X * p2.Y + p2.X * Y + X * p1.Y - p2.X * p1.Y - X * p2.Y - p1.X * Y);
            double bottom = Math.Sqrt(p1.Distance(p2));
            double height = area / bottom;

            return height;
        }

        public double Distance(Line2d d)
        {
            return Distance(d.P1, d.P2);
        }

    }
}
