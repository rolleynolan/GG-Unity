using UnityEngine;

public class DevSanityLog : MonoBehaviour
{
    [SerializeField] private bool logOnStart = true;
    void Start()
    {
        if (!logOnStart) return;
        var teams = LeagueRepository.GetTeams();
        Debug.Log($"[DevSanityLog] Teams loaded: {teams?.Length ?? 0}");
        var abbr = GameSession.SelectedTeamAbbr;
        var r = LeagueRepository.GetRoster(abbr);
        Debug.Log($"[DevSanityLog] Selected: {abbr} | Roster count: {r.Count}");
    }
}
