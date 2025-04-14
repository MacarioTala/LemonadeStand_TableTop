public interface iMarketEvent
{
       public float GetProbabilityOf();
       public int GetDuration();
       public void Invoke(Market market);
}