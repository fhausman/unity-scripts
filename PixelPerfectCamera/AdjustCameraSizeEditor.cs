using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AdjustCameraSize))]
public class AdjustCameraSizeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var script = (AdjustCameraSize)target;
        if(GUILayout.Button("Adjust camera to native resolution"))
        {
            script.AdjustCameraObjects();
        }
    }
}
