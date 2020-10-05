﻿using System;
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

            BuildEntityQueries();

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
                _systems.Items[i].System.OnUpdate(deltaTime);
            }
        }

        private void BuildEntityQueries()
        {
            for (int i = 0; i < _systems.Count; i++)
            {
                BuildEntityQueriesForSystem(World, _systems.Items[i].System);
            }
        }

        private static void BuildEntityQueriesForSystem(World world, SystemBase system)
        {
            var systemType = system.GetType();
            var worldType = world.GetType();
            var entityQueryType = typeof(EntityQuery);

            foreach (var fieldInfo in systemType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                // Inject World
                if (fieldInfo.FieldType.IsAssignableFrom(worldType))
                {
                    fieldInfo.SetValue(system, world);

                    continue;
                }
                // Inject entity query
                if (fieldInfo.FieldType == entityQueryType)
                {
                    var entityQuery = (EntityQuery) fieldInfo.GetValue(system);

                    world.AddEntityQuery(entityQuery);

                    entityQuery.World = world;

                    //fieldInfo.SetValue(system, world.GetFilter(fieldInfo.FieldType));
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
