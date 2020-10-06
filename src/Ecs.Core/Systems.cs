using System;
using System.Reflection;

namespace Ecs.Core
{
    public class Systems
    {
        public readonly World World;

        public AppendOnlyList<SystemData> _systems = new AppendOnlyList<SystemData>(EcsConstants.InitialSystemsCapacity);

        private bool _isInitialized = false;

        public Systems(World world)
        {
            World = world;
        }

        public void Init()
        {
            if (_isInitialized)
            {
                throw new InvalidOperationException($"{nameof(System)} already initialized.");
            }

            CreateEntityQueries();

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
                System = system
            });

            return this;
        }

        /// <summary>
        /// Run update on systems.
        /// </summary>
        public void Run(float deltaTime)
        {
            for (int i = 0; i < _systems.Count; i++)
            {
                var system = _systems.Items[i].System;

                system.GlobalSystemVersion = ++World.GlobalSystemVersion;
                system.LastSystemVersion = World.LastSystemVersion;

                system.OnUpdate(deltaTime);

                //World.GlobalSystemVersion++;
            }

            World.LastSystemVersion = World.GlobalSystemVersion;
        }

        private void CreateEntityQueries()
        {
            for (int i = 0; i < _systems.Count; i++)
            {
                AssignEntityQueriesToSystem(World, _systems.Items[i].System);
            }
        }

        private static void AssignEntityQueriesToSystem(World world, SystemBase system)
        {
            var systemType = system.GetType();
            var worldType = world.GetType();
            var entityQueryType = typeof(EntityQuery);

            foreach (var field in systemType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                // Assign World
                if (field.FieldType.IsAssignableFrom(worldType))
                {
                    field.SetValue(system, world);

                    continue;
                }

                // Assign entity query
                if (field.FieldType.IsSubclassOf(entityQueryType))
                {
                    field.SetValue(system, world.GetEntityQuery(field.FieldType));

                    continue;
                }
            }
        }

        public struct SystemData
        {
            public SystemBase System;
        }
    }
}
