using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal class TeamCapView : MonoBehaviour {
  [SerializeField] string teamAbbr;
  [SerializeField] int year;
  [SerializeField] RectTransform contentRoot;
  [SerializeField] TMP_Text rowTemplate;
  [SerializeField] TMP_Text errorBanner;

  public string TeamAbbr { get => teamAbbr; set => teamAbbr = value; }
  public int Year { get => year; set => year = value; }

  void OnEnable(){
    if (contentRoot == null) BuildRuntimeUI();
    try {
      var dto = DataIO.LoadJson<CapsheetDTO>($"data/cap/capsheet_{Year}.json");
      if (dto.ApiVersion != "gg.v1") throw new ContractValidator.GGDataException("GG2002", $"Version {dto.ApiVersion} not supported");
      Render(dto);
    } catch (ContractValidator.GGDataException ex) {
      GGLog.Warn("TeamCapView GGDataException " + ex.Code);
      ShowError($"Data version mismatch ({ex.Code})");
    } catch (IOException) {
      ShowError("Capsheet missing");
    } catch (Exception ex) {
      GGLog.Error("TeamCapView " + ex.Message);
      ShowError("Capsheet missing");
    }
  }

  void BuildRuntimeUI(){
    var scroll = new GameObject("ScrollView", typeof(RectTransform), typeof(ScrollRect));
    scroll.transform.SetParent(transform,false);
    var srect = scroll.GetComponent<RectTransform>();
    srect.anchorMin = Vector2.zero; srect.anchorMax = Vector2.one; srect.offsetMin = Vector2.zero; srect.offsetMax = Vector2.zero;

    var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Mask), typeof(Image));
    viewport.transform.SetParent(scroll.transform,false);
    var vrect = viewport.GetComponent<RectTransform>();
    vrect.anchorMin = Vector2.zero; vrect.anchorMax = Vector2.one; vrect.offsetMin = Vector2.zero; vrect.offsetMax = Vector2.zero;
    viewport.GetComponent<Image>().color = Color.clear;

    var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup));
    content.transform.SetParent(viewport.transform,false);
    contentRoot = content.GetComponent<RectTransform>();

    var sr = scroll.GetComponent<ScrollRect>();
    sr.viewport = vrect; sr.content = contentRoot;

    rowTemplate = new GameObject("Row", typeof(RectTransform), typeof(TMP_Text)).GetComponent<TMP_Text>();
    rowTemplate.transform.SetParent(contentRoot,false);
    rowTemplate.gameObject.SetActive(false);

    errorBanner = new GameObject("Error", typeof(RectTransform), typeof(TMP_Text)).GetComponent<TMP_Text>();
    errorBanner.transform.SetParent(transform,false);
    var brect = errorBanner.GetComponent<RectTransform>();
    brect.anchorMin = new Vector2(0,1); brect.anchorMax = new Vector2(1,1); brect.pivot = new Vector2(0.5f,1);
    brect.offsetMin = new Vector2(0,-30); brect.offsetMax = new Vector2(0,0);
    errorBanner.color = Color.red;
    errorBanner.gameObject.SetActive(false);
  }

  void Render(CapsheetDTO dto){
    foreach (Transform t in contentRoot){ if (t != rowTemplate.transform) Destroy(t.gameObject); }
    errorBanner.gameObject.SetActive(false);
    foreach (var r in dto.Rows){
      if (!string.Equals(r.TeamAbbr, TeamAbbr, StringComparison.OrdinalIgnoreCase)) continue;
      var row = Instantiate(rowTemplate, contentRoot);
      row.gameObject.SetActive(true);
      row.text = $"{r.PlayerName} {FormatMoney(r.CapHit)}";
    }
  }

  void ShowError(string msg){
    if (errorBanner == null) return;
    errorBanner.text = msg;
    errorBanner.gameObject.SetActive(true);
  }

  string FormatMoney(long v) => "$" + (v/1_000_000f).ToString("0.0") + "M";

  [Serializable]
  class CapsheetDTO {
    public string ApiVersion;
    public List<Row> Rows;
    public Totals Totals;
  }
  [Serializable]
  class Row {
    public string PlayerName;
    public string TeamAbbr;
    public int Year;
    public long Base;
    public long SigningProrated;
    public long RosterBonus;
    public long WorkoutBonus;
    public long CapHit;
    public long DeadCap;
  }
  [Serializable]
  class Totals {
    public long CapHit;
    public long DeadCap;
  }
}
