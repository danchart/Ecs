using System;

namespace Ecs.Core
{
    public struct Version
    {
        internal uint Value;

        public bool IsNewer(in Version version)
        {
            return (this.Value < version.Value);
        }

        internal Version GetNext()
        {
            var version = this;

            if (++version.Value == 0)
            {
                // TODO: Handle wrapping.
                throw new Exception("Version count wrapped to 0.");
            }

            return version;
        }
    }
}
