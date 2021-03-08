// <copyright file="Positioned.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteObjects.Components
{
    using System;
    using GameOffsets.Natives;
    using GameOffsets.Objects.Components;

    /// <summary>
    /// PathNames associated with the <see cref="Positioned"/> component in the entity.
    /// </summary>
    public class Positioned : RemoteObjectBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Positioned"/> class.
        /// </summary>
        /// <param name="address">address of the <see cref="Positioned"/> component.</param>
        public Positioned(IntPtr address)
            : base(address, true)
        {
        }

        /// <summary>
        /// Gets the flag to figure out if the entity is hostile or not.
        /// NOTE: For isHostile flag do bitwise-and with 0x7F.
        /// </summary>
        public byte Reaction { get; private set; } = 0x00;

        /// <summary>
        /// Gets the grid position of the entity.
        /// </summary>
        public StdTuple2D<int> GridPosition { get; private set; } = default;

        /// <summary>
        /// Gets the world position of the entity.
        /// </summary>
        public StdTuple2D<float> WorldPosition { get; private set; } = default;

        /// <inheritdoc/>
        protected override void UpdateData(bool hasAddressChanged)
        {
            var reader = Core.Process.Handle;
            var data = reader.ReadMemory<PositionedOffset>(this.Address);
            this.Reaction = data.Reaction;
            this.GridPosition = data.GridPosition;
            this.WorldPosition = data.WorldPosition;
        }

        /// <inheritdoc/>
        protected override void CleanUpData()
        {
            throw new NotImplementedException("Component Address should never be Zero.");
        }
    }
}
