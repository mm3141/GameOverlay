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
    using Coroutine;
    using GameHelper;
    using GameHelper.CoroutineEvents;
    using GameHelper.Plugin;
    using GameHelper.RemoteEnums;
    using GameHelper.RemoteObjects.Components;
    using GameHelper.RemoteObjects.States.InGameStateObjects;
    using GameHelper.Utils;
    using GameOffsets.Objects.States.InGameState;
    using ImGuiNET;
    using Newtonsoft.Json;

    /// <summary>
    /// <see cref="HealthBars"/> plugin.
    /// </summary>
    public sealed class HealthBars : PCore<HealthBarsSettings>
    {
        /// <summary>
        /// Sprites for HealthBars.
        /// </summary>
        private readonly Dictionary<string, IconPicker> sprites = new();
        private ActiveCoroutine onAreaChange;
        private ConcurrentDictionary<uint, Vector2> bPositions;

        private string SettingPathname => Path.Join(this.DllDirectory, "config", "settings.txt");

        /// <inheritdoc/>
        public override void DrawSettings()
        {
            ImGui.Text("NOTE: Turn off in game health bars for best result.");
            ImGui.NewLine();
            ImGui.Checkbox("Show in Town", ref this.Settings.ShowInTown);
            ImGui.Checkbox("Show in Hideout", ref this.Settings.ShowInHideout);
            ImGui.NewLine();
            ImGui.Checkbox("Show friendly bars", ref this.Settings.ShowFriendlyBars);
            ImGui.NewLine();
            ImGui.Checkbox("Show enemy Mana", ref this.Settings.ShowEnemyMana);

            ImGui.Checkbox("Normal bars", ref this.Settings.ShowNormalBar);
            ImGui.SameLine();
            ImGui.Checkbox("Magic bars", ref this.Settings.ShowMagicBar);
            ImGui.SameLine();
            ImGui.Checkbox("Rare bars", ref this.Settings.ShowRareBar);
            ImGui.SameLine();
            ImGui.Checkbox("Unique bars", ref this.Settings.ShowUniqueBar);

            ImGui.NewLine();
            ImGui.Checkbox("Show rarity borders", ref this.Settings.ShowRarityBorders);
            if (this.Settings.ShowRarityBorders)
            {
                ImGui.Checkbox("#NormalBorder", ref this.Settings.ShowNormalBorders);
                ImGui.SameLine();
                ImGui.ColorEdit4("Normal", ref this.Settings.NormalColor);

                ImGui.Checkbox("#MagicBorder", ref this.Settings.ShowMagicBorders);
                ImGui.SameLine();
                ImGui.ColorEdit4("Magic", ref this.Settings.MagicColor);

                ImGui.Checkbox("#RareBorder", ref this.Settings.ShowRareBorders);
                ImGui.SameLine();
                ImGui.ColorEdit4("Rare", ref this.Settings.RareColor);

                ImGui.Checkbox("#UniqueBorder", ref this.Settings.ShowUniqueBorders);
                ImGui.SameLine();
                ImGui.ColorEdit4("Unique", ref this.Settings.UniqueColor);
            }
        }

        /// <inheritdoc/>
        public override void DrawUI()
        {
            var cAI = Core.States.InGameStateObject.CurrentAreaInstance;

            foreach (var awakeEntity in cAI.AwakeEntities)
            {
                if ((!this.Settings.ShowInTown && cAI.AreaDetails.IsTown) || (!this.Settings.ShowInHideout && cAI.AreaDetails.IsHideout))
                {
                    continue;
                }

                if (awakeEntity.Value.IsValid)
                {
                    this.DrawEntityHealth(awakeEntity);
                }
            }
        }

        /// <inheritdoc/>
        public override void OnDisable()
        {
            this.onAreaChange?.Cancel();
            this.onAreaChange = null;
        }

        /// <inheritdoc/>
        public override void OnEnable(bool isGameOpened)
        {
            if (File.Exists(this.SettingPathname))
            {
                var content = File.ReadAllText(this.SettingPathname);
                this.Settings = JsonConvert.DeserializeObject<HealthBarsSettings>(content);
            }

            this.AddSpritesheet();

            this.bPositions = new ConcurrentDictionary<uint, Vector2>();

            this.onAreaChange = CoroutineHandler.Start(this.ClearData());
        }

        /// <inheritdoc/>
        public override void SaveSettings()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(this.SettingPathname));
            var settingsData = JsonConvert.SerializeObject(this.Settings, Formatting.Indented);
            File.WriteAllText(this.SettingPathname, settingsData);
        }

        private void DrawEntityHealth(KeyValuePair<EntityNodeKey, Entity> entity)
        {
            var hasVital = entity.Value.TryGetComponent<Life>(out var entityLife);
            var hasOMP = entity.Value.TryGetComponent<ObjectMagicProperties>(out var entityMagicProperties);
            var isBlockage = entity.Value.TryGetComponent<TriggerableBlockage>(out var _);
            var hasRender = entity.Value.TryGetComponent<Render>(out var eRender);
            var hasPositioned = entity.Value.TryGetComponent<Positioned>(out var entityPositioned);
            var isPlayer = entity.Value.TryGetComponent<Player>(out var _);
            var willDieAfterTime = entity.Value.TryGetComponent<DiesAfterTime>(out var _);

            if (!hasVital || !entityLife.IsAlive || (!hasOMP && !isPlayer) || !hasRender || !hasPositioned || willDieAfterTime || isBlockage)
            {
                return;
            }

            bool drawBorder = hasOMP && this.Settings.ShowRarityBorders && (
                (entityMagicProperties.Rarity == Rarity.Normal) && this.Settings.ShowNormalBorders || 
                (entityMagicProperties.Rarity == Rarity.Magic) && this.Settings.ShowMagicBorders ||
                (entityMagicProperties.Rarity == Rarity.Rare) && this.Settings.ShowRareBorders ||
                (entityMagicProperties.Rarity == Rarity.Unique) && this.Settings.ShowUniqueBorders
                );
            uint borderColor = hasOMP && drawBorder ? this.RarityColor(entityMagicProperties.Rarity) : 0;

            var curPos = eRender.WorldPosition;
            curPos.Z -= 1.4f * eRender.ModelBounds.Z;
            var location = Core.States.InGameStateObject.WorldToScreen(curPos);

            if (!this.bPositions.TryGetValue(entity.Key.id, out Vector2 prevLocation))
            {
                this.bPositions.TryAdd(entity.Key.id, location);
            }
            else
            {
                location = MathHelper.Lerp(prevLocation, location, 0.2f);
                this.bPositions.TryUpdate(entity.Key.id, location, prevLocation);
            }

            float hpReserved = entityLife.Health.ReservedPercent / 100;
            float hpPercent = entityLife.Health.CurrentInPercent() * ((100 - hpReserved) / 100);

            float esReserved = entityLife.EnergyShield.ReservedPercent / 100;
            float esPercent = entityLife.EnergyShield.CurrentInPercent() * ((100 - esReserved) / 100);

            float manaReserved = entityLife.Mana.ReservedPercent / 100;
            float manaPercent = entityLife.Mana.CurrentInPercent() * ((100 - manaReserved) / 100);

            Vector2 hpOffset = new(0, 1);
            Vector2 manaOffset = new(0, 10);

            // TODO: Make correct dictionary instead of IconPickers
            if (entityPositioned.IsFriendly)
            {
                if (!this.Settings.ShowFriendlyBars && entity.Value.Address != Core.States.InGameStateObject.CurrentAreaInstance.Player.Address)
                {
                    return;
                }

                if (entity.Value.Address == Core.States.InGameStateObject.CurrentAreaInstance.Player.Address)
                {
                    this.DrawSprite("EmptyDoubleBar", 1, 68, 108, 19, 110, 88, location, 108, 19, -1, -1);

                    this.DrawSprite("EmptyMana", 1, 19, 1, 8, 110, 88, location + manaOffset, 104, 8, 100f - manaReserved, -1);
                    this.DrawSprite("Mana", 1, 47, 1, 8, 110, 88, location + manaOffset, 104, 8, manaPercent, -1);
                }
                else
                {
                    this.DrawSprite("EmptyBar", 1, 57, 108, 9, 110, 88, location, 108, 9, -1, -1);
                }

                this.DrawSprite("EmptyHP", 1, 10, 1, 7, 110, 88, location + hpOffset, 104, 7, 100f - hpReserved, -1);
                this.DrawSprite("HP", 1, 38, 1, 7, 110, 88, location + hpOffset, 104, 7, hpPercent, -1);
            }
            else
            {
                if (this.Settings.ShowEnemyMana)
                {
                    this.DrawSprite("EmptyDoubleBar", 1, 68, 108, 19, 110, 88, location, 108, 19, -1, -1, drawBorder, borderColor);

                    this.DrawSprite("EmptyMana", 1, 19, 1, 8, 110, 88, location + manaOffset, 104, 8, 100f - manaReserved, -1);
                    this.DrawSprite("Mana", 1, 47, 1, 8, 110, 88, location + manaOffset, 104, 8, manaPercent, -1);
                }
                else
                {
                    this.DrawSprite("EmptyBar", 1, 57, 108, 9, 110, 88, location, 108, 9, -1, -1, drawBorder, borderColor);
                }

                this.DrawSprite("EmptyHP", 1, 10, 1, 7, 110, 88, location + hpOffset, 104, 7, 100f - hpReserved, -1);
                this.DrawSprite("EnemyHP", 1, 29, 1, 7, 110, 88, location + hpOffset, 104, 7, hpPercent, -1);
            }

            if (entityLife.EnergyShield.Total > 0)
            {
                this.DrawSprite("ES", 1, 1, 1, 7, 110, 88, location + hpOffset, 104, 7, esPercent, -1);
            }
        }

        private void DrawSprite(string spriteName, float sx, float sy, float sw, float sh, float ssw, float ssh, Vector2 t, float tw, float th, float mulw, float mulh)
        {
            this.DrawSprite(spriteName, sx, sy, sw, sh, ssw, ssh, t, tw, th, mulw, mulh, false, 0);
        }

        private void DrawSprite(string spriteName, float sx, float sy, float sw, float sh, float ssw, float ssh, Vector2 t, float tw, float th, float mulw, float mulh, bool border, uint borderColor)
        {
            var draw = ImGui.GetBackgroundDrawList();
            Vector2 uv0 = new(sx / ssw, sy / ssh);
            Vector2 uv1 = new((sx + sw) / ssw, (sy + sh) / ssh);
            var sprite = this.sprites[spriteName];
            Vector2 bounds = new(tw * (((mulw < 0) ? 100 : mulw) / 100), th * (((mulh < 0) ? 100 : mulh) / 100));
            Vector2 vbounds = new(tw, th);
            Vector2 half = new(vbounds.X / 2, 0);
            draw.AddImage(sprite.TexturePtr, t - half, t - half + bounds, uv0, uv1);

            if (border == true)
            {
                draw.AddRect(t - half, t - half + vbounds, borderColor);
            }
        }

        private uint RarityColor(Rarity rarity)
        {
            return rarity switch
            {
                Rarity.Unique => UiHelper.Color(this.Settings.UniqueColor * 255f),
                Rarity.Rare => UiHelper.Color(this.Settings.RareColor * 255f),
                Rarity.Magic => UiHelper.Color(this.Settings.MagicColor * 255f),
                _ => UiHelper.Color(this.Settings.NormalColor * 255f),
            };
        }

        private IEnumerator<Wait> ClearData()
        {
            while (true)
            {
                yield return new Wait(RemoteEvents.AreaChanged);
                this.bPositions.Clear();
            }
        }

        /// <summary>
        /// Adds the default icons if the setting file isn't available.
        /// </summary>
        private void AddSpritesheet()
        {
            var spritesheetPathName = Path.Join(this.DllDirectory, "spritesheet.png");
            this.AddSprites(spritesheetPathName);
        }

        private void AddSprites(string spritesheetPathName)
        {
            this.sprites.TryAdd("ES", new IconPicker(spritesheetPathName, 1, 8, 108, 19, 1));
            this.sprites.TryAdd("EmptyHP", new IconPicker(spritesheetPathName, 1, 8, 108, 19, 1));
            this.sprites.TryAdd("EmptyMana", new IconPicker(spritesheetPathName, 1, 8, 108, 19, 1));
            this.sprites.TryAdd("EnemyHP", new IconPicker(spritesheetPathName, 1, 8, 108, 19, 1));
            this.sprites.TryAdd("HP", new IconPicker(spritesheetPathName, 1, 8, 108, 19, 1));
            this.sprites.TryAdd("Mana", new IconPicker(spritesheetPathName, 1, 8, 108, 19, 1));

            this.sprites.TryAdd("EmptyBar", new IconPicker(spritesheetPathName, 1, 8, 108, 19, 1));
            this.sprites.TryAdd("EmptyDoubleBar", new IconPicker(spritesheetPathName, 1, 8, 108, 19, 1));
        }
    }
}
