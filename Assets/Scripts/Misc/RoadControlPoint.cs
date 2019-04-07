using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadControlPoint : MonoBehaviour {

    public RoadPoint roadPoint;
    public RoadControlPoint siblingCtrlPoint;
    public static readonly float ctrlDistanceScale = 10.0f;

    public Vector3 RoadPointPos { get { return roadPoint.transform.position; } }

    /// <summary>
    /// Display the RoadPoint in editor
    /// </summary>
    void OnDrawGizmos()
    {
        if (!RoadBuilder.Singleton.GizmosVisible || !RoadBuilder.Singleton.ShowRoadPoints)
            return;
        Gizmos.color = Color.green;
        float size = 1.0f;
        Gizmos.DrawSphere(transform.position, size);
    }

    void OnDrawGizmosSelected()
    {
        Vector3 myDirection = (transform.position - RoadPointPos).normalized;
        if (myDirection == Vector3.zero)
            return;
        float otherMagnitude = (siblingCtrlPoint.transform.position - RoadPointPos).magnitude;
        siblingCtrlPoint.transform.position = -myDirection * otherMagnitude + RoadPointPos;
    }

    public Vector3 GetScaledControlPoint()
    {
        return transform.position;
    }
}
