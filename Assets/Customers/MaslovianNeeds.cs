using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MaslovianNeeds
{
    public Dictionary<MaslovianTypeEnum,Need> Needs;

    public MaslovianNeeds()
    {
        Needs = Enum.GetValues(typeof(MaslovianTypeEnum))
                .Cast<MaslovianTypeEnum>()
                .ToDictionary(
                    type=> type,
                    type=> new Need
                        {
                            Type = type,
                            Current=0,
                            Target= MathHelper.RollD(1,8)
                        }
                );
    }

    public Need Get(MaslovianTypeEnum type) => Needs[type];

    public IEnumerable<Need> GetPrimaryNeeds()
    {
        var maxGap = Needs.Max(x=>x.Value.GetNeedGap());

        if(maxGap==0) return Enumerable.Empty<Need>();

        var returnNeeds = Needs.Where(x=>x.Value.GetNeedGap()==maxGap)
            .Select(x=>x.Value);
        return returnNeeds;
    }

    public IEnumerable<Need> GetRankedNeeds()
    => Needs.Where(x=>x.Value.GetNeedGap()>0).OrderByDescending(x=>x.Value.GetNeedGap()).Select(x=>x.Value);
    
    public bool HasUnfulfilledNeeds() => Needs.Any(x=>!x.Value.IsFulfilled());
}

public class Need
{
    public MaslovianTypeEnum Type;
    public int Current;
    public int Target;

    public void Affect(int effect)
        => Current+=effect;
    public bool IsFulfilled()
        => Current>=Target;
    public int GetNeedGap()=> Mathf.Max(Target-Current,0);
}

public enum MaslovianTypeEnum
{
    Physiological,
    Safety,
    Belonging,
    Esteem,
    SelfActualization
}
