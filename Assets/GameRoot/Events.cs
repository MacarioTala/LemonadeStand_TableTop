public readonly struct EconomyCoreReady
{
    public readonly TheEconomy Economy;
    public EconomyCoreReady (TheEconomy economy)=> Economy=economy;
}

public readonly struct RequestLoadScene
{
    public readonly string SceneName;
    public RequestLoadScene(string sceneName) => SceneName = sceneName;
}