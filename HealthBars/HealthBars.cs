// <copyright file="HealthBars.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace HealthBars
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Numerics;
    using Controller;
    using Coroutine;
    using GameHelper;
    using GameHelper.CoroutineEvents;
    using GameHelper.Plugin;
    using GameHelper.RemoteEnums;
    using GameHelper.RemoteObjects.Components;
    using GameHelper.Utils;
    using ImGuiNET;
    using Newtonsoft.Json;
    using View;
    using View.Entities;

    /// <summary>
    ///     <see cref="HealthBars" /> plugin.
    /// </summary>
    public sealed class HealthBars : PCore<HealthBarsSettings>
    {
        private readonly EntityFactory entityFactory = new();
        private ConcurrentDictionary<uint, Vector2> bPositions;
        private ActiveCoroutine onAreaChange;
        private SpriteController spriteController;
        private string SettingPathname => Path.Join(this.DllDirectory, "config", "settings.txt");

        /// <inheritdoc />
        public override void DrawSettings()
        {
            ImGui.Text("NOTE: Turn off in game health bars for best result.");
            ImGui.NewLine();
            ImGui.Checkbox("Interpolate position", ref this.Settings.InterpolatePosition);
            ImGui.NewLine();
            ImGui.Checkbox("Hide Health Bars when game is in the background", ref this.Settings.DrawWhenForeground);
            ImGui.Checkbox("Show in Town", ref this.Settings.ShowInTown);
            ImGui.Checkbox("Show in Hideout", ref this.Settings.ShowInHideout);
            ImGui.NewLine();
            ImGui.Checkbox("Show player bars", ref this.Settings.ShowPlayerBars);
            if (this.Settings.ShowPlayerBars)
            {
                ImGui.DragFloat("Player scale", ref this.Settings.PlayerBarScale, 0.01f, 0.3f, 5);
            }

            ImGui.Checkbox("Show friendly bars", ref this.Settings.ShowFriendlyBars);
            if (this.Settings.ShowFriendlyBars)
            {
                ImGui.DragFloat("Friendly scale", ref this.Settings.FriendlyBarScale, 0.01f, 0.3f, 5);
            }

            ImGui.Checkbox("Show enemy Mana", ref this.Settings.ShowEnemyMana);
            ImGui.NewLine();
            ImGui.Checkbox("Show friendly gradation marks", ref this.Settings.ShowFriendlyGradationMarks);
            ImGui.Checkbox("Show enemy gradation marks", ref this.Settings.ShowEnemyGradationMarks);
            ImGui.Checkbox("Show cull range", ref this.Settings.ShowCullRange);
            if (this.Settings.ShowCullRange)
            {
                ImGui.Checkbox("Normal monsters", ref this.Settings.ShowNormalCull);
                ImGui.SameLine();
                ImGui.Checkbox("Magic monsters", ref this.Settings.ShowMagicCull);
                ImGui.SameLine();
                ImGui.Checkbox("Rare monsters", ref this.Settings.ShowRareCull);
                ImGui.SameLine();
                ImGui.Checkbox("Unique monsters", ref this.Settings.ShowUniqueCull);

                ImGui.DragInt("Culling range", ref this.Settings.CullingRange, 0.01f, 0, 50);
                ImGui.ColorEdit4("Cull color", ref this.Settings.CullRangeColor);
            }

            ImGui.NewLine();
            ImGui.Checkbox("Normal bars", ref this.Settings.ShowNormalBar);
            if (this.Settings.ShowNormalBar)
            {
                ImGui.DragFloat("Normal scale", ref this.Settings.NormalBarScale, 0.01f, 0.3f, 5);
                ImGui.NewLine();
            }
            else
            {
                ImGui.SameLine();
            }

            ImGui.Checkbox("Magic bars", ref this.Settings.ShowMagicBar);
            if (this.Settings.ShowMagicBar)
            {
                ImGui.DragFloat("Magic scale", ref this.Settings.MagicBarScale, 0.01f, 0.3f, 5);
                ImGui.NewLine();
            }
            else
            {
                ImGui.SameLine();
            }

            ImGui.Checkbox("Rare bars", ref this.Settings.ShowRareBar);
            if (this.Settings.ShowRareBar)
            {
                ImGui.DragFloat("Rare scale", ref this.Settings.RareBarScale, 0.01f, 0.3f, 5);
                ImGui.NewLine();
            }
            else
            {
                ImGui.SameLine();
            }

            ImGui.Checkbox("Unique bars", ref this.Settings.ShowUniqueBar);
            if (this.Settings.ShowUniqueBar)
            {
                ImGui.DragFloat("Unique scale", ref this.Settings.UniqueBarScale, 0.01f, 0.3f, 5);
                ImGui.NewLine();
            }

            ImGui.NewLine();
            ImGui.Checkbox("Show rarity borders", ref this.Settings.ShowRarityBorders);
            if (this.Settings.ShowRarityBorders)
            {
                ImGui.Checkbox("Show normal border", ref this.Settings.ShowNormalBorders);
                if (this.Settings.ShowNormalBorders)
                {
                    ImGui.ColorEdit4("Normal color", ref this.Settings.NormalColor);
                }

                ImGui.Checkbox("Show magic border", ref this.Settings.ShowMagicBorders);
                if (this.Settings.ShowMagicBorders)
                {
                    ImGui.ColorEdit4("Magic color", ref this.Settings.MagicColor);
                }

                ImGui.Checkbox("Show rare border", ref this.Settings.ShowRareBorders);
                if (this.Settings.ShowRareBorders)
                {
                    ImGui.ColorEdit4("Rare color", ref this.Settings.RareColor);
                }

                ImGui.Checkbox("Show unique border", ref this.Settings.ShowUniqueBorders);
                if (this.Settings.ShowUniqueBorders)
                {
                    ImGui.ColorEdit4("Unique color", ref this.Settings.UniqueColor);
                }
            }
        }

        /// <inheritdoc />
        public override void DrawUI()
        {
            var cAreaInstance = Core.States.InGameStateObject.CurrentAreaInstance;
            var cWorldInstance = Core.States.InGameStateObject.CurrentWorldInstance;
            if (!this.Settings.ShowInTown && cWorldInstance.AreaDetails.IsTown ||
                !this.Settings.ShowInHideout && cWorldInstance.AreaDetails.IsHideout)
            {
                return;
            }

            if (Core.States.GameCurrentState != GameStateTypes.InGameState)
            {
                return;
            }

            if (this.Settings.DrawWhenForeground && !Core.Process.Foreground)
            {
                return;
            }

            foreach (var (gameEntityNodeKey, gameEntity) in cAreaInstance.AwakeEntities)
            {
                if (!gameEntity.IsValid)
                {
                    continue;
                }

                if (gameEntity.EntityType is not (
                    eTypes.FriendlyMonster or
                    eTypes.Monster or
                    eTypes.Stage1FIT or
                    eTypes.Stage1RewardFIT or
                    eTypes.Stage1EChestFIT))
                {
                    continue;
                }

                gameEntity.GetComp<Render>(out var render);
                var curPos = render.WorldPosition;
                curPos.Z -= render.ModelBounds.Z;
                var location = Core.States.InGameStateObject.CurrentWorldInstance.WorldToScreen(curPos);

                if (this.Settings.InterpolatePosition)
                {
                    if (this.bPositions.TryGetValue(gameEntityNodeKey.id, out var prevLocation))
                    {
                        location = MathHelper.Lerp(prevLocation, location, 0.2f);
                        this.bPositions.TryUpdate(gameEntityNodeKey.id, location, prevLocation);
                    }
                    else
                    {
                        this.bPositions.TryAdd(gameEntityNodeKey.id, location);
                    }
                }

                if (this.entityFactory.TryGetEntity(gameEntity, out var drawEntity))
                {
                    var entityParams = new EntityParams(this.Settings, location, gameEntity);
                    if (drawEntity.ShouldDraw(entityParams))
                    {
                        drawEntity.Draw(entityParams, this.spriteController);
                    }
                }
            }
        }

        /// <inheritdoc />
        public override void OnDisable()
        {
            this.onAreaChange?.Cancel();
            this.onAreaChange = null;
        }

        /// <inheritdoc />
        public override void OnEnable(bool isGameOpened)
        {
            if (File.Exists(this.SettingPathname))
            {
                var content = File.ReadAllText(this.SettingPathname);
                this.Settings = JsonConvert.DeserializeObject<HealthBarsSettings>(content);
            }

            var spriteSheetPathName = Path.Join(this.DllDirectory, "spritesheet.png");
            this.spriteController = new SpriteController(new SpriteAtlas(spriteSheetPathName));
            this.bPositions = new ConcurrentDictionary<uint, Vector2>();
            this.onAreaChange = CoroutineHandler.Start(this.ClearData());
        }

        /// <inheritdoc />
        public override void SaveSettings()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(this.SettingPathname));
            var settingsData = JsonConvert.SerializeObject(this.Settings, Formatting.Indented);
            File.WriteAllText(this.SettingPathname, settingsData);
        }

        private IEnumerator<Wait> ClearData()
        {
            while (true)
            {
                yield return new Wait(RemoteEvents.AreaChanged);
                this.bPositions.Clear();
            }
        }
    }
}