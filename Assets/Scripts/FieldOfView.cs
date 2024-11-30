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

    private void Start()
    {
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

    public Vector2 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.z;
        }

        var angleInRadians = (angleInDegrees + 90) * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));
    }
}