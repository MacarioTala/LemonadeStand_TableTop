using System;
using System.Collections.Generic;
using System.Linq;

public class MaslovianEffects
{
    public Dictionary<MaslovianTypeEnum,MaslovianEffect> Effects;

    public MaslovianEffects()
    {
        Enum.GetValues(typeof(MaslovianTypeEnum))
            .Cast<MaslovianTypeEnum>()
            .ToDictionary(
                type=>type,
                type=> new MaslovianEffect
                        {
                            Type=type,
                            Modifier=0
                        }
            );
    }
}
public class MaslovianEffect
{
    public MaslovianTypeEnum Type;
    public int Modifier;
}