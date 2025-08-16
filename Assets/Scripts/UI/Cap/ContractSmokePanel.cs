using TMPro;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using GG.Bridge.Dto;
using GG.Bridge.Validation;

namespace GG.UI.Cap
{
    public class ContractSmokePanel : MonoBehaviour
    {
        public TMP_Text content;
        public string RelativePath = "contracts/sample.json"; // under /data

        void OnEnable()
        {
            if (!content)
            {
                var go = new GameObject("ContractSmoke", typeof(RectTransform));
                go.transform.SetParent(transform, false);
                content = go.AddComponent<TMP_Text>();
            }

            var abs = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "data", RelativePath));
            if (!File.Exists(abs))
            {
                // write a sample file
                var sample = @"{
  \"api_version\": \"gg.v1\",
  \"startYear\": 2026,
  \"endYear\": 2029,
  \"terms\": [
    { \"year\": 2026, \"base\": 5000000, \"signingProrated\": 5000000, \"rosterBonus\": 0, \"workoutBonus\": 0, \"guaranteedBase\": 5000000 },
    { \"year\": 2027, \"base\": 6000000, \"signingProrated\": 5000000, \"rosterBonus\": 0, \"workoutBonus\": 0, \"guaranteedBase\": 3000000 }
  ]
}";
                Directory.CreateDirectory(Path.GetDirectoryName(abs));
                File.WriteAllText(abs, sample);
            }

            var json = File.ReadAllText(abs);
            var dto  = JsonConvert.DeserializeObject<ContractDTO>(json);
            try { ContractValidator.Validate(dto); content.text = "Contract OK (gg.v1)"; }
            catch (GGDataException ex) { content.text = $"Contract invalid: {ex.Code}"; }
        }
    }
}
