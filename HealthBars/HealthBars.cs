// <copyright file="HealthBars.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace HealthBars {
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Numerics;
    using Coroutine;
    using GameHelper;
    using GameHelper.CoroutineEvents;
    using GameHelper.Plugin;
    using GameHelper.RemoteObjects.Components;
    using GameHelper.Utils;
    using ImGuiNET;
    using Newtonsoft.Json;
    using View;
    using View.Entities;

    /// <summary>
    ///     <see cref="HealthBars" /> plugin.
    /// </summary>
    public sealed class HealthBars : PCore<HealthBarsSettings> {
        private readonly SpriteController spriteController = new();
        private ConcurrentDictionary<uint, Vector2> bPositions;
        private ActiveCoroutine onAreaChange;
        private string SettingPathname => Path.Join(DllDirectory, "config", "settings.txt");

        /// <inheritdoc />
        public override void DrawSettings() {
            ImGui.Text("NOTE: Turn off in game health bars for best result.");
            ImGui.NewLine();
            ImGui.Checkbox("Show in Town", ref Settings.ShowInTown);
            ImGui.Checkbox("Show in Hideout", ref Settings.ShowInHideout);
            ImGui.NewLine();
            ImGui.Checkbox("Show player bars", ref Settings.ShowPlayerBars);
            if (Settings.ShowPlayerBars) {
                ImGui.DragFloat("Player scale", ref Settings.PlayerBarScale, 0.01f, 0.3f, 5);
            }

            ImGui.Checkbox("Show friendly bars", ref Settings.ShowFriendlyBars);
            if (Settings.ShowFriendlyBars) {
                ImGui.DragFloat("Friendly scale", ref Settings.FriendlyBarScale, 0.01f, 0.3f, 5);
            }

            ImGui.Checkbox("Show enemy Mana", ref Settings.ShowEnemyMana);
            ImGui.NewLine();
            ImGui.Checkbox("Show friendly gradation marks", ref Settings.ShowFriendlyGradationMarks);
            ImGui.Checkbox("Show enemy gradation marks", ref Settings.ShowEnemyGradationMarks);
            ImGui.Checkbox("Show cull range", ref Settings.ShowCullRange);
            if (Settings.ShowCullRange) {
                ImGui.Checkbox("Normal monsters", ref Settings.ShowNormalCull);
                ImGui.SameLine();
                ImGui.Checkbox("Magic monsters", ref Settings.ShowMagicCull);
                ImGui.SameLine();
                ImGui.Checkbox("Rare monsters", ref Settings.ShowRareCull);
                ImGui.SameLine();
                ImGui.Checkbox("Unique monsters", ref Settings.ShowUniqueCull);

                ImGui.DragInt("Culling range", ref Settings.CullingRange, 0.01f, 0, 50);
                ImGui.ColorEdit4("Cull color", ref Settings.CullRangeColor);
            }

            ImGui.NewLine();

            ImGui.Checkbox("Normal bars", ref Settings.ShowNormalBar);
            if (Settings.ShowNormalBar) {
                ImGui.DragFloat("Normal scale", ref Settings.NormalBarScale, 0.01f, 0.3f, 5);
                ImGui.NewLine();
            }
            else {
                ImGui.SameLine();
            }

            ImGui.Checkbox("Magic bars", ref Settings.ShowMagicBar);
            if (Settings.ShowMagicBar) {
                ImGui.DragFloat("Magic scale", ref Settings.MagicBarScale, 0.01f, 0.3f, 5);
                ImGui.NewLine();
            }
            else {
                ImGui.SameLine();
            }

            ImGui.Checkbox("Rare bars", ref Settings.ShowRareBar);
            if (Settings.ShowRareBar) {
                ImGui.DragFloat("Rare scale", ref Settings.RareBarScale, 0.01f, 0.3f, 5);
                ImGui.NewLine();
            }
            else {
                ImGui.SameLine();
            }

            ImGui.Checkbox("Unique bars", ref Settings.ShowUniqueBar);
            if (Settings.ShowUniqueBar) {
                ImGui.DragFloat("Unique scale", ref Settings.UniqueBarScale, 0.01f, 0.3f, 5);
                ImGui.NewLine();
            }

            ImGui.NewLine();
            ImGui.Checkbox("Show rarity borders", ref Settings.ShowRarityBorders);
            if (Settings.ShowRarityBorders) {
                ImGui.Checkbox("Show normal border", ref Settings.ShowNormalBorders);
                if (Settings.ShowNormalBorders) {
                    ImGui.ColorEdit4("Normal color", ref Settings.NormalColor);
                }

                ImGui.Checkbox("Show magic border", ref Settings.ShowMagicBorders);
                if (Settings.ShowMagicBorders) {
                    ImGui.ColorEdit4("Magic color", ref Settings.MagicColor);
                }

                ImGui.Checkbox("Show rare border", ref Settings.ShowRareBorders);
                if (Settings.ShowRareBorders) {
                    ImGui.ColorEdit4("Rare color", ref Settings.RareColor);
                }

                ImGui.Checkbox("Show unique border", ref Settings.ShowUniqueBorders);
                if (Settings.ShowUniqueBorders) {
                    ImGui.ColorEdit4("Unique color", ref Settings.UniqueColor);
                }
            }
        }

        /// <inheritdoc />
        public override void DrawUI() {
            var cAreaInstance = Core.States.InGameStateObject.CurrentAreaInstance;

            foreach (var (gameEntityNodeKey, gameEntity) in cAreaInstance.AwakeEntities) {
                if (!Settings.ShowInTown && cAreaInstance.AreaDetails.IsTown ||
                    !Settings.ShowInHideout && cAreaInstance.AreaDetails.IsHideout) {
                    continue;
                }

                if (!gameEntity.IsValid) {
                    continue;
                }

                var hasRender = gameEntity.TryGetComponent<Render>(out var render);
                if (!hasRender) {
                    return;
                }

                var curPos = render.WorldPosition;
                curPos.X += 11.25f;
                curPos.Z -= 1.4f * render.ModelBounds.Z;
                var location = Core.States.InGameStateObject.WorldToScreen(curPos);

                if (bPositions.TryGetValue(gameEntityNodeKey.id, out var prevLocation)) {
                    location = MathHelper.Lerp(prevLocation, location, 0.2f);
                    bPositions.TryUpdate(gameEntityNodeKey.id, location, prevLocation);
                }
                else {
                    bPositions.TryAdd(gameEntityNodeKey.id, location);
                }

                var drawEntity = EntityFactory.GetEntity(gameEntity);
                if (drawEntity == null) {
                    continue;
                }

                var entityParams = new EntityParams(Settings, location, gameEntity);
                if (drawEntity.ShouldDraw(entityParams)) {
                    drawEntity.Draw(entityParams, spriteController);
                }
            }
        }

        /// <inheritdoc />
        public override void OnDisable() {
            onAreaChange?.Cancel();
            onAreaChange = null;
        }

        /// <inheritdoc />
        public override void OnEnable(bool isGameOpened) {
            if (File.Exists(SettingPathname)) {
                var content = File.ReadAllText(SettingPathname);
                Settings = JsonConvert.DeserializeObject<HealthBarsSettings>(content);
            }

            var spriteSheetPathName = Path.Join(DllDirectory, "spritesheet.png");
            spriteController.AddSprites(spriteSheetPathName);
            bPositions = new ConcurrentDictionary<uint, Vector2>();
            onAreaChange = CoroutineHandler.Start(ClearData());
        }

        /// <inheritdoc />
        public override void SaveSettings() {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingPathname));
            var settingsData = JsonConvert.SerializeObject(Settings, Formatting.Indented);
            File.WriteAllText(SettingPathname, settingsData);
        }

        private IEnumerator<Wait> ClearData() {
            while (true) {
                yield return new Wait(RemoteEvents.AreaChanged);
                bPositions.Clear();
            }
        }
    }
}