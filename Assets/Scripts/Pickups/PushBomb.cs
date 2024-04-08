using UnityEngine;
using UnityEngine.UIElements;

public class PushBomb : Pickup
{
    /// <summary>
    /// The player object used to calculate the position of the push center
    /// </summary>
    private GameObject _playerObject;

    /// <summary>
    /// The amount of force exerted onto players
    /// </summary>
    private float _pushForce = 120f;

    /// <summary>
    /// The radius that is used to detect the nearby players
    /// </summary>
    private float _pushRadius = 5f;

    /// <summary>
    /// The layer mask for the players
    /// </summary>
    private LayerMask _playerLayer = 1 << 3;

    /// <summary>
    /// Constructor for the push bomb pickup
    /// </summary>
    public PushBomb() : base("Push Bomb", "Pushes all nearby players away.", 1, 0f, 1)
    {
    }

    public void SetPlayerObject(GameObject playerObject)
    {
        _playerObject = playerObject;
    }

    public override void Use()
    {
        if (Uses > 0)
        {
            Debug.Log("Push bomb used");

            // Gets all of the player colliders within a sphere
            Collider[] colliders = Physics.OverlapSphere(_playerObject.transform.position, _pushRadius, _playerLayer, QueryTriggerInteraction.Ignore);

            // Iterate over them, get their movement components and push them away
            foreach (Collider collider in colliders)
            {
                // Omit the player that used the pickup
                if (collider.gameObject == _playerObject)
                    continue;

                Debug.Log("Detected collider: " + collider + "; its game object: " + collider.gameObject);

                // Get the player movement component from the collider owner
                var rigidBody = collider.gameObject.GetComponent<Rigidbody>();
                if (rigidBody)
                {
                    Debug.Log("Got the rigid body: " + rigidBody);
                    Vector3 pushDirection = (_playerObject.transform.position - rigidBody.transform.position).normalized;
                    var distanceMultiplier = (_pushRadius - Vector3.Distance(_playerObject.transform.position, rigidBody.transform.position)) / _pushRadius;
                    rigidBody.AddForce(pushDirection * _pushForce * distanceMultiplier, ForceMode.Impulse);
                }
                else
                {
                    Debug.Log("Failed to retrieve the rigid body");
                }
            }

            ReduceUses();
        }
    }
}
