using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
[ExecuteInEditMode]
public class WorldCanvasFitter : MonoBehaviour
{
    [SerializeField] private int widthInPixels = 1920;
    [SerializeField] private int heightInPixels = 1080;
    [SerializeField] private float canvasDepth = 100f;

    private Canvas canvas;
    private Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        canvas = GetComponent<Canvas>();
        cam = canvas.worldCamera;
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR

#endif
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (canvas != null)
        {
            Fit();
        }
    }
#endif

    public void Fit()
    {
        Mathf.Tan(Mathf.Deg2Rad * cam.fieldOfView * 0.5f);
        float depthAtScaleOne = 0.5f * heightInPixels / Mathf.Tan(Mathf.Deg2Rad * cam.fieldOfView * 0.5f);
        float desiredScale = canvasDepth / depthAtScaleOne;
        transform.localScale = new Vector3(desiredScale, desiredScale, desiredScale);
        transform.position = cam.transform.position + cam.transform.forward * canvasDepth;
    }
}
