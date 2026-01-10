// Assets/Scripts/AlgorithmImageTracker.cs
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class AlgorithmImageTracker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ARTrackedImageManager trackedImageManager;

    [Header("Behavior")]
    [Tooltip("If true, accepts Limited as detection too (recommended).")]
    [SerializeField] private bool acceptLimited = true;

    private Camera arCamera;
    private string lastCardName = null;

    private void Awake()
    {
        // Try to grab AR camera for distance scoring (optional)
        var origin = GetComponent<XROrigin>();
        if (origin != null && origin.Camera != null)
            arCamera = origin.Camera;
        else
            arCamera = Camera.main;
    }

    private void OnEnable()
    {
        if (trackedImageManager != null)
            trackedImageManager.trackablesChanged.AddListener(OnTrackablesChanged);
    }

    private void OnDisable()
    {
        if (trackedImageManager != null)
            trackedImageManager.trackablesChanged.RemoveListener(OnTrackablesChanged);
    }

    private void OnTrackablesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> args)
    {
        // Pick ONE best candidate per callback (prevents multi-image confusion)
        ARTrackedImage best = null;
        float bestScore = float.NegativeInfinity;

        Evaluate(args.added, ref best, ref bestScore);
        Evaluate(args.updated, ref best, ref bestScore);

        if (best == null) return;

        // Map card name -> Sort
        string name = best.referenceImage.name;
        if (string.IsNullOrEmpty(name)) return;

        // If same as last, do nothing (keeps updates fast for changes)
        if (name == lastCardName) return;
        lastCardName = name;

        Sort sort = NameToSort(name);
        if (sort == Sort.NotSelected) return;

        // IMPORTANT: AR selection wins (UIManager prevents button spam flicker)
        UIManager.RequestSelection(sort, UIManager.SelectionSource.AR);

        Debug.Log($"[AlgorithmImageTracker] AR detected '{name}' => {sort} (state={best.trackingState})");
    }

    private void Evaluate(
        Unity.XR.CoreUtils.Collections.ReadOnlyList<ARTrackedImage> list,
        ref ARTrackedImage best,
        ref float bestScore)
    {
        for (int i = 0; i < list.Count; i++)
        {
            var img = list[i];
            if (img == null) continue;

            if (img.trackingState == TrackingState.None)
                continue;

            if (!acceptLimited && img.trackingState != TrackingState.Tracking)
                continue;

            float score = 0f;

            // Tracking > Limited
            score += (img.trackingState == TrackingState.Tracking) ? 10000f : 1000f;

            // Prefer closer
            if (arCamera != null)
            {
                float dist = Vector3.Distance(arCamera.transform.position, img.transform.position);
                score += Mathf.Clamp(50f - dist, -50f, 50f);
            }

            // Prefer bigger physical size slightly
            score += (img.size.x * img.size.y) * 10f;

            if (score > bestScore)
            {
                bestScore = score;
                best = img;
            }
        }
    }

    private Sort NameToSort(string key)
    {
        switch (key)
        {
            case "bubble": return Sort.Bubble;
            case "selection": return Sort.Selection;
            case "insertion": return Sort.Insertion;
            case "merge": return Sort.Merge;
            case "quick": return Sort.Quick;
            case "heap": return Sort.Heap;
            default:
                Debug.LogWarning($"[AlgorithmImageTracker] Unknown card name: {key}");
                return Sort.NotSelected;
        }
    }
}
