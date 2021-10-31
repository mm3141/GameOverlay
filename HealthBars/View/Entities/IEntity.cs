namespace HealthBars.View.Entities
{
    public interface IEntity
    {
        public void Draw(EntityParams entityParams, SpriteController spriteController);
    }
}