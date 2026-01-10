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

    // Anti-flicker: if AR selected recently, ignore button spam
    [Header("Selection Priority")]
    [Tooltip("For how many seconds after an AR selection we ignore button re-selections (prevents flicker).")]
    [SerializeField] private float arPriorityHoldSeconds = 0.75f;

    private static float _lastARSelectTime = -999f;

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
            return true; // default ascending

        return Instance.MinOrMax.isOn;
    }

    /// <summary>
    /// The ONLY method that should change selection.
    /// Use this from buttons and from AR tracking.
    /// </summary>
    public static void RequestSelection(Sort sort, SelectionSource source)
    {
        // Safety
        if (sort == Sort.NotSelected)
        {
            SetSelectionInternal(Sort.NotSelected, SelectionSource.None);
            return;
        }

        // Anti-flicker rule:
        // If AR selected recently, ignore Buttons trying to "keep" selection.
        if (source == SelectionSource.Buttons)
        {
            float hold = (Instance != null) ? Instance.arPriorityHoldSeconds : 0.75f;
            if (LastSource == SelectionSource.AR && (Time.time - _lastARSelectTime) <= hold)
            {
                // Ignore button spam during AR priority window
                return;
            }
        }

        // Track AR timestamp
        if (source == SelectionSource.AR)
            _lastARSelectTime = Time.time;

        // Don’t spam events if nothing changed
        if (SelectedSort == sort && LastSource == source)
            return;

        SetSelectionInternal(sort, source);
    }

    private static void SetSelectionInternal(Sort sort, SelectionSource source)
    {
        SelectedSort = sort;
        LastSource = source;

        OnSelectionChanged?.Invoke(SelectedSort, LastSource);
        Debug.Log($"[UIManager] Selection => {SelectedSort} (source={LastSource})");
    }

    // ---- Button hooks (assign these in OnClick) ----
    public void BubbleSortSelected() => RequestSelection(Sort.Bubble, SelectionSource.Buttons);
    public void SelectionSortSelected() => RequestSelection(Sort.Selection, SelectionSource.Buttons);
    public void InsertionSortSelected() => RequestSelection(Sort.Insertion, SelectionSource.Buttons);
    public void MergeSortSelected() => RequestSelection(Sort.Merge, SelectionSource.Buttons);
    public void QuickSortSelected() => RequestSelection(Sort.Quick, SelectionSource.Buttons);
    public void HeapSortSelected() => RequestSelection(Sort.Heap, SelectionSource.Buttons);
}
