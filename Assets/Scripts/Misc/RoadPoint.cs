using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadPoint : MonoBehaviour {

    public RoadControlPoint ctrlPoint1, ctrlPoint2;

    /// <summary>
    /// Display the RoadPoint in editor
    /// </summary>
    void OnDrawGizmos()
    {
        if (!RoadBuilder.Singleton.GizmosVisible || !RoadBuilder.Singleton.ShowRoadPoints)
            return;
        Gizmos.color = Color.red;
        float size = 1.0f;
        Gizmos.DrawCube(transform.position, new Vector3(size, size, size));
    }
}
