using Ecs.Core;
using System;

namespace Game.Simulation.Client
{
    /// <summary>
    /// Ring buffer of World snapshots.
    /// </summary>
    internal class SnapShots<TInput>
        where TInput : unmanaged
    {
        private readonly int Count;

        private int _current;
        private SnapShot<TInput>[] _snapShots;

        public SnapShots(int count)
        {
            Count = count;

            _current = 0;
            _snapShots = new SnapShot<TInput>[Count];

            for (int i = 0; i < _snapShots.Length; i++)
            {
                _snapShots[i] = new SnapShot<TInput>
                {
                    ClientInputs = new AppendOnlyList<SnapShot<TInput>.ClientInputFrame>(capacity: 8)
                };
            }
        }

        /// <summary>
        /// Ends the current snapshot and begins the next.
        /// </summary>
        public void NextSnapshot(float fixedTime, World world)
        {
            _snapShots[_current].Time = fixedTime;

            // Advance to next snapshot. Reset it.
            MoveNext();

            world.State.CopyStateTo(ref _snapShots[_current].WorldState);
        }

        /// <summary>
        /// Returns the current snapshot.
        /// </summary>
        public ref readonly SnapShot<TInput> Current()
        {
            return ref _snapShots[_current];
        }

        public void Seek(int frameCount)
        {
            if (Math.Abs(frameCount) >= Count)
            {
                throw new InvalidOperationException($"Seek frame count too large. {nameof(frameCount)}={frameCount}, Count={Count}");
            }

            _current = (_current + Count + frameCount) % Count;
        }

        public ref readonly SnapShot<TInput> Peek(int frameCount)
        {
            if (frameCount < 0 ||
                Math.Abs(frameCount) >= Count)
            {
                throw new InvalidOperationException($"Invalid Peek frame count. {nameof(frameCount)}={frameCount}, Count={Count}");
            }

            var peekPos = (_current + Count + frameCount) % Count;

            return ref _snapShots[peekPos];
        }

        /// <summary>
        /// Move to the next snapshot position. The snapshot will be reset.
        /// </summary>
        private void MoveNext()
        {
            Seek(1);
            Current().Reset();
        }
    }
}
