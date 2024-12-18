using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(SpriteRenderer))]
public class GridChaserEnemyController : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private Sprite upSprite;
    [SerializeField] private Sprite downSprite;
    [SerializeField] private Sprite sideSprite;

    [Header("Behavior Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float meanness = 0.5f;
    [SerializeField] private float moveInterval = 1f;
    [SerializeField] private bool canMoveDiagonally = false;

    [Header("View Settings")]
    [SerializeField] private float viewRadius = 5f;
    [SerializeField] private float viewAngle = 90f;
    [SerializeField] private LayerMask targetMask;
    [SerializeField] private LayerMask obstacleMask;

    [Header("View Mesh")]
    [SerializeField] private MeshFilter viewMeshFilter;
    [SerializeField] private float meshResolution = 1f;
    [SerializeField] private int edgeResolveIterations = 4;
    [SerializeField] private float edgeDstThreshold = 0.5f;

    private SpriteRenderer spriteRenderer;
    private Mesh viewMesh;
    private Transform currentTarget;
    private Vector2 currentDirection;
    private Vector2 gridPosition;
    private bool isMoving = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        viewMesh = new Mesh { name = "View Mesh" };
        if (viewMeshFilter != null) viewMeshFilter.mesh = viewMesh;
        gridPosition = new Vector2(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y));
        transform.position = new Vector3(gridPosition.x, gridPosition.y, transform.position.z);
    }

    private void OnEnable()
    {
        StartCoroutine(MovementCoroutine());
        StartCoroutine(FindTargetsWithDelay(0.2f));
    }

    private void LateUpdate()
    {
        DrawFieldOfView();
    }

    private IEnumerator MovementCoroutine()
    {
        while (enabled)
        {
            if (!isMoving)
            {
                Vector2 nextMove = DecideNextMove();
                if (nextMove != Vector2.zero)
                {
                    StartCoroutine(MoveToNextPosition(nextMove));
                }
            }
            yield return new WaitForSeconds(moveInterval);
        }
    }

    private Vector2 DecideNextMove()
    {
        List<Vector2> possibleMoves = GetPossibleMoves();
        if (possibleMoves.Count == 0) return Vector2.zero;

        if (currentTarget != null && Random.value < meanness)
        {
            // Try to move towards target
            Vector2 directionToTarget = ((Vector2)currentTarget.position - gridPosition).normalized;
            Vector2 bestMove = possibleMoves[0];
            float bestDistance = float.MaxValue;

            foreach (Vector2 move in possibleMoves)
            {
                float distance = Vector2.Distance(gridPosition + move, currentTarget.position);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestMove = move;
                }
            }
            return bestMove;
        }
        else
        {
            // Random movement
            return possibleMoves[Random.Range(0, possibleMoves.Count)];
        }
    }

    private List<Vector2> GetPossibleMoves()
    {
        List<Vector2> possibleMoves = new List<Vector2>();
        Vector2[] directions = canMoveDiagonally ? 
            new Vector2[] { Vector2.up, Vector2.down, Vector2.left, Vector2.right, 
                          new Vector2(1, 1), new Vector2(-1, 1), new Vector2(1, -1), new Vector2(-1, -1) } :
            new Vector2[] { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

        foreach (Vector2 dir in directions)
        {
            Vector2 newPos = gridPosition + dir;
            if (!Physics2D.CircleCast(gridPosition, 0.4f, dir, 1f, obstacleMask))
            {
                possibleMoves.Add(dir);
            }
        }

        return possibleMoves;
    }

    private IEnumerator MoveToNextPosition(Vector2 moveDirection)
    {
        isMoving = true;
        Vector2 startPos = gridPosition;
        Vector2 targetPos = gridPosition + moveDirection;
        float elapsedTime = 0;

        // Update sprite based on movement direction
        if (moveDirection.y > 0)
        {
            spriteRenderer.sprite = upSprite;
            spriteRenderer.flipX = false;
        }
        else if (moveDirection.y < 0)
        {
            spriteRenderer.sprite = downSprite;
            spriteRenderer.flipX = false;
        }
        else
        {
            spriteRenderer.sprite = sideSprite;
            spriteRenderer.flipX = moveDirection.x < 0;
        }

        while (elapsedTime < moveInterval)
        {
            float t = elapsedTime / moveInterval;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        gridPosition = targetPos;
        transform.position = new Vector3(gridPosition.x, gridPosition.y, transform.position.z);
        isMoving = false;
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
        currentTarget = null;
        var targetsInRadius = Physics2D.OverlapCircleAll(transform.position, viewRadius, targetMask);

        foreach (var target in targetsInRadius)
        {
            Vector2 dirToTarget = (target.transform.position - transform.position).normalized;
            float angleToTarget = Vector2.Angle(Vector2.right, dirToTarget);

            if (angleToTarget < viewAngle / 2)
            {
                float distToTarget = Vector2.Distance(transform.position, target.transform.position);
                if (!Physics2D.Raycast(transform.position, dirToTarget, distToTarget, obstacleMask))
                {
                    currentTarget = target.transform;
                    break;
                }
            }
        }
    }

    private void DrawFieldOfView()
    {
        int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
        float stepAngleSize = viewAngle / stepCount;
        List<Vector2> viewPoints = new List<Vector2>();

        for (int i = 0; i <= stepCount; i++)
        {
            float angle = transform.eulerAngles.z - viewAngle / 2 + stepAngleSize * i;
            Vector2 direction = DirFromAngle(angle);
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, viewRadius, obstacleMask);

            if (hit.collider != null)
                viewPoints.Add(hit.point);
            else
                viewPoints.Add((Vector2)transform.position + direction * viewRadius);
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

    private Vector2 DirFromAngle(float angleInDegrees)
    {
        float angleInRadians = angleInDegrees * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.4f);
        Gizmos.color = Color.red;
        foreach (Vector2 move in GetPossibleMoves())
        {
            Gizmos.DrawLine(transform.position, (Vector2)transform.position + move);
        }
    }
#endif
}