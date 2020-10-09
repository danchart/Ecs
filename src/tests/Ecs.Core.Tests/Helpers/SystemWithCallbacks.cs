using System;
using System.Collections.Generic;

namespace Ecs.Core.Tests
{
    internal class SystemWithCallbacks<T> : SystemBase 
    {
        public Action<SystemWithCallbacks<T>> OnCreateAction = null;
        public Action<SystemWithCallbacks<T>, float> OnUpdateAction = null;

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

    internal class SystemWithCallbacksAndQuery<Comp> : SystemWithCallbacksAndQuery<object, Comp> 
        where Comp : unmanaged
    {
    }

    internal class SystemWithCallbacksAndQuery<T, Comp> : SystemBase 
        where Comp : unmanaged
    {
        public EntityQuery<Comp> Query = null;

        public Action<SystemWithCallbacksAndQuery<T, Comp>> OnCreateAction = null;
        public Action<SystemWithCallbacksAndQuery<T, Comp>, float> OnUpdateAction = null;

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

    internal class SystemWithQueryAndCallbacks<T> : SystemBase 
        where T : unmanaged
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
