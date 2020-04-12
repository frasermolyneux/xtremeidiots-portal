using System.Collections;
using XI.Servers.Models;

namespace XI.Servers
{
    public class PlayerCollection : CollectionBase
    {
        public LivePlayer this[int index]
        {
            get => (LivePlayer) List[index];
            set => List[index] = value;
        }

        public int Add(LivePlayer value)
        {
            return List.Add(value);
        }

        public void Remove(LivePlayer value)
        {
            List.Remove(value);
        }

        public void Insert(int index, LivePlayer value)
        {
            List.Insert(index, value);
        }

        public bool Contains(LivePlayer value)
        {
            return List.Contains(value);
        }
    }
}