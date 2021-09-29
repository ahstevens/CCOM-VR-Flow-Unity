using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class KeyboardCommands : MonoBehaviour
{
    public int FileCounter = 0;
    public Camera Cam = null;
    GameObject DSLR;
    AudioSource cameraClickSound;

    // Start is called before the first frame update
    void Start()
    {
        DSLR = Cam.transform.parent.gameObject;
        cameraClickSound = Cam.GetComponentInParent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetButtonDown("ScreenShot"))
        //{
        //    ScreenCapture.CaptureScreenshot("FlowShot.png", 4);
        //}
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            MakeLight();
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            DSLR.transform.position = GameObject.Find("finger_index_r_end").transform.position;
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            KillLight();
        }
        if (Input.GetKeyDown(KeyCode.F7))
        {
            string[] layerNames = { "Flow" };

            Cam.cullingMask = LayerMask.GetMask(layerNames);
            Cam.clearFlags = CameraClearFlags.SolidColor;           
        }
        if (Input.GetKeyDown(KeyCode.F8))
        {
            Cam.cullingMask = -1;
            Cam.clearFlags = CameraClearFlags.Skybox;
        }
        if (Input.GetKeyDown(KeyCode.F10) && Cam != null)
        {
            //SwapPlayerViews();
            Cam.backgroundColor = Color.white;
        }
        if (Input.GetKeyDown(KeyCode.F11) && Cam != null)
        {
            //SwapPlayerViews();
            Cam.backgroundColor = Color.black;
        }
    }

    private void LateUpdate()
    {        
        if (Input.GetKeyDown(KeyCode.F9))
        {
            CamCapture();

            cameraClickSound.Play();
        }
    }
    
    void MakeLight()
    {
        string[] layerNames = { "Flow", "Selfie" };
        GameObject light = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        light.name = "Interactable Light";
        Light lc = light.AddComponent<Light>();
        lc.type = LightType.Point;
        lc.range = 1f;
        lc.cullingMask = LayerMask.GetMask(layerNames);
        light.AddComponent<Grabable>();
        light.transform.localScale = Vector3.one * 0.01f;
        light.transform.position = GameObject.Find("finger_index_r_end").transform.position;

        
    }

    void KillLight()
    {
        GameObject rightHand = GameObject.Find("RightHand");

        for (int i = 0; i < rightHand.transform.childCount; ++i)
        {
            var thisChild = rightHand.transform.GetChild(i).gameObject;
            if (thisChild.name.Equals("Interactable Light"))
                Destroy(thisChild);
        }
    }

    void CamCapture()
    {
        if (Cam == null)
            return;        

        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = Cam.targetTexture;

        //Cam.Render();

        Texture2D Image = new Texture2D(Cam.targetTexture.width, Cam.targetTexture.height);
        Image.ReadPixels(new Rect(0, 0, Cam.targetTexture.width, Cam.targetTexture.height), 0, 0);

        // get gamma-corrected colors to save
        Color[] pixels = Image.GetPixels();
        for (int p = 0; p < pixels.Length; p++)
        {
            pixels[p] = pixels[p].gamma;
        }
        Image.SetPixels(pixels);

        Image.Apply();
        RenderTexture.active = currentRT;

        var Bytes = Image.EncodeToPNG();
        var jpgBytes = Image.EncodeToJPG(90);
        Destroy(Image);

        string pngName;
        string jpgName;

        do
        {
            pngName = Application.dataPath + "/../Screenshot_" + FileCounter + ".png";
            jpgName = Application.dataPath + "/../Screenshot_" + FileCounter + ".jpg";
            FileCounter++;
        }
        while (File.Exists(pngName) || File.Exists(jpgName));

        File.WriteAllBytes(pngName, Bytes);
        File.WriteAllBytes(jpgName, jpgBytes);
    }

    void SwapPlayerViews()
    {
        Camera[] cams = Camera.allCameras;
        float tempDepth = cams[0].depth;
        cams[0].depth = cams[1].depth;
        cams[1].depth = tempDepth;
    }
}
