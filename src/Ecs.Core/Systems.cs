using System;
using System.Collections.Generic;
using System.Reflection;

namespace Ecs.Core
{
    public sealed class Systems
    {
        public readonly World World;

        internal int _index;

        private bool _isInitialized = false;

        private readonly AppendOnlyList<SystemData> _systems = new AppendOnlyList<SystemData>(EcsConstants.InitialSystemsCapacity);
        private readonly Dictionary<Type, object> _injectedObjects = new Dictionary<Type, object>(32);

        public Systems(World world)
        {
            World = world;

            _index = World.NewSystems();
        }

        public void Create()
        {
            ThrowIfInitialized();

            InjectDependencies();

            for (int i = 0; i < _systems.Count; i++)
            {
                _systems.Items[i].System.OnCreate();
            }
        }

        public Systems Add(SystemBase system)
        {
            ThrowIfInitialized();

            _systems.Add(new SystemData
            {
                System = system,
                IsActive = true,
            });

            return this;
        }

        private void ThrowIfInitialized()
        {
            if (_isInitialized)
            {
                throw new InvalidOperationException("Invalid operation after intialization.");
            }
        }

        public Systems Inject(object obj, Type type = null)
        {
            ThrowIfInitialized();

            if (obj == null)
            {
                throw new InvalidOperationException($"Cannot inject null object.");
            }

            if (type?.IsInstanceOfType(obj) ?? false)
            {
                throw new InvalidOperationException($"{nameof(obj)} of type {obj.GetType()} is not an instance of type {type}.");
            }

            if (type == null)
            {
                type = obj.GetType();
            }

            _injectedObjects[type] = obj;

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

        private void InjectDependencies()
        {
            for (int i = 0; i < _systems.Count; i++)
            {
                InjectDependenciesToSystem(
                    World,
                    _index,
                    _systems.Items[i].System,
                    _injectedObjects);
            }
        }

        private static void InjectDependenciesToSystem(
            World world,
            int systemsIndex,
            SystemBase system,
            Dictionary<Type, object> injectedObjects)
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

                // Custom injections.
                foreach (var kv in injectedObjects)
                {
                    if (field.FieldType.IsAssignableFrom(kv.Key))
                    {
                        field.SetValue(system, kv.Value);
                        break;
                    }
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
