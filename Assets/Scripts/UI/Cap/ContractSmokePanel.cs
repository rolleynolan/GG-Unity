using System.IO;
using TMPro;
using UnityEngine;
using GG.Bridge.Dto;
using GG.Bridge.Validation;

namespace GG.UI.Cap
{
    public class ContractSmokePanel : MonoBehaviour
    {
        [SerializeField] TMP_Text content;
        [SerializeField, UnityEngine.Serialization.FormerlySerializedAs("RelativePath")] string relativePath = "sample.json"; // under /data/contracts/

        void OnEnable()
        {
            if (!content)
            {
                var go = new GameObject("ContractSmoke", typeof(RectTransform));
                go.transform.SetParent(transform, false);
                content = go.AddComponent<TextMeshProUGUI>();
            }

            EnsureSampleExists();
            ValidateAndShow();
        }

        void EnsureSampleExists()
        {
            var abs = GGPaths.ContractFile(relativePath);
            if (File.Exists(abs)) return;

            var dto = new ContractDTO
            {
                ApiVersion = "gg.v1",
                StartYear = 2026,
                EndYear = 2029,
                Terms = new System.Collections.Generic.List<ContractYearTerm>
                {
                    new ContractYearTerm { Year = 2026, Base = 5_000_000, SigningProrated = 5_000_000, RosterBonus = 0, WorkoutBonus = 0, GuaranteedBase = 5_000_000 },
                    new ContractYearTerm { Year = 2027, Base = 6_000_000, SigningProrated = 5_000_000, RosterBonus = 0, WorkoutBonus = 0, GuaranteedBase = 3_000_000 }
                }
            };
            var json = JsonUtility.ToJson(dto, true);
            File.WriteAllText(abs, json);
            GGLog.Info($"ContractSmokePanel: wrote sample to {abs}");
        }

        void ValidateAndShow()
        {
            var abs = GGPaths.ContractFile(relativePath);
            var json = File.ReadAllText(abs);
            var dto = JsonUtility.FromJson<ContractDTO>(json);

            try
            {
                ContractValidator.Validate(dto);
                content.text = "Contract OK (gg.v1)";
                GGLog.Info("ContractSmokePanel: contract valid.");
            }
            catch (GGDataException ex)
            {
                content.text = $"Contract invalid: {ex.Code}";
                GGLog.Warn($"ContractSmokePanel: contract invalid ({ex.Code}).");
            }
        }
    }
}

