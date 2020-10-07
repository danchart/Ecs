namespace Ecs.Core
{
    public abstract class SystemBase
    {
        protected internal Version GlobalSystemVersion;
        protected internal Version LastSystemVersion;

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

        public bool IsChanged(Version version)
        {
            return 
                version > this.LastSystemVersion ||
                version == Version.Zero;
        }
    }
}
