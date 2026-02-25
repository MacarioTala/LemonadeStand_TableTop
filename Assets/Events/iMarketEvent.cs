using System.Collections.Generic;

public interface iMarketEvent : iTaggable
{
       public float GetProbabilityOf();
       public int GetDuration();
       public IEnumerable<iMarketEffect> GetEffects();
       public void SetDuration(int duration);
       public bool IsExpiredAt(int startingPeriod,int currentPeriod);
       public void Invoke(Market market,ActiveMarketEvent activeEvent);
}