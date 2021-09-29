using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddSlice : MonoBehaviour
{
    GameObject slice;
    // Start is called before the first frame update
    void Start()
    {
        slice = new GameObject("Hairy Slice");
        slice.AddComponent<HairySlice>();
        slice.AddComponent<SliceInteractor>();
        slice.AddComponent<MeshCollider>().convex = true;

        CCOM.Flow.FlowFile flowFile = gameObject.transform.root.GetComponentInChildren<CCOM.Flow.FlowFile>();
                
        slice.transform.SetParent(flowFile.transform.parent);
        slice.transform.localRotation = Quaternion.identity;
        slice.transform.localScale = new Vector3(flowFile.GetMaxBound().x, flowFile.GetMaxBound().y, 1);
        Vector3 pos = flowFile.GetMidpoint();
        pos.z *= flowFile._verticalExaggeration;
        slice.transform.localPosition = pos;

        Destroy(this);
    }
}
