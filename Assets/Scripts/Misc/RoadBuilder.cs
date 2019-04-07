using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.Vehicles.Car;

public class RoadBuilder : MonoBehaviour
{
    private static RoadBuilder _singleton;
    public static RoadBuilder Singleton
    {
        get
        {
            if (_singleton == null)
                _singleton = FindObjectOfType<RoadBuilder>();
            return _singleton;
        }
    }

    public bool GizmosVisible = true;
    public bool ShowWaypointPoints = true;
    public bool ShowRoadPoints = true;

    // road parameters
    public float RoadScale = 5.0f;
    public enum Lane
    {
        LeftLane, RightLane
    }

    [SerializeField]
    private Lane currentLane = Lane.RightLane;
    public Lane CurrentLane
    {
        get { return currentLane; }
        private set { currentLane = value;}
    }


    // waypoints to follow (in order)
    public List<GameObject> Waypoints;
    public GameObject waypointsObject;

    public RoadPoint LastPoint 
    {
        get
        {
            var roadPoints = GetRoadPoints();
            return roadPoints.Length > 0 ? roadPoints[roadPoints.Length - 1] : null;
        }
    }

    public List<RoadCurve> Curves
    {
        get
        {
            var roadPoints = GetRoadPoints();
            if (roadPoints.Length < 3)
                return null;
            var curves = new List<RoadCurve>();
            for (int i = 0; i < roadPoints.Length; i++)
            {
                var curve = new RoadCurve(roadPoints[i], roadPoints[ClampIndex(i+1, roadPoints.Length)]);
                curves.Add(curve);
            }
            return curves;
        }
    }

    void OnDrawGizmos()
    {
        if (!GizmosVisible)
            return;
        DrawCurves();
        //BuildRoad();

        if(ShowWaypointPoints)
            ShowWaypoints();
    }

    public void ChangeLanes()
    {
        if (CurrentLane == Lane.LeftLane)
            CurrentLane = Lane.RightLane;
        else
            CurrentLane = Lane.LeftLane;
    }

    float GetLaneOffset()
    {
        if (CurrentLane == Lane.LeftLane)
            return -RoadScale / 2;
        else
            return RoadScale / 2;
    }

    public void ShowWaypoints()
    {   
        Gizmos.color = Color.yellow;

        for (int i = 0; i < Waypoints.Count - 1; i++)
        {
            var currentWaypoint = Waypoints[i].transform.position;
            var nextWaypoint = Waypoints[ClampIndex(i + 1, Waypoints.Count)].transform.position;

            // Lift points of the road a bit
            currentWaypoint.y += 1;
            nextWaypoint.y += 1;

            if (ShowWaypointPoints)
            {
                Gizmos.DrawLine(currentWaypoint, nextWaypoint);
                Gizmos.DrawSphere(currentWaypoint, 0.5f);
            }
        }
    }

    private List<Vector3> CalculateNormalizedPoints(List<Vector3> points, float separation)
    {
        var normalizedPoints = new List<Vector3>();
        normalizedPoints.Add(points[0]);

        float currentDistance = 0.0f;
        float pathDistance = 0.0f;

        for (int i = 0; i < points.Count; i++)
        {
            Vector3 point1 = points[i];
            Vector3 point2 = points[ClampIndex(i + 1, points.Count)];

            Vector3 direction = (point2 - point1);
            float magnitude = direction.magnitude;
            direction.Normalize();

            float nextDistance = pathDistance + magnitude;
            float potentialDistance = currentDistance + separation;

            while (potentialDistance < nextDistance)
            {
                currentDistance += separation;
                float distanceFromPoint1 = currentDistance - pathDistance;
                Vector3 newPoint = point1 + (direction * distanceFromPoint1);
                normalizedPoints.Add(newPoint);
                potentialDistance = currentDistance + separation;
            }
            pathDistance += magnitude;
        }
        return normalizedPoints;
    }

    public void GenerateWaypoints()
    {
        // ensure valid RoadCurves list
        var curves = Curves;
        if (curves == null)
            return;

        if (waypointsObject == null)
            waypointsObject = new GameObject("WayPoints");
        else
        {
            // Must use Destroy Immediate in Edit Mode
            foreach (Transform child in waypointsObject.transform)
                DestroyImmediate(child.gameObject);
        }

        Waypoints.Clear();

        var offsetCurves = new List<BezierCurve>();
        foreach (var curve in curves)
            offsetCurves.AddRange(curve.GetCurve().GetOffsetCurves(GetLaneOffset()));

        // Generate interpolated points
        var interpolatedPoints = new List<Vector3>();
        foreach (var curve in offsetCurves)
            interpolatedPoints.AddRange(curve.BuildLookUpTable());

        // Generate normalized points
        var normalizedPoints = CalculateNormalizedPoints(interpolatedPoints, 3.0f);

        for (int i=0; i < normalizedPoints.Count; i++)
        {
            var newobj = new GameObject("Waypoint " + i);
            newobj.transform.position = normalizedPoints[i];
            newobj.transform.parent = waypointsObject.transform;
            Waypoints.Add(newobj);
        };

        // Warn about sharp angles
        for (int i = 0; i < normalizedPoints.Count - 2; i++)
        {
            Vector3 v1 = normalizedPoints[i + 1] - normalizedPoints[i];
            Vector3 v2 = normalizedPoints[i + 2] - normalizedPoints[i + 1];
            float angle = Vector3.Angle(v1, v2);
            if (angle > 35)
                Debug.LogWarning("sharp angle between indicies " + i + " and " + (i + 2));
        }
    }

