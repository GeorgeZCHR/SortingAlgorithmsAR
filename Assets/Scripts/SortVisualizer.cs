// Assets/Scripts/SortVisualizer.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SortVisualizer : MonoBehaviour
{
    [Header("Refs")]
    public DigitManager digitManager;

    [Header("Timing")]
    [Tooltip("Delay between animation steps (seconds)")]
    public float stepDelay = 0.35f;

    [Header("UI")]
    [Tooltip("Drag here the TMP text component inside the Pause button")]
    public TMP_Text pauseButtonLabel;

    public Button startButton;
    public Button resetButton;

    [Header("Highlight (Text color)")]
    [Tooltip("Hex color for normal number text, e.g. #00E5FF")]
    public string normalTextHex = "#00E5FF";

    [Tooltip("Hex color for highlighted number text, e.g. #FFD54A")]
    public string highlightTextHex = "#FFD54A";

    private Color normalTextColor = Color.white;
    private Color highlightTextColor = Color.yellow;

    private Coroutine runningRoutine;
    private bool paused = false;

    // Snapshot for Reset
    private string originalInputText = "";

    private readonly List<int> realItems = new List<int>();

    // For swap-based animations
    private readonly List<MovementAnimation> movements = new List<MovementAnimation>();

    // For merge "write" animation
    private struct WriteOp
    {
        public int index;
        public int value;
        public WriteOp(int index, int value) { this.index = index; this.value = value; }
    }
    private readonly List<WriteOp> writes = new List<WriteOp>();

    private void Awake()
    {
        // Parse hex colors (supports #RRGGBB or #RRGGBBAA)
        if (!ColorUtility.TryParseHtmlString(normalTextHex, out normalTextColor))
            normalTextColor = Color.white;

        if (!ColorUtility.TryParseHtmlString(highlightTextHex, out highlightTextColor))
            highlightTextColor = Color.yellow;
    }

    private void OnEnable()
    {
        // ✅ NEW EVENT (αντί για OnSortSelected)
        UIManager.OnSelectionChanged += HandleSelectionChanged;

        UpdateStartInteractable();
        SetPauseLabel(false);
    }

    private void OnDisable()
    {
        UIManager.OnSelectionChanged -= HandleSelectionChanged;
    }

    private void HandleSelectionChanged(Sort sort, UIManager.SelectionSource source)
    {
        // Όταν αλλάξει επιλογή (AR ή button), δες αν πρέπει να ενεργοποιηθεί το Start.
        UpdateStartInteractable();
    }

    private void UpdateStartInteractable()
    {
        if (startButton == null) return;

        bool hasSelection = (UIManager.SelectedSort != Sort.NotSelected);

        bool hasNumbers = false;
        if (digitManager != null)
        {
            var items = digitManager.GetVisualItems();
            hasNumbers = (items != null && items.Count > 0);
        }

        // Αν τρέχει animation, Start πρέπει να μένει off.
        bool canStart = hasSelection && hasNumbers && (runningRoutine == null);
        startButton.interactable = canStart;
    }

    private void SetPauseLabel(bool isPaused)
    {
        if (pauseButtonLabel != null)
            pauseButtonLabel.text = isPaused ? "Resume" : "Pause";
    }

    public void TogglePause()
    {
        if (runningRoutine == null)
        {
            paused = false;
            SetPauseLabel(false);
            return;
        }

        paused = !paused;
        SetPauseLabel(paused);
    }

    public void StartRun()
    {
        Debug.Log("[SortVisualizer] StartRun called at frame: " + Time.frameCount);

        // OPTION A: δεν ξεκινάει αν δεν έχει επιλεγεί αλγόριθμος
        if (UIManager.SelectedSort == Sort.NotSelected)
        {
            Debug.LogWarning("[SortVisualizer] No algorithm selected. Select via card or button first.");
            UpdateStartInteractable();
            return;
        }

        if (digitManager == null)
        {
            Debug.LogError("[SortVisualizer] digitManager is NULL");
            return;
        }

        var items = digitManager.GetVisualItems();
        if (items == null || items.Count == 0)
        {
            Debug.LogWarning("[SortVisualizer] No visual items to sort.");
            UpdateStartInteractable();
            return;
        }

        // Stop any previous run completely
        StopRunInternal();

        // Snapshot for Reset
        originalInputText = digitManager.ArrayField != null ? digitManager.ArrayField.text : "";

        paused = false;
        SetPauseLabel(false);

        if (startButton != null) startButton.interactable = false;
        if (resetButton != null) resetButton.interactable = true;

        CreateRealItems(items);

        bool isMinToMax = UIManager.IsFromMinToMax();
        Debug.Log("[SortVisualizer] Sort: " + Helper.GetSortName() + " | order: " + (isMinToMax ? "Min→Max" : "Max→Min"));

        BuildOperations(isMinToMax);

        // Start animation
        if (UIManager.SelectedSort == Sort.Merge)
            runningRoutine = StartCoroutine(AnimateWrites(items));
        else
            runningRoutine = StartCoroutine(AnimateSwaps(items));
    }

    /// <summary>
    /// Reset to initial state (the numbers before StartRun).
    /// Hook this to your Reset button OnClick().
    /// </summary>
    public void ResetRun()
    {
        Debug.Log("[SortVisualizer] ResetRun");

        StopRunInternal();

        paused = false;
        SetPauseLabel(false);

        // Restore input & UI
        if (digitManager != null && digitManager.ArrayField != null)
        {
            digitManager.ArrayField.text = originalInputText;
            digitManager.ChangeValueInArrayField(originalInputText); // force rebuild so UI matches exactly
        }

        UpdateStartInteractable();
    }

    private void StopRunInternal()
    {
        if (runningRoutine != null)
        {
            StopCoroutine(runningRoutine);
            runningRoutine = null;
        }

        // Clear highlights if any
        var items = digitManager != null ? digitManager.GetVisualItems() : null;
        if (items != null)
        {
            for (int i = 0; i < items.Count; i++)
                SetHighlighted(items[i], false);
        }

        movements.Clear();
        writes.Clear();
        paused = false;
        SetPauseLabel(false);
    }

    private void FinishRun()
    {
        runningRoutine = null;
        paused = false;
        SetPauseLabel(false);

        UpdateStartInteractable();
    }

    private void CreateRealItems(List<VisualNumberItem> items)
    {
        realItems.Clear();
        movements.Clear();
        writes.Clear();

        for (int i = 0; i < items.Count; i++)
            realItems.Add(items[i].value);

        Debug.Log("[SortVisualizer] Input: " + string.Join(", ", realItems));
    }

    private void BuildOperations(bool isMinToMax)
    {
        switch (UIManager.SelectedSort)
        {
            case Sort.Bubble: BubbleSort(isMinToMax); break;
            case Sort.Selection: SelectionSort(isMinToMax); break;
            case Sort.Insertion: InsertionSort(isMinToMax); break;
            case Sort.Quick: QuickSort(isMinToMax); break;
            case Sort.Heap: HeapSort(isMinToMax); break;
            case Sort.Merge: MergeSortWithWrites(isMinToMax); break;

            case Sort.NotSelected:
            default:
                Debug.LogWarning("[SortVisualizer] BuildOperations called with NotSelected. Aborting.");
                break;
        }

        Debug.Log("[SortVisualizer] Result: " + string.Join(", ", realItems));
        Debug.Log("--------------------------------------");
    }

    // --------------------
    // Animations
    // --------------------

    private IEnumerator AnimateSwaps(List<VisualNumberItem> items)
    {
        for (int i = 0; i < movements.Count; i++)
        {
            int a = movements[i].from;
            int b = movements[i].to;

            if (a < 0 || b < 0 || a >= items.Count || b >= items.Count) continue;

            SetHighlighted(items[a], true);
            SetHighlighted(items[b], true);

            yield return WaitWhileNotPaused(stepDelay * 0.35f);

            yield return AnimateSwap(items[a], items[b]);

            SetHighlighted(items[a], false);
            SetHighlighted(items[b], false);

            yield return WaitWhileNotPaused(stepDelay);
        }

        FinishRun();
    }

    private IEnumerator AnimateWrites(List<VisualNumberItem> items)
    {
        for (int i = 0; i < writes.Count; i++)
        {
            var op = writes[i];
            if (op.index < 0 || op.index >= items.Count) continue;

            SetHighlighted(items[op.index], true);
            items[op.index].SetValue(op.value, op.index);

            yield return WaitWhileNotPaused(stepDelay);

            SetHighlighted(items[op.index], false);
        }

        FinishRun();
    }

    private IEnumerator WaitWhileNotPaused(float t)
    {
        float remaining = t;
        while (remaining > 0f)
        {
            if (!paused) remaining -= Time.unscaledDeltaTime;
            yield return null;
        }
    }

    private IEnumerator AnimateSwap(VisualNumberItem a, VisualNumberItem b)
    {
        RectTransform ra = a.GetComponent<RectTransform>();
        RectTransform rb = b.GetComponent<RectTransform>();

        Vector3 pa = ra.localPosition;
        Vector3 pb = rb.localPosition;

        float dur = 0.18f;
        float t = 0f;

        while (t < dur)
        {
            if (!paused)
            {
                t += Time.unscaledDeltaTime;
                float f = t / dur;
                ra.localPosition = Vector3.Lerp(pa, pb, f);
                rb.localPosition = Vector3.Lerp(pb, pa, f);
            }
            yield return null;
        }

        ra.localPosition = pa;
        rb.localPosition = pb;

        // Swap displayed values
        int tmp = a.value;
        a.value = b.value;
        b.value = tmp;

        string ta = a.numberText.text;
        a.numberText.text = b.numberText.text;
        b.numberText.text = ta;
    }

    private void SetHighlighted(VisualNumberItem item, bool on)
    {
        if (item == null || item.numberText == null) return;
        item.numberText.color = on ? highlightTextColor : normalTextColor;
    }

    // --------------------
    // Algorithms (record ops)
    // --------------------

    private bool OutOfOrder(int left, int right, bool isMinToMax)
    {
        return isMinToMax ? (left > right) : (left < right);
    }

    private void RecordSwap(int i, int j)
    {
        movements.Add(new MovementAnimation(i, j));

        int tmp = realItems[i];
        realItems[i] = realItems[j];
        realItems[j] = tmp;
    }

    private void BubbleSort(bool isMinToMax)
    {
        int n = realItems.Count;
        for (int i = 0; i < n - 1; i++)
        {
            bool swapped = false;
            for (int j = 0; j < n - i - 1; j++)
            {
                if (OutOfOrder(realItems[j], realItems[j + 1], isMinToMax))
                {
                    RecordSwap(j, j + 1);
                    swapped = true;
                }
            }
            if (!swapped) break;
        }
    }

    private void SelectionSort(bool isMinToMax)
    {
        int n = realItems.Count;
        for (int i = 0; i < n - 1; i++)
        {
            int bestIdx = i;
            for (int j = i + 1; j < n; j++)
            {
                bool better = isMinToMax ? realItems[j] < realItems[bestIdx] : realItems[j] > realItems[bestIdx];
                if (better) bestIdx = j;
            }
            if (bestIdx != i) RecordSwap(i, bestIdx);
        }
    }

    private void InsertionSort(bool isMinToMax)
    {
        int n = realItems.Count;
        for (int i = 1; i < n; i++)
        {
            int j = i;
            while (j > 0 && OutOfOrder(realItems[j - 1], realItems[j], isMinToMax))
            {
                RecordSwap(j - 1, j);
                j--;
            }
        }
    }

    private void QuickSort(bool isMinToMax)
    {
        QuickSortRec(0, realItems.Count - 1, isMinToMax);
    }

    private void QuickSortRec(int low, int high, bool isMinToMax)
    {
        if (low >= high) return;
        int p = Partition(low, high, isMinToMax);
        QuickSortRec(low, p - 1, isMinToMax);
        QuickSortRec(p + 1, high, isMinToMax);
    }

    private int Partition(int low, int high, bool isMinToMax)
    {
        int pivot = realItems[high];
        int i = low - 1;

        for (int j = low; j < high; j++)
        {
            bool goesLeft = isMinToMax ? (realItems[j] <= pivot) : (realItems[j] >= pivot);
            if (goesLeft)
            {
                i++;
                if (i != j) RecordSwap(i, j);
            }
        }

        if (i + 1 != high) RecordSwap(i + 1, high);
        return i + 1;
    }

    private void HeapSort(bool isMinToMax)
    {
        int n = realItems.Count;
        bool useMaxHeap = isMinToMax;

        for (int i = n / 2 - 1; i >= 0; i--)
            Heapify(n, i, useMaxHeap);

        for (int end = n - 1; end > 0; end--)
        {
            RecordSwap(0, end);
            Heapify(end, 0, useMaxHeap);
        }
    }

    private void Heapify(int heapSize, int i, bool useMaxHeap)
    {
        int best = i;
        int left = 2 * i + 1;
        int right = 2 * i + 2;

        if (left < heapSize)
        {
            bool better = useMaxHeap ? (realItems[left] > realItems[best]) : (realItems[left] < realItems[best]);
            if (better) best = left;
        }

        if (right < heapSize)
        {
            bool better = useMaxHeap ? (realItems[right] > realItems[best]) : (realItems[right] < realItems[best]);
            if (better) best = right;
        }

        if (best != i)
        {
            RecordSwap(i, best);
            Heapify(heapSize, best, useMaxHeap);
        }
    }

    private void MergeSortWithWrites(bool isMinToMax)
    {
        int n = realItems.Count;
        if (n <= 1) return;

        int[] temp = new int[n];

        for (int width = 1; width < n; width *= 2)
        {
            for (int i = 0; i < n; i += 2 * width)
            {
                int left = i;
                int mid = Mathf.Min(i + width, n);
                int right = Mathf.Min(i + 2 * width, n);

                int a = left, b = mid, k = left;

                while (a < mid && b < right)
                {
                    bool takeA = isMinToMax ? (realItems[a] <= realItems[b]) : (realItems[a] >= realItems[b]);
                    temp[k++] = takeA ? realItems[a++] : realItems[b++];
                }
                while (a < mid) temp[k++] = realItems[a++];
                while (b < right) temp[k++] = realItems[b++];

                for (int x = left; x < right; x++)
                {
                    realItems[x] = temp[x];
                    writes.Add(new WriteOp(x, realItems[x]));
                }
            }
        }
    }
}
