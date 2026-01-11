// Assets/Scripts/AlgorithmImageTracker.cs
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class AlgorithmImageTracker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ARTrackedImageManager trackedImageManager;

    [Tooltip("This label is ONLY for image tracking (cards).")]
    [SerializeField] private TMP_Text cardDetectedLabel;

    [Header("Label Text")]
    [SerializeField] private string noCardText = "No card detected";

    [Header("Behavior")]
    [Tooltip("If true, accepts Limited as detection too (recommended).")]
    [SerializeField] private bool acceptLimited = true;

    [Tooltip("How quickly we consider AR 'lost' and show No card detected (seconds).")]
    [SerializeField] private float clearAfterSeconds = 0.35f;

    [Header("Visibility Filter (IMPORTANT FIX)")]
    [Tooltip("Card must be inside camera view to count as 'seen'. 0.05 means a small margin outside edges.")]
    [SerializeField] private float viewportMargin = 0.05f;

    private Camera arCamera;

    private string lastCardName = null;
    private float lastSeenUnscaledTime = -999f;

    private void Awake()
    {
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

        if (cardDetectedLabel != null)
            cardDetectedLabel.text = noCardText;

        lastCardName = null;
        lastSeenUnscaledTime = -999f;
    }

    private void OnDisable()
    {
        if (trackedImageManager != null)
            trackedImageManager.trackablesChanged.RemoveListener(OnTrackablesChanged);
    }

    private void OnTrackablesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> args)
    {
        // Pick ONE best candidate per callback
        ARTrackedImage best = null;
        float bestScore = float.NegativeInfinity;

        Evaluate(args.added, ref best, ref bestScore);
        Evaluate(args.updated, ref best, ref bestScore);

        if (best == null)
            return;

        // We truly "see" a card right now:
        lastSeenUnscaledTime = Time.unscaledTime;
        UIManager.NotifyARSeen(); // locks buttons briefly (auto release in UIManager)

        string name = best.referenceImage.name;
        if (string.IsNullOrEmpty(name))
            return;

        // If same card, we still keep AR lock alive (done above), but no need to reselect/relabel.
        if (name == lastCardName)
            return;

        lastCardName = name;

        Sort sort = NameToSort(name);
        if (sort == Sort.NotSelected)
            return;

        // Update label IMMEDIATELY (no waiting for events)
        if (cardDetectedLabel != null)
            cardDetectedLabel.text = $"{ToTitle(sort)}";

        // AR overrides selection
        UIManager.RequestSelection(sort, UIManager.SelectionSource.AR);

        Debug.Log($"[AlgorithmImageTracker] AR detected '{name}' => {sort} (state={best.trackingState})");
    }

    private void Update()
    {
        // Show "No card detected" when we truly haven't seen anything recently
        if (!string.IsNullOrEmpty(lastCardName) && (Time.unscaledTime - lastSeenUnscaledTime) > clearAfterSeconds)
        {
            lastCardName = null;
            if (cardDetectedLabel != null)
                cardDetectedLabel.text = noCardText;
        }
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

            // Ignore None entirely
            if (img.trackingState == TrackingState.None)
                continue;

            // If we don't accept Limited, require Tracking
            if (!acceptLimited && img.trackingState != TrackingState.Tracking)
                continue;

            // IMPORTANT: Must be actually in camera view (prevents "stuck lock" when AR keeps Limited offscreen)
            if (!IsInCameraView(img))
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

    private bool IsInCameraView(ARTrackedImage img)
    {
        if (arCamera == null) return true; // if no camera reference, don't block

        Vector3 vp = arCamera.WorldToViewportPoint(img.transform.position);

        // Must be in front of camera
        if (vp.z <= 0f) return false;

        float m = viewportMargin;
        bool inside =
            vp.x >= -m && vp.x <= 1f + m &&
            vp.y >= -m && vp.y <= 1f + m;

        return inside;
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

    private string ToTitle(Sort s)
    {
        switch (s)
        {
            case Sort.Bubble: return "Bubble Sort";
            case Sort.Selection: return "Selection Sort";
            case Sort.Insertion: return "Insertion Sort";
            case Sort.Merge: return "Merge Sort";
            case Sort.Quick: return "Quick Sort";
            case Sort.Heap: return "Heap Sort";
            default: return "Unknown";
        }
    }
}
