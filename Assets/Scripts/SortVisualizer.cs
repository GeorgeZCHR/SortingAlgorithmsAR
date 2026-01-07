using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SortVisualizer : MonoBehaviour
{
    public ARNarrationController arNarration;
    public DigitManager digitManager;
    public float stepDelay = 0.6f;

    private Coroutine runningRoutine;
    private bool paused = false;

    private List<int> realItems = new List<int>();
    private List<MovementAnimation> movements = new List<MovementAnimation>();

    void Start()
    {
        Debug.Log("SortVisualizer initialized");
    }
    // Called from AR Image Tracker
    public void StartSortFromCard(string code)
    {
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
                return;
        }

        Debug.Log($"AR Card detected → Starting {UIManager.SelectedSort} sort");
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
        bool ascending = UIManager.IsFromMinToMax();

        switch (UIManager.SelectedSort)
        {
            case Sort.Bubble:
                if (ascending) BubbleSortAscending();
                else BubbleSortDescending();
                break;

            case Sort.Selection:
                if (ascending) SelectionSortAscending();
                else SelectionSortDescending();
                break;

            case Sort.Insertion:
                if (ascending) InsertionSortAscending();
                else InsertionSortDescending();
                break;

            case Sort.Merge:
                if (ascending) MergeSortAscending(0, realItems.Count - 1);
                else MergeSortDescending(0, realItems.Count - 1);
                break;

            case Sort.Quick:
                if (ascending) QuickSortAscending(0, realItems.Count - 1);
                else QuickSortDescending(0, realItems.Count - 1);
                break;
        }

        Debug.Log("Sorted array: " + string.Join(", ", realItems));
    }

    /* ===================== BUBBLE SORT ===================== */

    void BubbleSortAscending()
    {
        SetNarration("Starting Bubble Sort in ascending order.");

        for (int i = 0; i < realItems.Count - 1; i++)
        {
            for (int j = 0; j < realItems.Count - i - 1; j++)
            {
                if (realItems[j] > realItems[j + 1])
                {
                    SetNarration($"Swapping {realItems[j]} and {realItems[j + 1]}.");

                    Swap(j, j + 1);
                }
            }
        }

        SetNarration("Bubble Sort completed.");
    }

    void BubbleSortDescending()
    {
        SetNarration("Starting Bubble Sort in descending order.");

        for (int i = 0; i < realItems.Count - 1; i++)
        {
            for (int j = 0; j < realItems.Count - i - 1; j++)
            {
                if (realItems[j] < realItems[j + 1])
                {
                    SetNarration($"Swapping {realItems[j]} and {realItems[j + 1]}.");

                    Swap(j, j + 1);
                }
            }
        }

        SetNarration("Bubble Sort completed.");
    }

    /* ===================== SELECTION SORT ===================== */

    void SelectionSortAscending()
    {
        SetNarration("Starting Selection Sort in ascending order.");

        for (int i = 0; i < realItems.Count - 1; i++)
        {
            int min = i;

            for (int j = i + 1; j < realItems.Count; j++)
            {
                if (realItems[j] < realItems[min])
                {
                    min = j;
                    SetNarration($"New minimum found: {realItems[min]}.");
                }
            }

            if (min != i)
                Swap(i, min);
        }

        SetNarration("Selection Sort completed.");
    }

    void SelectionSortDescending()
    {
        SetNarration("Starting Selection Sort in descending order.");

        for (int i = 0; i < realItems.Count - 1; i++)
        {
            int max = i;

            for (int j = i + 1; j < realItems.Count; j++)
            {
                if (realItems[j] > realItems[max])
                {
                    max = j;
                    SetNarration($"New maximum found: {realItems[max]}.");
                }
            }

            if (max != i)
                Swap(i, max);
        }

        SetNarration("Selection Sort completed.");
    }

    /* ===================== INSERTION SORT ===================== */

    void InsertionSortAscending()
    {
        SetNarration("Starting Insertion Sort in ascending order.");

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

        SetNarration("Insertion Sort completed.");
    }

    void InsertionSortDescending()
    {
        SetNarration("Starting Insertion Sort in descending order.");

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

        SetNarration("Insertion Sort completed.");
    }

    /* ===================== MERGE SORT ===================== */

    void MergeSortAscending(int left, int right)
    {
        if (left >= right) return;

        int mid = (left + right) / 2;
        MergeSortAscending(left, mid);
        MergeSortAscending(mid + 1, right);
        MergeAscending(left, mid, right);
    }

    void MergeSortDescending(int left, int right)
    {
        if (left >= right) return;

        int mid = (left + right) / 2;
        MergeSortDescending(left, mid);
        MergeSortDescending(mid + 1, right);
        MergeDescending(left, mid, right);
    }

    void MergeAscending(int l, int m, int r)
    {
        List<int> temp = new List<int>(realItems);
        int i = l, j = m + 1, k = l;

        while (i <= m && j <= r)
        {
            if (temp[i] <= temp[j])
                realItems[k++] = temp[i++];
            else
            {
                movements.Add(new MovementAnimation(k, j));
                realItems[k++] = temp[j++];
            }
        }
    }

    void MergeDescending(int l, int m, int r)
    {
        List<int> temp = new List<int>(realItems);
        int i = l, j = m + 1, k = l;

        while (i <= m && j <= r)
        {
            if (temp[i] >= temp[j])
                realItems[k++] = temp[i++];
            else
            {
                movements.Add(new MovementAnimation(k, j));
                realItems[k++] = temp[j++];
            }
        }
    }

    /* ===================== QUICK SORT ===================== */

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
        {
            if (realItems[j] < pivot)
            {
                i++;
                Swap(i, j);
            }
        }

        Swap(i + 1, high);
        return i + 1;
    }

    int PartitionDescending(int low, int high)
    {
        int pivot = realItems[high];
        int i = low - 1;

        for (int j = low; j < high; j++)
        {
            if (realItems[j] > pivot)
            {
                i++;
                Swap(i, j);
            }
        }

        Swap(i + 1, high);
        return i + 1;
    }

    /* ===================== UTILITIES ===================== */

    void Swap(int a, int b)
    {
        int temp = realItems[a];
        realItems[a] = realItems[b];
        realItems[b] = temp;

        movements.Add(new MovementAnimation(a, b));
    }

    IEnumerator AnimateMovements(List<VisualNumberItem> items)
    {
        foreach (var m in movements)
        {
            yield return AnimateSwap(items[m.from], items[m.to]);
            yield return WaitWhileNotPaused(stepDelay);
        }
    }

    IEnumerator WaitWhileNotPaused(float t)
    {
        float elapsed = 0;
        while (elapsed < t)
        {
            if (!paused) elapsed += Time.deltaTime;
            yield return null;
        }
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
            float f = t / 0.25f;
            ra.localPosition = Vector3.Lerp(pa, pb, f);
            rb.localPosition = Vector3.Lerp(pb, pa, f);
            yield return null;
        }

        ra.localPosition = pa;
        rb.localPosition = pb;

        string txt = a.numberText.text;
        a.numberText.text = b.numberText.text;
        b.numberText.text = txt;

        int v = a.value;
        a.value = b.value;
        b.value = v;
    }
}
