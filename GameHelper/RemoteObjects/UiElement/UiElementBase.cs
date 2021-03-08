// <copyright file="UiElementBase.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteObjects.UiElement
{
    using System;
    using System.Numerics;
    using GameOffsets.Objects.UiElement;

    /// <summary>
    /// Points to the Ui Element of the game and reads its data.
    /// </summary>
    public class UiElementBase : RemoteObjectBase
    {
        private Vector2 positionModifier = Vector2.Zero;
        private string id = string.Empty;
        private byte scaleIndex = 0x00;
        private Vector2 relativePosition = Vector2.Zero;
        private float localScaleMultiplier = 0x00f;
        private uint flags = 0x00;
        private Vector2 unScaledSize = Vector2.Zero;

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
        public Vector2 Postion
        {
            get
            {
                var myScale = Core.GameWScale.GetScaleValue(
                    this.scaleIndex, this.localScaleMultiplier);
                var pos = this.GetUnScaledPosition();
                pos.X *= myScale.WidthScale;
                pos.Y *= myScale.HeightScale;
                return pos;
            }

            private set
            {
            }
        }

        /// <summary>
        /// Gets the size of the Ui Element w.r.t the game UI.
        /// </summary>
        public Vector2 Size
        {
            get
            {
                var scale = Core.GameWScale.GetScaleValue(
                    this.scaleIndex, this.localScaleMultiplier);
                var size = this.unScaledSize;
                size.X *= scale.WidthScale;
                size.Y *= scale.HeightScale;
                return size;
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
        public int TotalChildrens => this.ChildrenAddresses.Length;

        /// <summary>
        /// Gets the parent of the UiElement.
        /// </summary>
        internal UiElementBase Parent { get; private set; } = null;

        /// <summary>
        /// Gets the childrens pointers of the UiElements.
        /// </summary>
        internal IntPtr[] ChildrenAddresses
        {
            get;
            private set;
        }

        = new IntPtr[0];

        /// <summary>
        /// Gets the child Ui Element at specified index.
        /// </summary>
        /// <param name="i">index of the child Ui Element.</param>
        /// <returns>the child Ui Element.</returns>
        public UiElementBase this[int i]
        {
            get
            {
                return new UiElementBase(this.ChildrenAddresses[i]);
            }
        }

        /// <inheritdoc/>
        protected override void CleanUpData()
        {
            this.Parent = null;
            this.ChildrenAddresses = new IntPtr[0];
        }

        /// <inheritdoc/>
        protected override void UpdateData(bool hasAddressChanged)
        {
            var reader = Core.Process.Handle;
            var data = reader.ReadMemory<UiElementBaseOffset>(this.Address);
            if (data.Self != this.Address)
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

            this.ChildrenAddresses = reader.ReadStdVector<IntPtr>(data.ChildrensPtr);
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
                var parentScale = Core.GameWScale.GetScaleValue(
                    this.Parent.scaleIndex, this.Parent.localScaleMultiplier);
                var myScale = Core.GameWScale.GetScaleValue(
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
