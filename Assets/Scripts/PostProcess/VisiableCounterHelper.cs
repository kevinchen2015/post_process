using UnityEngine;

public class VisiableCounterHelper : MonoBehaviour
{
    void LateUpdate()
    {
        PostProcessMgr.OnVisiable(gameObject, true);
    }
}
