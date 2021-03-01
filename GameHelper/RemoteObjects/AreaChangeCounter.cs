// <copyright file="AreaChangeCounter.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteObjects
{
    using System;
    using System.Collections.Generic;
    using Coroutine;
    using GameHelper.CoroutineEvents;
    using GameHelper.RemoteEnums;
    using GameOffsets.Objects;

    /// <summary>
    /// Points to the AreaChangeCounter object and read/cache it's value
    /// on every area change.
    /// </summary>
    public class AreaChangeCounter : RemoteObjectBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AreaChangeCounter"/> class.
        /// </summary>
        /// <param name="address">address of the remote memory object.</param>
        internal AreaChangeCounter(IntPtr address)
            : base(address)
        {
            CoroutineHandler.Start(this.OnAreaChange());
            CoroutineHandler.Start(this.OnGameStateChange());
        }

        /// <summary>
        /// Gets the cached value of the AreaChangeCounter.
        /// </summary>
        public int Value { get; private set; } = 0x00;

        /// <inheritdoc/>
        protected override void CleanUpData()
        {
            this.Value = 0x00;
        }

        /// <inheritdoc/>
        protected override void UpdateData()
        {
            var reader = Core.Process.Handle;
            this.Value = reader.ReadMemory<AreaChangeOffset>(this.Address).counter;
        }

        private IEnumerator<Wait> OnAreaChange()
        {
            while (true)
            {
                yield return new Wait(RemoteEvents.AreaChangeDetected);
                if (this.Address != IntPtr.Zero)
                {
                    this.UpdateData();
                }
            }
        }

        private IEnumerator<Wait> OnGameStateChange()
        {
            while (true)
            {
                yield return new Wait(RemoteEvents.StateChanged);
                if (Core.States.CurrentStateInGame.Name != GameStateTypes.InGameState
                    && Core.States.CurrentStateInGame.Name != GameStateTypes.EscapeState
                    && Core.States.CurrentStateInGame.Name != GameStateTypes.AreaLoadingState)
                {
                    this.CleanUpData();
                }
            }
        }
    }
}
