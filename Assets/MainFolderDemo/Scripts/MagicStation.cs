using UnityEngine;

public class MagicStation : MonoBehaviour
{
    [Header("Station Settings")]
    public MagicType magicType;   //this is the enum in magic manager
    public float interactionRange = 3f;

    [Header("Cost")]
    public int cost = 3000;

    private Transform player;
    private bool playerInRange = false;
    private bool isCurrentlySelected = false;
    private MagicManager magicManager;  // Direct reference instead of Instance
    private PointManager points;          // direct ref

    void Start()
    {
        player = FindFirstObjectByType<PlayerMovement>().transform;   //finds player transform for distance checking 
        magicManager = FindFirstObjectByType<MagicManager>();         // assign and fetch script becasue we aint doing that instance BS
        points = FindFirstObjectByType<PointManager>();
    }

    void Update()
    {
        CheckPlayerDistance();
        CheckIfSelected();

        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            SelectMagic();
        }
    }

    void CheckPlayerDistance()
    {
        if (player == null) return;    // safe code if player DNE for some reason we can get rid of this.   

        float distance = Vector3.Distance(transform.position, player.position);  // we check the box and player position 
        bool wasInRange = playerInRange;   //saves prev frame  This lets you check later: “Did the player just enter or just leave the range?”
        playerInRange = distance <= interactionRange;   //if the dist is less or equal to player in range than true or otherwise its false

    }

    void CheckIfSelected()
    {
        bool wasSelected = isCurrentlySelected;
        if (magicManager != null)
        {
            isCurrentlySelected = magicManager.GetCurrentMagicType() == magicType;
        }

    }

    void SelectMagic()
    {
        if (magicManager == null || points == null) return;  //if we dont have one get out

        UI ui = FindFirstObjectByType<UI>();            //fetch scirpt ui

        // already equipped?
        if (magicManager.GetCurrentMagicType() == magicType)
        {
            if (ui != null) ui.ShowTemporaryMagicMessage(magicType.ToString() + " Magic already equipped");  //we got the same magic already equipped
            return;
        }

        // pay cost (block if not enough)
        if (!points.TrySpend(cost))
        {
            if (ui != null) ui.ShowTemporaryMagicMessage("Not enough points");
            return;
        }

        // equip and confirm
        magicManager.SetMagicType(magicType);
        if (ui != null) ui.ShowTemporaryMagicMessage(magicType.ToString() + " Magic purchased");
    }
}
