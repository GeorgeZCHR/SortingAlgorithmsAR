using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class MultipleImageTracker : MonoBehaviour
{
    [SerializeField] List<GameObject> prefabsToSpawn = new List<GameObject>();

    private ARTrackedImageManager trackedImageManager;
    private Dictionary<string, GameObject> _arObjects;

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

    // FIX 1: Change the parameter type back to the older 'ARTrackedImagesChangedEventArgs'
    // This matches the 'trackedImagesChanged' event perfectly.
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
            // In this older version, 'removed' is a list of images, so this loop works fine now!
            UpdateTrackedImages(trackedImage);
        }
    }



    //me8odoi gia diaforetikoi karta

    void DoSortingBubble(GameObject obj)
    {
        Debug.Log("Sorting for Bubble");
    }

    void DoSortingSelection(GameObject obj)
    {
        Debug.Log("Sorting for Selection");
    }

    void DoSortingMerge(GameObject obj)
    {
        Debug.Log("Sorting for Merge");
    }
    void DoSortingQuick(GameObject obj)
    {
        Debug.Log("Sorting for Quick");
    }

    void DoSortingInsertion(GameObject obj)
    {
        Debug.Log("Sorting for Insertion");
    }
    void DoSortingHeap(GameObject obj)
    {
        Debug.Log("Sorting for Heap");
    }
    private void UpdateTrackedImages(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name;

        if (!_arObjects.ContainsKey(imageName))
            return;

        GameObject obj = _arObjects[imageName];

        if (trackedImage.trackingState != TrackingState.Tracking)
        {
            obj.SetActive(false);
            return;
        }


        obj.SetActive(true);
        obj.transform.SetPositionAndRotation(
            trackedImage.transform.position,
            trackedImage.transform.rotation
        );

        //  ΔΙΑΦΟΡΕΤΙΚΟ SORTING / LOGIC ΑΝΑ ΚΑΡΤΑ
        switch (imageName)
        {
            case "bubble":
                DoSortingBubble(obj);
                break;

            case "quick":
                DoSortingQuick(obj);
                break;

            case "merge":
                DoSortingMerge(obj);
                break;

            case "insertion":
                DoSortingInsertion(obj);
                break;

            case "heap":
                DoSortingHeap(obj);
                break;

            case "selection":
                DoSortingSelection(obj);
                break;

        }
    }


    void Start()
    {
        trackedImageManager = GetComponent<ARTrackedImageManager>();

        if (trackedImageManager == null) return;

        _arObjects = new Dictionary<string, GameObject>();

        SetupSceneElements();

        // FIX 2: Subscribe to the OLD event.
        // We suppress the warning because we know it's obsolete, but it works and is safer for your code structure.
#pragma warning disable 618
        trackedImageManager.trackedImagesChanged += OnImagesTrackedChanged;
#pragma warning restore 618
    }

    private void OnDestroy()
    {
        if (trackedImageManager != null)
        {
            // FIX 3: Unsubscribe from the OLD event
#pragma warning disable 618
            trackedImageManager.trackedImagesChanged -= OnImagesTrackedChanged;
#pragma warning restore 618
        }
    }

    void Update()
    {
    }
}