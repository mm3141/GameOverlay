// <copyright file="HealthBars.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace HealthBars
{
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
        private ActiveCoroutine onAreaChange;

        private ConcurrentDictionary<uint, Vector2> bPositions;

        /// <summary>
        /// Sprites for HealthBars.
        /// </summary>
        private Dictionary<string, IconPicker> sprites = new Dictionary<string, IconPicker>();

        private string SettingPathname => Path.Join(this.DllDirectory, "config", "settings.txt");

        /// <inheritdoc/>
        public override void DrawSettings()
        {
            ImGui.Checkbox("Show in Town", ref this.Settings.ShowInTown);
            ImGui.Checkbox("Show in Hideout", ref this.Settings.ShowInHideout);
            ImGui.NewLine();
            ImGui.Checkbox("Show friendly bars", ref this.Settings.ShowFriendlyBars);
            ImGui.NewLine();
            ImGui.Checkbox("Show enemy Mana", ref this.Settings.ShowEnemyMana);

            ImGui.NewLine();
            ImGui.Checkbox("Show rarity borders", ref this.Settings.ShowRarityBorders);
            if (this.Settings.ShowRarityBorders)
            {
                ImGui.ColorEdit4("Normal", ref this.Settings.NormalColor);
                ImGui.ColorEdit4("Magic", ref this.Settings.MagicColor);
                ImGui.ColorEdit4("Rare", ref this.Settings.RareColor);
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
                    if (!awakeEntity.Value.TryGetComponent<Render>(out var eRender) || !awakeEntity.Value.TryGetComponent<Life>(out var entityLife) || !entityLife.IsAlive)
                    {
                        continue;
                    }

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
            var isBlockage = entity.Value.TryGetComponent<TriggerableBlockage>(out var blockageComp);
            var hasRender = entity.Value.TryGetComponent<Render>(out var eRender);
            var hasPositioned = entity.Value.TryGetComponent<Positioned>(out var entityPositioned);
            var isPlayer = entity.Value.TryGetComponent<Player>(out var playerComp);
            var willDieAfterTime = entity.Value.TryGetComponent<DiesAfterTime>(out var diesAfterTime);

            if (!hasVital || !entityLife.IsAlive || (!hasOMP && isBlockage && !isPlayer) || !hasRender || !hasPositioned || willDieAfterTime)
            {
                return;
            }

            bool drawBorder = hasOMP && this.Settings.ShowRarityBorders;
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

            Vector2 hpOffset = new Vector2(0, 1);
            Vector2 manaOffset = new Vector2(0, 10);

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
            Vector2 uv0 = new Vector2(sx / ssw, sy / ssh);
            Vector2 uv1 = new Vector2((sx + sw) / ssw, (sy + sh) / ssh);
            var sprite = this.sprites[spriteName];
            Vector2 bounds = new Vector2(tw * (((mulw < 0) ? 100 : mulw) / 100), th * (((mulh < 0) ? 100 : mulh) / 100));
            Vector2 vbounds = new Vector2(tw, th);
            Vector2 half = new Vector2(vbounds.X / 2, 0);
            draw.AddImage(sprite.TexturePtr, t - half, t - half + bounds, uv0, uv1);

            if (border == true)
            {
                draw.AddRect(t - half, t - half + vbounds, borderColor);
            }
        }

        private uint RarityColor(Rarity rarity)
        {
            switch (rarity)
            {
                case Rarity.Unique:
                    return UiHelper.Color(this.Settings.UniqueColor * 255f);
                case Rarity.Rare:
                    return UiHelper.Color(this.Settings.RareColor * 255f);
                case Rarity.Magic:
                    return UiHelper.Color(this.Settings.MagicColor * 255f);
                case Rarity.Normal:
                default:
                    return UiHelper.Color(this.Settings.NormalColor * 255f);
            }
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
