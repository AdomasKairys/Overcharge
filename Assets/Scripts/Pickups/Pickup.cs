using System.Collections;
using UnityEngine;

public abstract class Pickup : ScriptableObject
{
    /// <summary>
    /// The name of the pickup
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// The description of the pickup's effects
    /// </summary>
    public string Description { get; private set; }

    /// <summary>
    /// Number of uses that the pickup has
    /// </summary>
    public int Uses { get; private set; }

    /// <summary>
    /// If a pickup has more than one use, this property determines how long a player needs to wait before using it repeatedly
    /// </summary>
    public float Cooldown { get; private set; }

    /// <summary>
    /// Determines if the pickup can be used at the moment (is it not on cooldown)
    /// </summary>
    public bool CanUse {  get; private set; }

    /// <summary>
    /// The index of the pickup sprite in the UIController array for pickup sprites
    /// </summary>
    public int Sprite { get; private set; }    

    protected Pickup(string name, string description, int uses, float cooldown, int sprite)
    {
        Name = name;
        Description = description;
        Uses = uses;
        Cooldown = cooldown;
        Sprite = sprite;
        CanUse = true;
    }

    protected void ReduceUses()
    {
        if (Uses > 0)
        {
            Uses--;
        }
    }

    public IEnumerator WaitCooldown()
    {
        CanUse = false;
        yield return new WaitForSeconds(Cooldown);
        CanUse = true;
    }

    public abstract void Use();
}
