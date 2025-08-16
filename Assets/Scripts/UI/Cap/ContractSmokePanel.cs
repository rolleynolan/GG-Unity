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

            EnsureSampleExists();
            ValidateAndShow();
        }

        void EnsureSampleExists()
        {
            var abs = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "data", RelativePath));
            var dir = Path.GetDirectoryName(abs);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            if (File.Exists(abs)) return;

            var dto = new ContractDTO
            {
                ApiVersion = "gg.v1",
                StartYear = 2026,
                EndYear   = 2029,
                Terms = new System.Collections.Generic.List<ContractYearTerm>
                {
                    new ContractYearTerm {
                        Year = 2026, Base = 5_000_000, SigningProrated = 5_000_000,
                        RosterBonus = 0, WorkoutBonus = 0, GuaranteedBase = 5_000_000
                    },
                    new ContractYearTerm {
                        Year = 2027, Base = 6_000_000, SigningProrated = 5_000_000,
                        RosterBonus = 0, WorkoutBonus = 0, GuaranteedBase = 3_000_000
                    }
                }
            };

            var json = JsonConvert.SerializeObject(dto, Formatting.Indented);
            File.WriteAllText(abs, json);
        }

        void ValidateAndShow()
        {
            var abs  = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "data", RelativePath));
            var json = File.ReadAllText(abs);
            var dto  = JsonConvert.DeserializeObject<ContractDTO>(json);

            try { ContractValidator.Validate(dto); content.text = "Contract OK (gg.v1)"; }
            catch (GGDataException ex) { content.text = $"Contract invalid: {ex.Code}"; }
        }
    }
}