    void DrawCurves()
    {
        var curves = Curves;
        if (curves == null)
            return;
        
        foreach (var curve in curves)
            curve.DrawCurve();
    }

    public RoadPoint[] GetRoadPoints()
    {
        return GetComponentsInChildren<RoadPoint>();
    }

    private int ClampIndex(int index, int maxIndex)
    {
        if (index < 0)
            index += maxIndex;
        else if (index >= maxIndex)
            index -= maxIndex;
        return index;
    }

    public void BuildRoad()
    {
        var originalCurves = Curves;
        if (originalCurves == null)
            return;

        var leftCurves = new List<BezierCurve>();
        foreach (var curve in originalCurves)
            leftCurves.AddRange(curve.GetCurve().GetOffsetCurves(-RoadScale));

        var rightCurves = new List<BezierCurve>();
        foreach (var curve in originalCurves)
            rightCurves.AddRange(curve.GetCurve().GetOffsetCurves(RoadScale));

        var filter = GetComponent<MeshFilter>();
        if (filter == null)
            filter = gameObject.AddComponent<MeshFilter>();

        Mesh mesh;
        if (filter.sharedMesh == null)
        {
            mesh = new Mesh();
            filter.sharedMesh = mesh;
        }
        else
            mesh = filter.sharedMesh;

        mesh.name = "RoadMesh";
        mesh.Clear();
        CreateRoadMesh(mesh, leftCurves, rightCurves);

        var collider = GetComponent<MeshCollider>();
        collider.sharedMesh = mesh;
    }

    private void CreateRoadMesh(Mesh mesh, List<BezierCurve> leftCurves, List<BezierCurve> rightCurves)
    {
        int vertCount = BezierCurve.LineResolution * leftCurves.Count  * 4;
        int triCount = vertCount;
        int triIndexCount = triCount * 3;

        int[] triangleIndices = new int[triIndexCount];
        Vector3[] vertices = new Vector3[vertCount];
        Vector3[] normals = new Vector3[vertCount];
        Vector2[] uvs = new Vector2[vertCount];

        float currentDist = 0.0f;
        float resolutionFactor = 1.0f / BezierCurve.LineResolution;
        for (int i = 0; i < leftCurves.Count; i++)
        {
            var leftCurvePoints = leftCurves[i].BuildLookUpTable();
            var rightCurvePoints = rightCurves[i].BuildLookUpTable();

            for (int j = 0; j < BezierCurve.LineResolution; j++)
            {
                int offset = i * BezierCurve.LineResolution * 4 + j * 4;

                Vector3 rightNormal = rightCurves[i].GetUpNormal(j * resolutionFactor);
                Vector3 leftNormal = leftCurves[i].GetUpNormal(j * resolutionFactor);
                Vector3 middleNormal = Vector3.Lerp(rightNormal, leftNormal, 0.5f);

                vertices[offset] = rightCurvePoints[j];
                normals[offset] = rightNormal;
                uvs[offset] = new Vector2(0, currentDist);

                offset += 1;
                vertices[offset] = Vector3.Lerp(leftCurvePoints[j], rightCurvePoints[j], 0.5f);
                normals[offset] = middleNormal;
                uvs[offset] = new Vector2(1, currentDist);

                offset += 1;
                vertices[offset] = vertices[offset - 1];
                normals[offset] = middleNormal;
                uvs[offset] = new Vector2(0, currentDist);

                offset += 1;
                vertices[offset] = leftCurvePoints[j];
                normals[offset] = leftNormal;
                uvs[offset] = new Vector2(-1, currentDist);

                currentDist += resolutionFactor;
            }
        }
        
        int ti = 0;
        for (int i = 0; i < vertCount; i += 2)
        {
            // Triangle points
            int a = ClampIndex(i, vertCount);
            int b = ClampIndex(i + 1, vertCount);
            int c = ClampIndex(i + 5, vertCount);
            int d = ClampIndex(i + 4, vertCount);

            // Triangle 1
            triangleIndices[ti] = a; ti++;
            triangleIndices[ti] = b; ti++;
            triangleIndices[ti] = c; ti++;

            // Triangle 2
            triangleIndices[ti] = c; ti++;
            triangleIndices[ti] = d; ti++;
            triangleIndices[ti] = a; ti++;
        }
        mesh.vertices = vertices;
        mesh.triangles = triangleIndices;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.RecalculateBounds();
    }

