// <copyright file="InGameState.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteObjects.States
{
    using System;
    using System.Collections.Generic;
    using System.Numerics;
    using Coroutine;
    using GameHelper.CoroutineEvents;
    using GameHelper.RemoteObjects.States.InGameStateObjects;
    using GameHelper.RemoteObjects.UiElement;
    using GameOffsets.Objects.States;
    using ImGuiNET;

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
            Core.CoroutinesRegistrar.Add(CoroutineHandler.Start(
                this.OnPerFrame(), "[InGameState] Update Game State"));
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

        /// <summary>
        /// Gets the important Ui Elements required by the plugins
        /// to work properly. This is just a shortcut so plugins
        /// don't have to triverse the Ui Elements from the UiRoot.
        /// </summary>
        public ImportantUiElements UiImportantElements
        {
            get;
            private set;
        }

        = new ImportantUiElements(IntPtr.Zero);

        /// <summary>
        /// Gets the Window to Screen Matrix.
        /// </summary>
        public Matrix4x4 WindowToScreenMatrix
        {
            get;
            private set;
        }

        = Matrix4x4.Identity;

        /// <summary>
        /// Gets the data related to the root ui element.
        /// </summary>
        internal UiElementBase UiRoot
        {
            get;
            private set;
        }

        = new UiElementBase(IntPtr.Zero);

        /// <summary>
        /// Converts the <see cref="InGameState"/> class data to ImGui.
        /// </summary>
        internal override void ToImGui()
        {
            base.ToImGui();
            if (ImGui.TreeNode("WindowToScreenMatrix"))
            {
                var d = this.WindowToScreenMatrix;
                ImGui.Text($"{d.M11:0.00}\t{d.M12:0.00}\t{d.M13:0.00}\t{d.M14:0.00}");
                ImGui.Text($"{d.M21:0.00}\t{d.M22:0.00}\t{d.M23:0.00}\t{d.M24:0.00}");
                ImGui.Text($"{d.M31:0.00}\t{d.M32:0.00}\t{d.M33:0.00}\t{d.M34:0.00}");
                ImGui.Text($"{d.M41:0.00}\t{d.M42:0.00}\t{d.M43:0.00}\t{d.M44:0.00}");
                ImGui.TreePop();
            }
        }

        /// <inheritdoc/>
        protected override void CleanUpData()
        {
            this.CurrentAreaInstance.Address = IntPtr.Zero;
            this.UiRoot.Address = IntPtr.Zero;
            this.WindowToScreenMatrix = Matrix4x4.Identity;
        }

        /// <inheritdoc/>
        protected override void UpdateData(bool hasAddressChanged)
        {
            var reader = Core.Process.Handle;
            var data = reader.ReadMemory<InGameStateOffset>(this.Address);
            this.CurrentAreaInstance.Address = data.LocalData;
            this.UiRoot.Address = data.UiRootPtr;
            if (this.WindowToScreenMatrix != data.WindowToScreenMatrix)
            {
                this.WindowToScreenMatrix = data.WindowToScreenMatrix;
            }
        }

        private IEnumerator<Wait> OnPerFrame()
        {
            while (true)
            {
                yield return new Wait(GameHelperEvents.PerFrameDataUpdate);
                if (this.Address != IntPtr.Zero)
                {
#if RELEASE
                    if (Core.States.GameCurrentState == RemoteEnums.GameStateTypes.InGameState)
#endif
                    {
                        this.UpdateData(false);
                    }
                }
            }
        }
    }
}
