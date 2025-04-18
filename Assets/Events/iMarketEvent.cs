public interface iMarketEvent : iTaggable
{
       public float GetProbabilityOf();
       public int GetDuration();
       public bool IsExpiredAt(int startingPeriod,int currentPeriod);
       public void Invoke(Market market);

}