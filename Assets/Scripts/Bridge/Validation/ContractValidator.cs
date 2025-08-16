using System;
using GG.Bridge.Dto;   // <-- sees ContractDTO

namespace GG.Bridge.Validation
{
    public static class ContractValidator
    {
        public static void Validate(ContractDTO c)
        {
            if (c == null) throw new GGDataException("GG2001", "Null contract payload");
            if (c.ApiVersion != "gg.v1")
                throw new GGDataException("GG2002", $"Version {c.ApiVersion} not supported");
            if (c.Terms == null || c.Terms.Count == 0)
                throw new GGDataException("GG2003", "Missing terms[]");

            foreach (var t in c.Terms)
            {
                if (t.Year < 2000 || t.Year > 2100)
                    throw new GGDataException("GG2004", $"Term year out of range {t.Year}");
                if (t.Base < 0 || t.SigningProrated < 0 || t.RosterBonus < 0 || t.WorkoutBonus < 0 || t.GuaranteedBase < 0)
                    throw new GGDataException("GG2005", "Negative term amounts");
            }
        }
    }

    public class GGDataException : Exception
    {
        public string Code { get; }
        public GGDataException(string code, string msg) : base(msg) { Code = code; }
    }
}
