using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class HairySliceEditorWindow : EditorWindow
{
    [MenuItem("CCOM/Hairy Slices")]
    public static void OpenWindow()
    {
        GetWindow<HairySliceEditorWindow>("Hairy Slice Editor");
    }

    bool groupEnabled;
    float jitter = 0.25f;
    private void OnGUI()
    {
        //This is the Label for the Slider
        //GUI.Label(new Rect(0, 300, 100, 30), "Rectangle Width");
        //This is the Slider that changes the size of the Rectangle drawn
        //m_Value = GUI.HorizontalSlider(new Rect(100, 300, 100, 30), m_Value, 1.0f, 250.0f);

        //The rectangle is drawn in the Editor (when MyScript is attached) with the width depending on the value of the Slider
        //EditorGUI.DrawRect(new Rect(0, 350, m_Value, 70), Color.green);

        if (GUILayout.Button("Add Hairy Slice to Selection"))
        {
            var selectedObj = Selection.activeGameObject;

            GameObject slice = GameObject.CreatePrimitive(PrimitiveType.Quad);
            slice.name = "Hairy Slice";
            slice.AddComponent<HairySlice>();
            slice.AddComponent<SliceInteractor>();
            MeshCollider collider = slice.GetComponent<MeshCollider>();
            collider.convex = true;

            CCOM.Flow.FlowFile flowFile = selectedObj.transform.root.GetComponentInChildren<CCOM.Flow.FlowFile>();

            flowFile.LoadMetadata();

            slice.transform.SetParent(flowFile.gameObject.transform.parent);
            slice.transform.localRotation = Quaternion.identity;
            slice.transform.localScale = new Vector3(flowFile.GetMaxBound().x, flowFile.GetMaxBound().y, 1);
            Vector3 pos = flowFile.GetMidpoint();
            pos.z *= flowFile._verticalExaggeration;
            slice.transform.localPosition = pos;
        }

        groupEnabled = EditorGUILayout.BeginToggleGroup("Optional Settings", groupEnabled);
        jitter = EditorGUILayout.Slider("Jitter Amount", jitter, 0f, 1f);
        if (GUILayout.Button("Jitter Selected Slice"))
        {
            HairySlice slice = Selection.activeGameObject.GetComponent<HairySlice>();
            slice.JitterHair(jitter);
        }

        if (GUILayout.Button("Screenshot"))
        {
            ScreenCapture.CaptureScreenshot("FlowShot.png", 4);
        }
        EditorGUILayout.EndToggleGroup();
    }
}
