// <copyright file="UiElementBase.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteObjects.UiElement
{
    using System;
    using System.Collections.Generic;
    using System.Numerics;
    using GameOffsets.Objects.UiElement;

    /// <summary>
    /// Points to the Ui Element of the game and reads its data.
    /// </summary>
    public class UiElementBase : RemoteObjectBase
    {
        private string name;
        private string id;
        private UiElementBase parent;
        private IntPtr[] childrens;
        private float scale;
        private bool relativeIsVisible;
        private Vector2 relativePosition;
        private Vector2 size;

        /// <summary>
        /// Initializes a new instance of the <see cref="UiElementBase"/> class.
        /// </summary>
        /// <param name="address">address to the Ui Element of the game.</param>
        /// <param name="name">user friendly name to give this Ui Element.</param>
        internal UiElementBase(IntPtr address, string name)
            : base(address)
        {
            this.name = name;
        }

        /// <summary>
        /// Gets the Id associated with the UiElement.
        /// Returns empty string in case of no Id found.
        /// </summary>
        public string Id
        {
            get
            {
                if (this.parent != null)
                {
                    return string.Join('.', this.parent.Id, this.id);
                }

                return this.id;
            }

            private set
            {
            }
        }

        /// <summary>
        /// Gets a value indicating whether UiElement is visible or not.
        /// </summary>
        public bool IsVisible
        {
            get
            {
                if (this.parent != null)
                {
                    return this.relativeIsVisible && this.parent.IsVisible;
                }
                else
                {
                    return this.relativeIsVisible;
                }
            }

            private set
            {
            }
        }

        public Vector2 Size => this.size;

        public Vector2 Position
        {
            get
            {
                var scaledPosition = this.Position * this.scale;
                if (this.parent != null)
                {
                    return scaledPosition + this.parent.Position;
                }
                else
                {
                    return scaledPosition;
                }
            }

            private set
            {
            }
        }

        /// <summary>
        /// Reads and updates only the data that is suppose to change.
        /// </summary>
        /// <param name="address">address of this UiElement.</param>
        internal void Update(IntPtr address)
        {
            if (this.Address == address)
            {
                this.UpdateData();
            }
            else
            {
                this.Address = address;
                this.UpdateAll();
            }
        }

        /// <inheritdoc/>
        protected override void CleanUpData()
        {
            this.parent = null;
            this.childrens = null;
            this.relativeIsVisible = false;
            this.relativePosition = Vector2.Zero;
            this.size = Vector2.Zero;
            this.scale = 0f;
        }

        /// <inheritdoc/>
        protected override void UpdateData()
        {
            if (this.parent != null)
            {
                this.parent.UpdateData();
            }

            var reader = Core.Process.Handle;
            var data = reader.ReadMemory<UiElementBaseOffset>(this.Address);
            this.relativeIsVisible = (data.RelativeIsVisible & 1024) == 1024;
            this.relativePosition.X = data.RelativePosition.X;
            this.relativePosition.Y = data.RelativePosition.Y;
            (this.size.X, this.size.Y) = (data.Size.X, data.Size.Y);
            this.scale = data.Scale;
        }

        private void UpdateAll()
        {
            var reader = Core.Process.Handle;
            var data = reader.ReadMemory<UiElementBaseOffset>(this.Address);
            if (data.Self != this.Address)
            {
                throw new Exception($"{this.Address.ToInt64():X} is not pointing to" +
                    $" the {this.name} Ui Element. Please fix the offsets.");
            }

            this.relativeIsVisible = (data.RelativeIsVisible & 1024) == 1024;
            this.relativePosition.X = data.RelativePosition.X;
            this.relativePosition.Y = data.RelativePosition.Y;
            (this.size.X, this.size.Y) = (data.Size.X, data.Size.Y);
            this.scale = data.Scale;
            this.childrens = reader.ReadStdVector<IntPtr>(data.ChildrensPtr0);
            this.id = reader.ReadStdWString(data.Id);
            this.parent = null;
            if (data.ParentPtr != IntPtr.Zero)
            {
                this.parent = new UiElementBase(data.ParentPtr, $"parentOf{this.name}");
            }
        }
    }
}
