// <copyright file="CurrentState.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteObjects
{
    using System;
    using System.Collections.Generic;
    using Coroutine;
    using GameHelper.CoroutineEvents;
    using GameHelper.RemoteEnums;
    using GameHelper.Utils;
    using GameOffsets.Controller;
    using GameOffsets.Native;

    /// <summary>
    /// Points to the current state information object in
    /// the game and cache it's value.
    /// </summary>
    internal class CurrentState : RemoteObjectBase
    {
        private IntPtr currentStateAddress = IntPtr.Zero;
        private GameStateTypes name = GameStateTypes.GameNotLoaded;

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentState"/> class.
        /// </summary>
        /// <param name="address">address of the remote memory object.</param>
        internal CurrentState(IntPtr address)
            : base(address)
        {
            CoroutineHandler.Start(this.OnPerFrame());
        }

        /// <summary>
        /// Gets the current state name.
        /// </summary>
        internal GameStateTypes Name
        {
            get => this.name;
            private set
            {
                if (this.name != value)
                {
                    this.name = value;
                    CoroutineHandler.RaiseEvent(RemoteEvents.StateChanged);
                }
            }
        }

        /// <inheritdoc/>
        protected override void CleanUpData()
        {
            this.currentStateAddress = IntPtr.Zero;
            this.Name = GameStateTypes.GameNotLoaded;
        }

        /// <inheritdoc/>
        protected override void UpdateData()
        {
            var reader = Core.Process.Handle;
            var data = reader.ReadMemory<GameStateOffset>(this.Address);
            IntPtr stateAddress = this.GetSecondLastPtr(reader, data.CurrentStateOffset1);
            if (stateAddress != IntPtr.Zero && stateAddress != this.currentStateAddress)
            {
                this.currentStateAddress = stateAddress;
                foreach (var state in Core.States.AllStates)
                {
                    if (state.Value == stateAddress)
                    {
                        this.name = this.ConvertStringToEnum(state.Key);
                        break;
                    }
                }
            }
        }

        private IEnumerator<Wait> OnPerFrame()
        {
            while (true)
            {
                yield return new Wait(GameHelperEvents.PerFrameDataUpdate);
                if (this.Address != IntPtr.Zero)
                {
                    this.UpdateData();
                }
                else
                {
                    this.CleanUpData();
                }
            }
        }

        private GameStateTypes ConvertStringToEnum(string data)
        {
            if (Enum.TryParse<GameStateTypes>(data, out var result))
            {
                return result;
            }
            else
            {
                throw new Exception($"New GameStateTypes discovered: {data}");
            }
        }

        private IntPtr GetSecondLastPtr(SafeMemoryHandle reader, StdVector vector)
        {
            return reader.ReadMemory<IntPtr>(vector.Last - 0x10);
        }
    }
}
