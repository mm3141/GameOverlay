// <copyright file="RemoteObjectBase.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteObjects
{
    using System;
    using System.Reflection;
    using GameHelper.Utils;
    using ImGuiNET;

    /// <summary>
    /// Points to a Memory location and reads/understands all the data from there.
    /// CurrentAreaInstance in remote memory location changes w.r.t time or event. Due to this,
    /// each remote memory object requires to implement a time/event based coroutine.
    /// </summary>
    public abstract class RemoteObjectBase
    {
        private readonly bool forceUpdate;
        private IntPtr address;

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteObjectBase"/> class.
        /// </summary>
        internal RemoteObjectBase()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteObjectBase"/> class.
        /// </summary>
        /// <param name="address">address of the remote memory object.</param>
        /// <param name="forceUpdate">
        /// True in case the object should be updated even if address hasn't changed.
        /// </param>
        internal RemoteObjectBase(IntPtr address, bool forceUpdate = false)
        {
            this.forceUpdate = forceUpdate;
            this.Address = address;
        }

        /// <summary>
        /// Gets or sets the address of the memory location.
        /// </summary>
        public IntPtr Address
        {
            get => this.address;
            set
            {
                bool hasAddressChanged = this.address != value;
                if (hasAddressChanged || this.forceUpdate)
                {
                    this.address = value;
                    if (value == IntPtr.Zero)
                    {
                        this.CleanUpData();
                    }
                    else
                    {
                        this.UpdateData(hasAddressChanged);
                    }
                }
            }
        }

        /// <summary>
        /// Converts the <see cref="RemoteObjectBase"/> to ImGui Widget via reflection.
        /// By default, only knows how to convert <see cref="address"/> field
        /// and <see cref="RemoteObjectBase"/> properties of the calling class.
        /// For details on which specific properties are ignored read
        /// <see cref="UiHelper.GetToImGuiMethods"/> method description.
        /// Any other properties or fields of the derived <see cref="RemoteObjectBase"/>
        /// class should be handled by that class.
        /// </summary>
        internal virtual void ToImGui()
        {
            var propFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
            var properties = UiHelper.GetToImGuiMethods(this.GetType(), propFlags, this);
            UiHelper.IntPtrToImGui("Address", this.address);
            foreach (var property in properties)
            {
                if (ImGui.TreeNode(property.Name))
                {
                    property.ToImGui.Invoke(property.Value, null);

                    ImGui.TreePop();
                }
            }
        }

        /// <summary>
        /// Reads the memory and update all the data known by this Object.
        /// </summary>
        /// <param name="hasAddressChanged">
        /// true in case the address has changed; otherwise false.
        /// </param>
        protected abstract void UpdateData(bool hasAddressChanged);

        /// <summary>
        /// Knows how to clean up the object.
        /// </summary>
        protected abstract void CleanUpData();
    }
}