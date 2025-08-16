using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

internal class ContractDetailPanel : MonoBehaviour {
  [SerializeField] TMP_Text content;
  [SerializeField] TMP_Text errorBanner;

  public ContractDTO Contract;
  public int Year;

  void OnEnable(){
    if (content == null) content = gameObject.AddComponent<TMP_Text>();
    Render();
  }

  void Render(){
    if (Contract == null){ ShowError("No contract"); return; }
    try {
      ContractValidator.Validate(Contract);
    } catch (ContractValidator.GGDataException ex){
      ShowError($"Data version mismatch ({ex.Code})");
      return;
    }
    var rows = new List<string>();
    foreach (var t in Contract.Terms){
      if (t.Year == Year || t.Year == Year + 1){
        rows.Add($"{t.Year}: {FormatMoney(t.Base)} base + {FormatMoney(t.SigningProrated)} signing");
      }
    }
    if (rows.Count == 0) ShowError("No terms found"); else content.text = string.Join("\n", rows);
  }

  void ShowError(string msg){
    if (errorBanner == null){
      errorBanner = new GameObject("Error", typeof(RectTransform), typeof(TMP_Text)).GetComponent<TMP_Text>();
      errorBanner.transform.SetParent(transform,false);
      var r = errorBanner.GetComponent<RectTransform>();
      r.anchorMin = new Vector2(0,1); r.anchorMax = new Vector2(1,1); r.pivot = new Vector2(0.5f,1); r.offsetMin = new Vector2(0,-30); r.offsetMax = new Vector2(0,0);
      errorBanner.color = Color.red;
    }
    errorBanner.text = msg; errorBanner.gameObject.SetActive(true);
  }

  string FormatMoney(long v)=>"$" + (v/1_000_000f).ToString("0.0") + "M";
}
