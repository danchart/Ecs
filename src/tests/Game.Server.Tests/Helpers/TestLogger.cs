using System.Collections.Generic;

namespace Game.Server.Tests.Helpers
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

        public struct Message
        {
            public LevelEnum Level;
            public string Text;
        }

        public enum LevelEnum
        {
            Info,
            Error
        }
    }
}
