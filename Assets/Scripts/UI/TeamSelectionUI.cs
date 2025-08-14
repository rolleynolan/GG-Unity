using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using GridironGM.Data;

public class TeamSelectionUI : MonoBehaviour
{
    [Header("Left List")]
    [SerializeField] RectTransform listContent;
    [SerializeField] GameObject teamRowPrefab;

    [Header("Buttons")]
    [SerializeField] Button confirmButton;

    [Header("Right Panel Preview")]
    [SerializeField] RosterPreviewView preview;

    string selectedAbbr = "";

    void Start()
    {
        if (!listContent || !teamRowPrefab) AutoWireList();
        if (confirmButton) confirmButton.interactable = false;

        var data = LeagueRepository.LoadTeams()?.teams;
        if (data == null || data.Count == 0) { Debug.LogError("[TeamSelectionUI] No teams loaded."); return; }

        foreach (Transform c in listContent) Destroy(c.gameObject);
        foreach (var t in data.OrderBy(t => t.city).ThenBy(t => t.name))
        {
            var row = Instantiate(teamRowPrefab, listContent);
            var binder = row.GetComponent<TeamRowBinder>() ?? row.AddComponent<TeamRowBinder>();
            binder.Set(t);

            var btn = row.GetComponent<Button>() ?? row.AddComponent<Button>();
            btn.onClick.AddListener(() => OnTeamClicked(t.abbreviation));
        }
        Debug.Log($"[TeamSelectionUI] Teams count: {data.Count}");
    }

    void AutoWireList()
    {
        var sr = GetComponentsInChildren<ScrollRect>(true).FirstOrDefault();
        if (sr && sr.content) listContent = sr.content;
        if (!teamRowPrefab)
        {
            var go = Resources.Load<GameObject>("UI/TeamRowUI");
            if (go) teamRowPrefab = go;
        }
    }

    void OnTeamClicked(string abbr)
    {
        selectedAbbr = abbr;
        if (confirmButton) confirmButton.interactable = true;
        preview?.Show(abbr);
        Debug.Log($"[TeamSelectionUI] Selected {abbr}");
    }
}
