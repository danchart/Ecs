using Ecs.Core;
using Volatile;

namespace Simulation.Core
{
    public interface IPhysicsSystemProxy
    {
        VoltBody GetRigidBody(Entity entity);
    }
}
