using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DrobSpawner))]
public class DrobSpawnerEditor : Editor
{
    private void OnSceneGUI()
    {
        DrobSpawner drobSpawner = target as DrobSpawner;
        for (int i = 0; i < drobSpawner.path.Count; i++)
        {
            for (int j = 0; j < drobSpawner.path[i].pathPoints.Count; j++)
            {
                if (i > 0 && j == 0)
                {
                    drobSpawner.path[i].pathPoints[j] = drobSpawner.path[i-1].pathPoints[drobSpawner.path[i - 1].pathPoints.Count - 1];
                    continue;
                }

                Vector3 point = drobSpawner.path[i].pathPoints[j];
                EditorGUI.BeginChangeCheck();
                Vector3 newPoint = Handles.PositionHandle(drobSpawner.transform.TransformPoint(point), drobSpawner.transform.rotation);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(drobSpawner, "Change Look At Target Position");
                    drobSpawner.path[i].pathPoints[j] = drobSpawner.transform.InverseTransformPoint(newPoint);
                }
            }
        }
    }
}
