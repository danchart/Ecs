using Common.Core;
using System.Collections.Generic;

namespace Test.Common
{
    public class TestLogger : ILogger
    {
        public List<Message> Messages = new List<Message>();

        public void Error(string message)
        {
            Messages.Add(new Message
            {
                Level = LevelEnum.Error,
                Text = message
            });
        }

        public void Info(string message)
        {
            Messages.Add(new Message
            {
                Level = LevelEnum.Info,
                Text = message
            });
        }

        public void Verbose(string message)
        {
            Messages.Add(new Message
            {
                Level = LevelEnum.Verbose,
                Text = message
            });
        }

        public void VerboseError(string message)
        {
            Messages.Add(new Message
            {
                Level = LevelEnum.VerboseError,
                Text = message
            });
        }

        public struct Message
        {
            public LevelEnum Level;
            public string Text;
        }

        public enum LevelEnum
        {
            Info,
            Error,
            Verbose,
            VerboseError
        }
    }
}
