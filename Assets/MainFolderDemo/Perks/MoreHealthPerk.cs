using UnityEngine;
using System.Collections;

public class MoreHealthPerkk : MonoBehaviour
{
    [Header("Interact")]
    public Transform player;                 // optional; auto-finds if left empty
    public float interactDistance = 2.2f;
    public KeyCode useKey = KeyCode.E;

    [Header("Flask (optional)")]
    public GameObject flaskPrefab;           // leave null if you only want the arm anim

    [Header("Flask Offsets")]
    public Vector3 flaskStartLocalPos = new Vector3(-0.09f, -1.1f, 0.42f);
    public Vector3 flaskMouthLocalPos = new Vector3(-0.01f, -0.12f, 0.16f);
    public Vector3 flaskStartLocalEuler = Vector3.zero;
    public Vector3 flaskSipLocalEuler = new Vector3(-65f, 0f, 0f);

    [Header("Timing")]
    public float moveInTime = 0.18f;
    public float sipTime = 0.60f;
    public float moveOutTime = 0.18f;

    Transform cam;   // Camera.main
    bool busy;

    void Awake()
    {
        cam = Camera.main ? Camera.main.transform : null;

        if (player == null)
        {
            var ptag = GameObject.FindGameObjectWithTag("Player");
            if (ptag) player = ptag.transform;
            if (player == null)
            {
                var cc = FindFirstObjectByType<CharacterController>();
                if (cc) player = cc.transform;
            }
        }
    }

    void Update()
    {
        if (busy || player == null) return;

        bool inRange = Vector3.Distance(player.position, transform.position) <= interactDistance;
        if (inRange && Input.GetKeyDown(useKey))
        {
            var arms = FindFirstObjectByType<ArmMovementMegaScript>();
            if (arms == null || arms.IsGrenadeAnimating || arms.IsPerkAnimating) return;

            StartCoroutine(DoPerkDrink(arms));
        }
    }

    IEnumerator DoPerkDrink(ArmMovementMegaScript arms)
    {
        busy = true;

        // arm animation
        arms.StartCoroutine(arms.PerkDrinkDropOnly());

        // flask (optional)
        if (cam != null && flaskPrefab != null)
        {
            var flask = Instantiate(flaskPrefab, cam, false);   // parent directly to camera
            var tf = flask.transform;

            tf.localPosition = flaskStartLocalPos;
            tf.localRotation = Quaternion.Euler(flaskStartLocalEuler);

            if (flask.TryGetComponent<Rigidbody>(out var rb))
            { rb.isKinematic = true; rb.useGravity = false; rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }
            if (flask.TryGetComponent<Collider>(out var col)) col.enabled = false;

            // in
            yield return LerpLocal(tf,
                flaskStartLocalPos, flaskMouthLocalPos,
                Quaternion.Euler(flaskStartLocalEuler), Quaternion.Euler(flaskSipLocalEuler),
                moveInTime);

            // sip
            yield return new WaitForSeconds(sipTime);

            // out
            yield return LerpLocal(tf,
                flaskMouthLocalPos, flaskStartLocalPos,
                Quaternion.Euler(flaskSipLocalEuler), Quaternion.Euler(flaskStartLocalEuler),
                moveOutTime);

            Destroy(flask);
        }
        else
        {
            // no prefab/cam → just wait roughly same total time so arms look synced
            yield return new WaitForSeconds(moveInTime + sipTime + moveOutTime);
        }

        // wait for arm anim to fully finish
        while (arms != null && arms.IsPerkAnimating) yield return null;

        busy = false;
    }

    IEnumerator LerpLocal(Transform t, Vector3 p0, Vector3 p1, Quaternion r0, Quaternion r1, float dur)
    {
        dur = Mathf.Max(0.01f, dur);
        float k = 0f;
        while (k < 1f)
        {
            k += Time.deltaTime / dur;
            t.localPosition = Vector3.Lerp(p0, p1, k);
            t.localRotation = Quaternion.Slerp(r0, r1, k);
            yield return null;
        }
    }
}
