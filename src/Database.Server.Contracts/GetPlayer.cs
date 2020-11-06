using Common.Core;
using MessagePack;

namespace Database.Server.Contracts
{
    public enum DatabaseMessageIds
    {
        GetPlayer,
    };

    [MessagePackObject]
    public class GetPlayerRequest
    {
        [Key(0)]
        public PlayerId PlayerId;
    }

    [MessagePackObject]
    public class GetPlayerResponse
    {
        [Key(0)]
        public string Name;
    }
}
