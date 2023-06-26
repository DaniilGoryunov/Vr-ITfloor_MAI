using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using RoomArchitectEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(RoomArchitect), true)]
public class RoomArchitectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        RoomArchitect myTarget = (RoomArchitect)target;
        if (GUILayout.Button("generate"))
        {
            myTarget.Build();
        }
        if (GUILayout.Button("clear"))
        {
            myTarget.clear();
        }
        base.OnInspectorGUI();
    }
}

[CustomEditor(typeof(RoofStyle), true)]
public class RoofStyleEditor : Editor
{
    public override void OnInspectorGUI()
    {
        RoofStyle myTarget = (RoofStyle)target;
        GUILayout.BeginHorizontal("box");
        GUILayout.FlexibleSpace();
        GUILayout.Label("This module override code based\nroof settings. setRoofStyle()\nis no longer taken into account", GUILayout.ExpandWidth(false));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        base.OnInspectorGUI();
    }
}

#endif
