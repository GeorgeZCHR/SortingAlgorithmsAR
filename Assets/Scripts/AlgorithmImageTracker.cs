using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class AlgorithmImageTracker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ARTrackedImageManager trackedImageManager;
    [SerializeField] private TMP_Text uiLabel; // UI label στην οθόνη

    [Header("Behavior")]
    [Tooltip("If true, accepts Limited as detection too. Recommended because Tracking may be rare.")]
    [SerializeField] private bool acceptLimited = true;

    [Tooltip("Clear label if nothing detected for X seconds. Set 0 to disable.")]
    [SerializeField] private float clearAfterSeconds = 0.8f;

    private string lastSelected = null;
    private float lastSeenTime = -999f;

    private Camera arCamera;

    private void Awake()
    {
        // Try to grab AR camera for distance checks (optional)
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
        // In one callback you may get many updated images (Limited flicker).
        // Pick ONE best candidate and apply it ONCE (prevents UI staying on wrong card).
        ARTrackedImage best = null;
        float bestScore = float.NegativeInfinity;

        Evaluate(args.added, ref best, ref bestScore);
        Evaluate(args.updated, ref best, ref bestScore);

        if (best == null) return;

        HandleBest(best);
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

            // Score: Tracking > Limited, and closer to camera wins
            float score = 0f;

            if (img.trackingState == TrackingState.Tracking) score += 10000f;
            else if (img.trackingState == TrackingState.Limited) score += 1000f;

            if (arCamera != null)
            {
                float dist = Vector3.Distance(arCamera.transform.position, img.transform.position);
                score += Mathf.Clamp(50f - dist, -50f, 50f);
            }

            // small boost for bigger physical size
            score += (img.size.x * img.size.y) * 10f;

            if (score > bestScore)
            {
                bestScore = score;
                best = img;
            }
        }
    }

    private void HandleBest(ARTrackedImage img)
    {
        lastSeenTime = Time.time;

        string name = img.referenceImage.name; // bubble, selection, quick...
        if (string.IsNullOrEmpty(name)) return;

        // If the same card is still best, no need to rewrite UI every frame
        if (name == lastSelected) return;

        lastSelected = name;

        // Update UI immediately
        if (uiLabel != null)
            uiLabel.text = $"{ToTitle(name)} selected";

        // Select algorithm immediately
        switch (name)
        {
            case "bubble": UIManager.SelectedSort = Sort.Bubble; break;
            case "selection": UIManager.SelectedSort = Sort.Selection; break;
            case "insertion": UIManager.SelectedSort = Sort.Insertion; break;
            case "merge": UIManager.SelectedSort = Sort.Merge; break;
            case "quick": UIManager.SelectedSort = Sort.Quick; break;
            case "heap": UIManager.SelectedSort = Sort.Heap; break;
            default:
                Debug.LogWarning($"[AlgorithmImageTracker] Unknown card name: {name}");
                break;
        }

        Debug.Log($"[AlgorithmImageTracker] SelectedSort = {UIManager.SelectedSort} from card '{name}' (state={img.trackingState})");
    }

    private void Update()
    {
        if (clearAfterSeconds <= 0f) return;

        // Clear UI if nothing detected recently
        if (!string.IsNullOrEmpty(lastSelected) && (Time.time - lastSeenTime) > clearAfterSeconds)
        {
            lastSelected = null;
            if (uiLabel != null)
                uiLabel.text = "No card detected";
        }
    }

    private string ToTitle(string key)
    {
        switch (key)
        {
            case "bubble": return "Bubble Sort";
            case "selection": return "Selection Sort";
            case "insertion": return "Insertion Sort";
            case "merge": return "Merge Sort";
            case "quick": return "Quick Sort";
            case "heap": return "Heap Sort";
            default: return key;
        }
    }
}
