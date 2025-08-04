using UnityEngine;

public class MagicStation : MonoBehaviour
{
    [Header("Station Settings")]
    public MagicType magicType;
    public float interactionRange = 3f;

    [Header("Visual Feedback")]
    public GameObject interactionPrompt;

    [Header("Station Materials")]
    public Material defaultMaterial;     // When not selected
    public Material selectedMaterial;    // When this magic is currently equipped

    private Transform player;
    private bool playerInRange = false;
    private bool isCurrentlySelected = false;
    private MeshRenderer meshRenderer;

    void Start()
    {
        player = FindFirstObjectByType<PlayerMovement>().transform;
        meshRenderer = GetComponent<MeshRenderer>();

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        UpdateStationVisuals();
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
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        bool wasInRange = playerInRange;
        playerInRange = distance <= interactionRange;

        if (playerInRange != wasInRange)
        {
            if (interactionPrompt != null)
                interactionPrompt.SetActive(playerInRange);
        }
    }

    void CheckIfSelected()
    {
        bool wasSelected = isCurrentlySelected;

        if (MagicManager.Instance != null)
        {
            isCurrentlySelected = MagicManager.Instance.GetCurrentMagicType() == magicType;
        }

        if (wasSelected != isCurrentlySelected)
        {
            UpdateStationVisuals();
        }
    }

    void UpdateStationVisuals()
    {
        if (meshRenderer == null) return;

        if (isCurrentlySelected && selectedMaterial != null)
        {
            meshRenderer.material = selectedMaterial;
        }
        else if (defaultMaterial != null)
        {
            meshRenderer.material = defaultMaterial;
        }
    }

    void SelectMagic()
    {
        if (MagicManager.Instance != null)
        {
            bool wasEquipped = MagicManager.Instance.HasMagicEquipped();

            MagicManager.Instance.SetMagicType(magicType);

            if (!wasEquipped)
            {
                Debug.Log($"First magic selected: {magicType} - You can now press Q to cast!");
            }
            else
            {
                Debug.Log($"Switched to: {magicType} magic");
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, interactionRange);

        // Show what type this station is
        if (magicType == MagicType.Normal)
            Gizmos.color = Color.red;
        else if (magicType == MagicType.Sulfuric)
            Gizmos.color = Color.green;

        Gizmos.DrawCube(transform.position + Vector3.up * 2f, Vector3.one * 0.3f);
    }
}
