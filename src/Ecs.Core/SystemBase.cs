namespace Ecs.Core
{
    public abstract class SystemBase
    {
        /// <summary>
        /// Invoked on System.Init();
        /// </summary>
        public virtual void OnCreate()
        {
        }

        /// <summary>
        /// Invoked on Systems.Run()
        /// </summary>
        public virtual void OnUpdate(float deltaTime)
        {
        }
    }

}
