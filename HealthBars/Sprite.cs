namespace HealthBars
{
    using System.Numerics;

    /// <summary>
    ///     Sprite class.
    /// </summary>
    public class Sprite
    {
        private readonly CubeObject SpritesheetSize;
        /// <summary>
        ///     Precalculated uv.
        /// </summary>
        public Vector2[] Uv { get; }

        /// <summary>
        ///     Initialization of <see cref="Sprite" /> instance.
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="spritesheetSize"></param>
        public Sprite(CubeObject frame, CubeObject spritesheetSize)
        {
            this.X = frame.X;
            this.Y = frame.Y;
            this.W = frame.W;
            this.H = frame.H;

            this.SpritesheetSize = spritesheetSize;

            this.Uv = new Vector2[]
            {
                new(this.X / this.SpritesheetSize.W, this.Y / this.SpritesheetSize.H),
                new((this.X + this.W) / this.SpritesheetSize.W, (this.Y + this.H) / this.SpritesheetSize.H)
            };
        }

        public float H { get; }
        public float W { get; }
        public float X { get; }
        public float Y { get; }
    }
}