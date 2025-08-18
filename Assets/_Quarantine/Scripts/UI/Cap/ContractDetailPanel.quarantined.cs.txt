using UnityEngine;
using TMPro;
using GG.Bridge.Dto;
using GG.Bridge.Validation;

public class ContractDetailPanel : MonoBehaviour
{
    [SerializeField] TMP_Text content;
    [SerializeField] TMP_Text errorBanner;

    public ContractDTO Contract;
    public int Year;

    void OnEnable()
    {
        if (!content) content = gameObject.GetComponent<TMP_Text>() ?? gameObject.AddComponent<TextMeshProUGUI>();
        Render();
    }

    void Render()
    {
        if (Contract == null) { ShowError("No contract"); return; }
        try { ContractValidator.Validate(Contract); }
        catch (GGDataException ex) { ShowError($"Data version mismatch ({ex.Code})"); return; }

        var sb = new System.Text.StringBuilder();
        foreach (var t in Contract.Terms)
            if (t.Year == Year || t.Year == Year + 1)
                sb.AppendLine($"{t.Year}: {Fmt(t.Base)} base + {Fmt(t.SigningProrated)} signing");

        if (sb.Length == 0) ShowError("No terms found");
        else content.text = sb.ToString();
    }

    static string Fmt(long v) => "$" + (v / 1_000_000f).ToString("0.0") + "M";

    void ShowError(string msg)
    {
        if (!errorBanner)
        {
            var go = new GameObject("ErrorBanner", typeof(RectTransform));
            go.transform.SetParent(transform, false);
            errorBanner = go.AddComponent<TextMeshProUGUI>();
        }
        errorBanner.text = msg;
    }
}
