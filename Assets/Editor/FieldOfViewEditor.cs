using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FieldOfView))]
public class FieldOfViewEditor : Editor
{
    private void OnSceneGUI()
    {
        var fov = (FieldOfView)target;
        Handles.color = Color.white;
        Handles.DrawWireArc(fov.transform.position, Vector3.forward, Vector3.up, 360, fov.viewRadius);
        var viewAngleA = fov.DirFromAngle(-fov.viewAngle / 2, false);
        var viewAngleB = fov.DirFromAngle(fov.viewAngle / 2, false);
        
        Handles.DrawLine(fov.transform.position, fov.transform.position + (Vector3)viewAngleA * fov.viewRadius);
        Handles.DrawLine(fov.transform.position, fov.transform.position + (Vector3)viewAngleB * fov.viewRadius);

        Handles.color = Color.red;
        foreach (var visibleTarget in fov.visibleTargets) 
        {
            Handles.DrawLine(fov.transform.position, visibleTarget.position);
        }
        
        Handles.color = Color.green;
        // Draw the player's forward vector
        Handles.DrawLine(fov.transform.position, fov.transform.position + fov.transform.up * 100);
    }
}