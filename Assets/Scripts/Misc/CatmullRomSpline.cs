using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatmullRomSpline {

    public class InterpolationPoint
    {
        /// <summary>
        /// The interpolation value associated with the normalized point
        /// </summary>
        public float t = 0.0f;

        /// <summary>
        /// The normalized point
        /// </summary>
        public Vector3 point;

        public InterpolationPoint(Vector3 point, float interpolationValue)
        {
            t = interpolationValue;
            this.point = point;
        }
    }

    public static readonly float InterpolationResolution = 0.05f;
    public static readonly float LoftResolution = 0.05f;
    public List<Vector3> controlPoints;
    public List<InterpolationPoint> catmullRomPoints;
    public List<InterpolationPoint> normalizedPoints;
    public bool isLooping;

    public CatmullRomSpline(IEnumerable<Vector3> points, bool isLooping)
    {
        this.isLooping = isLooping;
        controlPoints = new List<Vector3>();
        controlPoints.AddRange(points);
        CalculateCatmullRomPoints();
        CalculateNormalizedPoints();
    }

    public CatmullRomSpline(CatmullRomSpline originalSpline, float scale, bool isLooping)
    {
        this.isLooping = isLooping;
        var unTranslatedControlPoints = InterpolatePoints(originalSpline.controlPoints);

        controlPoints = new List<Vector3>();
        int numPoints = unTranslatedControlPoints.Count;
        for (int i=0; i < numPoints; i++)
        {
            Vector3 p0 = unTranslatedControlPoints[ClampIndex(i - 1, numPoints)];
            Vector3 p1 = unTranslatedControlPoints[ClampIndex(i, numPoints)];
            Vector3 p2 = unTranslatedControlPoints[ClampIndex(i + 1, numPoints)];
            Vector3 p3 = unTranslatedControlPoints[ClampIndex(i + 2, numPoints)];
            var curve = new BezierCurve(p0, p1, p2, p3);
            Vector3 tangent = curve.GetTangent(0).normalized;
;
            Vector3 directionVector = Vector3.Cross(tangent, Vector3.up).normalized * scale;
            Vector3 translatedPoint = p1 + directionVector;

            controlPoints.Add(translatedPoint);
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(translatedPoint, 0.2f);
            Gizmos.DrawRay(new Ray(translatedPoint, tangent));

        }
        controlPoints = unTranslatedControlPoints;
        CalculateCatmullRomPoints();
        CalculateNormalizedPoints();
    }

    private int ClampIndex(int index, int maxIndex)
    {
        if (index < 0)
            index += maxIndex;
        else if (index >= maxIndex)
            index -= maxIndex;
        return index;
    }

    /// <summary>
    /// Returns a position between 4 Vector3's with Catmull-Rom spline algorithm
    /// </summary>
    /// <param name="t">The interpolation factor. Values range from 0 to 1.</param>
    /// <remarks>
    /// http://www.iquilezles.org/www/articles/minispline/minispline.htm
    /// </remarks>
    private Vector3 GetCatmullRomPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        //The coefficients of the cubic polynomial (except the 0.5f * which I added later for performance)
        Vector3 a = 2f * p1;
        Vector3 b = p2 - p0;
        Vector3 c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
        Vector3 d = -p0 + 3f * p1 - 3f * p2 + p3;

        //The cubic polynomial: a + b * t + c * t^2 + d * t^3
        Vector3 pos = 0.5f * (a + (b * t) + (c * t * t) + (d * t * t * t));
        return pos;
    }

    public void DrawCatmullRomPoints()
    {
        if (catmullRomPoints == null)
            return;
        int numPoints = catmullRomPoints.Count;
        int endPoint = isLooping ? numPoints : numPoints - 1;
        for (int i = 0; i < endPoint; i++)
        {
            Vector3 point1 = catmullRomPoints[i].point;
            Vector3 point2 = catmullRomPoints[ClampIndex(i + 1, numPoints)].point;
            Gizmos.DrawLine(point1, point2);
        }
    }

    public void DrawNormalizedPoints()
    {
        if (normalizedPoints == null)
            return;
        /*
        var max = normalizedPoints[0];
        int index = 0;
        for (int i=0 ; i < normalizedPoints.Count; i++)
        {
            if (max.t < normalizedPoints[i].t)
            {
                max = normalizedPoints[i];
                index = i;
            }
        }
        Debug.Log(max.t + " " + index + " " + normalizedPoints.Count);*/

        int numPoints = normalizedPoints.Count;
        int endPoint = isLooping ? numPoints : numPoints - 1;
        for (int i = 0; i < endPoint; i++)
        {
            Vector3 point1 = normalizedPoints[i].point;
            Vector3 point2 = normalizedPoints[ClampIndex(i + 1, numPoints)].point;
            Gizmos.DrawLine(point1, point2);
        }
    }

    private Vector3 CalculateMidPoint(int index, List<Vector3> points)
    {
        var numPoints = points.Count;
        Vector3 p0 = points[ClampIndex(index - 1, numPoints)];
        Vector3 p1 = points[ClampIndex(index, numPoints)];
        Vector3 p2 = points[ClampIndex(index + 1, numPoints)];
        Vector3 p3 = points[ClampIndex(index + 2, numPoints)];

        return GetCatmullRomPosition(0.5f, p0, p1, p2, p3);
    }

    private List<Vector3> InterpolatePoints(List<Vector3> points)
    {
        var newPoints = new List<Vector3>();
        for (int i = 0; i < points.Count; i++)
        {
            Vector3 p0 = points[ClampIndex(i - 1, points.Count)];
            Vector3 p1 = points[ClampIndex(i, points.Count)];
            Vector3 p2 = points[ClampIndex(i + 1, points.Count)];
            Vector3 p3 = points[ClampIndex(i + 2, points.Count)];
            newPoints.Add(p1);
            var curve = new BezierCurve(p0, p1, p2, p3);
            newPoints.AddRange(curve.GetRootPoints());
        }
        Gizmos.color = Color.gray;
        foreach (var point in newPoints)
            Gizmos.DrawSphere(point, 0.2f);
        return newPoints;
    }

    /*
    private void InterpolatePoints(List<Vector3> points)
    {
        int i = 0;
        var interpolatedPoints = new List<Vector3>();
        while (i < points.Count)
        {
            Vector3 midPoint = CalculateMidPoint(i, points);
            interpolatedPoints.Add(midPoint);
            i++;
        }
        for (i = 0; i < interpolatedPoints.Count; i++)
        {
            int pointsIndex = i * 2;
            points.Insert(pointsIndex, interpolatedPoints[i]);
        }
    }*/

    /// <summary>
    /// Display a spline between 2 points derived with the Catmull-Rom spline algorithm
    /// </summary>
    /// <param name="pos">Index of control point</param>
    private void CalculateCatmullRomPoints()
    {
        if (controlPoints.Count < 4)
            return;

        int startIndex = 0;
        int endIndex = controlPoints.Count;

        // Skip certain indices if not looping
        if (!isLooping)
        {
            startIndex = 1;
            endIndex -= 2;
        }

        float currentT = 0.0f;
        catmullRomPoints = new List<InterpolationPoint>();
        for (int i = startIndex; i < endIndex; i++)
        {
            
            var numControlPoints = controlPoints.Count;
            Vector3 p0 = controlPoints[ClampIndex(i - 1, numControlPoints)];
            Vector3 p1 = controlPoints[ClampIndex(i, numControlPoints)];
            Vector3 p2 = controlPoints[ClampIndex(i + 1, numControlPoints)];
            Vector3 p3 = controlPoints[ClampIndex(i + 2, numControlPoints)];

            int loops = Mathf.FloorToInt(1f / InterpolationResolution);
            for (int j = 1; j < loops; j++)
            {
                float t = j * InterpolationResolution;
                currentT += t;
                Vector3 newPoint = GetCatmullRomPosition(t, p0, p1, p2, p3);
                catmullRomPoints.Add(new InterpolationPoint(newPoint, currentT));
            }
        }
    }

    private void CalculateNormalizedPoints()
    {
        normalizedPoints = new List<InterpolationPoint>();

        float currentDistance = 0.0f;
        float pathDistance = 0.0f;

        float currentT = 0.0f;

        for (int i = 0; i < catmullRomPoints.Count; i++)
        {
            Vector3 point1 = catmullRomPoints[i].point;
            Vector3 point2 = catmullRomPoints[ClampIndex(i + 1, catmullRomPoints.Count)].point;

            float t1 = catmullRomPoints[i].t;
            float t2 = catmullRomPoints[ClampIndex(i + 1, catmullRomPoints.Count)].t;

            Vector3 direction = (point2 - point1);
            float magnitude = direction.magnitude;
            direction.Normalize();

            float nextDistance = pathDistance + magnitude;

            while (currentDistance < nextDistance)
            {
                currentDistance += LoftResolution;
                float distanceFromPoint1 = currentDistance - pathDistance;
                float progress = distanceFromPoint1 / magnitude;

                float tValue = Mathf.Lerp(t1, t2, progress) + currentT;
                Vector3 newPoint = point1 + (direction * distanceFromPoint1);

                normalizedPoints.Add(new InterpolationPoint(newPoint, tValue));
            }
            pathDistance += magnitude;
            currentT += t2 - t1;
        }
    }
}
