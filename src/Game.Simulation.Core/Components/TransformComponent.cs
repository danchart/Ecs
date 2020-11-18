using Common.Core.Numerics;
using System;

namespace Game.Simulation.Core
{
    [ComponentId(ComponentId.Transform)]
    public struct TransformComponent
    {
        public Vector2 position;
        public float rotation;

        private const float NotEqualEpsilon = 0.00001f;
        private const float NotEqualEpsilonSquared = NotEqualEpsilon * NotEqualEpsilon;

        public bool IsNotEqualTo(in Vector2 p, float r)
        {
            return
                (this.position - p).LengthSquared() > NotEqualEpsilonSquared ||
                Math.Abs(this.rotation - r) > NotEqualEpsilon;
        }
    }
}
