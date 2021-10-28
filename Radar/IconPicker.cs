// <copyright file="IconPicker.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Radar
{
    using System;
    using System.IO;
    using System.Numerics;
    using GameHelper;
    using ImGuiNET;
    using Newtonsoft.Json;

    /// <summary>
    /// A class to store the currently selected icon.
    /// This class assumes that the icon sprite (png) file has:
    ///   (1) Arranged icons in the center of the Icon box.
    ///   (2) All icon boxes are of exact same size.
    ///   (3) There is no padding/margins/buffer-pixel between icon boxes
    ///       (i.e. where 1 box ends, another starts).
    /// </summary>
    public class IconPicker
    {
        private static readonly ImGuiWindowFlags PopUpFlags =
            ImGuiWindowFlags.AlwaysHorizontalScrollbar |
            ImGuiWindowFlags.NoSavedSettings |
            ImGuiWindowFlags.NoBackground;

        private float iconScale = 10;
        private Vector2 popUpPos = Vector2.Zero;
        private bool showPopUp = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="IconPicker"/> class.
        /// </summary>
        /// <param name="filepathname">file pathname to the icon sprite file.</param>
        /// <param name="iconDimension">dimension of the icon in the sprite.</param>
        /// <param name="clicked">Row and Column information of the icon user has clicked.</param>
        /// <param name="iconScale">how big you want to display the icon.</param>
        [JsonConstructor]
        public IconPicker(
            string filepathname,
            Vector2 iconDimension,
            Vector2 clicked,
            float iconScale)
        {
            this.FilePathName = filepathname;
            this.Clicked = clicked;
            this.IconDimension = iconDimension;
            this.iconScale = iconScale;
            this.Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IconPicker"/> class.
        /// </summary>
        /// <param name="filepathname">file pathname to the icon sprite file.</param>
        /// <param name="totalRows">total number of rows in icon sprite file.</param>
        /// <param name="totalColumns">total number of columns in icon sprite file.</param>
        /// <param name="x">Default Icon Column number. Note: start from 0.</param>
        /// <param name="y">Default Icon Row number. Note: start from 0.</param>
        /// <param name="s">Default Icon size.</param>
        public IconPicker(string filepathname, int totalColumns, int totalRows, int x, int y, int s)
        {
            this.FilePathName = filepathname;
            this.IconDimension = new Vector2(1f / totalColumns, 1f / totalRows);
            this.Clicked = new Vector2(x, y);
            this.iconScale = s;
            this.Initialize();
        }

        /// <summary>
        /// Gets a value indicating which icon user has clicked.
        /// </summary>
        public Vector2 Clicked { get; private set; } = Vector2.Zero;

        /// <summary>
        /// Gets a value indicating how big you want to display the icon.
        /// </summary>
        public float IconScale => this.iconScale;

        /// <summary>
        /// Gets a value indicating dimension of the icon in the sprite.
        /// This value is between 0f and 1f, where 0f means 0% of the width/height
        /// and 1f means 100% of the width/height.
        /// </summary>
        public Vector2 IconDimension { get; private set; } = Vector2.One;

        /// <summary>
        /// Gets the icon sprite file pathname.
        /// </summary>
        public string FilePathName { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the texture pointer.
        /// </summary>
        [JsonIgnore]
        public IntPtr TexturePtr { get; private set; } = IntPtr.Zero;

        /// <summary>
        /// Gets the vector pointing to start (top left) of the Box.
        /// </summary>
        [JsonIgnore]
        public Vector2 UV0 { get; private set; } = Vector2.Zero;

        /// <summary>
        /// Gets the vector pointing to end ( bottom right ) of the Box.
        /// </summary>
        [JsonIgnore]
        public Vector2 UV1 { get; private set; } = Vector2.Zero;

        /// <summary>
        /// Show the Setting Widget for the selection of icon. This function assumes
        /// that the ImGui window is already created.
        /// </summary>
        public void ShowSettingWidget()
        {
            ImGui.PushID(this.GetHashCode());
            ImGui.PushItemWidth(200);
            ImGui.InputFloat($"##iconscale", ref this.iconScale, 1f, 1f);
            ImGui.PopItemWidth();
            ImGui.SameLine();

            var buttonSize = new Vector2(ImGui.GetFontSize());
            Core.Overlay.AddOrGetImagePointer(this.FilePathName, out var p, out var w, out var h);

            if (ImGui.ImageButton(this.TexturePtr, buttonSize, this.UV0, this.UV1))
            {
                this.popUpPos = ImGui.GetWindowPos();
                this.popUpPos.X += ImGui.GetWindowSize().X;
                this.showPopUp = true;
            }

            if (this.showPopUp)
            {
                ImGui.SetNextWindowPos(this.popUpPos, ImGuiCond.Appearing);
                ImGui.SetNextWindowSize(new Vector2(400), ImGuiCond.Appearing);
                var title = $"Icon Picker (Double click to select an item)";
                if (ImGui.Begin(title, ref this.showPopUp, PopUpFlags))
                {
                    if (ImGui.IsWindowHovered() && ImGui.GetIO().MouseDoubleClicked[0])
                    {
                        var clicked = ImGui.GetIO().MouseClickedPos[0] - ImGui.GetCursorScreenPos();
                        var x = (int)(clicked.X / (w * this.IconDimension.X));
                        var y = (int)(clicked.Y / (h * this.IconDimension.Y));
                        this.Clicked = new Vector2(x, y);
                        this.UpdateUV0UV1();
                        this.showPopUp = false;
                    }
                    else if (!ImGui.IsWindowFocused())
                    {
                        this.showPopUp = false;
                    }

                    ImGui.Image(p, new Vector2(w, h));
                }

                ImGui.End();
            }

            ImGui.PopID();
        }

        /// <summary>
        /// Uploads the sprite icon file as texture and updates the class data.
        /// </summary>
        private void Initialize()
        {
            if (File.Exists(this.FilePathName))
            {
                this.UploadIconSpriteFile();
                this.UpdateUV0UV1();
            }
            else
            {
                string message = $"Missing Icons (sprite) file with name: {this.FilePathName}";
                throw new FileNotFoundException(message);
            }
        }

        private void UploadIconSpriteFile()
        {
            Core.Overlay.AddOrGetImagePointer(this.FilePathName, out var p, out var _, out var _);
            this.TexturePtr = p;
        }

        private void UpdateUV0UV1()
        {
            var selected = this.Clicked;
            var size = this.IconDimension;
            this.UV0 = new Vector2(selected.X++ * size.X, selected.Y++ * size.Y);
            this.UV1 = new Vector2(selected.X * size.X, selected.Y * size.Y);
        }
    }
}
