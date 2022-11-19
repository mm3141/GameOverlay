// <copyright file="MathHelper.cs"  company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.Utils
{
    using System;
    using System.Numerics;
    using V2 = System.Numerics.Vector2;
    /// <summary>
    ///     Utility functions to help with some math.
    /// </summary>
    public static class MathHelper
    {
        public static V2 Increase(this V2 vector, float dx = 0f, float dy = 0f) {
            return new V2(vector.X + dx, vector.Y + dy);
        }
        /// <summary>
        /// Converts the color into a packed integer.
        /// </summary>
        /// <returns>A packed integer containing all four color components.</returns>
        public static uint ToImgui(this System.Drawing.Color c) {
            int value = c.R;
            value |= c.G << 8;
            value |= c.B << 16;
            value |= c.A << 24;

            return (uint)value;
        }
        public static string ToRoundStr(this double f, int round = 3) {
            return (Math.Round(f, round)).ToString();
        }
        public static string ToRoundStr(this float f, int round = 3) {
            return (Math.Round(f, round)).ToString();
        }
        /// <summary>
        ///     Linearly interpolates between two points.
        ///     Interpolates between the points a and b by the interpolant t.
        ///     The parameter t is clamped to the range[0, 1].
        ///     This is most commonly used to find a point some fraction of the way along a line between two endpoints
        ///     (e.g.to move an object gradually between those points).
        ///     The value returned equals a + (b - a) * t (which can also be written a * (1-t) + b*t).
        ///     When t = 0, Lerp(a, b, t) returns a.
        ///     When t = 1, Lerp(a, b, t) returns b.
        ///     When t = 0.5, Lerp(a, b, t) returns the point midway between a and b.
        /// </summary>
        /// <param name="a">Starting point.</param>
        /// <param name="b">Destination point.</param>
        /// <param name="t">Interpolation ratio.</param>
        /// <returns>Interpolated value, equals to a + (b - a) * t.</returns>
        public static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * t;
        }

        /// <summary>
        ///     Linearly interpolates between two vectors.
        ///     Interpolates between the vectors a and b by the interpolant t.
        ///     The parameter t is clamped to the range[0, 1].
        ///     This is most commonly used to find a point some fraction of the way along a line between two endpoints
        ///     (e.g.to move an object gradually between those points).
        ///     Similar to <see cref="Lerp(float, float, float)" />.
        /// </summary>
        /// <param name="a">Starting point.</param>
        /// <param name="b">Destination point.</param>
        /// <param name="t">Interpolation ratio.</param>
        /// <returns>Interpolated value, equals to a + (b - a) * t.</returns>
        public static Vector2 Lerp(Vector2 a, Vector2 b, float t)
        {
            return new Vector2(Lerp(a.X, b.X, t), Lerp(a.Y, b.Y, t));
        }
    }
}