using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurveFitting.Core
{
    public class Line2d
    {
        public Point2d P1 { get; set; }
        public Point2d P2 { get; set; }

        public Line2d(Point2d p1, Point2d p2)
        {
            P1 = p1; P2 = p2;
        }
    }
}
