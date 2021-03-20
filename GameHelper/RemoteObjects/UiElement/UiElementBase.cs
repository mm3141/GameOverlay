// <copyright file="UiElementBase.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteObjects.UiElement
{
    using System;
    using System.Numerics;
    using GameHelper.Ui;
    using GameHelper.Utils;
    using GameOffsets.Objects.UiElement;
    using ImGuiNET;

    /// <summary>
    /// Points to the Ui Element of the game and reads its data.
    /// </summary>
    public class UiElementBase : RemoteObjectBase
    {
#pragma warning disable SA1401, SA1600 // Fields should be private
        protected IntPtr[] childrenAddresses = new IntPtr[0];
        protected Vector2 positionModifier = Vector2.Zero;
        protected string id = string.Empty;
        protected byte scaleIndex = 0x00;
        protected Vector2 relativePosition = Vector2.Zero;
        protected float localScaleMultiplier = 0x00;
        protected uint flags = 0x00;
        protected Vector2 unScaledSize = Vector2.Zero;
#pragma warning restore SA1401, SA1600 // Fields should be private
        private bool show = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="UiElementBase"/> class.
        /// </summary>
        /// <param name="address">address to the Ui Element of the game.</param>
        internal UiElementBase(IntPtr address)
            : base(address, true)
        {
        }

        /// <summary>
        /// Gets the Id of the Ui Element.
        /// </summary>
        public string Id
        {
            get
            {
                if (this.Parent != null)
                {
                    return string.Join('.', this.Parent.Id, this.id);
                }
                else
                {
                    return this.id;
                }
            }

            private set
            {
            }
        }

        /// <summary>
        /// Gets the position of the Ui Element w.r.t the game UI.
        /// </summary>
        public virtual Vector2 Postion
        {
            get
            {
                var myScale = Core.GameScale.GetScaleValue(
                    this.scaleIndex, this.localScaleMultiplier);
                var pos = this.GetUnScaledPosition();
                pos.X *= myScale.WidthScale;
                pos.Y *= myScale.HeightScale;
                return pos / Core.GHSettings.WindowScale;
            }

            private set
            {
            }
        }

        /// <summary>
        /// Gets the size of the Ui Element w.r.t the game UI.
        /// </summary>
        public virtual Vector2 Size
        {
            get
            {
                var scale = Core.GameScale.GetScaleValue(
                    this.scaleIndex, this.localScaleMultiplier);
                var size = this.unScaledSize;
                size.X *= scale.WidthScale;
                size.Y *= scale.HeightScale;
                return size / Core.GHSettings.WindowScale;
            }

            private set
            {
            }
        }

        /// <summary>
        /// Gets a value indicating whether the Ui Element is visible or not.
        /// </summary>
        public bool IsVisible
        {
            get
            {
                if (this.Parent == null)
                {
                    return UiElementBaseFuncs.IsVisibleChecker(this.flags);
                }
                else
                {
                    return UiElementBaseFuncs.IsVisibleChecker(this.flags) &&
                        this.Parent.IsVisible;
                }
            }

            private set
            {
            }
        }

        /// <summary>
        /// Gets the total number of childrens this Ui Element has.
        /// </summary>
        public int TotalChildrens => this.childrenAddresses.Length;

        /// <summary>
        /// Gets the parent of the UiElement. Will be null in case of no parent.
        /// </summary>
        public UiElementBase Parent { get; private set; } = null;

        /// <summary>
        /// Gets the child Ui Element at specified index.
        /// returns null in case of invalid index.
        /// </summary>
        /// <param name="i">index of the child Ui Element.</param>
        /// <returns>the child Ui Element.</returns>
        public UiElementBase this[int i]
        {
            get
            {
                if (this.childrenAddresses.Length <= i)
                {
                    return null;
                }

                return new UiElementBase(this.childrenAddresses[i]);
            }
        }

        /// <summary>
        /// Converts the <see cref="UiElementBase"/> class data to ImGui.
        /// </summary>
        internal override void ToImGui()
        {
            ImGui.Checkbox("Show", ref this.show);
            ImGui.SameLine();
            if (ImGui.Button("Explore"))
            {
                GameUiExplorer.AddUiElement(this);
            }

            base.ToImGui();
            if (this.show)
            {
                UiHelper.DrawRect(this.Postion, this.Size, 255, 255, 0);
            }

            ImGui.Text($"Id {this.Id}");
            ImGui.Text($"Position  {this.Postion}");
            ImGui.Text($"Size  {this.Size}");
            ImGui.Text($"IsVisible  {this.IsVisible}");
            ImGui.Text($"Total Childrens  {this.TotalChildrens}");
            ImGui.Text($"Parent  {this.Parent}");
            ImGui.Text($"Position Modifier {this.positionModifier}");
            ImGui.Text($"Scale Indenx {this.scaleIndex}");
            ImGui.Text($"Local Scale Multiplier {this.localScaleMultiplier}");
            ImGui.Text($"Flags: {this.flags:X}");
    }

        /// <inheritdoc/>
        protected override void CleanUpData()
        {
            this.Parent = null;
            this.childrenAddresses = new IntPtr[0];
        }

        /// <inheritdoc/>
        protected override void UpdateData(bool hasAddressChanged)
        {
            var reader = Core.Process.Handle;
            var data = reader.ReadMemory<UiElementBaseOffset>(this.Address);
            if (data.Self != IntPtr.Zero && data.Self != this.Address)
            {
                throw new Exception($"This (address: {this.Address.ToInt64():X})" +
                    $"is not a Ui Element. Self Address = {data.Self.ToInt64():X}");
            }

            if (data.ParentPtr != IntPtr.Zero)
            {
                if (hasAddressChanged)
                {
                    this.Parent = new UiElementBase(data.ParentPtr);
                }
                else
                {
                    this.Parent.Address = data.ParentPtr;
                }
            }

            this.childrenAddresses = reader.ReadStdVector<IntPtr>(data.ChildrensPtr);
            if (hasAddressChanged)
            {
                this.id = reader.ReadStdWString(data.Id);
            }

            this.positionModifier.X = data.PositionModifier.X;
            this.positionModifier.Y = data.PositionModifier.Y;

            this.scaleIndex = data.ScaleIndex;
            this.localScaleMultiplier = data.LocalScaleMultiplier;
            this.flags = data.Flags;

            this.relativePosition.X = data.RelativePosition.X;
            this.relativePosition.Y = data.RelativePosition.Y;

            this.unScaledSize.X = data.UnscaledSize.X;
            this.unScaledSize.Y = data.UnscaledSize.Y;
        }

        /// <summary>
        /// This function was basically parsed/read/decompiled from the game.
        /// To find this function in the game, follow the data used in this function.
        /// Although, this function haven't changed since last 3-4 years.
        /// </summary>
        /// <returns>Returns position without applying current element scaling values.</returns>
        private Vector2 GetUnScaledPosition()
        {
            if (this.Parent == null)
            {
                return this.relativePosition;
            }

            var parentPos = this.Parent.GetUnScaledPosition();
            if (UiElementBaseFuncs.ShouldModifyPos(this.flags))
            {
                parentPos += this.Parent.positionModifier;
            }

            if (this.Parent.scaleIndex == this.scaleIndex &&
                this.Parent.localScaleMultiplier == this.localScaleMultiplier)
            {
                return parentPos + this.relativePosition;
            }
            else
            {
                var parentScale = Core.GameScale.GetScaleValue(
                    this.Parent.scaleIndex, this.Parent.localScaleMultiplier);
                var myScale = Core.GameScale.GetScaleValue(
                    this.scaleIndex, this.localScaleMultiplier);
                Vector2 myPos;
                myPos.X = (parentPos.X * parentScale.WidthScale / myScale.WidthScale)
                    + this.relativePosition.X;
                myPos.Y = (parentPos.Y * parentScale.HeightScale / myScale.HeightScale)
                    + this.relativePosition.Y;
                return myPos;
            }
        }
    }
}
