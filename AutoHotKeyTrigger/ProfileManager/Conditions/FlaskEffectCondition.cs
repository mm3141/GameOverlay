﻿// <copyright file="FlaskEffectCondition.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AutoHotKeyTrigger.ProfileManager.Conditions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GameHelper;
    using GameHelper.RemoteObjects.Components;
    using ImGuiNET;
    using Newtonsoft.Json;

    /// <summary>
    ///     For triggering an action when flask effect is not active on player.
    ///     NOTE: will not trigger the action if flask isn't available on the slot.
    /// </summary>
    public class FlaskEffectCondition : ICondition
    {
        private static readonly FlaskEffectCondition ConfigurationInstance = new(1);

        [JsonProperty] private int flaskSlot;
        private IntPtr flaskAddressCache = IntPtr.Zero;
        private List<string> flaskBuffsCache = new();

        /// <summary>
        ///     Initializes a new instance of the <see cref="FlaskEffectCondition" /> class.
        /// </summary>
        /// <param name="flaskSlot">flask number whos effect to use in the condition.</param>
        public FlaskEffectCondition(int flaskSlot)
        {
            this.flaskSlot = flaskSlot;
        }

        /// <summary>
        ///     Draws the ImGui widget for adding the condition.
        /// </summary>
        /// <returns>
        ///     <see cref="ICondition" /> if user wants to add the condition, otherwise null.
        /// </returns>
        public static FlaskEffectCondition Add()
        {
            ConfigurationInstance.ToImGui();
            ImGui.SameLine();
            if (ImGui.Button("Add##FlaskEffect"))
            {
                return new FlaskEffectCondition(ConfigurationInstance.flaskSlot);
            }

            return null;
        }

        /// <inheritdoc />
        public void Display()
        {
            this.ToImGui();
        }

        /// <inheritdoc />
        public bool Evaluate()
        {
            var flask = Core.States.InGameStateObject.CurrentAreaInstance.ServerDataObject.FlaskInventory[0, this.flaskSlot - 1];
            if (flask.Address == IntPtr.Zero)
            {
                return false;
            }

            if (flask.Address != this.flaskAddressCache)
            {
                if (flask.TryGetComponent<Base>(out var baseComponent))
                {
                    if (JsonDataHelper.FlaskNameToBuffGroups.TryGetValue(
                        baseComponent.ItemBaseName,
                        out var buffNames))
                    {
                        this.flaskBuffsCache = buffNames;
                        this.flaskAddressCache = flask.Address;
                    }
                    else
                    {
                        var allItems = Core.States.InGameStateObject.CurrentAreaInstance.
                            ServerDataObject.FlaskInventory.Items.Values;
                        var data = string.Empty;
                        foreach (var item in allItems)
                        {
                            data += item.Path;
                            data += " ";
                        }

                        throw new Exception($"New (IsValid={flask.IsValid}) flask base found " +
                            $"{baseComponent.ItemBaseName}. Please let the developer know. All items in list are {data}");
                    }
                }
            }

            var player = Core.States.InGameStateObject.CurrentAreaInstance.Player;
            if (player.TryGetComponent<Buffs>(out var buffComponent))
            {
                if (!this.flaskBuffsCache.Any(buffName => buffComponent.StatusEffects.ContainsKey(buffName)))
                {
                    return true;
                }
            }

            return false;
        }

        private void ToImGui()
        {
            ImGui.Text("Player does not have effect of flask");
            ImGui.SameLine();
            ImGui.DragInt("##FlaskEffectFlaskSlot", ref this.flaskSlot, 0.05f, 1, 5);
        }
    }
}