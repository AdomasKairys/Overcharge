using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Pickup : ScriptableObject
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public int Uses { get; private set; }

    // TODO: implement different sprites for different pickups
    //public Sprite Sprite { get; private set; }

    protected Pickup(string name, string description, int uses/*, Sprite sprite*/)
    {
        Name = name;
        Description = description;
        Uses = uses;
        //Sprite = sprite;
    }

    protected void ReduceUses()
    {
        if (Uses > 0)
        {
            Uses--;
        }
    }

    public abstract void Use();
}
