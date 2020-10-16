using Common.Core.Numerics;

namespace Game.Simulation.Core
{
    [ComponentId(ComponentId.Transform)]
    public struct TransformComponent
    {
        public Vector2 position;
        public float rotation;
    }
}
