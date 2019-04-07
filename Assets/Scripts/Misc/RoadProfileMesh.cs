using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrientedPoint {
    
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 Normal { get { return rotation * Vector3.up; } }

    public OrientedPoint(Vector3 point, Vector3 direction)
    {
        position = point;
        rotation = Quaternion.FromToRotation(Vector3.forward, direction);
    }
}

public class RoadProfileMesh {

    public class ExtrudePoint
    {
        public Vector3 normal;
        public float uXCoord;

        public ExtrudePoint(Vector3 normal, float uXCoord)
        {
            this.normal = normal;
            this.uXCoord = uXCoord;
        }
    }

    public Vector3[] points;

    public RoadProfileMesh() {
        points = new Vector3[4];
        points[0] = new Vector3(1, 0, 0);
        points[1] = new Vector3(-1, 0, 0);
        points[2] = new Vector3(-1, -0.1f, 0);
        points[3] = new Vector3(1, -0.1f, 0);
    }

    public Vector3 TransformedPoint(int index, Vector3 direction, float scale = 8.0f)
    {
        Vector3 point = points[index];
        var rotation = Quaternion.FromToRotation(Vector3.forward, direction);
        Vector3 angles = rotation.eulerAngles;
        rotation = Quaternion.Euler(angles.x, angles.y, 0.0f);
        return rotation * point * scale;
    }

    public Vector3 TransformedNormal(int index, Vector3 direction)
    {
        var rotation = Quaternion.FromToRotation(Vector3.forward, direction);
        return rotation * points[index].normalized;
    }
}