    /*
    public List<Transform> controlPointsList;
    public bool isLooping = true;
    public Mesh _roadMesh;
    private CatmullRomSpline centerSpline, rightSpline, leftSpline;

    /// <summary>
    /// Display splines in editor
    /// </summary>
    void OnDrawGizmos()
    {
        controlPointsList = new List<Transform>();
        foreach (Transform child in transform)
            controlPointsList.Add(child);
        if (controlPointsList.Count < 4)
            return;
        
        var controlPoints = new List<Vector3>();
        foreach (var controlPoint in controlPointsList)
            controlPoints.Add(controlPoint.position);
        
        var centerSpline = new CatmullRomSpline(controlPoints, true);
        Gizmos.color = Color.white;
        centerSpline.DrawNormalizedPoints();


        for (int i = 0; i < controlPoints.Count; i++)
        {
            Vector3 p1 = centerSpline.controlPoints[i];
            Vector3 p2 = centerSpline.controlPoints[ClampIndex(i + 1, controlPoints.Count)];
            Vector3 p3 = centerSpline.controlPoints[ClampIndex(i + 2, controlPoints.Count)];
            Vector3 p4 = centerSpline.controlPoints[ClampIndex(i + 3, controlPoints.Count)];
            var curve = new BezierCurve(p1, p2, p3, p4);
            curve.DrawRoots();
        }

        rightSpline = new CatmullRomSpline(centerSpline, 2.0f, true);
        Gizmos.color = Color.red;
        rightSpline.DrawNormalizedPoints();

        leftSpline= new CatmullRomSpline(centerSpline, -2.0f, true);
        Gizmos.color = Color.green;
        leftSpline.DrawNormalizedPoints();
        
        var filter = GetComponent<MeshFilter>();
        if (filter == null)
            return;
        if (filter.sharedMesh == null)
            filter.sharedMesh = new Mesh();
        _roadMesh = filter.sharedMesh;
        //ExtrudeMesh(normalizedPoints);
    }

    private int ClampIndex(int index, int maxIndex)
    {
        if (index < 0)
            index += maxIndex;
        else if (index >= maxIndex)
            index -= maxIndex;
        return index;
    }

    private void CreateRoadMesh()
    {
        var triangleIndices = new List<int>();
        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var uvs = new List<Vector2>();

        //vertices.AddRange(rightSpline.normalizedPoints);
        //vertices.AddRange(leftSpline.normalizedPoints);
    }*/

    /*public void ExtrudeMesh(List<Vector3> normalizedPoints)
    {
        var roadProfileMesh = new RoadProfileMesh();
        int profileVertCount= roadProfileMesh.points.Length;

        int numSegments = normalizedPoints.Count + 2;
        int vertCount = profileVertCount * normalizedPoints.Count;
        int triCount = numSegments * 2 * profileVertCount;
        int triIndexCount = triCount * 3;

        int[] triangleIndices = new int[triIndexCount];
        Vector3[] vertices = new Vector3[vertCount];
        Vector3[] normals = new Vector3[vertCount];
        Vector2[] uvs = new Vector2[vertCount];

        float currentDist = 0.0f;
        for (int i = 0; i < normalizedPoints.Count; i++)
        {
            Vector3 point1 = normalizedPoints[i];
            Vector3 point2 = normalizedPoints[ClampIndex(i + 1, normalizedPoints.Count)];
            Vector3 direction = (point2 - point1).normalized;

            int offset = i * profileVertCount;
            for (int j = 0; j < profileVertCount; j++)
            {
                int index = offset + j;
                vertices[index] = point1 + roadProfileMesh.TransformedPoint(j, direction);
                normals[index] = roadProfileMesh.TransformedNormal(j, direction);
                uvs[index] = new Vector2(currentDist, roadProfileMesh.points[j].x - 0.5f);
                currentDist += 0.01f;
            }
        }

        int ti = 0;
        for (int i = 0; i < numSegments; i++)
        {
            int offset = i * profileVertCount;
            for (int j = 0; j < profileVertCount; j++)
            {
                int profileIndex = ClampIndex(offset + j, profileVertCount);
                int nextProfileIndex = ClampIndex(profileIndex + 1, profileVertCount);

                // Triangle points
                int a = ClampIndex(profileIndex, vertCount);
                int b = ClampIndex(profileIndex + profileVertCount, vertCount);
                int c = ClampIndex(nextProfileIndex + profileVertCount, vertCount);
                int d = ClampIndex(nextProfileIndex, vertCount);

                // Triangle 1
                triangleIndices[ti] = c; ti++;
                triangleIndices[ti] = b; ti++;
                triangleIndices[ti] = a; ti++;

                // Triangle 2
                triangleIndices[ti] = a; ti++;
                triangleIndices[ti] = d; ti++;
                triangleIndices[ti] = c; ti++;
            }
        }

        _roadMesh.Clear();
        _roadMesh.vertices = vertices;
        _roadMesh.triangles = triangleIndices;
        _roadMesh.normals = normals;
        _roadMesh.uv = uvs;
        _roadMesh.RecalculateBounds();
    }*/
}
