using System;

namespace GG.Season
{
    [Serializable]
    public struct TeamRecord { public int W, L, PF, PA; }

    [Serializable]
    public struct GameInfo { public int week; public string home, away; }

    [Serializable]
    public struct GameResult { public int week; public string home, away; public int homeScore, awayScore; }
}
