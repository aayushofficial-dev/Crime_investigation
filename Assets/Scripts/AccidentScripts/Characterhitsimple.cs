using System.Collections;
using UnityEngine;

/// <summary>
/// Attach to your Character GameObject.
/// Works on ANY simple model — no Animator, no pre-built ragdoll needed.
/// Adds a Rigidbody at runtime and physically throws the character on impact.
/// Also spawns blood decal on ground.
/// </summary>
public class CharacterHitSimple : MonoBehaviour
{
    [Header("─── Hit Physics ───")]
    public float forwardForce = 6f;     // how far character is thrown forward
    public float upwardForce  = 5f;     // how high character goes
    public float spinTorque   = 80f;    // how much the character tumbles/spins

    [Header("─── VFX Prefabs ───")]
    public GameObject bloodSplatterPrefab;   // particle burst — spawned at chest on hit
    public GameObject bloodPoolPrefab;       // decal quad — spawned on ground after 1.5s

    [Header("─── Audio ───")]
    public AudioClip bodyImpactClip;         // optional flesh-hit sound

    // ── private ─────────────────────────────────────────────────────────────
    private Rigidbody _rb;
    private Collider  _col;
    private bool _hit = false;

    void Awake()
    {
        // Make sure we have a collider (add box if none exists)
        _col = GetComponent<Collider>();
        if (_col == null)
        {
            var box = gameObject.AddComponent<BoxCollider>();
            // auto-size based on renderers
            var renderers = GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                Bounds b = renderers[0].bounds;
                foreach (var r in renderers) b.Encapsulate(r.bounds);
                box.center = transform.InverseTransformPoint(b.center);
                box.size   = b.size;
            }
            else
            {
                box.size = new Vector3(0.5f, 1.8f, 0.3f);
                box.center = new Vector3(0, 0.9f, 0);
            }
            _col = box;
        }

        // Add Rigidbody — frozen until hit
        _rb = GetComponent<Rigidbody>();
        if (_rb == null) _rb = gameObject.AddComponent<Rigidbody>();
        _rb.isKinematic  = true;   // stays still until car hits
        _rb.mass         = 70f;
        _rb.linearDamping        = 0.5f;
        _rb.angularDamping = 2f;
    }

    /// <summary>
    /// Called by AccidentSceneDirector when car impacts.
    /// hitDirection = car's forward vector.
    /// </summary>
    public void TriggerHit(Vector3 hitDirection)
    {
        if (_hit) return;
        _hit = true;

        // ── Enable physics ──────────────────────────────────────────────────
        _rb.isKinematic = false;

        // ── Apply impulse force (forward + up) ──────────────────────────────
        Vector3 force = hitDirection.normalized * forwardForce + Vector3.up * upwardForce;
        _rb.AddForce(force, ForceMode.Impulse);

        // ── Add random spin so character tumbles realistically ───────────────
        Vector3 torque = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-0.3f, 0.3f),
            Random.Range(-1f, 1f)
        ) * spinTorque;
        _rb.AddTorque(torque, ForceMode.Impulse);

        // ── Blood splatter at chest height ───────────────────────────────────
        if (bloodSplatterPrefab != null)
        {
            Vector3 chestPos = transform.position + Vector3.up * 1.1f;
            Instantiate(bloodSplatterPrefab, chestPos,
                        Quaternion.LookRotation(-hitDirection));
        }

        // ── Optional flesh impact sound ──────────────────────────────────────
        if (bodyImpactClip != null)
            AudioSource.PlayClipAtPoint(bodyImpactClip, transform.position, 0.8f);

        // ── Spawn blood pool on ground after character settles ───────────────
        StartCoroutine(SpawnBloodPool(2f));
    }

    IEnumerator SpawnBloodPool(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (bloodPoolPrefab == null) yield break;

        Vector3 origin = transform.position + Vector3.up * 0.5f;
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 5f))
        {
            Instantiate(bloodPoolPrefab,
                        hit.point + Vector3.up * 0.01f,
                        Quaternion.FromToRotation(Vector3.up, hit.normal));
        }
        else
        {
            // fallback: just place at character's feet
            Instantiate(bloodPoolPrefab,
                        transform.position + Vector3.up * 0.01f,
                        Quaternion.identity);
        }
    }
}