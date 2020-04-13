using System.Collections.Generic;

namespace XI.Servers.Query.Models
{
    public interface IQueryResponse
    {
        string ServerName { get; }
        string Map { get; }
        string Mod { get; }
        int PlayerCount { get; }

        IDictionary<string, string> ServerParams { get; }
        IList<IQueryPlayer> Players { get; }
    }
}