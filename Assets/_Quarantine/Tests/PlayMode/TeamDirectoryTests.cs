#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GG.Bridge.Repositories;

public class TeamDirectoryTests
{
    [Test]
    public void returns_non_empty_list()
    {
        var list = TeamDirectory.GetAbbrs();
        Assert.IsNotNull(list);
        Assert.GreaterOrEqual(list.Count, 1);
    }
}
#endif
