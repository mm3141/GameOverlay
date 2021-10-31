#nullable enable
using System.Collections.Generic;
using GameHelper;
using GameHelper.RemoteEnums;
using GameHelper.RemoteObjects.Components;
using GameHelper.RemoteObjects.States.InGameStateObjects;
using GameOffsets.Objects.States.InGameState;
using HealthBars.View.Entities;

namespace HealthBars.View
{
    public class EntityFactory
    {
        public static IEntity? GetEntity(KeyValuePair<EntityNodeKey, Entity> entity, HealthBarsSettings settings)
        {
            var hasVital = entity.Value.TryGetComponent<Life>(out var entityLife);
            if (!hasVital || !entityLife.IsAlive)
            {
                return null;
            }

            var isBlockage = entity.Value.TryGetComponent<TriggerableBlockage>(out _);
            if (isBlockage)
            {
                return null;
            }

            var hasRender = entity.Value.TryGetComponent<Render>(out var render);
            if (!hasRender)
            {
                return null;
            }

            var hasPositioned = entity.Value.TryGetComponent<Positioned>(out var positioned);
            if (!hasPositioned)
            {
                return null;
            }

            var isPlayer = entity.Value.TryGetComponent<Player>(out _);
            var hasMagicProperties = entity.Value.TryGetComponent<ObjectMagicProperties>(out var magicProperties);
            if (!hasMagicProperties && !isPlayer)
            {
                return null;
            }

            var willDieAfterTime = entity.Value.TryGetComponent<DiesAfterTime>(out _);
            if (willDieAfterTime)
            {
                return null;
            }

            var isFriendly = positioned.IsFriendly;
            var rarity = hasMagicProperties ? magicProperties.Rarity : Rarity.Normal;
            var isCurrentPlayer =
                entity.Value.Address == Core.States.InGameStateObject.CurrentAreaInstance.Player.Address;
            var drawBar = (isPlayer && isCurrentPlayer && settings.ShowPlayerBars) ||
                          (isFriendly && settings.ShowFriendlyBars && !isCurrentPlayer) ||
                          (!isFriendly && hasMagicProperties && (
                                  (rarity == Rarity.Normal) && settings.ShowNormalBar ||
                                  (rarity == Rarity.Magic) && settings.ShowMagicBar ||
                                  (rarity == Rarity.Rare) && settings.ShowRareBar ||
                                  (rarity == Rarity.Unique) && settings.ShowUniqueBar
                              )
                          );

            if (!drawBar)
            {
                return null;
            }

            if (isCurrentPlayer)
            {
                return new CurrentPlayer();
            }

            if (isFriendly)
            {
                return new Friendly();
            }

            return new Enemy();
        }
    }
}