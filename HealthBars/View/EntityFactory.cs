#nullable enable
namespace HealthBars.View
{
    using Entities;
    using GameHelper;
    using GameHelper.RemoteObjects.Components;
    using GameHelper.RemoteObjects.States.InGameStateObjects;

    /// <summary>
    /// </summary>
    public class EntityFactory
    {
        private readonly Invalid invalid = new();
        private readonly Enemy enemy = new();
        private readonly Friendly friendly = new();
        private readonly CurrentPlayer player = new();

        /// <summary>
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="drawEntity"></param>
        /// <returns></returns>
        public bool TryGetEntity(Entity entity, out IEntity drawEntity)
        {
            drawEntity = this.invalid;
            var hasVital = entity.GetComp<Life>(out var entityLife);
            if (!hasVital || !entityLife.IsAlive)
            {
                return false;
            }

            var isBlockage = entity.GetComp<TriggerableBlockage>(out _);
            if (isBlockage)
            {
                return false;
            }

            var hasRender = entity.GetComp<Render>(out _);
            if (!hasRender)
            {
                return false;
            }

            var hasPositioned = entity.GetComp<Positioned>(out var positioned);
            if (!hasPositioned)
            {
                return false;
            }

            var isPlayer = entity.GetComp<Player>(out _);
            var hasMagicProperties = entity.GetComp<ObjectMagicProperties>(out _);
            if (!hasMagicProperties && !isPlayer)
            {
                return false;
            }

            var willDieAfterTime = entity.GetComp<DiesAfterTime>(out _);
            if (willDieAfterTime)
            {
                return false;
            }

            var isCurrentPlayer =
                entity.Address == Core.States.InGameStateObject.CurrentAreaInstance.Player.Address;

            if (isCurrentPlayer)
            {
                drawEntity = this.player;
                return true;
            }

            var isFriendly = positioned.IsFriendly;
            if (isFriendly)
            {
                drawEntity = this.friendly;
                return true;
            }

            drawEntity = this.enemy;
            return true;
        }
    }
}