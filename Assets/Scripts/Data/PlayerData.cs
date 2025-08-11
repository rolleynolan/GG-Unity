using System;

namespace GridironGM.Data
{
    [Serializable]
    public class PlayerData
    {
        public int id;
        public string first;
        public string last;
        public string pos;
        public int ovr;
        public int age;

        // Allow extra fields without breaking
    }
}
