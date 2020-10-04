using System.Runtime.ExceptionServices;

namespace Ecs.Core
{
    public abstract class SystemBase
    {
        public abstract void OnUpdate();
    }


    public class Systems
    {
        public readonly World World;

        public AppendOnlyList<SystemData> _systems = new AppendOnlyList<SystemData>(EcsConstants.InitialSystemsCapacity);

        public Systems(World world)
        {
            World = world;
        }

        public void Add(SystemBase system)
        {
            _systems.Add(new SystemData
            {
                System = system
            });
        }

        /// <summary>
        /// Run update on systems.
        /// </summary>
        public void Run()
        {
            for (int i = 0; i < _systems.Count; i++)
            {
                _systems.Items[i].System.OnUpdate();
            }
        }

        public struct SystemData
        {
            public SystemBase System;
        }
    }
}
