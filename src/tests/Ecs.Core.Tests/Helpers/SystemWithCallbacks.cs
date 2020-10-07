using System;
using System.Collections.Generic;

namespace Ecs.Core.Tests
{
    internal class SystemWithCallbacks<Comp> : SystemWithCallbacks<object, Comp> where Comp : struct
    {
    }

    internal class SystemWithCallbacks<T, Comp> : SystemBase where Comp : struct
    {
        public EntityQuery<Comp> Query = null;

        public Action<SystemWithCallbacks<T, Comp>> OnCreateAction = null;
        public Action<SystemWithCallbacks<T, Comp>, float> OnUpdateAction = null;

        public Dictionary<string, T> Data = new Dictionary<string, T>();

        public override void OnCreate()
        {
            OnCreateAction?.Invoke(this);
        }

        public override void OnUpdate(float deltaTime)
        {
            OnUpdateAction?.Invoke(this, deltaTime);
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
