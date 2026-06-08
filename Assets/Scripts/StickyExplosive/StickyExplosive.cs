using UnityEngine;

public class StickyExplosive : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float launchSpeed = 20f;

    private bool isAttached;

    private PlayerController owner;

    public void Initialize(PlayerController combatController, Vector3 launchDirection)
    {
        owner = combatController;
        rb.linearVelocity = launchDirection.normalized * launchSpeed;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isAttached)
            return;

        isAttached = true;

        rb.isKinematic = true;

        ContactPoint hit = collision.contacts[0];

        transform.position = hit.point;

        transform.rotation =
            Quaternion.LookRotation(hit.normal);

        transform.SetParent(collision.transform);
    }

    public void Detonate()
    {
        if (!isAttached) return;

        // Force field opening logic goes here
        Collider[] hits =
            Physics.OverlapSphere(
                transform.position,
                5f);

        foreach (Collider hit in hits)
        {
            // damage / effects
        }

        Destroy(gameObject);
    }
}