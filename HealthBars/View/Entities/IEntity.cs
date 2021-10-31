namespace HealthBars.View.Entities {
    /// <summary>
    /// 
    /// </summary>
    public interface IEntity {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="eP"></param>
        /// <param name="spriteController"></param>
        public void Draw(EntityParams eP, SpriteController spriteController);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityParams"></param>
        /// <returns></returns>
        public bool ShouldDraw(EntityParams entityParams);
    }
}