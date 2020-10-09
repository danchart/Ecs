using System;

namespace Ecs.Core
{
    public readonly struct Version : IComparable<Version>, IEquatable<Version>
    {
        /// <summary>
        /// "Zero" is considered always changed (it's either the initial or wrap around state).
        /// </summary>
        static public readonly Version Zero = new Version(0);

        internal uint Value { get; }

        internal Version(uint value = 0)
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
        public static bool operator <=(Version a, Version b) => (a.Value <= b.Value);
        public static bool operator >(Version a, Version b) => (a.Value > b.Value);
        public static bool operator >=(Version a, Version b) => (a.Value >= b.Value);
    }

    public static class VersionExtensions
    {
        /// <summary>
        /// Returns the next Version.
        /// </summary>
        internal static Version GetNext(in this Version version)
        {
            return new Version(version.Value + 1);
        }
    }
}