using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TMPro;

[RequireComponent(typeof(ARTrackedImageManager))]
public class CardRecognitionScanner : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _textOut;

    private ARTrackedImageManager _trackedImageManager;

    void Awake()
    {
        _trackedImageManager = GetComponent<ARTrackedImageManager>();
    }

    void OnEnable()
    {
        // Subscribe to the event that triggers when images are detected
        _trackedImageManager.trackedImagesChanged += OnChanged;
    }

    void OnDisable()
    {
        _trackedImageManager.trackedImagesChanged -= OnChanged;
    }

    void OnChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        // Loop through images that were just detected/updated
        foreach (var trackedImage in eventArgs.added)
        {
            UpdateUI(trackedImage);
        }

        foreach (var trackedImage in eventArgs.updated)
        {
            // Only update UI if the tracking state is high quality
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                UpdateUI(trackedImage);
            }
        }
    }

    private void UpdateUI(ARTrackedImage trackedImage)
    {
        // ReferenceImage.name corresponds to the name you set in the Reference Library
        string cardName = trackedImage.referenceImage.name;
        _textOut.text = "Detected Card: " + cardName;
    }
}


/*using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ZXing;
using TMPro;
using UnityEngine.UI;

public class QRCodeScanner : MonoBehaviour
{
    [SerializeField]
    private RawImage _rawImageBackground;
    [SerializeField]
    private AspectRatioFitter _aspectRatioFitter;
    [SerializeField]
    private TextMeshProUGUI _textOut;
    [SerializeField]
    private RectTransform _scanZone;


    private bool _isCamAvailable;
    private WebCamTexture _cameraTexture;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateCameraRender();
    }

    private void SetUpCamera()
    {
        WebCamDevice[] devices = WebCamTexture.devices; 
        if(devices.Length == 0)
        {
            _isCamAvailable = false;
            return;
        }

        for (int i = 0; i<devices.Length; i++)
        {
            if (devices[i].isFrontFacing == false)
            {
                _cameraTexture = new WebCamTexture(devices[i].name, (int)_scanZone.rect.width, (int)_scanZone.rect.height);
            }
        }
        _cameraTexture.Play();
        _rawImageBackground.texture = _cameraTexture;
        _isCamAvailable=true;
    }
    private void Scan() 
    {
        try
        {
            IBarcodeReader barcodeReader = new IBarcodeReader();
            Result result = barcodeReader.Decode(_cameraTexture.GetPixels32(), _cameraTexture.width.height);
            if (result!=null)
            {
                _textOut.text = result.Text;
            }
            else
            {
                _textOut.text = "Failed to read the card";
            }
        }
        catch
        {
            _textOut.text = "Failed in try";
        }
    }
    public void OnClickScan()
    {
        Scan();
    }
    private void UpdateCameraRender()
    {
        if (_isCamAvailable==false) { return; }
        float ratio = (float)_cameraTexture.width / (float)_cameraTexture.height;
        _aspectRationFitter.aspectRatio = ratio;

        int orientation = -_cameraTexture.videoRotationAngle;
        _rawImageBackground.rectTransform.localEulerAngles = new Vector3(0, 0, orientation);
    }

}
*/