#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
public class TeamProviderTests {
  [Test] public void provider_returns_multiple() {
    var n = new TeamProvider().GetAllTeamAbbrs().Count;
    Assert.GreaterOrEqual(n, 2, "Need >=2 teams for schedule.");
  }
}
#endif
