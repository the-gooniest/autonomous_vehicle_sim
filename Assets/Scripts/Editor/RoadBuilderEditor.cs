using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RoadBuilder))]
public class RoadBuilderEditor : Editor {

    private RoadBuilder roadBuilder;

    public override void OnInspectorGUI()
    {
        roadBuilder = (RoadBuilder)target;
        if (GUILayout.Button("Generate Waypoints"))
            GenerateWaypoints();
        if (GUILayout.Button("Add Road Point"))
            AddRoadPoint();
        if (GUILayout.Button("Build Road"))
            roadBuilder.BuildRoad();
        DrawDefaultInspector();
    }

    void GenerateWaypoints()
    {
        Debug.Log("Generating Waypoints");
        roadBuilder.GenerateWaypoints();
    }

    void AddRoadPoint()
    {
        Debug.Log("Adding Road Point");
        var roadPoints = roadBuilder.GetRoadPoints();

        var roadPointObj = new GameObject();
        if (roadPoints.Length > 0)
        {
            var lastRoadPoint = roadPoints[roadPoints.Length - 1];
            roadPointObj.transform.position = lastRoadPoint.transform.position + Vector3.forward;
        }

        roadPointObj.transform.parent = roadBuilder.transform;
        roadPointObj.name = "Road Point " + roadPoints.Length;
        var point = roadPointObj.AddComponent<RoadPoint>();

        CreateControlPoints(point);
    }

    void CreateControlPoints(RoadPoint point)
    {
        var ctrlPointObj1 = new GameObject();
        ctrlPointObj1.name = "Control Point";
        ctrlPointObj1.transform.position = point.transform.position + Vector3.right;
        ctrlPointObj1.transform.parent = point.transform;
        var ctrlPoint1 = ctrlPointObj1.AddComponent<RoadControlPoint>();
        ctrlPoint1.roadPoint = point;
        point.ctrlPoint1 = ctrlPoint1;

        var ctrlPointObj2 = new GameObject();
        ctrlPointObj2.name = "Control Point";
        ctrlPointObj2.transform.position = point.transform.position + Vector3.left;
        ctrlPointObj2.transform.parent = point.transform;
        var ctrlPoint2 = ctrlPointObj2.AddComponent<RoadControlPoint>();
        ctrlPoint2.roadPoint = point;
        point.ctrlPoint2 = ctrlPoint2;

        ctrlPoint1.siblingCtrlPoint = ctrlPoint2;
        ctrlPoint2.siblingCtrlPoint = ctrlPoint1;
    }
}
