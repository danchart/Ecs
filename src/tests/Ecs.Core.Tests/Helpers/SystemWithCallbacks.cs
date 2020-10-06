using System;
using System.Collections.Generic;

namespace Ecs.Core.Tests
{
    internal class SystemWithCallbacks : SystemBase
    {
        public Action OnCreateAction = null;
        public Action<float> OnUpdateAction = null;

        public override void OnCreate()
        {
            OnCreateAction?.Invoke();
        }

        public override void OnUpdate(float deltaTime)
        {
            OnUpdateAction?.Invoke(deltaTime);
        }
    }

    internal class SystemWithQueryAndCallbacks<T> : SystemBase where T : struct
    {
        public EntityQuery<T> Query = null;

        public Action OnCreateAction = null;
        public Action<float> OnUpdateAction = null;

        public Dictionary<string, object> Data = new Dictionary<string, object>();

        public override void OnCreate()
        {
            OnCreateAction?.Invoke();
        }

        public override void OnUpdate(float deltaTime)
        {
            OnUpdateAction?.Invoke(deltaTime);
        }
    }
}
