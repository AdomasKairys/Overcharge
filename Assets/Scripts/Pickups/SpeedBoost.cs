using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedBoost : Pickup
{
    public SpeedBoost() : base("Speed Boost", "Increases player speed for a short duration.", 3, 0)
    {
    }

    public override void Use()
    {
        if(Uses > 0)
        {
            // TODO: access the player movement and increase speed here
            Debug.Log("Speed boost used");
            ReduceUses();
        }
    }
}
