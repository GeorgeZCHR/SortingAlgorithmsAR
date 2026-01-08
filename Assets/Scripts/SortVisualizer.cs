using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SortVisualizer : MonoBehaviour
{
    [Header("AR / UI")]
    public ARNarrationController arNarration;
    public DigitManager digitManager;

    [Header("Bars Parent")]
    public Transform barsParent;

    [Header("Timing")]
    public float stepDelay = 0.6f;

    private Coroutine runningRoutine;
    private bool paused;
    private bool isSorting;

    private List<int> realItems = new List<int>();
    private List<MovementAnimation> movements = new List<MovementAnimation>();

    void Awake()
    {
        if (barsParent == null)
        {
            Debug.LogError("SortVisualizer: barsParent is NULL (Inspector)");
            enabled = false;
        }
    }

    /* ===================== AR ENTRY ===================== */

    public void StartSortFromCard(string code)
    {
        if (isSorting) return;

        isSorting = true;

        switch (code.ToUpper())
        {
            case "B": UIManager.SelectedSort = Sort.Bubble; break;
            case "S": UIManager.SelectedSort = Sort.Selection; break;
            case "I": UIManager.SelectedSort = Sort.Insertion; break;
            case "M": UIManager.SelectedSort = Sort.Merge; break;
            case "Q": UIManager.SelectedSort = Sort.Quick; break;
            case "H": UIManager.SelectedSort = Sort.Heap; break;

            default:
                Debug.LogWarning("Unknown AR card: " + code);
                isSorting = false;
                return;
        }

        Debug.Log("AR Card detected → " + UIManager.SelectedSort);
        StartRun();
    }

    /* ===================== CORE ===================== */

    public void StartRun()
    {
        if (runningRoutine != null)
            StopCoroutine(runningRoutine);

        var items = digitManager.GetVisualItems();
        CreateRealItems(items);

        RunAlgorithm();
        runningRoutine = StartCoroutine(AnimateMovements(items));
    }

    void CreateRealItems(List<VisualNumberItem> items)
    {
        realItems.Clear();
        movements.Clear();

        foreach (var item in items)
            realItems.Add(item.value);

        Debug.Log("Initial array: " + string.Join(", ", realItems));
    }

    void SetNarration(string text)
    {
        if (arNarration != null)
            arNarration.SetText(text);
    }

    /* ===================== ALGORITHM SELECTOR ===================== */

    void RunAlgorithm()
    {
        bool asc = UIManager.IsFromMinToMax();

        switch (UIManager.SelectedSort)
        {
            case Sort.Bubble:
                if (asc) BubbleSortAscending();
                else BubbleSortDescending();
                break;

            case Sort.Selection:
                if (asc) SelectionSortAscending();
                else SelectionSortDescending();
                break;

            case Sort.Insertion:
                if (asc) InsertionSortAscending();
                else InsertionSortDescending();
                break;

            case Sort.Merge:
                if (asc) MergeSortAscending(0, realItems.Count - 1);
                else MergeSortDescending(0, realItems.Count - 1);
                break;

            case Sort.Quick:
                if (asc) QuickSortAscending(0, realItems.Count - 1);
                else QuickSortDescending(0, realItems.Count - 1);
                break;
        }
    }

    /* ===================== SORTS ===================== */

    void BubbleSortAscending()
    {
        for (int i = 0; i < realItems.Count - 1; i++)
            for (int j = 0; j < realItems.Count - i - 1; j++)
                if (realItems[j] > realItems[j + 1])
                    Swap(j, j + 1);
    }

    void BubbleSortDescending()
    {
        for (int i = 0; i < realItems.Count - 1; i++)
            for (int j = 0; j < realItems.Count - i - 1; j++)
                if (realItems[j] < realItems[j + 1])
                    Swap(j, j + 1);
    }

    void SelectionSortAscending()
    {
        for (int i = 0; i < realItems.Count - 1; i++)
        {
            int min = i;
            for (int j = i + 1; j < realItems.Count; j++)
                if (realItems[j] < realItems[min]) min = j;

            if (min != i) Swap(i, min);
        }
    }

    void SelectionSortDescending()
    {
        for (int i = 0; i < realItems.Count - 1; i++)
        {
            int max = i;
            for (int j = i + 1; j < realItems.Count; j++)
                if (realItems[j] > realItems[max]) max = j;

            if (max != i) Swap(i, max);
        }
    }

    void InsertionSortAscending()
    {
        for (int i = 1; i < realItems.Count; i++)
        {
            int key = realItems[i];
            int j = i - 1;

            while (j >= 0 && realItems[j] > key)
            {
                realItems[j + 1] = realItems[j];
                movements.Add(new MovementAnimation(j + 1, j));
                j--;
            }

            realItems[j + 1] = key;
        }
    }

    void InsertionSortDescending()
    {
        for (int i = 1; i < realItems.Count; i++)
        {
            int key = realItems[i];
            int j = i - 1;

            while (j >= 0 && realItems[j] < key)
            {
                realItems[j + 1] = realItems[j];
                movements.Add(new MovementAnimation(j + 1, j));
                j--;
            }

            realItems[j + 1] = key;
        }
    }

    void MergeSortAscending(int l, int r)
    {
        if (l >= r) return;
        int m = (l + r) / 2;
        MergeSortAscending(l, m);
        MergeSortAscending(m + 1, r);
    }

    void MergeSortDescending(int l, int r)
    {
        if (l >= r) return;
        int m = (l + r) / 2;
        MergeSortDescending(l, m);
        MergeSortDescending(m + 1, r);
    }

    void QuickSortAscending(int low, int high)
    {
        if (low < high)
        {
            int pi = PartitionAscending(low, high);
            QuickSortAscending(low, pi - 1);
            QuickSortAscending(pi + 1, high);
        }
    }

    void QuickSortDescending(int low, int high)
    {
        if (low < high)
        {
            int pi = PartitionDescending(low, high);
            QuickSortDescending(low, pi - 1);
            QuickSortDescending(pi + 1, high);
        }
    }

    int PartitionAscending(int low, int high)
    {
        int pivot = realItems[high];
        int i = low - 1;

        for (int j = low; j < high; j++)
            if (realItems[j] < pivot)
                Swap(++i, j);

        Swap(i + 1, high);
        return i + 1;
    }

    int PartitionDescending(int low, int high)
    {
        int pivot = realItems[high];
        int i = low - 1;

        for (int j = low; j < high; j++)
            if (realItems[j] > pivot)
                Swap(++i, j);

        Swap(i + 1, high);
        return i + 1;
    }

    /* ===================== ANIMATION ===================== */

    void Swap(int a, int b)
    {
        int t = realItems[a];
        realItems[a] = realItems[b];
        realItems[b] = t;

        movements.Add(new MovementAnimation(a, b));
    }

    IEnumerator AnimateMovements(List<VisualNumberItem> items)
    {
        foreach (var m in movements)
        {
            yield return AnimateSwap(items[m.from], items[m.to]);
            yield return new WaitForSeconds(stepDelay);
        }
        isSorting = false;
    }

    IEnumerator AnimateSwap(VisualNumberItem a, VisualNumberItem b)
    {
        RectTransform ra = a.GetComponent<RectTransform>();
        RectTransform rb = b.GetComponent<RectTransform>();

        Vector3 pa = ra.localPosition;
        Vector3 pb = rb.localPosition;

        float t = 0;
        while (t < 0.25f)
        {
            t += Time.deltaTime;
            ra.localPosition = Vector3.Lerp(pa, pb, t / 0.25f);
            rb.localPosition = Vector3.Lerp(pb, pa, t / 0.25f);
            yield return null;
        }

        ra.localPosition = pa;
        rb.localPosition = pb;

        (a.value, b.value) = (b.value, a.value);
        (a.numberText.text, b.numberText.text) = (b.numberText.text, a.numberText.text);
    }
}
