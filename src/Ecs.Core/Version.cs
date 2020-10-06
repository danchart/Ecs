using System;

namespace Ecs.Core
{
    //public struct Version
    //{
    //    internal uint Value;

    //    public bool IsNewer(in Version version)
    //    {
    //        return (this.Value < version.Value);
    //    }

    //    internal Version GetNext()
    //    {
    //        var version = this;

    //        if (++version.Value == 0)
    //        {
    //            // TODO: Handle wrapping.
    //            throw new Exception("Version count wrapped to 0.");
    //        }

    //        return version;
    //    }
    //}

    public readonly struct Version : IComparable<Version>, IEquatable<Version>
    {
        internal uint Value { get; }

        internal Version(uint value = 1)
        {
            Value = value;
        }

        public bool Equals(Version other) => this.Value.Equals(other.Value);
        public int CompareTo(Version other) => Value.CompareTo(other.Value);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return
                obj is Version other &&
                Equals(other);
        }

        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString();

        public static bool operator ==(Version a, Version b) => a.CompareTo(b) == 0;
        public static bool operator !=(Version a, Version b) => !(a == b);
        public static bool operator <(Version a, Version b) => (a.Value < b.Value);
        public static bool operator >(Version a, Version b) => (a.Value > b.Value);
    }


    public static class VersionExtensions
    {
        internal static Version GetNext(in this Version version)
        {
            if (version.Value == uint.MaxValue)
            {
                // TODO: Handle wrapping.
                throw new Exception("Version count wrapped to 0.");
            }

            return new Version(version.Value + 1);
        }
    }
}