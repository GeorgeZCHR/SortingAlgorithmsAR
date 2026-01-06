using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class MultipleImageTracker : MonoBehaviour
{
    [SerializeField] List<GameObject> prefabsToSpawn = new List<GameObject>();
    public GameObject narrationPrefab;
    private Dictionary<string, ARNarrationController> narrations = new();

    public SortVisualizer sortVisualizer;
    private ARTrackedImageManager trackedImageManager;
    private Dictionary<string, GameObject> _arObjects;
    private HashSet<string> activeCards = new HashSet<string>();

    // Move this variable here so it is accessible to the whole class, 
    // not just inside a single method execution.
    private string lastDetectedCard = null;

    void Start()
    {
        trackedImageManager = GetComponent<ARTrackedImageManager>();

        if (trackedImageManager == null) return;

        _arObjects = new Dictionary<string, GameObject>();

        SetupSceneElements();

        // FIX 2: Subscribe to the OLD event.
#pragma warning disable 618
        trackedImageManager.trackedImagesChanged += OnImagesTrackedChanged;
#pragma warning restore 618
    }

    private void OnDestroy()
    {
        if (trackedImageManager != null)
        {
#pragma warning disable 618
            trackedImageManager.trackedImagesChanged -= OnImagesTrackedChanged;
#pragma warning restore 618
        }
    }

    private void SetupSceneElements()
    {

        foreach (var prefab in prefabsToSpawn)
        {

            var arObject = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            arObject.name = prefab.name;
            arObject.gameObject.SetActive(false);
            _arObjects.Add(arObject.name, arObject);
        }
    }

    private void OnImagesTrackedChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var trackedImage in eventArgs.added)
        {
            UpdateTrackedImages(trackedImage);
        }
        foreach (var trackedImage in eventArgs.updated)
        {
            UpdateTrackedImages(trackedImage);
        }
        foreach (var trackedImage in eventArgs.removed)
        {
            // Reset logic for removed images
            string imageName = trackedImage.referenceImage.name;
            if (_arObjects.ContainsKey(imageName))
            {
                _arObjects[imageName].SetActive(false);
            }
            activeCards.Remove(imageName);
        }
    }

    private void UpdateTrackedImages(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name;

        if (!_arObjects.ContainsKey(imageName))
            return;

        GameObject obj = _arObjects[imageName];

        // If the image is no longer being tracked reliably
        if (trackedImage.trackingState != TrackingState.Tracking)
        {
            obj.SetActive(false);
            activeCards.Remove(imageName);

            if (lastDetectedCard == imageName)
                lastDetectedCard = null;

            return;
        }

        // If tracking is active
        obj.SetActive(true);
        obj.transform.SetPositionAndRotation(
            trackedImage.transform.position,
            trackedImage.transform.rotation
        );

        // Prevent restarting the sort if the card is already active
        if (activeCards.Contains(imageName))
            return;

        activeCards.Add(imageName);
        lastDetectedCard = imageName;

        // Trigger the specific sorting logic
        switch (imageName)
        {
            case "bubble":
                sortVisualizer.StartSortFromCard("B");
                break;
            case "selection":
                sortVisualizer.StartSortFromCard("S");
                break;
            case "insertion":
                sortVisualizer.StartSortFromCard("I");
                break;
            case "merge":
                sortVisualizer.StartSortFromCard("M");
                break;
            case "quick":
                sortVisualizer.StartSortFromCard("Q");
                break;
            case "heap":
                sortVisualizer.StartSortFromCard("H");
                break;
        }
    }
}