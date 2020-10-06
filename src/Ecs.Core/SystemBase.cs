namespace Ecs.Core
{
    public abstract class SystemBase
    {
        protected internal uint GlobalSystemVersion;
        protected internal uint LastSystemVersion;

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
