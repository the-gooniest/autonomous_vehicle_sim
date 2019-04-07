using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierCurve {

    public static readonly int LineResolution = 40;
    public Vector3[] controlPoints = new Vector3[4];

    public BezierCurve(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
        controlPoints[0] = p1;
        controlPoints[1] = p2;
        controlPoints[2] = p3;
        controlPoints[3] = p4;
    }

    public BezierCurve[] GetOffsetCurves(float offset)
    {
        // Split the curve into 4 pieces
        var initialCurves = SplitCurve(0.5f);
        var subCurves1 = initialCurves[0].SplitCurve(0.5f);
        var subCurves2 = initialCurves[1].SplitCurve(0.5f);

        // Collect all subcurves
        var curves = new BezierCurve[4];
        curves[0] = subCurves1[0];
        curves[1] = subCurves1[1];
        curves[2] = subCurves2[0];
        curves[3] = subCurves2[1];

        // Point scale the curves toward their scaling origins by the given offset
        foreach (var curve in curves)
            curve.ScaleRelativeToScalingOrigin(offset);

        return curves;
    }

    public void DrawControlPoints()
    {
        Gizmos.color = Color.red;
        float size= 0.15f;
        foreach (var point in controlPoints)
            Gizmos.DrawCube(point, new Vector3(size, size, size));
    }

    public Vector3[] BoundingBox()
    {
        // Get roots and endpoints
        var pointsToCheck = GetRootPoints();
        pointsToCheck.Add(controlPoints[0]);
        pointsToCheck.Add(controlPoints[3]);

        Vector3 closestPoint = pointsToCheck[0];
        Vector3 farthestPoint = pointsToCheck[0];
        foreach(var point in pointsToCheck)
        {
            if (point.x >= farthestPoint.x && point.z >= farthestPoint.z)
                farthestPoint = point;
            else if (point.x <= farthestPoint.x && point.z <= farthestPoint.z)
                closestPoint = point;
        }
        Vector3[] points = new Vector3[2];
        points[0] = closestPoint;
        points[1] = farthestPoint;
        return points;
    }

    public Vector3 CurveCenter()
    {
        var boundingBox = BoundingBox();
        return Vector3.Lerp(boundingBox[0], boundingBox[1], 0.5f);
    }

    public bool IsCurveSafe()
    {
        var boundingBox = BoundingBox();
        float boxMagnitude = (boundingBox[1] - boundingBox[0]).magnitude / 2.0f;
        var center = CurveCenter();
        float distFromCenter = (GetPoint(0.5f) - center).magnitude;
        return (distFromCenter < boxMagnitude * 0.25f);
    }

    /// <summary>
    /// Returns the closest point on the curve by its t value relative the given point
    /// </summary>
    public float ProjectPointOntoCurve(Vector3 point)
    {
        var lookUpTable = BuildLookUpTable();
        float closestDist = Mathf.Infinity;
        float tValue = 0.0f;
        float iterationLength = 1.0f / lookUpTable.Length;
        for (int i=0; i < lookUpTable.Length; i++)
        {
            float dist = (lookUpTable[i] - point).sqrMagnitude;
            if (dist < closestDist)
            {
                closestDist = dist;
                tValue = i * iterationLength;
            }
        }
        return tValue;
    }

    public void ScaleRelativeToScalingOrigin(float offset)
    {
        Vector3 p1Withouty = controlPoints[0];
        p1Withouty.y = 0;
        Vector3 p4Withouty = controlPoints[3];
        p4Withouty.y = 0;

        var newPoints = new Vector3[4];

        for (int i = 0; i < controlPoints.Length; i++)
        {
            Vector3 normal = GetNormal(ProjectPointOntoCurve(controlPoints[i])) * offset;
            newPoints[i] = normal + controlPoints[i];
        }
        for (int i = 0; i < controlPoints.Length; i++)
            controlPoints[i] = newPoints[i];
    }

    /// <summary>
    /// Returns an array containing the two new curves made by splitting this curve at point t
    /// </summary>
    public BezierCurve[] SplitCurve(float t)
    {
        Vector3 p1 = controlPoints[0];
        Vector3 p2 = controlPoints[1];
        Vector3 p3 = controlPoints[2];
        Vector3 p4 = controlPoints[3];

        // Calculate first 3 skeleton points
        Vector3 skelPoint1 = Vector3.Lerp(p1, p2, t);
        Vector3 skelPoint2 = Vector3.Lerp(p2, p3, t);
        Vector3 skelPoint3 = Vector3.Lerp(p3, p4, t);

        // Calculate next two sub-points
        Vector3 skelSubPoint1 = Vector3.Lerp(skelPoint1, skelPoint2, t);
        Vector3 skelSubPoint2 = Vector3.Lerp(skelPoint2, skelPoint3, t);

        // Calculate splitPoint
        Vector3 splitPoint = Vector3.Lerp(skelSubPoint1, skelSubPoint2, t);

        // Create Curves
        BezierCurve[] curves = new BezierCurve[2];
        curves[0] = new BezierCurve(p1, skelPoint1, skelSubPoint1, splitPoint);
        curves[1] = new BezierCurve(splitPoint, skelSubPoint2, skelPoint3, p4);
        return curves;
    }

    /// <summary>
    /// Points perpendicularly to the right of the point
    /// </summary>
    public Vector3 GetNormal(float t)
    {
        Vector3 tangent = GetTangent(t);
        Vector3 normal = -Vector3.Cross(tangent, Vector3.up);
        return normal.normalized;
    }

    /// <summary>
    /// Points perpendicularly to the right of the point
    /// </summary>
    public Vector3 GetUpNormal(float t)
    {
        Vector3 tangent = GetTangent(t);
        Vector3 normal = Vector3.Cross(tangent, GetNormal(t));
        return normal.normalized;
    }

    public List<Vector3> GetRootPoints()
    {
        var curveRoots = new List<Vector3>();
        var roots = GetRoots();

        foreach (float t in roots)
            curveRoots.Add(GetPoint(t)); 
        return curveRoots;
    }

    public List<float> GetRoots()
    {
        Vector3 p1 = controlPoints[0];
        Vector3 p2 = controlPoints[1];
        Vector3 p3 = controlPoints[2];
        Vector3 p4 = controlPoints[3];

        var xRoots = QuadraticRoots(p1.x, p2.x, p3.x, p4.x);
        var yRoots = QuadraticRoots(p1.y, p2.y, p3.y, p4.y);
        var zRoots = QuadraticRoots(p1.z, p2.z, p3.z, p4.z);

        var tSet = new HashSet<float>();
        tSet.UnionWith(xRoots);
        tSet.UnionWith(yRoots);
        tSet.UnionWith(zRoots);


        var sortedList = new List<float>();
        sortedList.AddRange(tSet);
        sortedList.Sort();
        return sortedList;
    }

    private HashSet<float> QuadraticRoots(float p1, float p2, float p3, float p4)
    {
        var roots = new HashSet<float>();

        float lowerLimit = 0.1f;
        float upperLimit = 0.9f;

        // Quadratic Coefficients
        float a = 3 * (-p1 + (3 * p2) - (3 * p3) + p4);
        if (a == 0)
            return roots;
        float b = 6 * (p1 - (2 * p2) + p3);
        float c = 3 * (p1 - p2);

        // Get the 2nd derivative root
        float firstRoot = -b / a;
        if (firstRoot > lowerLimit && firstRoot < upperLimit)
            roots.Add(firstRoot);

        // Now get 1st derivative roots
        float descrimenent = (b * b) - (4 * a * c);
        if (descrimenent < 0)
            return roots;

        float sqrRoot = Mathf.Sqrt(descrimenent);
        float root1 = (-b + sqrRoot) / (2 * a);
        float root2 = (-b - sqrRoot) / (2 * a);

        if (root1 > lowerLimit && root1 < upperLimit)
            roots.Add(root1);
        if (root2 > lowerLimit && root2 < upperLimit)
            roots.Add(root2);

        return roots;
    }

    private string PrintList<T>(List<T> list)
    {
        if (list.Count == 0)
            return "None";
        string output = "";
        foreach (var obj in list)
            output += obj.ToString() + ", ";
        return output;
    }

    private Vector3 GetPoint(float t)
    {
        Vector3 p1 = controlPoints[0];
        Vector3 p2 = controlPoints[1];
        Vector3 p3 = controlPoints[2];
        Vector3 p4 = controlPoints[3];

        // inverse t
        float it = (1-t);
        return (it * it * it * p1) + (3 * it * it * t * p2) + (3 * it * t * t * p3) + (t * t * t * p4);
    }

    public void DrawRoots()
    {
        float size = 0.15f;
        Gizmos.color = Color.blue;
        foreach (var root in GetRootPoints())
            Gizmos.DrawCube(root, new Vector3(size, size, size));
    }

    public Vector3[] BuildLookUpTable()
    {
        Vector3[] lookUpTable = new Vector3[LineResolution];
        float resolutionFactor = 1.0f / LineResolution;
        for (int i=0; i < LineResolution; i++)
            lookUpTable[i] = GetPoint(i * resolutionFactor);
        return lookUpTable;
    }

    public void DrawCurveNoLookUp(Color color)
    {
        Vector3 lastPosition = controlPoints[0];
        float resolutionFactor = 1.0f / LineResolution;
        for (int i=0; i < LineResolution; i++)
        {
            Vector3 newPosition = GetPoint(i * resolutionFactor);
            Gizmos.color = color;
            Gizmos.DrawLine(lastPosition, newPosition);
            lastPosition = newPosition;
        }
    }

    public void DrawCurve(Color color)
    {
        var lookUpTable = BuildLookUpTable();
        for (int i=0; i < lookUpTable.Length - 1; i++)
        {
            Vector3 oldPosition = lookUpTable[i];
            Vector3 newPosition = lookUpTable[i+1];
            Gizmos.color = color;
            Gizmos.DrawLine(oldPosition, newPosition);
        }
    }

    public Vector3 GetTangent(float t)
    {
        Vector3 p1 = controlPoints[0];
        Vector3 p2 = controlPoints[1];
        Vector3 p3 = controlPoints[2];
        Vector3 p4 = controlPoints[3];

        // Get first derivative quadratic-bezier control points
        Vector3 v1 = 3 * (p2 - p1);
        Vector3 v2 = 3 * (p3 - p2);
        Vector3 v3 = 3 * (p4 - p3);

        // t inverse
        float it = 1 - t;

        // Quadratic bezier curve point
        Vector3 qp = (it * it * v1) + (2 * it * t * v2) + (t * t * v3);
        return qp;
    }

    public bool LineLineIntersection(out Vector3 intersection, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2) {

        Vector3 lineVec3 = linePoint2 - linePoint1;
        Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
        Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);

        float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

        //is coplanar, and not parrallel
        if(Mathf.Abs(planarFactor) < 0.0001f && crossVec1and2.sqrMagnitude > 0.0001f)
        {
            float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;
            intersection = linePoint1 + (lineVec1 * s);
            return true;
        }
        else
        {
            intersection = Vector3.zero;
            return false;
        }
    }
}
