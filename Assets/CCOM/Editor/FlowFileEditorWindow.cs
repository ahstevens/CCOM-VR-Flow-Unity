using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class FlowFileEditorWindow : EditorWindow
{
    GameObject _obj = null;
    CCOM.Flow.FlowFile _flowFile;

    bool maintainParentAspect = true;

    [MenuItem("CCOM/Load Flow File")]
    public static void OpenWindow()
    {
        GetWindow<FlowFileEditorWindow>("Flow File Loader");
    }

    private void OnGUI()
    {
        //DirectoryInfo levelDirectoryPath = new DirectoryInfo(Application.dataPath);
        //FileInfo[] fileInfo = levelDirectoryPath.GetFiles("*.*", SearchOption.AllDirectories);
        GUILayout.BeginVertical("Box");

        if (GUILayout.Button("Test"))
        {
            LoadFlowGrid("test.fg", true);
            CreateObjects();
        }
        if (GUILayout.Button("Tidal Walls"))
        {
            LoadFlowGrid("tidalwalls.fg", true);
            CreateObjects();
        }
        if (GUILayout.Button("Lorenz"))
        {

            if (_obj != null)
            {
                Destroy(_obj);
            }

            _obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _obj.name = "Flow Grid";
            
            CCOM.Flow.LorenzAttractor la = _obj.AddComponent<CCOM.Flow.LorenzAttractor>();
            la.LoadMetadata();
            _flowFile = la;
            CreateObjects();
        }
        if (GUILayout.Button("Bedouin"))
        {
            if (_obj != null)
            {
                Destroy(_obj);
            }

            _obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _obj.name = "Flow Grid";
            
            CCOM.Flow.ThomasAttractor bf = _obj.AddComponent<CCOM.Flow.ThomasAttractor>();
            bf.LoadMetadata();
            _flowFile = bf;
            CreateObjects();
        }
        GUILayout.EndVertical();

        maintainParentAspect = GUILayout.Toggle(maintainParentAspect, "Match Flowgrid Extents to Parent");

        if (_flowFile != null)
        {
            Vector3 mins = _flowFile.GetMinBound();
            Vector3 maxs = _flowFile.GetMaxBound();
            Vector3 ranges = _flowFile.GetRange();
            GUILayout.Label("Flow Grid Info:");
            GUILayout.Label("X: [" + mins.x + ", " + maxs.x + "] = " + ranges.x);
            GUILayout.Label("Y: [" + mins.y + ", " + maxs.y + "] = " + ranges.y);            
            GUILayout.Label("Z: [" + mins.z + ", " + maxs.z + "] = " + ranges.z);

            if (_flowFile.GetType() == typeof(CCOM.Flow.FlowGrid))
            {
                CCOM.Flow.FlowGrid fg = _flowFile as CCOM.Flow.FlowGrid;
                GUILayout.Label(fg.GetTimestepCount() + (fg.GetTimestepCount() == 1 ? " timestep" : " timesteps"));
            }
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }

        void LoadFlowGrid(string filename, bool usesZInsteadOfDepth)
        {
            if (_obj != null)
            {
                Destroy(_obj);
            }

            _obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _obj.name = "Flow Grid";
            CCOM.Flow.FlowGrid fg = _obj.AddComponent<CCOM.Flow.FlowGrid>();
            fg._flowfile = Resources.Load(filename) as TextAsset;
            fg._usesZInsteadOfDepth = usesZInsteadOfDepth;
            fg.LoadMetadata();
            _flowFile = fg;
        }

        void CreateObjects()
        {
            Vector3 mins = _flowFile.GetMinBound();
            Vector3 ranges = _flowFile.GetRange();

            var tempMaterial = new Material(_obj.GetComponent<Renderer>().sharedMaterial);
            tempMaterial.color = Color.red;
            tempMaterial.shader = Shader.Find("Flip Normals");
            _obj.GetComponent<Renderer>().sharedMaterial = tempMaterial;
            _obj.GetComponent<Renderer>().receiveShadows = false;

            GameObject alignment = new GameObject("Alignment Layer");
            alignment.transform.SetParent(Selection.activeGameObject.transform);
            alignment.transform.localScale = Vector3.one;
            alignment.transform.localRotation = Quaternion.identity;


            GameObject coordChange = new GameObject("Coordinate Frame");
            coordChange.transform.SetParent(alignment.transform);
            coordChange.transform.localPosition = Vector3.zero;
            coordChange.transform.localRotation = Quaternion.identity;


            _obj.transform.SetParent(coordChange.transform);
            _obj.transform.localRotation = Quaternion.identity;


            coordChange.transform.localScale = Vector3.one / _flowFile.GetMaxDim();
            _obj.transform.localPosition = new Vector3(
                mins.x + ranges.x * 0.5f,
                mins.y + ranges.y * 0.5f,
                mins.z + ranges.z * 0.5f);

            if (maintainParentAspect)
            {
                float maxRange = Mathf.Max(ranges.x, ranges.y, ranges.z);
                _obj.transform.localScale = Vector3.one * maxRange;
                _flowFile._verticalExaggeration = maxRange / ranges.z;
            }
            else
            {
                _obj.transform.localScale = new Vector3(ranges.x, ranges.y, ranges.z);
            }

            _flowFile.RealignVerticalExaggeration();
        }
    }
}
