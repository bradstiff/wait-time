﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using WaitTime.Entities;

namespace WaitTime.Components
{
    public class TrackSimplifier
    {
        public static IList<ActivityLocation> Simplify(IList<ActivityLocation> points, double tolerance = 1.0)
        {
            var simplifier2D = new Simplifier2D<ActivityLocation>(l => l.Longitude, y => y.Latitude);
            return simplifier2D.Simplify(points, tolerance, true);
        }
    }

    public class Simplifier2D<T> : BaseSimplifier<T>
    {
        readonly Func<T, double> _xSelector;
        readonly Func<T, double> _ySelector;

        public Simplifier2D(Func<T, double> xSelector, Func<T, double> ySelector) :
            base((point1, point2) => xSelector(point1) == xSelector(point2) && ySelector(point1) == ySelector(point2))
        {
            _xSelector = xSelector;
            _ySelector = ySelector;
        }

        protected override double GetSquareDistance(T p1, T p2)
        {
            double dx = _xSelector(p1) - _xSelector(p2);
            double dy = _ySelector(p1) - _ySelector(p2);

            return dx * dx + dy * dy;
        }

        protected override double GetSquareSegmentDistance(T p0, T p1, T p2)
        {
            double x1 = _xSelector(p1);
            double y1 = _ySelector(p1);
            double x2 = _xSelector(p2);
            double y2 = _ySelector(p2);
            double x0 = _xSelector(p0);
            double y0 = _ySelector(p0);

            double dx = x2 - x1;
            double dy = y2 - y1;

            double t;

            if (dx != 0.0d || dy != 0.0d)
            {
                t = ((x0 - x1) * dx + (y0 - y1) * dy)
                        / (dx * dx + dy * dy);

                if (t > 1.0d)
                {
                    x1 = x2;
                    y1 = y2;
                }
                else if (t > 0.0d)
                {
                    x1 += dx * t;
                    y1 += dy * t;
                }
            }

            dx = x0 - x1;
            dy = y0 - y1;

            return dx * dx + dy * dy;
        }
    }

    public abstract class BaseSimplifier<T>
    {
        private class Range
        {
            public int First { get; }
            public int Last { get; }

            public Range(int first, int last)
            {
                First = first;
                Last = last;
            }
        }

        protected BaseSimplifier(Func<T, T, Boolean> equalityChecker)
        {
            _equalityChecker = equalityChecker;
        }

        Func<T, T, Boolean> _equalityChecker;

        /// <summary>
        /// Simplified data points
        /// </summary>
        /// <param name="points">Points to be simplified</param>
        /// <param name="tolerance">Amount of wiggle to be tolerated between coordinates.</param>
        /// <param name="highestQuality">
        /// True for Douglas-Peucker. 
        /// False for Radial-Distance before Douglas-Peucker (Runs Faster)
        /// </param>
        /// <returns>Simplified points</returns>
        public IList<T> Simplify(IList<T> points,
                            double tolerance,
                            bool highestQuality)
        {

            if (points == null || points.Count <= 2)
            {
                return points;
            }

            double sqTolerance = tolerance * tolerance;

            if (!highestQuality)
            {
                points = SimplifyRadialDistance(points, sqTolerance);
            }

            points = SimplifyDouglasPeucker(points, sqTolerance);

            return points;
        }

        IList<T> SimplifyRadialDistance(IList<T> points, double sqTolerance)
        {
            T point = default(T);
            T prevPoint = points[0];

            IList<T> newPoints = new List<T>();
            newPoints.Add(prevPoint);

            for (int i = 1; i < points.Count; ++i)
            {
                point = points[i];

                if (GetSquareDistance(point, prevPoint) > sqTolerance)
                {
                    newPoints.Add(point);
                    prevPoint = point;
                }
            }

            if (!_equalityChecker(prevPoint, point))
            {
                newPoints.Add(point);
            }

            return newPoints.ToArray();
        }

        IList<T> SimplifyDouglasPeucker(IList<T> points, double sqTolerance)
        {

            BitArray bitArray = new BitArray(points.Count);
            bitArray.Set(0, true);
            bitArray.Set(points.Count - 1, true);

            Stack<Range> stack = new Stack<Range>();
            stack.Push(new Range(0, points.Count - 1));

            while (stack.Count > 0)
            {
                Range range = stack.Pop();

                int index = -1;
                double maxSqDist = 0f;

                // Find index of point with maximum square distance from first and last point
                for (int i = range.First + 1; i < range.Last; ++i)
                {
                    double sqDist = GetSquareSegmentDistance(
                        points[i], points[range.First], points[range.Last]);

                    if (sqDist > maxSqDist)
                    {
                        index = i;
                        maxSqDist = sqDist;
                    }
                }

                if (maxSqDist > sqTolerance)
                {
                    bitArray.Set(index, true);

                    stack.Push(new Range(range.First, index));
                    stack.Push(new Range(index, range.Last));
                }
            }

            List<T> newPoints = new List<T>(CountNumberOfSetBits(bitArray));

            for (int i = 0; i < bitArray.Count; i++)
            {
                if (bitArray[i])
                {
                    newPoints.Add(points[i]);
                }
            }

            return newPoints.ToArray();
        }

        int CountNumberOfSetBits(BitArray bitArray)
        {
            int counter = 0;
            for (int i = 0; i < bitArray.Length; i++)
            {
                if (bitArray[i])
                {
                    counter++;
                }
            }
            return counter;
        }

        protected abstract double GetSquareDistance(T p1, T p2);
        protected abstract double GetSquareSegmentDistance(T p0, T p1, T p2);
    }
}