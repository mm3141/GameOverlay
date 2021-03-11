// <copyright file="UiHelper.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using System.Reflection;
    using GameHelper.RemoteObjects;
    using ImGuiNET;

    /// <summary>
    /// Has helper functions to DRY out the Ui creation.
    /// </summary>
    public static class UiHelper
    {
        /// <summary>
        /// Flags associated with transparent ImGui window.
        /// </summary>
        public const ImGuiWindowFlags TransparentWindowFlags = ImGuiWindowFlags.NoInputs |
            ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoCollapse |
            ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoScrollbar |
            ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.AlwaysAutoResize |
            ImGuiWindowFlags.NoTitleBar;

        /// <summary>
        /// Stores rgba in uint32.
        /// </summary>
        /// <param name="r">red color number between 0 - 255.</param>
        /// <param name="g">green color number between 0 - 255.</param>
        /// <param name="b">blue color number between 0 - 255.</param>
        /// <param name="a">alpha number between 0 - 255.</param>
        /// <returns>color in uint32 format.</returns>
        public static uint Color(byte r, byte g, byte b, byte a)
        {
            uint ret = a;
            ret <<= 8;
            ret += b;
            ret <<= 8;
            ret += g;
            ret <<= 8;
            ret += r;
            return ret;
        }

        /// <summary>
        /// Draws the Rectangle on the screen.
        /// </summary>
        /// <param name="pos">Postion of the rectange.</param>
        /// <param name="size">Size of the rectange.</param>
        /// <param name="r">color selector red 0 - 255.</param>
        /// <param name="g">color selector green 0 - 255.</param>
        /// <param name="b">color selector blue 0 - 255.</param>
        public static void DrawRect(Vector2 pos, Vector2 size, byte r, byte g, byte b)
        {
            ImGui.GetForegroundDrawList().AddRect(pos, pos + size, UiHelper.Color(r, g, b, 255));
        }

        /// <summary>
        /// Helps convert address to ImGui Widget.
        /// </summary>
        /// <param name="name">name of the object whos address it is.</param>
        /// <param name="address">address of the object in the game.</param>
        public static void IntPtrToImGui(string name, IntPtr address)
        {
            var addr = address.ToInt64().ToString("X");
            ImGui.Text(name);
            ImGui.SameLine();
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0, 0, 0, 0));
            if (ImGui.SmallButton(addr))
            {
                ImGui.SetClipboardText(addr);
            }

            ImGui.PopStyleColor();
        }

        /// <summary>
        /// Iterates over properties of the given class via reflection
        /// and yields the <see cref="RemoteObjectBase"/> property name and its
        /// <see cref="RemoteObjectBase.ToImGui"/> method. Any property
        /// that doesn't have both the getter and setter method are ignored.
        /// </summary>
        /// <param name="classType">Type of the class to traverse.</param>
        /// <param name="propertyFlags">flags to filter the class properties.</param>
        /// <param name="classObject">Class object, or null for static class.</param>
        /// <returns>Yield the <see cref="RemoteObjectBase.ToImGui"/> method.</returns>
        internal static IEnumerable<RemoteObjectPropertyDetail> GetToImGuiMethods(
            Type classType,
            BindingFlags propertyFlags,
            object classObject)
        {
            var methodFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            var properties = classType.GetProperties(propertyFlags).ToList();
            for (int i = 0; i < properties.Count; i++)
            {
                PropertyInfo property = properties[i];
                if (property.SetMethod == null)
                {
                    continue;
                }

                object propertyValue = property.GetValue(classObject);
                if (propertyValue == null)
                {
                    continue;
                }

                Type propertyType = propertyValue.GetType();
                if (propertyType.BaseType.Name != "RemoteObjectBase")
                {
                    continue;
                }

                yield return new RemoteObjectPropertyDetail()
                {
                    Name = property.Name,
                    Value = propertyValue,
                    ToImGui = propertyType.GetMethod("ToImGui", methodFlags),
                };
            }
        }
    }
}