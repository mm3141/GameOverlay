// <copyright file="InGameState.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteObjects.States
{
    using System;
    using System.Collections.Generic;
    using Coroutine;
    using GameHelper.CoroutineEvents;
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
        /// Gets the data related to the current area instance.
        /// </summary>
        public AreaInstance CurrentAreaInstance
        {
            get;
            private set;
        }

        = new AreaInstance(IntPtr.Zero);

        /// <inheritdoc/>
        protected override void CleanUpData()
        {
            this.CurrentAreaInstance.Address = IntPtr.Zero;
        }

        /// <inheritdoc/>
        protected override void UpdateData()
        {
            var reader = Core.Process.Handle;
            var data = reader.ReadMemory<InGameStateOffset>(this.Address);
            this.CurrentAreaInstance.Address = data.LocalData;
        }

        private IEnumerator<Wait> OnPerFrame()
        {
            while (true)
            {
                yield return new Wait(GameHelperEvents.PerFrameDataUpdate);
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
