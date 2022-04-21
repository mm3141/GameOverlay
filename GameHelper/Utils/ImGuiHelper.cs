// <copyright file="ImGuiHelper.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using GameOffsets.Natives;
    using ImGuiNET;

    /// <summary>
    ///     Has helper functions to DRY out the Ui creation.
    /// </summary>
    public static class ImGuiHelper
    {
        /// <summary>
        ///     Flags associated with transparent ImGui window.
        /// </summary>
        public const ImGuiWindowFlags TransparentWindowFlags = ImGuiWindowFlags.NoInputs |
                                                               ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoCollapse |
                                                               ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoScrollbar |
                                                               ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.AlwaysAutoResize |
                                                               ImGuiWindowFlags.NoTitleBar;

        /// <summary>
        ///     Converts rgba color information to uint32 color format.
        /// </summary>
        /// <param name="r">red color number between 0 - 255.</param>
        /// <param name="g">green color number between 0 - 255.</param>
        /// <param name="b">blue color number between 0 - 255.</param>
        /// <param name="a">alpha number between 0 - 255.</param>
        /// <returns>color in uint32 format.</returns>
        public static uint Color(uint r, uint g, uint b, uint a)
        {
            return (a << 24) | (b << 16) | (g << 8) | r;
        }

        /// <summary>
        ///     Converts rgba color information to uint32 color format.
        /// </summary>
        /// <param name="color">x,y,z,w = alpha number between 0 - 255.</param>
        /// <returns>color in uint32 format.</returns>
        public static uint Color(Vector4 color)
        {
            return ((uint)color.W << 24) | ((uint)color.Z << 16) | ((uint)color.Y << 8) | (uint)color.X;
        }

        /// <summary>
        ///     Draws the Rectangle on the screen.
        /// </summary>
        /// <param name="pos">Postion of the rectange.</param>
        /// <param name="size">Size of the rectange.</param>
        /// <param name="r">color selector red 0 - 255.</param>
        /// <param name="g">color selector green 0 - 255.</param>
        /// <param name="b">color selector blue 0 - 255.</param>
        public static void DrawRect(Vector2 pos, Vector2 size, byte r, byte g, byte b)
        {
            ImGui.GetForegroundDrawList().AddRect(pos, pos + size, Color(r, g, b, 255), 0f, ImDrawFlags.RoundCornersNone, 4f);
        }

        /// <summary>
        ///     Draws the text on the screen.
        /// </summary>
        /// <param name="pos">world location to draw the text.</param>
        /// <param name="text">text to draw.</param>
        public static void DrawText(StdTuple3D<float> pos, string text)
        {
            var colBg = Color(0, 0, 0, 255);
            var colFg = Color(255, 255, 255, 255);
            var textSizeHalf = ImGui.CalcTextSize(text) / 2;
            var location = Core.States.InGameStateObject.CurrentWorldInstance.WorldToScreen(pos);
            var max = location + textSizeHalf;
            location -= textSizeHalf;
            ImGui.GetBackgroundDrawList().AddRectFilled(location, max, colBg);
            ImGui.GetForegroundDrawList().AddText(location, colFg, text);
        }

        /// <summary>
        ///     Draws the disabled button on the ImGui.
        /// </summary>
        /// <param name="buttonLabel">text to write on the button.</param>
        public static void DrawDisabledButton(string buttonLabel)
        {
            var col = Color(204, 204, 204, 128);
            ImGui.PushStyleColor(ImGuiCol.Button, col);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, col);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, col);
            ImGui.Button(buttonLabel);
            ImGui.PopStyleColor(3);
        }

        /// <summary>
        ///     Helps convert address to ImGui Widget.
        /// </summary>
        /// <param name="name">name of the object whos address it is.</param>
        /// <param name="address">address of the object in the game.</param>
        public static void IntPtrToImGui(string name, IntPtr address)
        {
            var addr = address.ToInt64().ToString("X");
            ImGui.Text(name);
            ImGui.SameLine();
            ImGui.PushStyleColor(ImGuiCol.Button, Color(0, 0, 0, 0));
            if (ImGui.SmallButton(addr))
            {
                ImGui.SetClipboardText(addr);
            }

            ImGui.PopStyleColor();
        }

        /// <summary>
        ///     Helps convert the text into ImGui widget that display the text
        ///     and copy it if user click on it.
        /// </summary>
        /// <param name="displayText">text to display on the ImGui.</param>
        /// <param name="copyText">text to copy when user click.</param>
        public static void DisplayTextAndCopyOnClick(string displayText, string copyText)
        {
            ImGui.PushStyleColor(ImGuiCol.Button, Color(0, 0, 0, 0));
            if (ImGui.SmallButton(displayText))
            {
                ImGui.SetClipboardText(copyText);
            }

            ImGui.PopStyleColor();
        }

        /// <summary>
        ///     Creates a ImGui ComboBox for C# Enums.
        /// </summary>
        /// <typeparam name="T">Enum type to display in the ComboBox.</typeparam>
        /// <param name="displayText">Text to display along the ComboBox.</param>
        /// <param name="selected">Selected enum value in the ComboBox.</param>
        /// <returns>true in case user select an item otherwise false.</returns>
        public static bool EnumComboBox<T>(string displayText, ref T selected)
            where T : struct, Enum
        {
            var enumNames = Enum.GetNames<T>();
            var selectedIndex = (int) Convert.ChangeType(selected, typeof(int));
            if (ImGui.Combo(displayText, ref selectedIndex, enumNames, enumNames.Length))
            {
                selected = Enum.Parse<T>(enumNames[selectedIndex]);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Creates a ImGui ComboBox for C# Enums whos values are not continous.
        /// </summary>
        /// <typeparam name="T">Enum type to display in the ComboBox.</typeparam>
        /// <param name="displayText">Text to display along the ComboBox.</param>
        /// <param name="selected">Selected enum value in the ComboBox.</param>
        /// <returns>true in case user select an item otherwise false.</returns>
        public static bool NonContinuousEnumComboBox<T>(string displayText, ref T selected)
            where T : struct, Enum
        {
            var enumNames = Enum.GetNames<T>();
            var selectedIndex = Array.IndexOf(enumNames, $"{selected}");
            if (ImGui.Combo(displayText, ref selectedIndex, enumNames, enumNames.Length))
            {
                selected = Enum.Parse<T>(enumNames[selectedIndex]);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Creates a ImGui ComboBox for a subset of C# Enum values.
        /// </summary>
        /// <typeparam name="T">Enum type to display in the ComboBox.</typeparam>
        /// <param name="displayText">Text to display along the ComboBox.</param>
        /// <param name="selected">Selected enum value in the ComboBox.</param>
        /// <param name="allowedItems">Items the user is allowed to select.</param>
        /// <returns>True if an item was selected; otherwise, false.</returns>
        public static bool EnumComboBox<T>(string displayText, ref T selected, IReadOnlyList<T> allowedItems)
            where T : struct, Enum
        {
            var enumNames = allowedItems.Select(x => x.ToString()).ToArray();
            var selectedIndex = Array.IndexOf(enumNames, selected.ToString());
            if (ImGui.Combo(displayText, ref selectedIndex, enumNames, enumNames.Length))
            {
                selected = allowedItems[selectedIndex];
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Creates a ImGui ComboBox for C# IEnumerable.
        /// </summary>
        /// <typeparam name="T">The type of objects in the IEnumerable.</typeparam>
        /// <param name="displayText">Text to display along the ComboBox.</param>
        /// <param name="items">IEnumerable data to choose from in the ComboBox.</param>
        /// <param name="current">Currently selected object of the IEnumerable data.</param>
        /// <returns>Returns a value indicating whether user has selected an item or not.</returns>
        public static bool IEnumerableComboBox<T>(string displayText, IEnumerable<T> items, ref T current)
        {
            var ret = false;
            if (ImGui.BeginCombo(displayText, $"{current}"))
            {
                var counter = 0;
                foreach (var item in items)
                {
                    var selected = item.Equals(current);
                    if (ImGui.IsWindowAppearing() && selected)
                    {
                        ImGui.SetScrollHereY();
                    }

                    if (ImGui.Selectable($"{counter}:{item}", selected))
                    {
                        current = item;
                        ret = true;
                    }

                    counter++;
                }

                ImGui.EndCombo();
            }

            return ret;
        }

        /// <summary>
        ///     Displays the text in ImGui tooltip.
        /// </summary>
        /// <param name="text">text to display in the ImGui tooltip.</param>
        public static void ToolTip(string text)
        {
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35.0f);
                ImGui.TextUnformatted(text);
                ImGui.PopTextWrapPos();
                ImGui.EndTooltip();
            }
        }
    }
}