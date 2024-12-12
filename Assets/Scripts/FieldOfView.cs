using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    public float viewRadius;
    [Range(0, 360)] public float viewAngle;

    public LayerMask targetMask;
    public LayerMask obstacleMask;

    [HideInInspector] public List<Transform> visibleTargets = new();

    public float meshResolution;
    public int edgeResolveIterations;
    public float edgeDstThreshold;

    public MeshFilter viewMeshFilter;
    private Mesh _viewMesh;

    private void Start()
    {
        _viewMesh = new Mesh { name = "View Mesh" };
        viewMeshFilter.mesh = _viewMesh;

        StartCoroutine(FindTargetsWithDelay(.2f));
    }

    IEnumerator FindTargetsWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            FindVisibleTargets();
        }
    }

    private void LateUpdate()
    {
        DrawFieldOfView();
    }

    private void FindVisibleTargets()
    {
        visibleTargets.Clear();
        var targetsInViewRadius = Physics2D.OverlapCircleAll(transform.position, viewRadius, targetMask);
        foreach (var target in targetsInViewRadius)
        {
            var dirToTarget = (target.transform.position - transform.position).normalized;
            if (Vector2.Angle(transform.up, dirToTarget) < viewAngle / 2)
            {
                var dstToTarget = Vector2.Distance(transform.position, target.transform.position);
                if (!Physics2D.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask))
                {
                    visibleTargets.Add(target.transform);
                }
            }
        }
    }

    private void DrawFieldOfView()
    {
        int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
        float stepAngleSize = viewAngle / stepCount;

        List<Vector2> viewPoints = new();
        ViewCastInfo oldViewCast = new ViewCastInfo();
        for (int i = 0; i <= stepCount; i++)
        {
            float angle = transform.eulerAngles.z - viewAngle / 2 + stepAngleSize * i;
            ViewCastInfo newViewCast = ViewCast(angle);

            if (i > 0)
            {
                bool edgeDstThresholdExceeded = Mathf.Abs(oldViewCast.Dst - newViewCast.Dst) > edgeDstThreshold;
                if (oldViewCast.Hit != newViewCast.Hit ||
                    (oldViewCast.Hit && newViewCast.Hit && edgeDstThresholdExceeded))
                {
                    EdgeInfo edge = FindEdge(oldViewCast, newViewCast);
                    if (edge.PointA != Vector3.zero)
                    {
                        viewPoints.Add(edge.PointA);
                    }
                }
            }

            viewPoints.Add(newViewCast.Point);
            oldViewCast = newViewCast;
            // var newVector = DirFromAngle(angle, true);
            // Debug.DrawRay(transform.position, newVector * viewRadius, Color.red);
        }

        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector3.zero;
        for (var i = 0; i < vertexCount - 1; i++)
        {
            vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]);
            if (i >= vertexCount - 2) continue;
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        _viewMesh.Clear();
        _viewMesh.vertices = vertices;
        _viewMesh.triangles = triangles;
        _viewMesh.RecalculateNormals();
    }

    EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast)
    {
        float minAngle = minViewCast.Angle;
        float maxAngle = maxViewCast.Angle;
        Vector2 minPoint = Vector2.zero;
        Vector2 maxPoint = Vector2.zero;

        for (int i = 0; i < edgeResolveIterations; i++)
        {
            float angle = (minAngle + maxAngle) / 2;
            ViewCastInfo newViewCast = ViewCast(angle);

            bool edgeDstThresholdExceeded = Mathf.Abs(minViewCast.Dst - newViewCast.Dst) > edgeDstThreshold;
            if (newViewCast.Hit == minViewCast.Hit && !edgeDstThresholdExceeded)
            {
                minAngle = angle;
                minPoint = newViewCast.Point;
            }
            else
            {
                maxAngle = angle;
                maxPoint = newViewCast.Point;
            }
        }

        return new EdgeInfo(minPoint, maxPoint);
    }

    private ViewCastInfo ViewCast(float globalAngle)
    {
        Vector3 dir = DirFromAngle(globalAngle, true);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, viewRadius, obstacleMask);
        return hit.collider != null
            ? new ViewCastInfo(true, hit.point, hit.distance, globalAngle)
            : new ViewCastInfo(false, transform.position + dir * viewRadius, viewRadius, globalAngle);
    }

    public Vector2 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.z;
        }

        var angleInRadians = (angleInDegrees + 90) * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));
    }

    public struct ViewCastInfo
    {
        public bool Hit;
        public Vector2 Point;
        public float Dst;
        public float Angle;

        public ViewCastInfo(bool hit, Vector3 point, float dst, float angle)
        {
            Hit = hit;
            Point = point;
            Dst = dst;
            Angle = angle;
        }
    }

    public struct EdgeInfo
    {
        public Vector3 PointA;
        public Vector3 PointB;

        public EdgeInfo(Vector3 pointA, Vector3 pointB)
        {
            PointA = pointA;
            PointB = pointB;
        }
    }
}