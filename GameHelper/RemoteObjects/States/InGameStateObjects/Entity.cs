// <copyright file="Entity.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteObjects.States.InGameStateObjects; 

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Components;
using GameHelper.RemoteEnums;
using GameOffsets.Objects.States.InGameState;
using ImGuiNET;
using Utils;

/// <summary>
///     Points to an Entity/Object in the game.
///     Entity is basically item/monster/effect/player/etc on the ground.
/// </summary>
public class Entity : RemoteObjectBase {
    public override string ToString() {
        return this.EntityType + " d=" + this.DistanceFrom(Core.me);
    }
    private static List<string> diesAfterTimeIgnore = new()
    {
        "Metadata/Monsters/AtlasExiles/CrusaderInfluenceMonsters/CrusaderArcaneRune",
        "Metadata/Monsters/Daemon/DaemonLaboratoryBlackhole",
        "Metadata/Monsters/AtlasExiles/AtlasExile",
        "Metadata/Monsters/Daemon/MaligaroBladeVortexDaemon",
        "Metadata/Monsters/Daemon/DoNothingDaemon",
        "Metadata/Monsters/Daemon/ShakariQuicksandDaemon",
        "Metadata/Monsters/AtlasInvaders/CleansingMonsters/CleansingPhantasmPossessionDemon",
        "Metadata/Monsters/Daemon/Archnemesis"
    };

    private static string deliriumHiddenMonsterStarting =
        "Metadata/Monsters/LeagueAffliction/DoodadDaemons/DoodadDaemon";

