using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(SpriteRenderer))]
public class EyeEnemyViewController : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private Sprite[] directionSprites;
    [SerializeField] private float animationInterval = 0.3f;
    [SerializeField] private bool clockwise = false;
    [SerializeField] private float degreesPerSprite = 30f;

    [Header("View Settings")]
    [SerializeField] private float viewRadius = 5f;
    [SerializeField] private float viewAngle = 90f;
    [SerializeField] private LayerMask targetMask;
    [SerializeField] private LayerMask obstacleMask;

    // For mesh creation
    [SerializeField] private MeshFilter viewMeshFilter;
    [SerializeField] private float meshResolution = 1f;
    [SerializeField] private int edgeResolveIterations = 4;
    [SerializeField] private float edgeDstThreshold = 0.5f;

    private SpriteRenderer spriteRenderer;
    private Mesh viewMesh;
    private int currentSpriteIndex = 0;
    private Coroutine animationCoroutine;
    private List<Transform> visibleTargets = new List<Transform>();
    private float currentViewAngle = 0f;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        viewMesh = new Mesh { name = "View Mesh" };
        if (viewMeshFilter != null) viewMeshFilter.mesh = viewMesh;
    }

    private void OnEnable()
    {
        if (directionSprites == null || directionSprites.Length == 0)
        {
            Debug.LogError("No sprites assigned to EnemyViewController!");
            enabled = false;
            return;
        }

        StartAnimationCycle();
        StartCoroutine(FindTargetsWithDelay(0.2f));
    }

    private void OnDisable()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }
    }

    private void LateUpdate()
    {
        DrawFieldOfView();
    }

    private void StartAnimationCycle()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        animationCoroutine = StartCoroutine(AnimationCycleCoroutine());
    }

    private IEnumerator AnimationCycleCoroutine()
    {
        while (enabled)
        {
            // Update sprite
            if (directionSprites[currentSpriteIndex] != null)
            {
                spriteRenderer.sprite = directionSprites[currentSpriteIndex];
                
                // Update view angle based on current sprite
                float angleOffset = currentSpriteIndex * degreesPerSprite;
                currentViewAngle = clockwise ? -angleOffset + 180 : angleOffset + 180;
            }

            // Move to next sprite
            currentSpriteIndex = (currentSpriteIndex + 1) % directionSprites.Length;

            yield return new WaitForSeconds(animationInterval);
        }
    }

    private IEnumerator FindTargetsWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            FindVisibleTargets();
        }
    }

    private void FindVisibleTargets()
    {
        visibleTargets.Clear();
        var targetsInRadius = Physics2D.OverlapCircleAll(transform.position, viewRadius, targetMask);

        foreach (var target in targetsInRadius)
        {
            Vector2 dirToTarget = (target.transform.position - transform.position).normalized;
            Vector2 viewDirection = DirFromAngle(currentViewAngle);

            float angleToTarget = Vector2.Angle(viewDirection, dirToTarget);

            if (angleToTarget < viewAngle / 2)
            {
                float distToTarget = Vector2.Distance(transform.position, target.transform.position);
                if (!Physics2D.Raycast(transform.position, dirToTarget, distToTarget, obstacleMask))
                {
                    visibleTargets.Add(target.transform);
                    GameManager.instance.PlayerDetected();
                }
            }
        }
    }

    private void DrawFieldOfView()
    {
        int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
        float stepAngleSize = viewAngle / stepCount;

        List<Vector2> viewPoints = new List<Vector2>();
        ViewCastInfo oldViewCast = new ViewCastInfo();

        for (int i = 0; i <= stepCount; i++)
        {
            float angle = currentViewAngle - viewAngle / 2 + stepAngleSize * i;
            ViewCastInfo newViewCast = ViewCast(angle);

            if (i > 0)
            {
                bool edgeDstThresholdExceeded = Mathf.Abs(oldViewCast.dst - newViewCast.dst) > edgeDstThreshold;
                if (oldViewCast.hit != newViewCast.hit || (oldViewCast.hit && newViewCast.hit && edgeDstThresholdExceeded))
                {
                    EdgeInfo edge = FindEdge(oldViewCast, newViewCast);
                    if (edge.pointA != Vector2.zero)
                    {
                        viewPoints.Add(edge.pointA);
                    }
                }
            }

            viewPoints.Add(newViewCast.point);
            oldViewCast = newViewCast;
        }

        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector3.zero;
        for (int i = 0; i < vertexCount - 1; i++)
        {
            vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]);

            if (i < vertexCount - 2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }

        viewMesh.Clear();
        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();
    }

    private EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast)
    {
        float minAngle = minViewCast.angle;
        float maxAngle = maxViewCast.angle;
        Vector2 minPoint = minViewCast.point;
        Vector2 maxPoint = maxViewCast.point;

        for (int i = 0; i < edgeResolveIterations; i++)
        {
            float angle = (minAngle + maxAngle) / 2;
            ViewCastInfo newViewCast = ViewCast(angle);

            bool edgeDstThresholdExceeded = Mathf.Abs(minViewCast.dst - newViewCast.dst) > edgeDstThreshold;
            if (newViewCast.hit == minViewCast.hit && !edgeDstThresholdExceeded)
            {
                minAngle = angle;
                minPoint = newViewCast.point;
            }
            else
            {
                maxAngle = angle;
                maxPoint = newViewCast.point;
            }
        }

        return new EdgeInfo(minPoint, maxPoint);
    }

    private ViewCastInfo ViewCast(float angle)
    {
        Vector2 dir = DirFromAngle(angle);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, viewRadius, obstacleMask);

        if (hit.collider != null)
        {
            return new ViewCastInfo(true, hit.point, hit.distance, angle);
        }
        
        Vector2 point = (Vector2)transform.position + dir * viewRadius;
        return new ViewCastInfo(false, point, viewRadius, angle);
    }

    private Vector2 DirFromAngle(float angleInDegrees)
    {
        angleInDegrees += 90; // Adjust to make 0 degrees point upward
        float angleInRadians = angleInDegrees * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));
    }

    private struct ViewCastInfo
    {
        public bool hit;
        public Vector2 point;
        public float dst;
        public float angle;

        public ViewCastInfo(bool _hit, Vector2 _point, float _dst, float _angle)
        {
            hit = _hit;
            point = _point;
            dst = _dst;
            angle = _angle;
        }
    }

    private struct EdgeInfo
    {
        public Vector2 pointA;
        public Vector2 pointB;

        public EdgeInfo(Vector2 _pointA, Vector2 _pointB)
        {
            pointA = _pointA;
            pointB = _pointB;
        }
    }
}