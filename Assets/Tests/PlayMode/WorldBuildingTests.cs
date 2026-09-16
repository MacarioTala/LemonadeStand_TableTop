using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

[TestFixture]
public class WorldBuildingTests
{
    
    [UnityTest]
    public IEnumerator CountryGeneration_EveryNeighbourhoodHasBetween1and3Goods()
    {
        SceneManager.LoadScene(Scenes.GameRoot);

        yield return null;
        yield return null;

        var country = GameRoot.Instance.Country;

        Assert.That(
            country.GetNeighbourhoods()
                .Select(x => x.StockPile.Count),
            Has.All.InRange(1,3)
        );
    }
}