using Ecs.Core;
using Volatile;

namespace Simulation.Core
{
    public interface IPhysicsSystemProxy
    {
        void Run(float deltaTime);

        VoltBody GetRigidBody(Entity entity);
    }
}
