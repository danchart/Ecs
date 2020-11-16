namespace Ecs.Core
{
    public abstract class SystemBase
    {
        /// <summary>
        /// The version of the current system update.
        /// </summary>
        protected internal Version GlobalVersion;

        /// <summary>
        /// The version of the last system update.
        /// </summary>
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
                this.LastSystemVersion == Version.Zero;
        }
    }
}
