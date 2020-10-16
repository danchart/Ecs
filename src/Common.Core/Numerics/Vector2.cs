using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace Common.Core.Numerics
{
    public partial struct Vector2 : IEquatable<Vector2>, IFormattable
    {
        #region Public Static Properties
        /// <summary>
        /// Returns the vector (0,0).
        /// </summary>
        public static readonly Vector2 Zero = new Vector2();
        /// <summary>
        /// Returns the vector (1,1).
        /// </summary>
        public static readonly Vector2 One = new Vector2(1.0f, 1.0f);
        /// <summary>
        /// Returns the vector (1,0).
        /// </summary>
        public static readonly Vector2 UnitX = new Vector2(1.0f, 0.0f);
        /// <summary>
        /// Returns the vector (0,1).
        /// </summary>
        public static readonly Vector2 UnitY = new Vector2(0.0f, 1.0f);
        #endregion Public Static Properties

        #region Public instance methods
        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            int hash = this.x.GetHashCode();
            hash = HashCodeHelper.CombineHashCodes(hash, this.y.GetHashCode());
            return hash;
        }

        /// <summary>
        /// Returns a boolean indicating whether the given Object is equal to this Vector2 instance.
        /// </summary>
        /// <param name="obj">The Object to compare against.</param>
        /// <returns>True if the Object is equal to this Vector2; False otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            if (!(obj is Vector2))
                return false;
            return Equals((Vector2)obj);
        }

        /// <summary>
        /// Returns a String representing this Vector2 instance.
        /// </summary>
        /// <returns>The string representation.</returns>
        public override string ToString()
        {
            return ToString("G", CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Returns a String representing this Vector2 instance, using the specified format to format individual elements.
        /// </summary>
        /// <param name="format">The format of individual elements.</param>
        /// <returns>The string representation.</returns>
        public string ToString(string format)
        {
            return ToString(format, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Returns a String representing this Vector2 instance, using the specified format to format individual elements 
        /// and the given IFormatProvider.
        /// </summary>
        /// <param name="format">The format of individual elements.</param>
        /// <param name="formatProvider">The format provider to use when formatting elements.</param>
        /// <returns>The string representation.</returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            StringBuilder sb = new StringBuilder();
            string separator = NumberFormatInfo.GetInstance(formatProvider).NumberGroupSeparator;
            sb.Append('<');
            sb.Append(this.x.ToString(format, formatProvider));
            sb.Append(separator);
            sb.Append(' ');
            sb.Append(this.y.ToString(format, formatProvider));
            sb.Append('>');
            return sb.ToString();
        }

        /// <summary>
        /// Returns the length of the vector.
        /// </summary>
        /// <returns>The vector's length.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Length()
        {
            float ls = x * x + y * y;
            return (float)Math.Sqrt((double)ls);
        }

        /// <summary>
        /// Returns the length of the vector squared. This operation is cheaper than Length().
        /// </summary>
        /// <returns>The vector's length squared.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float LengthSquared()
        {
            return x * x + y * y;
        }
        #endregion Public Instance Methods

        #region Public Static Methods
        /// <summary>
        /// Returns the Euclidean distance between the two given points.
        /// </summary>
        /// <param name="value1">The first point.</param>
        /// <param name="value2">The second point.</param>
        /// <returns>The distance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Distance(Vector2 value1, Vector2 value2)
        {
            float dx = value1.x - value2.x;
            float dy = value1.y - value2.y;

            float ls = dx * dx + dy * dy;

            return (float)Math.Sqrt((double)ls);
        }

        /// <summary>
        /// Returns the Euclidean distance squared between the two given points.
        /// </summary>
        /// <param name="value1">The first point.</param>
        /// <param name="value2">The second point.</param>
        /// <returns>The distance squared.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DistanceSquared(Vector2 value1, Vector2 value2)
        {
            float dx = value1.x - value2.x;
            float dy = value1.y - value2.y;

            return dx * dx + dy * dy;
        }

        /// <summary>
        /// Returns a vector with the same direction as the given vector, but with a length of 1.
        /// </summary>
        /// <param name="value">The vector to normalize.</param>
        /// <returns>The normalized vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Normalize(in Vector2 value)
        {
            float ls = value.x * value.x + value.y * value.y;
            float invNorm = 1.0f / (float)Math.Sqrt((double)ls);

            return new Vector2(
                value.x * invNorm,
                value.y * invNorm);
        }

        /// <summary>
        /// Returns the reflection of a vector off a surface that has the specified normal.
        /// </summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="normal">The normal of the surface being reflected off.</param>
        /// <returns>The reflected vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Reflect(in Vector2 vector, in Vector2 normal)
        {
            //if (Vector.IsHardwareAccelerated)
            //{
            //    float dot = Vector2.Dot(vector, normal);
            //    return vector - (2 * dot * normal);
            //}
            //else
            //{
                float dot = vector.x * normal.x + vector.y * normal.y;

                return new Vector2(
                    vector.x - 2.0f * dot * normal.x,
                    vector.y - 2.0f * dot * normal.y);
            //}
        }

        /// <summary>
        /// Restricts a vector between a min and max value.
        /// </summary>
        /// <param name="value1">The source vector.</param>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Clamp(in Vector2 value1, in Vector2 min, in Vector2 max)
        {
            // This compare order is very important!!!
            // We must follow HLSL behavior in the case user specified min value is bigger than max value.
            float x = value1.x;
            x = (x > max.x) ? max.x : x;
            x = (x < min.x) ? min.x : x;

            float y = value1.y;
            y = (y > max.y) ? max.y : y;
            y = (y < min.y) ? min.y : y;

            return new Vector2(x, y);
        }

        /// <summary>
        /// Linearly interpolates between two vectors based on the given weighting.
        /// </summary>
        /// <param name="value1">The first source vector.</param>
        /// <param name="value2">The second source vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of the second source vector.</param>
        /// <returns>The interpolated vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Lerp(in Vector2 value1, in Vector2 value2, float amount)
        {
            return new Vector2(
                value1.x + (value2.x - value1.x) * amount,
                value1.y + (value2.y - value1.y) * amount);
        }
#if LATER
        /// <summary>
        /// Transforms a vector by the given matrix.
        /// </summary>
        /// <param name="position">The source vector.</param>
        /// <param name="matrix">The transformation matrix.</param>
        /// <returns>The transformed vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Transform(Vector2 position, Matrix3x2 matrix)
        {
            return new Vector2(
                position.X * matrix.M11 + position.Y * matrix.M21 + matrix.M31,
                position.X * matrix.M12 + position.Y * matrix.M22 + matrix.M32);
        }

        /// <summary>
        /// Transforms a vector by the given matrix.
        /// </summary>
        /// <param name="position">The source vector.</param>
        /// <param name="matrix">The transformation matrix.</param>
        /// <returns>The transformed vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Transform(Vector2 position, Matrix4x4 matrix)
        {
            return new Vector2(
                position.X * matrix.M11 + position.Y * matrix.M21 + matrix.M41,
                position.X * matrix.M12 + position.Y * matrix.M22 + matrix.M42);
        }

        /// <summary>
        /// Transforms a vector normal by the given matrix.
        /// </summary>
        /// <param name="normal">The source vector.</param>
        /// <param name="matrix">The transformation matrix.</param>
        /// <returns>The transformed vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 TransformNormal(Vector2 normal, Matrix3x2 matrix)
        {
            return new Vector2(
                normal.X * matrix.M11 + normal.Y * matrix.M21,
                normal.X * matrix.M12 + normal.Y * matrix.M22);
        }

        /// <summary>
        /// Transforms a vector normal by the given matrix.
        /// </summary>
        /// <param name="normal">The source vector.</param>
        /// <param name="matrix">The transformation matrix.</param>
        /// <returns>The transformed vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 TransformNormal(Vector2 normal, Matrix4x4 matrix)
        {
            return new Vector2(
                normal.X * matrix.M11 + normal.Y * matrix.M21,
                normal.X * matrix.M12 + normal.Y * matrix.M22);
        }

        /// <summary>
        /// Transforms a vector by the given Quaternion rotation value.
        /// </summary>
        /// <param name="value">The source vector to be rotated.</param>
        /// <param name="rotation">The rotation to apply.</param>
        /// <returns>The transformed vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Transform(Vector2 value, Quaternion rotation)
        {
            float x2 = rotation.X + rotation.X;
            float y2 = rotation.Y + rotation.Y;
            float z2 = rotation.Z + rotation.Z;

            float wz2 = rotation.W * z2;
            float xx2 = rotation.X * x2;
            float xy2 = rotation.X * y2;
            float yy2 = rotation.Y * y2;
            float zz2 = rotation.Z * z2;

            return new Vector2(
                value.X * (1.0f - yy2 - zz2) + value.Y * (xy2 - wz2),
                value.X * (xy2 + wz2) + value.Y * (1.0f - xx2 - zz2));
        }
#endif
#endregion Public Static Methods

#region Public operator methods
        // all the below methods should be inlined as they are 
        // implemented over JIT intrinsics

        /// <summary>
        /// Adds two vectors together.
        /// </summary>
        /// <param name="left">The first source vector.</param>
        /// <param name="right">The second source vector.</param>
        /// <returns>The summed vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Add(in Vector2 left, in Vector2 right)
        {
            return left + right;
        }

        /// <summary>
        /// Subtracts the second vector from the first.
        /// </summary>
        /// <param name="left">The first source vector.</param>
        /// <param name="right">The second source vector.</param>
        /// <returns>The difference vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Subtract(in Vector2 left, in Vector2 right)
        {
            return left - right;
        }

        /// <summary>
        /// Multiplies two vectors together.
        /// </summary>
        /// <param name="left">The first source vector.</param>
        /// <param name="right">The second source vector.</param>
        /// <returns>The product vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Multiply(in Vector2 left, in Vector2 right)
        {
            return left * right;
        }

        /// <summary>
        /// Multiplies a vector by the given scalar.
        /// </summary>
        /// <param name="left">The source vector.</param>
        /// <param name="right">The scalar value.</param>
        /// <returns>The scaled vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Multiply(in Vector2 left, float right)
        {
            return left * right;
        }

        /// <summary>
        /// Multiplies a vector by the given scalar.
        /// </summary>
        /// <param name="left">The scalar value.</param>
        /// <param name="right">The source vector.</param>
        /// <returns>The scaled vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Multiply(float left, in Vector2 right)
        {
            return left * right;
        }

        /// <summary>
        /// Divides the first vector by the second.
        /// </summary>
        /// <param name="left">The first source vector.</param>
        /// <param name="right">The second source vector.</param>
        /// <returns>The vector resulting from the division.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Divide(in Vector2 left, in Vector2 right)
        {
            return left / right;
        }

        /// <summary>
        /// Divides the vector by the given scalar.
        /// </summary>
        /// <param name="left">The source vector.</param>
        /// <param name="divisor">The scalar value.</param>
        /// <returns>The result of the division.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Divide(in Vector2 left, float divisor)
        {
            return left / divisor;
        }

        /// <summary>
        /// Negates a given vector.
        /// </summary>
        /// <param name="value">The source vector.</param>
        /// <returns>The negated vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Negate(in Vector2 value)
        {
            return -value;
        }
        #endregion Public operator methods
    }
}
