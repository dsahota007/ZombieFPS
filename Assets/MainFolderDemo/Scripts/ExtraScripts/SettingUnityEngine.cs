using UnityEngine;

public class SettingUnityEngine : MonoBehaviour
{
    void Start()
    {
        QualitySettings.vSyncCount = 0;  
        Application.targetFrameRate = 60;  
    }
}
