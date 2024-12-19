using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(SpriteRenderer))]
public class EnemySleepWakeController : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private Sprite sleepSprite;
    [SerializeField] private Sprite transitionSprite;
    [SerializeField] private Sprite awakeSprite;

    [Header("State Durations")]
    [SerializeField] private float sleepDuration = 3f;
    [SerializeField] private float transitionDuration = 0.5f;
    [SerializeField] private float awakeDuration = 2f;

    [Header("View Settings")]
    [SerializeField] private float viewRadius = 5f;
    [SerializeField] private float viewAngle = 90f;
    [SerializeField] private LayerMask targetMask;
    [SerializeField] private LayerMask obstacleMask;
    
    [Header("Direction")]
    [SerializeField] private float viewDirection = 0f;
    [SerializeField] private bool flipSpriteWithDirection = true;

    [Header("View Mesh")]
    [SerializeField] private MeshFilter viewMeshFilter;
    [SerializeField] private float meshResolution = 1f;
    [SerializeField] private int edgeResolveIterations = 4;
    [SerializeField] private float edgeDstThreshold = 0.5f;

    private enum EnemyState { Sleep, Transition, Awake }
    private EnemyState currentState = EnemyState.Sleep;
    private SpriteRenderer spriteRenderer;
    private Mesh viewMesh;
    private List<Transform> visibleTargets = new List<Transform>();
    private Coroutine stateCoroutine;
    private Coroutine detectionCoroutine;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        viewMesh = new Mesh { name = "View Mesh" };
        if (viewMeshFilter != null) viewMeshFilter.mesh = viewMesh;
        UpdateSpriteFlip();
    }

    private void OnEnable()
    {
        StartStateCycle();
    }

    private void OnDisable()
    {
        if (stateCoroutine != null) StopCoroutine(stateCoroutine);
        if (detectionCoroutine != null) StopCoroutine(detectionCoroutine);
    }

    private void LateUpdate()
    {
        if (currentState == EnemyState.Awake)
        {
            DrawFieldOfView();
        }
        else if (viewMesh != null)
        {
            viewMesh.Clear();
        }
    }

    private void UpdateSpriteFlip()
    {
        if (flipSpriteWithDirection)
        {
            spriteRenderer.flipX = viewDirection > 90 && viewDirection < 270;
        }
    }

    private void StartStateCycle()
    {
        if (stateCoroutine != null) StopCoroutine(stateCoroutine);
        stateCoroutine = StartCoroutine(StateCycleCoroutine());
        
        if (detectionCoroutine != null) StopCoroutine(detectionCoroutine);
        detectionCoroutine = StartCoroutine(FindTargetsWithDelay(0.2f));
    }

    private IEnumerator StateCycleCoroutine()
    {
        while (enabled)
        {
            // Sleep State
            currentState = EnemyState.Sleep;
            spriteRenderer.sprite = sleepSprite;
            yield return new WaitForSeconds(sleepDuration);

            // Transition State
            currentState = EnemyState.Transition;
            spriteRenderer.sprite = transitionSprite;
            yield return new WaitForSeconds(transitionDuration);

            // Awake State
            currentState = EnemyState.Awake;
            spriteRenderer.sprite = awakeSprite;
            yield return new WaitForSeconds(awakeDuration);
        }
    }

    private IEnumerator FindTargetsWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            if (currentState == EnemyState.Awake)
            {
                FindVisibleTargets();
            }
            else
            {
                visibleTargets.Clear();
            }
        }
    }

    private void FindVisibleTargets()
    {
        visibleTargets.Clear();
        var targetsInRadius = Physics2D.OverlapCircleAll(transform.position, viewRadius, targetMask);

        foreach (var target in targetsInRadius)
        {
            Vector2 dirToTarget = (target.transform.position - transform.position).normalized;
            Vector2 viewDir = DirFromAngle(0);

            float angleToTarget = Vector2.Angle(viewDir, dirToTarget);

            if (angleToTarget < viewAngle / 2)
            {
                float distToTarget = Vector2.Distance(transform.position, target.transform.position);
                if (!Physics2D.Raycast(transform.position, dirToTarget, distToTarget, obstacleMask))
                {
                    visibleTargets.Add(target.transform);
                    GameManager.instance.detections++;
                    
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
            float angle = -viewAngle / 2 + stepAngleSize * i;
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
        float finalAngle = viewDirection + angleInDegrees;
        float angleInRadians = finalAngle * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));
    }

    public void SetViewDirection(float newDirection)
    {
        viewDirection = newDirection;
        UpdateSpriteFlip();
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

#if UNITY_EDITOR
    [CustomEditor(typeof(EnemySleepWakeController))]
    public class EnemySleepWakeControllerEditor : Editor
    {
        private void OnSceneGUI()
        {
            EnemySleepWakeController controller = (EnemySleepWakeController)target;
            Transform transform = controller.transform;
            
            // Draw direction handle
            Vector3 viewAngleA = controller.DirFromAngle(-controller.viewAngle / 2);
            Vector3 viewAngleB = controller.DirFromAngle(controller.viewAngle / 2);

            Handles.color = Color.white;
            Vector3 direction = controller.DirFromAngle(0);
            Vector3 handlePos = transform.position + (Vector3)direction * 2f;
            
            EditorGUI.BeginChangeCheck();
            Vector3 newHandlePos = Handles.DoPositionHandle(handlePos, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Vector2 newDirection = (newHandlePos - transform.position).normalized;
                float angle = Mathf.Atan2(newDirection.y, newDirection.x) * Mathf.Rad2Deg;
                controller.SetViewDirection(angle);
                EditorUtility.SetDirty(target);
            }

            // Draw direction visualization
            Handles.color = Color.yellow;
            Handles.DrawWireArc(transform.position, Vector3.forward, Vector3.right, 360, 0.5f);
            Handles.DrawLine(transform.position, transform.position + (Vector3)direction * 2f);
        }
    }
#endif
}