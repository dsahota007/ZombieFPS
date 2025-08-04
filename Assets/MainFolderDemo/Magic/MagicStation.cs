using UnityEngine;

public class MagicStation : MonoBehaviour
{
    [Header("Station Settings")]
    public MagicType magicType;   //this is the enum in magic manager
    public float interactionRange = 3f;

    private Transform player;
    private bool playerInRange = false;
    private bool isCurrentlySelected = false;
 
    void Start()
    {
        player = FindFirstObjectByType<PlayerMovement>().transform;   //finds player transform for distance checking 
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
        if (MagicManager.Instance != null)
        {
            isCurrentlySelected = MagicManager.Instance.GetCurrentMagicType() == magicType;
        }
    }

    void SelectMagic()
    {
        if (MagicManager.Instance != null)
        {
            bool wasEquipped = MagicManager.Instance.HasMagicEquipped();
            MagicManager.Instance.SetMagicType(magicType);
        }
    }
}
