// Assets/Scripts/UIManager.cs
using System;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI")]
    public Toggle MinOrMax;

    // ---- Selection state ----
    public enum SelectionSource
    {
        None = 0,
        Buttons = 1,
        AR = 2
    }

    public static Sort SelectedSort { get; private set; } = Sort.NotSelected;
    public static SelectionSource LastSource { get; private set; } = SelectionSource.None;

    public static event Action<Sort, SelectionSource> OnSelectionChanged;

    // ---- AR Lock (auto-release) ----
    [Header("AR Lock")]
    [Tooltip("How many seconds after last AR 'seen' we keep buttons locked. Use small value like 0.35 - 0.6")]
    [SerializeField] private float arLockReleaseSeconds = 0.45f;

    private static float _lastARSeenUnscaledTime = -999f;

    /// <summary>
    /// True only if AR has been seen recently (auto releases).
    /// </summary>
    public static bool ARLockActive
    {
        get
        {
            float release = (Instance != null) ? Instance.arLockReleaseSeconds : 0.45f;
            return (Time.unscaledTime - _lastARSeenUnscaledTime) <= release;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (MinOrMax == null)
            Debug.LogError("UIManager: MinOrMax Toggle is not assigned in Inspector!");
    }

    public static bool IsFromMinToMax()
    {
        if (Instance == null || Instance.MinOrMax == null)
            return true;

        return Instance.MinOrMax.isOn;
    }

    /// <summary>
    /// Called by AlgorithmImageTracker when a card is ACTUALLY visible in camera view.
    /// Keeps buttons locked briefly; auto-unlocks when AR not seen anymore.
    /// </summary>
    public static void NotifyARSeen()
    {
        _lastARSeenUnscaledTime = Time.unscaledTime;
    }

    /// <summary>
    /// Single entry point for selection.
    /// </summary>
    public static void RequestSelection(Sort sort, SelectionSource source)
    {
        // Ignore button input while AR is actively seeing a card
        if (source == SelectionSource.Buttons && ARLockActive)
            return;

        if (sort == Sort.NotSelected)
        {
            SelectedSort = Sort.NotSelected;
            LastSource = SelectionSource.None;
            OnSelectionChanged?.Invoke(SelectedSort, LastSource);
            return;
        }

        // Don’t spam events if nothing changed
        if (SelectedSort == sort && LastSource == source)
            return;

        SelectedSort = sort;
        LastSource = source;

        OnSelectionChanged?.Invoke(SelectedSort, LastSource);
        Debug.Log($"[UIManager] Selection => {SelectedSort} (source={LastSource}, ARLock={ARLockActive})");
    }

    // ---- Button hooks (assign these in OnClick) ----
    public void BubbleSortSelected() => RequestSelection(Sort.Bubble, SelectionSource.Buttons);
    public void SelectionSortSelected() => RequestSelection(Sort.Selection, SelectionSource.Buttons);
    public void InsertionSortSelected() => RequestSelection(Sort.Insertion, SelectionSource.Buttons);
    public void MergeSortSelected() => RequestSelection(Sort.Merge, SelectionSource.Buttons);
    public void QuickSortSelected() => RequestSelection(Sort.Quick, SelectionSource.Buttons);
    public void HeapSortSelected() => RequestSelection(Sort.Heap, SelectionSource.Buttons);
}
