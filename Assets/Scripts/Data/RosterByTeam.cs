using System.Collections.Generic;

namespace GridironGM.Data
{
    using PlayerList = List<PlayerData>;

    public class RosterByTeam : Dictionary<string, PlayerList>
    {
    }
}
