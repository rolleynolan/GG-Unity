using UnityEngine;

[DefaultExecutionOrder(-600)]
public class DashboardBootstrap : MonoBehaviour
{
    void Awake()
    {
        if (!UnityEngine.Object.FindFirstObjectByType<DashboardTabsController>(FindObjectsInactive.Include))
        {
            var root = GameObject.Find("Dashboard") ?? new GameObject("Dashboard");
            root.AddComponent<DashboardTabsController>();
        }
    }
}
