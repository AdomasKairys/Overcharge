using UnityEngine;

public class SpeedBoost : Pickup
{
    /// <summary>
    /// The player movement used to boost the player
    /// </summary>
    private PlayerMovement _playerMovement;

    /// <summary>
    /// The duration of the boost
    /// </summary>
    private float _boostDuration = 2f;

    /// <summary>
    /// The multiplier applied to the player's speed during boosting
    /// </summary>
    private float _boostSpeedMultiplier = 4f;

    /// <summary>
    /// Constructor for the speed boost pickup
    /// </summary>
    public SpeedBoost() : base("Speed Boost", "Increases player speed for a short duration.", 3, 1.5f, 0)
    {
    }

    /// <summary>
    /// Sets the player movement to be referenced by this speed boost instance
    /// </summary>
    /// <param name="playerMovement">
    /// PlayerMovement component of the player that has the pickup
    /// </param>
    public void SetPlayerMovement(PlayerMovement playerMovement)
    {
        _playerMovement = playerMovement;
    }

    public override void Use()
    {
        if(Uses > 0)
        {
            // TODO: access the player movement and increase speed here
            Debug.Log("Speed boost used");
            _playerMovement.StartCoroutine(_playerMovement.UseSpeedBoost(_boostSpeedMultiplier, _boostDuration));
            ReduceUses();
        }
    }
}
