using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TubeRendererInternals;

public class HairySlice : MonoBehaviour
{
    GameObject _root;
    GameObject _alignment;
    GameObject _coordFrame;
    GameObject _slice;
    GameObject[] hairs = null;
    TubeRenderer[] _tubes;
    public CCOM.Flow.FlowFile _flowFile;

    public bool rebuild = false;
    public int nx = 10;
    public int ny = 10;
    public int pointCount = 10;
    public float stepSize = 0.1f;
    public bool useRungeKutta4 = true;
    public bool colorByVelocity = true;
    public float _startRadius = 1f;
    public float _endRadius = 0f;
    public bool _animateTexture = false;
    public float _animationMultiplier = 1f;

    float lastStepSize;
    bool usedRK4;
    bool usedColor;

    float _xSpacing;
    float _ySpacing;

    bool _needsUpdate = true;

    public Material _mainMat;

    // Start is called before the first frame update
    void Start()
    {
        _root = gameObject.transform.parent.parent.parent.gameObject;
        _alignment = gameObject.transform.parent.parent.gameObject;
        _coordFrame = gameObject.transform.parent.gameObject;
        _slice = gameObject;

        lastStepSize = stepSize;
        usedRK4 = useRungeKutta4;
        usedColor = colorByVelocity;

        if (_flowFile == null)
        {
            _flowFile = gameObject.transform.parent.GetComponentInChildren<CCOM.Flow.FlowFile>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        nx = Mathf.Clamp(nx, 1, 100);
        ny = Mathf.Clamp(ny, 1, 100);
        pointCount = Mathf.Clamp(pointCount, 2, 1000);

        if (hairs == null || 
            hairs.Length != nx * ny ||
            _tubes[0].points.Length != pointCount ||
            usedColor != colorByVelocity ||
            rebuild)
        {
            usedColor = colorByVelocity;

            if (hairs != null && hairs.Length > 0)
            {
                foreach (GameObject hair in hairs)
                {
                    Destroy(hair);
                }
            }

            MakeHair();

            rebuild = false;
        }
        
        if (transform.hasChanged)
        {
            _needsUpdate = true;
            transform.hasChanged = false;
        }

        if (_flowFile.HasChanged())
            _needsUpdate = true;

        if (lastStepSize != stepSize)
        {
            _needsUpdate = true;
            lastStepSize = stepSize;
        }

        if (usedRK4 != useRungeKutta4)
        {
            _needsUpdate = true;
            usedRK4 = useRungeKutta4;
        }

        // ONLY UPDATE EVERY 3 FRAMES TO KEEP THINGS REASONABLE
        // Doing batch updates would probably be better -- Coroutines
        //if (Time.frameCount % 3 != 0)
        //    return;

        if (_animateTexture)
        {
            Vector2 offset = _mainMat.mainTextureOffset;
            offset.x = (offset.x - Time.deltaTime * _animationMultiplier) % 1;
            _mainMat.mainTextureOffset = offset;
        }


        if (_needsUpdate && _flowFile.IsLoaded())
        {
            Matrix4x4 sliceToFlowgrid = Matrix4x4.Scale(new Vector3(1f, 1f, 1f / (_flowFile._verticalExaggeration))) *
                    Matrix4x4.TRS(_slice.transform.localPosition, _slice.transform.localRotation, _slice.transform.localScale);

            for (int i = 0; i < hairs.Length; ++i)
            {
                GameObject thisHair = hairs[i];

                thisHair.transform.localScale = new Vector3(
                    (_flowFile._verticalExaggeration / _slice.transform.localScale.x),
                    (_flowFile._verticalExaggeration / _slice.transform.localScale.y),
                    (_flowFile._verticalExaggeration / _slice.transform.localScale.z));

                Vector4 seedPosLocal = Vector4.zero;
                seedPosLocal.w = 1f;

                Matrix4x4 xform = sliceToFlowgrid * Matrix4x4.TRS(thisHair.transform.localPosition, thisHair.transform.localRotation, thisHair.transform.localScale);

                Matrix4x4 xformInv = xform.inverse;

                Vector3 seedPos = xform * seedPosLocal;
                if (_flowFile.InBounds(seedPos))
                {
                    thisHair.SetActive(true);

                    // Start sampling at the seed position
                    Vector3 samplePos = seedPos;

                    //Vector3[] streamline = new Vector3[pointCount];
                    Vector3[] streamline = _tubes[i].points;
                    Color32[] colors = _tubes[i].colors;
                    for (int j = 0; j < pointCount; ++j)
                    {
                        streamline[j] = xformInv * new Vector4(samplePos.x, samplePos.y, samplePos.z, 1f);

                        Vector3 uvw = _flowFile.Sample(samplePos);

                        Vector3 nextPos = useRungeKutta4 ? rk4(samplePos, stepSize) : euler(samplePos, stepSize);

                        if (colorByVelocity && _flowFile.GetVelocityRange() != 0f)
                        {
                            float magNorm = (uvw.magnitude - _flowFile.GetMinVelocity()) / _flowFile.GetVelocityRange();
                            colors[j] = Color.HSVToRGB(magNorm, 1f, 1f, false);
                        }
                        else // make it transparent
                        {
                            colors[j] = new Color32(255, 255, 255, 0);
                        }

                        if (j == 0 && nextPos == samplePos)
                        {
                            thisHair.SetActive(false);
                        }

                        samplePos = nextPos;
                    }

                    if (_tubes[i].radiuses[0] != _startRadius || _tubes[i].radiuses[pointCount - 1] != _endRadius)
                    {
                        for (int j = 0; j < pointCount; ++j)
                            _tubes[i].radiuses[j] = Mathf.Lerp(_startRadius, _endRadius, (float)j / (float)(pointCount - 1f));
                    }

                    _tubes[i].points = streamline;
                    _tubes[i].colors = colors;
                }
                else
                {
                    thisHair.SetActive(false);
                }
            }
            _needsUpdate = false;
        }
    }

    Vector3 euler(Vector3 start, float step)
    {
        // TODO: Implement me. It's easy, just remember to do it.
        return start + _flowFile.Sample(start) * step;
    }

    Vector3 rk4(Vector3 start, float step)
    {
        Vector3 x = start;
        float h = step;

        Vector3 xprime = _flowFile.Sample(x); // start using current position

        Vector3 k1 = xprime * h;
        Vector3 xstore = x + 0.5f * k1;

        xprime = _flowFile.Sample(xstore);

        Vector3 k2 = xprime * h;
        xstore = x + 0.5f * k2;

        xprime = _flowFile.Sample(xstore);

        Vector3 k3 = xprime * h;
        xstore = x + k3;

        xprime = _flowFile.Sample(xstore);

        Vector3 k4 = xprime * h;

        return x + (1f / 6f) *
            (k1 + 2f * k2 +
            2f * k3 + k4);        
    }    

    void MakeHair()
    {
        hairs = new GameObject[nx * ny];
        _tubes = new TubeRenderer[nx * ny];

        _xSpacing = 1f / (float)(nx);
        _ySpacing = 1f / (float)(ny);

        for (int i = 0; i < nx; ++i)
        {
            for (int j = 0; j < ny; ++j)
            {
                hairs[i * ny + j] = new GameObject("Hair (" + i + "," + j + ")");
                hairs[i * ny + j].layer = LayerMask.NameToLayer("Flow");
                hairs[i * ny + j].transform.SetParent(gameObject.transform);
                hairs[i * ny + j].transform.localPosition = new Vector3(-0.5f + _xSpacing * 0.5f + (float)i * _xSpacing, -0.5f + _ySpacing * 0.5f + (float)j * _ySpacing, 0f);
                hairs[i * ny + j].transform.localRotation = Quaternion.identity;                

                // Add a tube, set texture, points, radius and uv mapping, then optimise for realtime.
                _tubes[i * ny + j] = hairs[i * ny + j].AddComponent<TubeRenderer>();                
                _tubes[i * ny + j].GetComponent<Renderer>().material = _mainMat;

                _tubes[i * ny + j].points = new Vector3[pointCount];
                                
                _tubes[i * ny + j].colors = new Color32[pointCount];                

                //_tubes[i * ny + j].radius = _radius;
                _tubes[i * ny + j].radiuses = new float[pointCount];
                _tubes[i * ny + j].uvRect = new Rect(0, 0, 1, 1);
                _tubes[i * ny + j].uvRectCap = new Rect(0, 0, 0.0001f, 0.0001f);
                _tubes[i * ny + j].caps = TubeRenderer.CapMode.Begin;
                _tubes[i * ny + j].MarkDynamic();      
            }      
        }

        _needsUpdate = true;
    }

    public void JitterHair(float amount)
    {
        if (hairs == null)
            return;

        for (int i = 0; i < nx; ++i)
        {
            for (int j = 0; j < ny; ++j)
            {
                Vector3 gridPos = new Vector3(-0.5f + _xSpacing * 0.5f + (float)i * _xSpacing, -0.5f + _ySpacing * 0.5f + (float)j * _ySpacing, 0f);
                Vector3 pos = gridPos;
                pos.x += _xSpacing * Random.Range(-amount, amount);
                pos.y += _ySpacing * Random.Range(-amount, amount);
                hairs[i * ny + j].transform.localPosition = pos;
            }
        }

        _needsUpdate = true;
    }
}