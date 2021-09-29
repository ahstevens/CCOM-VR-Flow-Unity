using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(HairySlice))]
public class HairySliceEditor : Editor
{
    int _nx, _ny;

    //public override void OnInspectorGUI()
    //{
    //    HairySlice mySlice = (HairySlice)target;
    //
    //    [Range(1,50)]
    //    mySlice.nx = EditorGUILayout.IntField("nx", mySlice.nx,);
    //    mySlice.ny = EditorGUILayout.IntField("ny", mySlice.ny);
    //}
}
