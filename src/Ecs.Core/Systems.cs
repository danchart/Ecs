using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ecs.Core
{
    public class Systems
    {
        public readonly World World;

        public AppendOnlyList<SystemData> _systems = new AppendOnlyList<SystemData>(EcsConstants.InitialSystemsCapacity);

        internal int _index;

        private bool _isInitialized = false;

        public Systems(World world)
        {
            World = world;

            _index = World.NewSystems();
        }

        public void Create()
        {
            if (_isInitialized)
            {
                throw new InvalidOperationException($"{nameof(System)} already initialized.");
            }

            InjectEntityQueries();

            for (int i = 0; i < _systems.Count; i++)
            {
                _systems.Items[i].System.OnCreate();
            }
        }

        public Systems Add(SystemBase system)
        {
            if (_isInitialized)
            {
                throw new InvalidOperationException("Cannot add system after intialization.");
            }

            _systems.Add(new SystemData
            {
                System = system,
                IsActive = true,
            });

            return this;
        }

        public Systems SingleFrame<T>() 
            where T : unmanaged
        {
            return Add(new RemoveSingleFrames<T>());
        }

        /// <summary>
        /// Run update on systems.
        /// </summary>
        public void Run(float deltaTime)
        {
            for (int i = 0; i < _systems.Count; i++)
            {
                if (_systems.Items[i].IsActive)
                {
                    var system = _systems.Items[i].System;

                    World.State.GlobalSystemVersion = World.State.GlobalSystemVersion.GetNext();

                    system.GlobalSystemVersion = World.State.GlobalSystemVersion;
                    system.LastSystemVersion = World.State.LastSystemVersion.Items[_index];

                    system.OnUpdate(deltaTime);
                }
            }

            // Update per-systems last version.
            World.State.LastSystemVersion.Items[_index] = World.State.GlobalSystemVersion;

            // Increment global system version here to handle updates outside the Run() loop.
            World.State.GlobalSystemVersion = World.State.GlobalSystemVersion.GetNext();
        }

        public void SetActive(SystemBase system, bool isActive)
        {
            for (int i = 0; i < _systems.Count; i++)
            {
                ref var systemData = ref _systems.Items[i];

                if (system == systemData.System)
                {
                    systemData.IsActive = isActive;

                    return;
                }
            }
        }

        private void InjectEntityQueries()
        {
            for (int i = 0; i < _systems.Count; i++)
            {
                InjectEntityQueriesToSystem(
                    World,
                    _index,
                    _systems.Items[i].System);
            }
        }

        private static void InjectEntityQueriesToSystem(
            World world,
            int systemsIndex,
            SystemBase system)
        {
            var systemType = system.GetType();
            var worldType = world.GetType();

            var perSystemEntityQueryType = typeof(PerSystemsEntityQuery);
            var globalEntityQueryType = typeof(GlobalEntityQuery);


            foreach (var field in systemType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                // Assign World
                if (field.FieldType.IsAssignableFrom(worldType))
                {
                    field.SetValue(system, world);

                    continue;
                }

                // Assign entity query
                if (field.FieldType.IsSubclassOf(perSystemEntityQueryType))
                {
                    field.SetValue(system, world.GetPerSystemsEntityQuery(field.FieldType, systemsIndex));

                    continue;
                }
                else if (field.FieldType.IsSubclassOf(globalEntityQueryType))
                {
                    field.SetValue(system, world.GetGlobalEntityQuery(field.FieldType));

                    continue;
                }
            }
        }

        public struct SystemData
        {
            public SystemBase System;
            public bool IsActive;
        }

        internal sealed class RemoveSingleFrames<T> : SystemBase 
            where T : unmanaged
        {
            readonly EntityQuery<T> Query = null;

            public override void OnUpdate(float deltaTime)
            {
                foreach (var entity in Query)
                {
                    entity.RemoveComponent<T>();
                }
            }
        }
    }
}
