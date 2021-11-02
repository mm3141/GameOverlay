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
    ///     <see cref="HealthBars" /> plugin.
    /// </summary>
    public sealed class HealthBars : PCore<HealthBarsSettings>
    {
        /// <summary>
        ///     Sprites for HealthBars.
        /// </summary>
        private readonly Dictionary<string, IconPicker> sprites = new();

        private ConcurrentDictionary<uint, Vector2> bPositions;
        private ActiveCoroutine onAreaChange;

        private string SettingPathname => Path.Join(this.DllDirectory, "config", "settings.txt");

        /// <inheritdoc />
        public override void DrawSettings()
        {
            ImGui.Text("NOTE: Turn off in game health bars for best result.");
            ImGui.NewLine();
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
            var cAI = Core.States.InGameStateObject.CurrentAreaInstance;

            foreach (var awakeEntity in cAI.AwakeEntities)
            {
                if (!this.Settings.ShowInTown && cAI.AreaDetails.IsTown ||
                    !this.Settings.ShowInHideout && cAI.AreaDetails.IsHideout)
                {
                    continue;
                }

                if (awakeEntity.Value.IsValid)
                {
                    this.DrawEntityHealth(awakeEntity);
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

            this.AddSpritesheet();

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

        private void DrawEntityHealth(KeyValuePair<EntityNodeKey, Entity> entity)
        {
            var hasVital = entity.Value.TryGetComponent<Life>(out var entityLife);
            var hasOMP = entity.Value.TryGetComponent<ObjectMagicProperties>(out var entityMagicProperties);
            var isBlockage = entity.Value.TryGetComponent<TriggerableBlockage>(out var _);
            var hasRender = entity.Value.TryGetComponent<Render>(out var eRender);
            var hasPositioned = entity.Value.TryGetComponent<Positioned>(out var entityPositioned);
            var isPlayer = entity.Value.TryGetComponent<Player>(out var _);
            var willDieAfterTime = entity.Value.TryGetComponent<DiesAfterTime>(out var _);
            var isFriendly = hasPositioned && entityPositioned.IsFriendly;

            var rarity = hasOMP ? entityMagicProperties.Rarity : Rarity.Normal;

            var isCurrentPlayer =
                entity.Value.Address == Core.States.InGameStateObject.CurrentAreaInstance.Player.Address;

            var drawBar = isPlayer && isCurrentPlayer && this.Settings.ShowPlayerBars ||
                          hasPositioned && isFriendly && this.Settings.ShowFriendlyBars && !isCurrentPlayer ||
                          hasPositioned && !isFriendly && hasOMP && (
                              rarity == Rarity.Normal && this.Settings.ShowNormalBar ||
                              rarity == Rarity.Magic && this.Settings.ShowMagicBar ||
                              rarity == Rarity.Rare && this.Settings.ShowRareBar ||
                              rarity == Rarity.Unique && this.Settings.ShowUniqueBar
                          );

            if (!drawBar || !hasVital || !entityLife.IsAlive || !hasOMP && !isPlayer || !hasRender || !hasPositioned ||
                willDieAfterTime || isBlockage)
            {
                return;
            }

            var drawBorder = hasOMP && this.Settings.ShowRarityBorders && (
                rarity == Rarity.Normal && this.Settings.ShowNormalBorders ||
                rarity == Rarity.Magic && this.Settings.ShowMagicBorders ||
                rarity == Rarity.Rare && this.Settings.ShowRareBorders ||
                rarity == Rarity.Unique && this.Settings.ShowUniqueBorders
            );
            var borderColor = hasOMP && drawBorder ? this.RarityColor(rarity) : 0;

            var curPos = eRender.WorldPosition;
            curPos.X += 11.25f;
            curPos.Z -= 1.4f * eRender.ModelBounds.Z;
            var location = Core.States.InGameStateObject.WorldToScreen(curPos);

            if (!this.bPositions.TryGetValue(entity.Key.id, out var prevLocation))
            {
                this.bPositions.TryAdd(entity.Key.id, location);
            }
            else
            {
                location = MathHelper.Lerp(prevLocation, location, 0.2f);
                this.bPositions.TryUpdate(entity.Key.id, location, prevLocation);
            }

            float hpReserved = entityLife.Health.ReservedPercent / 100;
            var hpPercent = entityLife.Health.CurrentInPercent() * ((100 - hpReserved) / 100);

            float esReserved = entityLife.EnergyShield.ReservedPercent / 100;
            var esPercent = entityLife.EnergyShield.CurrentInPercent() * ((100 - esReserved) / 100);

            float manaReserved = entityLife.Mana.ReservedPercent / 100;
            var manaPercent = entityLife.Mana.CurrentInPercent() * ((100 - manaReserved) / 100);

            var scale = this.RarityBarScale(rarity, isPlayer, isFriendly);

            var hpOffset = new Vector2(0, 1) * scale;
            var manaOffset = new Vector2(0, 10) * scale;

            var showCulling = hasOMP && this.Settings.ShowCullRange && (
                rarity == Rarity.Normal && this.Settings.ShowNormalCull ||
                rarity == Rarity.Magic && this.Settings.ShowMagicCull ||
                rarity == Rarity.Rare && this.Settings.ShowRareCull ||
                rarity == Rarity.Unique && this.Settings.ShowUniqueCull
            );
            var inCullingRange = hpPercent > 0 && hpPercent < this.Settings.CullingRange && showCulling;
            var cullingColor = UiHelper.Color(this.Settings.CullRangeColor * 255f);

            // TODO: Make correct dictionary instead of IconPickers
            Vector2 d1 = new(location.X, location.Y);
            Vector2 d2 = new(location.X, location.Y);
            Vector2 d3 = new(location.X, location.Y);
            Vector2 d4 = new(location.X, location.Y);
            Vector2 d5 = new(location.X, location.Y);
            Vector2 d6 = new(location.X, location.Y);
            if (isFriendly)
            {
                if (isCurrentPlayer)
                {
                    this.DrawSprite("EmptyDoubleBar", scale, 1, 68, 108, 19, 110, 88, d1, 108, 19, -1, -1, false);

                    this.DrawSprite("EmptyMana", scale, 1, 19, 1, 8, 110, 88, d2 + manaOffset, 104, 8,
                        100f - manaReserved, -1, false);
                    this.DrawSprite("Mana", scale, 1, 47, 1, 8, 110, 88, d3 + manaOffset, 104, 8, manaPercent, -1,
                        false);
                }
                else
                {
                    this.DrawSprite("EmptyBar", scale, 1, 57, 108, 9, 110, 88, d1, 108, 9, -1, -1, false);
                }

                this.DrawSprite("EmptyHP", scale, 1, 10, 1, 7, 110, 88, d4 + hpOffset, 104, 7, 100f - hpReserved, -1,
                    false);
                this.DrawSprite("HP", scale, 1, 38, 1, 7, 110, 88, d5 + hpOffset, 104, 7, hpPercent, -1,
                    this.Settings.ShowFriendlyGradationMarks);
            }
            else
            {
                if (this.Settings.ShowEnemyMana)
                {
                    this.DrawSprite("EmptyDoubleBar", scale, 1, 68, 108, 19, 110, 88, d1, 108, 19, -1, -1, false,
                        drawBorder, borderColor, false, false);

                    this.DrawSprite("EmptyMana", scale, 1, 19, 1, 8, 110, 88, d2 + manaOffset, 104, 8,
                        100f - manaReserved, -1, false);
                    this.DrawSprite("Mana", scale, 1, 47, 1, 8, 110, 88, d3 + manaOffset, 104, 8, manaPercent, -1,
                        false);
                }
                else
                {
                    this.DrawSprite("EmptyBar", scale, 1, 57, 108, 9, 110, 88, d1, 108, 9, -1, -1, false, drawBorder,
                        borderColor, false, false);
                }

                this.DrawSprite("EmptyHP", scale, 1, 10, 1, 7, 110, 88, d4 + hpOffset, 104, 7, 100f - hpReserved, -1,
                    false);
                this.DrawSprite("EnemyHP", scale, 1, 29, 1, 7, 110, 88, d5 + hpOffset, 104, 7, hpPercent, -1,
                    this.Settings.ShowEnemyGradationMarks, inCullingRange, cullingColor, true, true);
            }

            if (entityLife.EnergyShield.Total > 0)
            {
                this.DrawSprite("ES", scale, 1, 1, 1, 7, 110, 88, d6 + hpOffset, 104, 7, esPercent, -1, false);
            }
        }

        private void DrawSprite(
            string spriteName,
            float scale,
            float sx,
            float sy,
            float sw,
            float sh,
            float ssw,
            float ssh,
            Vector2 t,
            float tw,
            float th,
            float mulw,
            float mulh,
            bool marks)
        {
            this.DrawSprite(spriteName, scale, sx, sy, sw, sh, ssw, ssh, t, tw, th, mulw, mulh, marks, false, 0, false,
                false);
        }

        private void DrawSprite(
            string spriteName,
            float scale,
            float sx,
            float sy,
            float sw,
            float sh,
            float ssw,
            float ssh,
            Vector2 t,
            float tw,
            float th,
            float mulw,
            float mulh,
            bool marks,
            bool border,
            uint borderColor,
            bool inner,
            bool fill)
        {
            var draw = ImGui.GetBackgroundDrawList();
            Vector2 uv0 = new(sx / ssw, sy / ssh);
            Vector2 uv1 = new((sx + sw) / ssw, (sy + sh) / ssh);
            var sprite = this.sprites[spriteName];
            var bounds = new Vector2(tw * ((mulw < 0 ? 100 : mulw) / 100), th * ((mulh < 0 ? 100 : mulh) / 100)) *
                         scale;
            var vbounds = new Vector2(tw, th) * scale;
            Vector2 half = new(10 + vbounds.X / 2, 0);
            var pos = t - half;

            draw.AddImage(sprite.TexturePtr, pos, pos + bounds, uv0, uv1);

            if (marks)
            {
                var markColor = UiHelper.Color(255, 255, 255, 100);
                Vector2 markLine = new(0, vbounds.Y - 1.5f);
                Vector2 mark25 = new(vbounds.X * 0.25f, 0);
                Vector2 mark50 = new(vbounds.X * 0.5f, 0);
                Vector2 mark75 = new(vbounds.X * 0.75f, 0);

                draw.AddLine(pos + mark25, pos + markLine + mark25, markColor);
                draw.AddLine(pos + mark50, pos + markLine + mark50, markColor);
                draw.AddLine(pos + mark75, pos + markLine + mark75, markColor);
            }

            if (border)
            {
                var b1 = pos;
                var b2 = pos + (inner ? bounds : vbounds);

                if (fill)
                {
                    draw.AddRectFilled(b1, b2, borderColor);
                }
                else
                {
                    draw.AddRect(b1, b2, borderColor);
                }
            }
        }

        private uint RarityColor(Rarity rarity)
        {
            return rarity switch
            {
                Rarity.Unique => UiHelper.Color(this.Settings.UniqueColor * 255f),
                Rarity.Rare => UiHelper.Color(this.Settings.RareColor * 255f),
                Rarity.Magic => UiHelper.Color(this.Settings.MagicColor * 255f),
                _ => UiHelper.Color(this.Settings.NormalColor * 255f)
            };
        }

        private float RarityBarScale(Rarity rarity, bool isPlayer, bool isFriendly)
        {
            if (isPlayer)
            {
                return this.Settings.PlayerBarScale;
            }

            if (!isPlayer && isFriendly)
            {
                return this.Settings.FriendlyBarScale;
            }

            return rarity switch
            {
                Rarity.Unique => this.Settings.UniqueBarScale,
                Rarity.Rare => this.Settings.RareBarScale,
                Rarity.Magic => this.Settings.MagicBarScale,
                _ => this.Settings.NormalBarScale
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
        ///     Adds the default icons if the setting file isn't available.
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