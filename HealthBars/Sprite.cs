namespace HealthBars
{
    using System.Numerics;

    /// <summary>
    ///     Sprite class.
    /// </summary>
    public class Sprite
    {
        /// <summary>
        ///     Sprite virtual outer space.
        /// </summary>
        public readonly Vector2 VirtualBounds;

        /// <summary>
        ///     Initialization of <see cref="Sprite" /> instance.
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="spriteSheetSize"></param>
        /// <param name="virtualBounds"> </param>
        public Sprite(CubeObject frame, CubeObject spriteSheetSize, Vector2 virtualBounds)
        {
            this.VirtualBounds = virtualBounds;
            
            this.X = frame.X;
            this.Y = frame.Y;
            this.W = frame.W;
            this.H = frame.H;

            this.Uv = new Vector2[]
            {
                new(this.X / spriteSheetSize.W, this.Y / spriteSheetSize.H),
                new((this.X + this.W) / spriteSheetSize.W, (this.Y + this.H) / spriteSheetSize.H)
            };
        }

        /// <summary>
        ///     Precalculated uv.
        /// </summary>
        public Vector2[] Uv { get; }

        private float H { get; }
        private float W { get; }
        private float X { get; }
        private float Y { get; }
    }
}