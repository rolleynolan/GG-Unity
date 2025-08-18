using System.Collections.Generic;
using TMPro;
using UnityEngine;
using GG.Bridge.Dto;
using GG.Bridge.Validation;

namespace GG.UI.Cap
{
    public class ContractSmokePanel : MonoBehaviour
    {
        [SerializeField] TMP_Text content;
        [SerializeField, UnityEngine.Serialization.FormerlySerializedAs("RelativePath"), HideInInspector]
        string _legacyRelativePath;

        void OnEnable()
        {
            if (!content)
            {
                var go = new GameObject("ContractSmoke", typeof(RectTransform));
                go.transform.SetParent(transform, false);
                content = go.AddComponent<TextMeshProUGUI>();
            }

            var dto = new ContractDTO
            {
                ApiVersion = "gg.v1",
                StartYear = 2026,
                EndYear = 2027,
                Terms = new List<ContractYearTerm>
                {
                    new ContractYearTerm { Year = 2026, Base = 5_000_000, SigningProrated = 5_000_000, RosterBonus = 0, WorkoutBonus = 0, GuaranteedBase = 5_000_000 },
                    new ContractYearTerm { Year = 2027, Base = 6_000_000, SigningProrated = 5_000_000, RosterBonus = 0, WorkoutBonus = 0, GuaranteedBase = 3_000_000 }
                }
            };

            try { ContractValidator.Validate(dto); content.text = "Contract OK (gg.v1)"; }
            catch (GGDataException ex) { content.text = $"Contract invalid: {ex.Code}"; }
        }
    }
}
