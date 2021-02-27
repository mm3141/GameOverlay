// <copyright file="InGameState.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteObjects.States
{
    using System;
    using System.Collections.Generic;
    using Coroutine;
    using GameHelper.RemoteEnums;
    using GameHelper.RemoteObjects.States.InGameStateObjects;
    using GameOffsets.Objects.States;

    /// <summary>
    /// Reads InGameState Game Object.
    /// </summary>
    public class InGameState : RemoteObjectBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InGameState"/> class.
        /// </summary>
        /// <param name="address">address of the remote memory object.</param>
        internal InGameState(IntPtr address)
            : base(address)
        {
            CoroutineHandler.Start(this.OnPerFrame());
        }

        /// <summary>
        /// Gets the data related to the current area.
        /// </summary>
        public CurrentAreaData Data
        {
            get;
            private set;
        }

        = new CurrentAreaData(IntPtr.Zero);

        /// <inheritdoc/>
        protected override void CleanUpData()
        {
            this.Data.Address = IntPtr.Zero;
        }

        /// <inheritdoc/>
        protected override void UpdateData()
        {
            var reader = Core.Process.Handle;
            var data = reader.ReadMemory<InGameStateOffset>(this.Address);
            this.Data.Address = data.LocalData;
        }

        private IEnumerator<Wait> OnPerFrame()
        {
            yield return new Wait(0);
            while (true)
            {
                yield return new Wait(GameOverlay.PerFrameDataUpdate);
                if (this.Address != IntPtr.Zero
                    && Core.States.CurrentStateInGame.Name == GameStateTypes.InGameState)
                {
                    this.UpdateData();
                }
                else
                {
                    this.CleanUpData();
                }
            }
        }
    }
}
