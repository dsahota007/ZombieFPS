using UnityEngine;

public class RemoveForearmsOnly : MonoBehaviour
{
    public Transform fallbackBone; // Drag something like Spine2 or a dummy near chest

    void Start()
    {
        SkinnedMeshRenderer smr = GetComponentInChildren<SkinnedMeshRenderer>();
        if (smr == null || fallbackBone == null) return;

        Transform[] bones = smr.bones;

        for (int i = 0; i < bones.Length; i++)
        {
            if (bones[i] == null) continue;

            string bone = bones[i].name.ToLower();

            // Remove only from forearm down — hands & fingers
            if (
                bone.Contains("forearm") ||
                bone.Contains("hand") ||
                bone.Contains("thumb") ||
                bone.Contains("index") ||
                bone.Contains("middle") ||
                bone.Contains("ring") ||
                bone.Contains("pinky")
            )
            {
                bones[i] = fallbackBone;
            }
        }

        smr.bones = bones;
        smr.updateWhenOffscreen = true;
    }
}
