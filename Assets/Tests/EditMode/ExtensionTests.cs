using System.Collections.Generic;
using NUnit.Framework;

[TestFixture]
public class ExtensionTests
{
    [Test]
    public void TestShuffle()
    {
        var list = new List<int> { 1, 2, 3, 4, 5 };
        list.Shuffle();
        Assert.AreNotEqual(new List<int> { 1, 2, 3, 4, 5 }, list);
    }
}