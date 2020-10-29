using Common.Core;
using System;

namespace Test.Common
{
    public class TestClock : IClock
    {
        public DateTime DateTime;

        public TestClock(DateTime dateTime)
        {
            this.DateTime = dateTime;
        }

        public DateTime UtcNow => this.DateTime;
    }
}
