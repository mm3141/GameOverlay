// <copyright file="Actor.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteObjects.Components
{
    using System;
    using GameOffsets.Objects.Components;
    using ImGuiNET;
    using RemoteEnums;

    /// <summary>
    ///     The <see cref="Actor" /> component in the entity.
    /// </summary>
    public class Actor : RemoteObjectBase
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Actor" /> class.
        /// </summary>
        /// <param name="address">address of the <see cref="Actor" /> component.</param>
        public Actor(IntPtr address)
            : base(address, true) { }

        /// <summary>
        ///     Gets a value indicating what the player is doing.
        /// </summary>
        public Animation Animation { get; private set; } = Animation.Idle;

        /// <summary>
        ///     Converts the <see cref="Actor" /> class data to ImGui.
        /// </summary>
        internal override void ToImGui()
        {
            base.ToImGui();
            ImGui.Text($"AnimationId: {(int)this.Animation}, Animation: {this.Animation}");
        }

        /// <inheritdoc />
        protected override void CleanUpData()
        {
            throw new Exception("Component Address should never be Zero.");
        }

        /// <inheritdoc />
        protected override void UpdateData(bool hasAddressChanged)
        {
            var reader = Core.Process.Handle;
            var data = reader.ReadMemory<ActorOffset>(this.Address);
            this.Animation = (Animation)data.AnimationId;
        }
    }
}