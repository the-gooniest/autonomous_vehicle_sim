using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadCurve {

    public RoadPoint endPoint1, endPoint2;
    public RoadControlPoint controlPoint1, controlPoint2;

    public RoadCurve(RoadPoint endPoint1, RoadPoint endPoint2)
    {
        this.endPoint1 = endPoint1;
        this.endPoint2 = endPoint2;
        controlPoint1 = endPoint1.ctrlPoint2;
        controlPoint2 = endPoint2.ctrlPoint1;
    }

    public BezierCurve GetCurve()
    {
        return new BezierCurve(
            endPoint1.transform.position,
            controlPoint1.GetScaledControlPoint(),
            controlPoint2.GetScaledControlPoint(),
            endPoint2.transform.position);
    }

    public void DrawCurve()
    {
        var curve = GetCurve();
        curve.DrawCurve(Color.blue);
    }

    public void DrawCurve(float offset)
    {
        var originalCurve = new BezierCurve(
             endPoint1.transform.position,
             controlPoint1.GetScaledControlPoint(),
             controlPoint2.GetScaledControlPoint(),
             endPoint2.transform.position);
        var curves = originalCurve.GetOffsetCurves(offset);
        foreach (var curve in curves)
            curve.DrawControlPoints();
    }
}
