using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class TrackedImageDebug : MonoBehaviour
{
    [SerializeField] private ARTrackedImageManager trackedImageManager;

    private void Reset()
    {
        trackedImageManager = GetComponent<ARTrackedImageManager>();
    }

    private void OnEnable()
    {
        if (trackedImageManager == null) return;
        trackedImageManager.trackablesChanged.AddListener(OnTrackablesChanged);
        Debug.Log("[TrackedImageDebug] Listener ON");
    }

    private void OnDisable()
    {
        if (trackedImageManager == null) return;
        trackedImageManager.trackablesChanged.RemoveListener(OnTrackablesChanged);
        Debug.Log("[TrackedImageDebug] Listener OFF");
    }

    private void OnTrackablesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> args)
    {
        LogReadOnlyList(args.added, "ADDED");
        LogReadOnlyList(args.updated, "UPDATED");

        // Σε κάποιες εκδόσεις, το removed δεν είναι ίδιο type. Το αφήνουμε εκτός για debug.
        // Αν το χρειαστούμε, το προσθέτουμε μετά με βάση το ακριβές type που έχει στο δικό σου project.
    }

    private void LogReadOnlyList(IReadOnlyList<ARTrackedImage> list, string tag)
    {
        if (list == null) return;

        for (int i = 0; i < list.Count; i++)
        {
            var img = list[i];
            Debug.Log($"[TrackedImageDebug] {tag}: {img.referenceImage.name} | state={img.trackingState}");
        }
    }
}
