using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Bruised", menuName = "Status Effects/Bruised")]
public class Bruised : StatusEffect
{

    public override void ApplyEffect(CharacterHandler handler, int amplifier)
    {
        CurrAmplifier = amplifier;
        return;
    }

    public override bool UpdateEffect(CharacterHandler handler)
    {
        // Skip the first turn of effect
        if (JustApplied)
        {
            JustApplied = false;
            return CurrAmplifier == 0;
        }
        handler.HealthHandler.TakeDamage(1);
        CurrAmplifier--;
        return CurrAmplifier == 0;
    }

}
