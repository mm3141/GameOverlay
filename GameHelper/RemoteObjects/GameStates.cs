// <copyright file="GameStates.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteObjects
{
    using System;
    using System.Collections.Generic;
    using Coroutine;
    using GameHelper.CoroutineEvents;
    using GameHelper.RemoteEnums;
    using GameHelper.RemoteObjects.States;
    using GameHelper.Utils;
    using GameOffsets.Natives;
    using GameOffsets.Objects;
    using ImGuiNET;

    /// <summary>
    /// Reads and stores the global states of the game.
    /// </summary>
    public class GameStates : RemoteObjectBase
    {
        private GameStateStaticOffset myStaticObj = default;
        private IntPtr currentStateAddress = IntPtr.Zero;
        private GameStateTypes currentStateName = GameStateTypes.GameNotLoaded;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameStates"/> class.
        /// </summary>
        /// <param name="address">address of the remote memory object.</param>
        internal GameStates(IntPtr address)
            : base(address)
        {
            CoroutineHandler.Start(this.OnPerFrame());
        }

        /// <summary>
        /// Gets a dictionary containing all the Game States addresses.
        /// </summary>
        public Dictionary<string, IntPtr> AllStates
        {
            get;
            private set;
        }

        = new Dictionary<string, IntPtr>();

        /// <summary>
        /// Gets the AreaLoadingState object.
        /// </summary>
        public AreaLoadingState AreaLoading
        {
            get;
            private set;
        }

        = new AreaLoadingState(IntPtr.Zero);

        /// <summary>
        /// Gets the InGameState Object.
        /// </summary>
        public InGameState InGameStateObject
        {
            get;
            private set;
        }

        = new InGameState(IntPtr.Zero);

        /// <summary>
        /// Gets the current state the game is in.
        /// </summary>
        public GameStateTypes GameCurrentState
        {
            get => this.currentStateName;
            private set
            {
                if (this.currentStateName != value)
                {
                    this.currentStateName = value;
                    if (value != GameStateTypes.GameNotLoaded)
                    {
                        CoroutineHandler.RaiseEvent(RemoteEvents.StateChanged);
                    }
                }
            }
        }

        /// <summary>
        /// Converts the <see cref="GameStates"/> class data to ImGui.
        /// </summary>
        internal override void ToImGui()
        {
            base.ToImGui();
            if (ImGui.TreeNode("All States Info"))
            {
                foreach (var state in this.AllStates)
                {
                    UiHelper.IntPtrToImGui(state.Key, state.Value);
                }

                ImGui.TreePop();
            }

            ImGui.Text($"Current State: {this.GameCurrentState}");
        }

        /// <inheritdoc/>
        protected override void UpdateData(bool hasAddressChanged)
        {
            var reader = Core.Process.Handle;
            if (hasAddressChanged)
            {
                this.myStaticObj = reader.ReadMemory<GameStateStaticOffset>(this.Address);
                var data = reader.ReadMemory<GameStateOffset>(this.myStaticObj.GameState);
                var states = reader.ReadStdMapAsList<StdWString, IntPtr>(data.States, true);
                for (int i = 0; i < states.Count; i++)
                {
                    var state = states[i];
                    string name = reader.ReadStdWString(state.Key);
                    this.UpdateKnownStatesObjects(name, state.Value);
                    if (this.AllStates.ContainsKey(name))
                    {
                        this.AllStates[name] = state.Value;
                    }
                    else
                    {
                        this.AllStates.Add(name, state.Value);
                    }
                }
            }
            else
            {
                var data = reader.ReadMemory<GameStateOffset>(this.myStaticObj.GameState);
                var cStateAddr = reader.ReadMemory<IntPtr>(data.CurrentStatePtr.Last - 0x10); // Get 2nd-last ptr.
                if (cStateAddr != IntPtr.Zero && cStateAddr != this.currentStateAddress)
                {
                    this.currentStateAddress = cStateAddr;
                    foreach (var state in Core.States.AllStates)
                    {
                        if (state.Value == cStateAddr)
                        {
                            this.currentStateName = this.ConvertStringToEnum(state.Key);
                            break;
                        }
                    }
                }
            }
        }

        /// <inheritdoc/>
        protected override void CleanUpData()
        {
            this.myStaticObj = default;
            this.currentStateAddress = IntPtr.Zero;
            this.GameCurrentState = GameStateTypes.GameNotLoaded;
            this.AllStates.Clear();
            this.AreaLoading.Address = IntPtr.Zero;
            this.InGameStateObject.Address = IntPtr.Zero;
        }

        /// <summary>
        /// Updates the known states Objects and silently skips the unknown ones.
        /// </summary>
        /// <param name="name">State name.</param>
        /// <param name="address">State address.</param>
        private void UpdateKnownStatesObjects(string name, IntPtr address)
        {
            switch (name)
            {
                case "AreaLoadingState":
                    this.AreaLoading.Address = address;
                    break;
                case "InGameState":
                    this.InGameStateObject.Address = address;
                    break;
                default:
                    break;
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

        private IEnumerator<Wait> OnPerFrame()
        {
            while (true)
            {
                yield return new Wait(GameHelperEvents.PerFrameDataUpdate);
                if (this.Address != IntPtr.Zero)
                {
                    this.UpdateData(false);
                }
            }
        }
    }
}
