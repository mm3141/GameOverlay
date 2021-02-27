// <copyright file="AreaChangeCounter.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteObjects
{
    using System;
    using System.Collections.Generic;
    using Coroutine;
    using GameHelper.RemoteEnums;

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
            this.Value = reader.ReadMemory<int>(this.Address);
        }

        private IEnumerator<Wait> OnAreaChange()
        {
            yield return new Wait(0);
            while (true)
            {
                yield return new Wait(Core.States.AreaLoading.AreaChangeDetected);
                if (this.Address != IntPtr.Zero)
                {
                    this.UpdateData();
                }
            }
        }

        private IEnumerator<Wait> OnGameStateChange()
        {
            yield return new Wait(0);
            while (true)
            {
                yield return new Wait(Core.States.CurrentStateInGame.StateChanged);
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
