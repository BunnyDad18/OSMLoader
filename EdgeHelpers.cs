using UnityEngine;

public static class EdgeHelpers
{
    public static bool CheckIntersect(Vector3 vectorA, Vector3 vectorB, Vector3 vector1, Vector3 vector2, out Vector3 point)
    {
        point = Vector3.zero;
        float A1 = vectorB.z - vectorA.z;
        float B1 = vectorA.x - vectorB.x;
        float C1 = A1 * vectorA.x + B1 * vectorA.z;


        float A2 = vector2.z - vector1.z;
        float B2 = vector1.x - vector2.x;
        float C2 = A2 * vector1.x + B2 * vector1.z;

        float delta = (A1 * B2) - (A2 * B1);
        if (delta == 0) return false;
        float x = (B2 * C1 - B1 * C2) / delta;
        float y = (A1 * C2 - A2 * C1) / delta;
        point = new Vector3(x, 0, y);
        return (OnLine(vectorA, vectorB, x, y) && OnLine(vector1, vector2, x, y));
    }

    private static bool OnLine(Vector3 vectorA, Vector3 vectorB, float x, float y)
    {
        float offset = .001f;
        float smallX = vectorA.x < vectorB.x ? vectorA.x : vectorB.x;
        if (x + offset < smallX) return false;
        float bigX = vectorA.x > vectorB.x ? vectorA.x : vectorB.x;
        if (x - offset > bigX) return false;
        float smallY = vectorA.z < vectorB.z ? vectorA.z : vectorB.z;
        if (y + offset < smallY) return false;
        float bigY = vectorA.z > vectorB.z ? vectorA.z : vectorB.z;
        if (y - offset > bigY) return false;
        return true;
    }
}