    private readonly ConcurrentDictionary<string, IntPtr> componentAddresses;
    private readonly ConcurrentDictionary<string, RemoteObjectBase> componentCache;
    private bool isnearby;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Entity" /> class.
    /// </summary>
    /// <param name="address">address of the Entity.</param>
    internal Entity(IntPtr address) : this() {
        this.Address = address;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="Entity" /> class.
    ///     NOTE: Without providing an address, only invalid and empty entity is created.
    /// </summary>
    internal Entity() : base(IntPtr.Zero, true) {
        this.componentAddresses = new();
        this.componentCache = new();
        this.isnearby = false;
        this.Path = string.Empty;
        this.Id = 0;
        this.IsValid = false;
        this.EntityType = eTypes.Unidentified;
    }

    /// <summary>
    ///     Gets the Path (e.g. Metadata/Character/int/int) assocaited to the entity.
    /// </summary>
    public string Path { get; private set; }

    /// <summary>
    ///     Gets the Id associated to the entity. This is unique per map/Area.
    /// </summary>
    public uint Id { get; private set; }

    /// <summary>
    ///     Gets a value indicating whether the entity is nearby the player or not.
    /// </summary>
    public bool IsNearby => this.IsValid && this.isnearby;

    /// <summary>
    ///     Gets or Sets a value indicating whether the entity
    ///     exists in the game or not.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    ///     Gets a value indicating the type of entity this is.
    /// </summary>
    public eTypes EntityType { get; protected set; }

    /// <summary>
    ///     Gets a value indicating whether this entity can explode or not.
    /// </summary>
    public bool CanExplode =>
        this.EntityType == eTypes.Monster ||
        this.EntityType == eTypes.Useless ||
        this.EntityType == eTypes.Stage1RewardFIT ||
        this.EntityType == eTypes.Stage1FIT ||
        this.EntityType == eTypes.Stage1EChestFIT;

    /// <summary>
    ///     Calculate the distance from the other entity.
    /// </summary>
    /// <param name="other">Other entity object.</param>
    /// <returns>
    ///     the distance from the other entity
    ///     if it can calculate; otherwise, return 0.
    /// </returns>
    public int DistanceFrom(Entity other) {
        if (this.GetComp<Render>(out var myPosComp) &&
            other.GetComp<Render>(out var otherPosComp)) {
            var dx = myPosComp.GridPosition.X - otherPosComp.GridPosition.X;
            var dy = myPosComp.GridPosition.Y - otherPosComp.GridPosition.Y;
            return (int)Math.Sqrt(dx * dx + dy * dy);
        }

        // Console.WriteLine($"Render Component missing in {this.Path} or {other.Path}");
        return 0;
    }

    /// <summary>
    ///     Gets the Component data associated with the entity.
    /// </summary>
    /// <typeparam name="T">Component type to get.</typeparam>
    /// <param name="component">component data.</param>
    /// <returns>true if the entity contains the component; otherwise, false.</returns>
    public bool GetComp<T>(out T component) where T : RemoteObjectBase {
        component = null;
        var componenName = typeof(T).Name;
        if (this.componentCache.TryGetValue(componenName, out var comp)) {
            component = (T)comp;
            return true;
        }

        if (this.componentAddresses.TryGetValue(componenName, out var compAddr)) {
            if (compAddr != IntPtr.Zero) {
                component = Activator.CreateInstance(typeof(T), compAddr) as T;
                if (component != null) {
                    this.componentCache[componenName] = component;
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    ///     Converts the <see cref="Entity" /> class data to ImGui.
    /// </summary>
    internal override void ToImGui() {
        base.ToImGui();
        ImGui.Text($"Path: {this.Path}");
        ImGui.Text($"Id: {this.Id}");
        ImGui.Text($"Is Valid: {this.IsValid}");
        ImGui.Text($"Entity Type: {this.EntityType}");
        if (ImGui.TreeNode("Components")) {
            foreach (var kv in this.componentAddresses) {
                if (this.componentCache.ContainsKey(kv.Key)) {
                    if (ImGui.TreeNode($"{kv.Key}")) {
                        this.componentCache[kv.Key].ToImGui();
                        ImGui.TreePop();
                    }
                }
                else {
                    ImGuiHelper.IntPtrToImGui(kv.Key, kv.Value);
                }
            }

            ImGui.TreePop();
        }
    }

    internal void UpdateNearby(Entity player) {
        if (this.EntityType == eTypes.Useless ||
            this.DistanceFrom(player) >= Core.GHSettings.NearbyMeaning) {
            this.isnearby = false;
        }
        else {
            this.isnearby = true;
        }
    }

    /// <summary>
    ///     Updates the component data associated with the Entity base object (i.e. item).
    /// </summary>
    /// <param name="idata">Entity base (i.e. item) data.</param>
    /// <param name="hasAddressChanged">has this class Address changed or not.</param>
    protected void UpdateComponentData(ItemStruct idata, bool hasAddressChanged) {
        var reader = Core.Process.Handle;
        if (hasAddressChanged) {
            this.componentAddresses.Clear();
            this.componentCache.Clear();
            var entityComponent = reader.ReadStdVector<IntPtr>(idata.ComponentListPtr);
            var entityDetails = reader.ReadMemory<EntityDetails>(idata.EntityDetailsPtr);
            this.Path = reader.ReadStdWString(entityDetails.name);
            var lookupPtr = reader.ReadMemory<ComponentLookUpStruct>(
                entityDetails.ComponentLookUpPtr);

            var namesAndIndexes = reader.ReadStdBucket<ComponentNameAndIndexStruct>(
                lookupPtr.ComponentsNameAndIndex);
            for (var i = 0; i < namesAndIndexes.Count; i++) {
                var nameAndIndex = namesAndIndexes[i];
                if (nameAndIndex.Index >= 0 && nameAndIndex.Index < entityComponent.Length) {
                    var name = reader.ReadString(nameAndIndex.NamePtr);
                    if (!string.IsNullOrEmpty(name)) {
                        this.componentAddresses.TryAdd(name, entityComponent[nameAndIndex.Index]);
                    }
                }
            }
        }
        else {
            foreach (var kv in this.componentCache) {
                kv.Value.Address = kv.Value.Address;
            }
        }
    }

    /// <inheritdoc />
    protected override void CleanUpData() {
        this.componentAddresses?.Clear();
        this.componentCache?.Clear();
    }

    /// <inheritdoc />
    protected override void UpdateData(bool hasAddressChanged) {
        var reader = Core.Process.Handle;
        var entityData = reader.ReadMemory<EntityOffsets>(this.Address);
        this.IsValid = EntityHelper.IsValidEntity(entityData.IsValid);
        if (!this.IsValid) {
            // Invalid entity data is normally corrupted. let's not parse it.
            return;
        }

        this.Id = entityData.Id;
        if (this.EntityType == eTypes.Useless) {
            // let's not read or parse any useless entity components.
            return;
        }

        this.UpdateComponentData(entityData.ItemBase, hasAddressChanged);
        this.ParseEntityType();
    }

    private void ParseEntityType() {
        switch (this.EntityType) {
            // There is no use case (yet) to re-evaluate the following entity types
            case eTypes.SelfPlayer:
            case eTypes.OtherPlayer:
            case eTypes.Blockage:
            case eTypes.Shrine:
                return;
        }

        if (!this.GetComp<Render>(out var _)) {
            this.EntityType = eTypes.Useless;
        }
        else if (this.GetComp<Chest>(out var chestComp)) {
            if (chestComp.IsOpened) {
                this.EntityType = eTypes.Useless;
            }
            else if (this.EntityType == eTypes.Unidentified) // so it only happen once.
            {
                if (this.GetComp<MinimapIcon>(out var _)) {
                    if (this.Path.StartsWith("Metadata/Chests/LeaguesExpedition")) {
                        this.EntityType = eTypes.ExpeditionChest;
                    }
                    else if (this.Path.StartsWith("Metadata/Chests/LeagueHeist")) {
                        this.EntityType = eTypes.HeistChest;
                    }
                    else {
                        this.EntityType = eTypes.Useless;
                    }
                }
                else if (this.Path.StartsWith("Metadata/Chests/LegionChests")) {
                    this.EntityType = eTypes.Useless;
                }
                else if (this.Path.StartsWith("Metadata/Chests/DelveChests/")) {
                    this.EntityType = eTypes.DelveChest;
                }
                else if (this.Path.StartsWith("Metadata/Chests/Breach")) {
                    this.EntityType = eTypes.BreachChest;
                }
                else if (chestComp.IsStrongbox) {
                    if (this.Path.StartsWith("Metadata/Chests/StrongBoxes/Arcanist") ||
                        this.Path.StartsWith("Metadata/Chests/StrongBoxes/Cartographer") ||
                        this.Path.StartsWith("Metadata/Chests/StrongBoxes/StrongboxDivination") ||
                        this.Path.StartsWith("Metadata/Chests/StrongBoxes/StrongboxScarab")) {
                        this.EntityType = eTypes.ImportantStrongboxChest;
                    }
                    else {
                        this.EntityType = eTypes.StrongboxChest;
                    }
                }
                else if (chestComp.IsLabelVisible) {
                    this.EntityType = eTypes.ChestWithLabels;
                }
                else {
                    this.EntityType = eTypes.Chest;
                }
            }
        }
        else if (this.GetComp<Player>(out var _)) {
            if (this.Id == Core.States.InGameStateObject.CurrentAreaInstance.Player.Id) {
                this.EntityType = eTypes.SelfPlayer;
            }
            else {
                this.EntityType = eTypes.OtherPlayer;
            }
        }
        else if (this.GetComp<Shrine>(out var _)) {
            // NOTE: Do not send Shrine to useless because it can go back to not used.
            //       e.g. Shrine in PVP area can do that.
            this.EntityType = eTypes.Shrine;
        }
        else if (this.GetComp<Life>(out var lifeComp)) {
            if (!lifeComp.IsAlive) {
                this.EntityType = eTypes.Useless;
                return;
            }

            if (this.GetComp<TriggerableBlockage>(out var _)) {
                // NOTE: Do not send blockage to useless because it can go back to blocked.
                // NOTE: If blockage Life is 0 (not IsAlive), it can be send to useless
                //       but they are so rare, it's not worth it.
                this.EntityType = eTypes.Blockage;
                return;
            }

            if (!this.GetComp<Positioned>(out var posComp)) {
                this.EntityType = eTypes.Useless;
                return;
            }

            if (!this.GetComp<ObjectMagicProperties>(out var OMP)) {
                this.EntityType = eTypes.Useless;
                return;
            }

            if (posComp.IsFriendly) {
                this.EntityType = eTypes.FriendlyMonster;
                return;
            }

            if (this.EntityType == eTypes.Unidentified &&
                   this.GetComp<DiesAfterTime>(out var _) &&
                   diesAfterTimeIgnore.Any(ignorePath => this.Path.StartsWith(ignorePath))) {
                this.EntityType = eTypes.Useless;
                return;
            }

            if (this.GetComp<Buffs>(out var buffComp)) {
                // When Legion monolith is not clicked by the user (Stage 0),
                //     Legion monsters (a.k.a FIT) has Frozen in time + Hidden buff.

                // When Legion monolith is clicked (Stage 1),
                //     FIT Not Killed by User: Just have frozen in time buff.
                //     FIT Killed by user: Just have hidden buff.

                // When Legion monolith is destroyed (Stage 2),
                //     FIT are basically same as regular monster with no Frozen-in-time/hidden buff.

                // NOTE: There are other hidden monsters in the game as well
                // e.g. Delirium monsters (a.k.a DELI), underground crabs, hidden sea witches
                var isFrozenInTime = buffComp.StatusEffects.ContainsKey("frozen_in_time");
                var isHidden = buffComp.StatusEffects.ContainsKey("hidden_monster");
                if (isFrozenInTime && isHidden) {
                    if (this.EntityType != eTypes.Stage0RewardFIT &&
                        this.EntityType != eTypes.Stage0EChestFIT &&
                        this.EntityType != eTypes.Stage0FIT) // New FITs only.
                    {
                        if (buffComp.StatusEffects.ContainsKey("legion_reward_display")) {
                            this.EntityType = eTypes.Stage0RewardFIT;
                        }
                        else if (this.Path.Contains("ChestEpic")) {
                            this.EntityType = eTypes.Stage0EChestFIT;
                        }
                        else if (this.Path.Contains("Chest")) {
                            this.EntityType = eTypes.Stage0RewardFIT;
                        }
                        else {
                            this.EntityType = eTypes.Stage0FIT;
                        }
                    }

                    return;
                }
                else if (isFrozenInTime) {
                    if (this.EntityType != eTypes.Stage1RewardFIT &&
                        this.EntityType != eTypes.Stage1EChestFIT &&
                        this.EntityType != eTypes.Stage1FIT) // New FITs only.
                    {
                        if (buffComp.StatusEffects.ContainsKey("legion_reward_display")) {
                            this.EntityType = eTypes.Stage1RewardFIT;
                        }
                        else if (this.Path.Contains("ChestEpic")) {
                            this.EntityType = eTypes.Stage1EChestFIT;
                        }
                        else if (this.Path.Contains("Chest")) {
                            this.EntityType = eTypes.Stage1RewardFIT;
                        }
                        else {
                            this.EntityType = eTypes.Stage1FIT;
                        }
                    }

                    return;
                }
                else if (isHidden) {
                    switch (this.EntityType) {
                        case eTypes.Stage0EChestFIT:
                        case eTypes.Stage0RewardFIT:
                        case eTypes.Stage0FIT:
                        case eTypes.Stage1EChestFIT:
                        case eTypes.Stage1RewardFIT:
                        case eTypes.Stage1FIT:
                        case eTypes.Stage1DeadFIT:
                            this.EntityType = eTypes.Stage1DeadFIT;
                            return;
                        case eTypes.DeliriumBomb:
                        case eTypes.DeliriumSpawner:
                            return;
                        case eTypes.Unidentified:
                            if (this.Path.StartsWith(deliriumHiddenMonsterStarting)) {
                                if (this.Path.Contains("BloodBag")) {
                                    this.EntityType = eTypes.DeliriumBomb;
                                }
                                else if (this.Path.Contains("EggFodder")) {
                                    this.EntityType = eTypes.DeliriumSpawner;
                                }
                                else if (this.Path.Contains("GlobSpawn")) {
                                    this.EntityType = eTypes.DeliriumSpawner;
                                }
                                else {
                                    this.EntityType = eTypes.Useless;
                                }

                                return;
                            }

                            break;
                    }
                }
            }

            this.EntityType = eTypes.Monster;
        }
        else {
            this.EntityType = eTypes.Useless;
        }
    }
}
