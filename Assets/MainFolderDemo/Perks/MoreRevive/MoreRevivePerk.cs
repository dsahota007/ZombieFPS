using UnityEngine;
using System.Collections;


public class MoreRevivePerk : MonoBehaviour
{
    public PointManager points;

    [Header("Interact")]
    public Transform player;
    public PerkType type = PerkType.Revive;
    public int cost = 3000;
    public float interactDistance = 2.2f;

    [Header("Flask")]
    public GameObject flaskPrefab;           // leave null if you only want the arm anim

    [Header("Flask Offsets")]
    public Vector3 flaskStartLocalPos = new Vector3(-0.09f, -1.1f, 0.42f);
    public Vector3 flaskMouthLocalPos = new Vector3(-0.01f, -0.12f, 0.16f);
    public Vector3 flaskStartLocalEuler = Vector3.zero;
    public Vector3 flaskSipLocalEuler = new Vector3(-65f, 0f, 0f);

    [Header("Timing")]
    public float moveInTime = 0.5f;
    public float sipTime = 1f;
    public float moveOutTime = 0.25f;

    [Header("Perk Upgrade")]
    public float timeToRegen = 3f;
    public float regenRatePerSecondIncrease = 10f;
    public GameObject PlayerDrinkVFX;
    [HideInInspector] public bool hasMoreRevivePerk = false;

    Transform cam;   // Camera.main


    void Awake()
    {
        cam = (Camera.main != null) ? Camera.main.transform : null;

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (points == null)      // <-- ADD
            points = FindFirstObjectByType<PointManager>();
    }

    void Update()
    {
        bool inRange = Vector3.Distance(player.position, transform.position) <= interactDistance;
        if (inRange && Input.GetKeyDown(KeyCode.E) && !hasMoreRevivePerk)
        {

            UI ui = FindFirstObjectByType<UI>();

            // **PAY OR BLOCK**
            if (!points.TrySpend(cost))   // <-- ADD
            {
                if (ui != null) ui.ShowTemporaryPerkMessage("Not enough points");
                return;
            }

            ArmMovementMegaScript arms = FindFirstObjectByType<ArmMovementMegaScript>();   //arm script
            if (arms == null || arms.IsGrenadeAnimating || arms.IsPerkAnimating) return;

            StartCoroutine(DoPerkDrink(arms));  //drink if in range
        }
    }

    IEnumerator DoPerkDrink(ArmMovementMegaScript arms)
    {
        hasMoreRevivePerk = true;
        player?.GetComponent<PlayerAttributes>()?.IncreaseRegenFromMoreRevivePerk(timeToRegen, regenRatePerSecondIncrease);

        FindFirstObjectByType<UI>()?.ShowPerkIcon(PerkType.Revive);

        if (player != null && PlayerDrinkVFX != null)
        {
            //GameObject PerkVFX = Instantiate(PlayerDrinkVFX, player.transform.position + Vector3.up * 2.85f, Quaternion.Euler(180f, 0f, 0f));  
            GameObject PerkVFX = Instantiate(PlayerDrinkVFX, player.transform.position, Quaternion.identity);
            PerkVFX.transform.SetParent(player.transform, true);
            Destroy(PerkVFX, 4f);

        }

        // arm animation
        arms.StartCoroutine(arms.PerkDrinkDropOnly());    // Play the left-arm drop/drink animation

        // flask (optional)
        if (cam != null && flaskPrefab != null)
        {
            GameObject flask = Instantiate(flaskPrefab, cam, false);   // parent directly to camera
            Transform tf = flask.transform;        //we get the point into tf

            tf.localPosition = flaskStartLocalPos;      //start offset
            tf.localRotation = Quaternion.Euler(flaskStartLocalEuler);      //start rot

            yield return LerpLocal(
                tf,
                flaskStartLocalPos, flaskMouthLocalPos,
                Quaternion.Euler(flaskStartLocalEuler), Quaternion.Euler(flaskSipLocalEuler),
                moveInTime
            );

            // sip
            yield return new WaitForSeconds(sipTime);

            // out
            yield return LerpLocal(         //this func is combign lerp and slerp so we can do rotation and movment 
                tf,
                flaskMouthLocalPos, flaskStartLocalPos,
                Quaternion.Euler(flaskSipLocalEuler), Quaternion.Euler(flaskStartLocalEuler),
                moveOutTime
            );

            Destroy(flask);
        }
        //else
        //{
        //    no prefab/cam → just wait roughly same total time so arms look synced
        //    yield return new WaitForSeconds(moveInTime + sipTime + moveOutTime);
        //}

        // wait for arm anim to fully finish
        while (arms != null && arms.IsPerkAnimating) yield return null;

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
