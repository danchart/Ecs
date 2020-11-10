using Ecs.Core;
using System.Collections.Generic;
using Volatile;

namespace Simulation.Core
{
    public interface IPhysicsWorld
    {
        void AddPolygon(
            Entity entity,
            bool isStatic,
            Common.Core.Numerics.Vector2 originWS,
            float rotation,
            Common.Core.Numerics.Vector2[] vertices,
            float density = 1);

        void AddCircle(
            Entity entity,
            bool isStatic,
            Common.Core.Numerics.Vector2 originWS,
            float rotation,
            float radius,
            float density = 1);

        bool Remove(Entity entity);
    }

    public sealed class VolatilePhysicsWorld : IPhysicsWorld, IPhysicsSystemProxy
    {
        private readonly VoltWorld _world;
        private readonly Dictionary<Entity, VoltBody> _entityToBody;

        public VolatilePhysicsWorld(int historyLength = 0, int capacity = 1024)
        {
            this._world = new VoltWorld(historyLength);
            this._entityToBody = new Dictionary<Entity, VoltBody>(capacity);
        }

        #region IPhysicsWorld

        void IPhysicsWorld.AddCircle(
            Entity entity,
            bool isStatic,
            Common.Core.Numerics.Vector2 originWS,
            float rotation,
            float radius,
            float density = 1)
        {
            var shape = this._world.CreateCircleWorldSpace(
                ToVolt(originWS),
                radius,
                density);

            this._entityToBody[entity] = CreateBody(isStatic, originWS, rotation, shape);
        }

        void IPhysicsWorld.AddPolygon(
            Entity entity,
            bool isStatic,
            Common.Core.Numerics.Vector2 originWS,
            float rotation,
            Common.Core.Numerics.Vector2[] vertices,
            float density = 1)
        {
            var shape = this._world.CreatePolygonBodySpace(
                ToVolt(vertices),
                density);

            this._entityToBody[entity] = CreateBody(isStatic, originWS, rotation, shape);

        }

        bool IPhysicsWorld.Remove(Entity entity)
        {
            throw new System.NotImplementedException();
        }

        #endregion

        #region IPhysicsSystemProxy

        VoltBody IPhysicsSystemProxy.GetRigidBody(Entity entity)
        {
            return this._entityToBody[entity];
        }

        #endregion
        private VoltBody CreateBody(
            bool isStatic,
            Common.Core.Numerics.Vector2 originWS,
            float rotation,
            VoltShape shape)
        {
            if (isStatic)
            {
                return this._world.CreateStaticBody(ToVolt(originWS), rotation, new VoltShape[] { shape });
            }
            else
            {
                return this._world.CreateDynamicBody(ToVolt(originWS), rotation, new VoltShape[] { shape });
            }
        }

        private static Volatile.Vector2 ToVolt(Common.Core.Numerics.Vector2 vector)
        {
            return new Volatile.Vector2(vector.x, vector.y);
        }

        private static Volatile.Vector2[] ToVolt(Common.Core.Numerics.Vector2[] vectors)
        {
            var result = new Volatile.Vector2[vectors.Length];

            for (int i = 0; i < vectors.Length; i++)
            {
                result[i] = ToVolt(vectors[i]);
            }

            return result;
        }
    }
}
