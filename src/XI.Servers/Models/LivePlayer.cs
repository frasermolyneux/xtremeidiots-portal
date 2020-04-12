using System;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations.Schema;

namespace XI.Servers.Models
{
    public class LivePlayer
    {
        public LivePlayer()
        {
            Parameters = new StringDictionary();
        }

        public LivePlayer(string name, int score) : this()
        {
            Name = name;
            Score = score;
        }

        public LivePlayer(string name, int score, int ping) : this(name, score)
        {
            Ping = ping;
        }

        public string Name { get; set; }
        public int Score { get; set; }
        public int Ping { get; set; }
        public string Team { get; set; }
        public TimeSpan Time { get; set; }

        [NotMapped] public StringDictionary Parameters { get; set; }
    }
}