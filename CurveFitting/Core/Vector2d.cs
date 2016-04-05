using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurveFitting.Core
{
    public class Vector2d
    {
        public double X { get; set; }
        public double Y { get; set; }

        public double SqLength { get { return X * X + Y * Y; } }
        public double Length { get { return Math.Sqrt(SqLength); } }
        public Vector2d Unit { get { return this * (1 / Length); } }

        public Vector2d(double x = 0, double y = 0)
        {
            X = x; Y = y;
        }

        public Vector2d(Point2d p)
        {
            X = p.X; Y = p.Y;
        }

        public Vector2d(Point2d p1, Point2d p2)
        {
            X = p2.X - p1.X;
            Y = p2.Y - p1.Y;
        }

        public static Vector2d operator +(Vector2d v1, Vector2d v2)
        {
            return new Vector2d(v1.X + v2.X, v1.Y + v2.Y);
        }

        public static Vector2d operator +(Vector2d v, Point2d p)
        {
            return new Vector2d(v.X + p.X, v.Y + p.Y);
        }

        public static Vector2d operator -(Vector2d v1, Vector2d v2)
        {
            return new Vector2d(v1.X - v2.X, v1.Y - v2.Y);
        }

        public static Vector2d operator -(Vector2d v, Point2d p)
        {
            return new Vector2d(v.X - p.X, v.Y - p.Y);
        }

        public static Vector2d operator *(Vector2d v, double k)
        {
            return new Vector2d(v.X * k, v.Y * k);
        }

        public static Vector2d operator *(double k, Vector2d v)
        {
            return v * k;
        }

        public static Vector2d operator -(Vector2d v)
        {
            return v * (-1);
        }

        public double Dot(Vector2d v) 
        {
            return X * v.X + Y * v.Y; 
        }

        public double Cross(Vector2d v)
        {
            return X * v.Y - Y * v.X;
        }

        public Point2d GetPoint()
        {
            return new Point2d(X, Y);
        }
    }
}
