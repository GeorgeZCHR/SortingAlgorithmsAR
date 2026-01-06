// Assets/Scripts/SortVisualizer.cs
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SortVisualizer : MonoBehaviour
{ 
    public ARNarrationController arNarration;
    public DigitManager digitManager;
    public float stepDelay = 0.6f;
    public Button startButton;
    public Button pauseButton;
    public Slider speedSlider;

    private Coroutine runningRoutine;
    private bool paused = false;

    private List<int> realItems = new List<int>();
    private List<MovementAnimation> movements = new List<MovementAnimation>();
    public enum SortType
    {
        Bubble,
        Selection,
        Insertion,
        Merge,
        Quick
    }
    IEnumerator NarrateAndAnimate(string text, IEnumerator animation)
    {
        arNarration.Show(text);          // δείχνει narration πάνω από κάρτα
        yield return WaitWhileNotPaused(stepDelay);
        yield return animation;          // παίζει 1 animation
    }

    void SetNarration(string text)
    {
        if (arNarration != null)
            arNarration.SetText(text);
    }

    public void StartSortFromCard(string code)
    {
        // 1. Assign the sort type based on the card detected
        switch (code)
        {
            case "B": UIManager.SelectedSort = Sort.Bubble; break;
            case "S": UIManager.SelectedSort = Sort.Selection; break;
            case "I": UIManager.SelectedSort = Sort.Insertion; break;
            case "M": UIManager.SelectedSort = Sort.Merge; break;
            case "Q": UIManager.SelectedSort = Sort.Quick; break;
            case "H": UIManager.SelectedSort = Sort.Heap; break;
        }

        // 2. Trigger the existing StartRun logic
        Debug.Log($"AR Card detected: Starting {UIManager.SelectedSort} sort.");
        StartRun();
    }
    /* public void StartSortFromCard(string cardLetter)
    {
        if (runningRoutine != null)
            StopCoroutine(runningRoutine);

        var items = digitManager.GetVisualItems();

        switch (cardLetter.ToUpper())
        {
            case "B":
             //   runningRoutine = StartCoroutine(BubbleSort(items));
                break;

            case "S":
            //    runningRoutine = StartCoroutine(SelectionSort(items));
                break;

            case "I":
                runningRoutine = StartCoroutine(InsertionSort(items));
                break;

            case "M":
         //       runningRoutine = StartCoroutine(MergeSort(items));
                break;

            case "Q":
           //     runningRoutine = StartCoroutine(QuickSort(items));
                break;

            default:
                Debug.LogWarning("Unknown card: " + cardLetter);
                break;
        }
    }*/

    void Start()
    {
        speedSlider.onValueChanged.AddListener((v) => stepDelay = Mathf.Lerp(0.1f, 1.0f, 1f - v)); // slider 0..1

        Debug.Log("SortVisualizer listeners assigned");
    }
   /* void MergeSortLogic(int left, int right)
    {
        if (left < right)
        {
            int mid = left + (right - left) / 2;
            MergeSortLogic(left, mid);
            MergeSortLogic(mid + 1, right);
            Merge(left, mid, right);
        }
    }

    void Merge(int left, int mid, int right)
    {
        // Temporary list to hold the merged result
        List<int> temp = new List<int>(realItems);
        int i = left, j = mid + 1, k = left;

        while (i <= mid && j <= right)
        {
            if (temp[i] <= temp[j])
                realItems[k++] = temp[i++];
            else
            {
                // Record a movement for the visualizer
                // In a real merge sort, this isn't a "swap," 
                // but for your AnimateSwap system, we treat it as one.
                movements.Add(new MovementAnimation(k, j));
                realItems[k++] = temp[j++];
            }
        }

        while (i <= mid) realItems[k++] = temp[i++];
    }*/

    private void CreateRealItems(List<VisualNumberItem> items)
    {
        movements.Clear();
        realItems.Clear();

        for (int i = 0; i < items.Count; i++)
            realItems.Add(items[i].value);

        Debug.Log(string.Join(", ", realItems));
    }

    public void StartRun()
    {
        Debug.Log("StartRun called at frame: " + Time.frameCount);
        if (runningRoutine != null) StopCoroutine(runningRoutine);

        var items = digitManager.GetVisualItems();
        CreateRealItems(items);
        //digitManager.ChangeValueInArrayField(digitManager.ArrayField.text);
        RunAlgorithm();
        runningRoutine = StartCoroutine(AnimateMovements(items));
    }

    // Bad practise i know
    public void ResetArrayField()
    {
        digitManager.ArrayField.text = digitManager.ArrayField.text + " ";
    }

    public void TogglePause() => paused = !paused;

    void RunAlgorithm()
    {
        Debug.Log("Sort : " + Helper.GetSortName());

        if (UIManager.IsFromMinToMax())
        {
            switch (UIManager.SelectedSort)
            {
                case Sort.Bubble:           BubbleSortMinToMax(); break;
                case Sort.Selection: SelectionSortMinToMax(); break;
                //case Sort.Insertion:    yield return StartCoroutine(InsertionSort(items)); break;
                //case Sort.Merge:        yield return StartCoroutine(MergeSort(items)); break;
                //case Sort.Quick:        yield return StartCoroutine(QuickSort(items)); break;
                //case Sort.Heap:         yield return StartCoroutine(HeapSort(items)); break;
                default:                    BubbleSortMinToMax(); break;
            }
        }
        else
        {
            switch (UIManager.SelectedSort)
            {
                case Sort.Bubble: BubbleSortMinToMax(); break;
                case Sort.Selection: SelectionSortMinToMax(); break;
                //case Sort.Insertion:    yield return StartCoroutine(InsertionSort(items)); break;
                //case Sort.Merge:        yield return StartCoroutine(MergeSort(items)); break;
                //case Sort.Quick:        yield return StartCoroutine(QuickSort(items)); break;
                //case Sort.Heap:         yield return StartCoroutine(HeapSort(items)); break;
                default: BubbleSortMinToMax(); break;
            }
        }

        PrintRealItems();
    }

    void PrintRealItems()
    {
        Debug.Log(string.Join(", ", realItems));
        Debug.Log("--------------------------------------");
    }
    IEnumerator AnimateMovements(List<VisualNumberItem> items)
    {
        for (int i = 0; i < movements.Count; i++)
        {
            // Αν θέλεις το Narration να αλλάζει ανάλογα με την κίνηση, 
            // θα πρέπει να αποθηκεύεις και το κείμενο στη λίστα movements.
            yield return AnimateSwap(items[movements[i].from], items[movements[i].to]);
            yield return WaitWhileNotPaused(stepDelay); // Καθυστέρηση μεταξύ κινήσεων
        }
    }
    /* IEnumerator AnimateMovements(List<VisualNumberItem> items)
     {
         for (int i = 0; i < movements.Count; i++)
         {
             yield return AnimateSwap(items[movements[i].from], items[movements[i].to]);
         }
     }*/

    IEnumerator WaitWhileNotPaused(float t)
    {
        float elapsed = 0;
        while (elapsed < t)
        {
            if (!paused)
                elapsed += Time.deltaTime;
            yield return null;
        }
    }
    /*

    void BubbleSortMinToMax()
    {
        int length = realItems.Count;
        bool swapped;

        for (int i = 0; i < length - 1; i++)
        {
            swapped = false;

            for (int j = 0; j < length - i - 1; j++)
            {
                if (realItems[j] > realItems[j +1])
                {
                    swapped = true;

                    var temp = realItems[j];
                    realItems[j] = realItems[j + 1];
                    realItems[j + 1] = temp;

                    movements.Add(new MovementAnimation(j, j + 1));
                }
            }

            if (!swapped) break;
        }
    }
    /* IEnumerator BubbleSortMinToMax(List<VisualNumberItem> items)
     {
         int n = items.Count;
         for (int i = 0; i < n - 1; i++)
         {
             for (int j = 0; j < n - i - 1; j++)
             {
                 if (items[j].value > items[j + 1].value)
                 {
                     yield return AnimateSwap(items[j], items[j + 1]);

                     // IMPORTANT: swap references in the list
                     var tmp = items[j];
                     items[j] = items[j + 1];
                     items[j + 1] = tmp;
                 }
             }
         }
     }

     void BubbleSortMaxToMin()
     {
         int length = realItems.Count;
         bool swapped;

         for (int i = 0; i < length - 1; i++)
         {
             swapped = false;

             for (int j = 0; j < length - i - 1; j++)
             {
                 if (realItems[j] < realItems[j + 1])
                 {
                     swapped = true;

                     var temp = realItems[j];
                     realItems[j] = realItems[j + 1];
                     realItems[j + 1] = temp;

                     movements.Add(new MovementAnimation(j, j + 1));
                 }
             }

             if (!swapped) break;
         }
     }*/
    void BubbleSortMinToMax()
    {
        SetNarration("Ξεκινάμε την Bubble Sort συγκρίνοντας γειτονικά στοιχεία.");

        int length = realItems.Count;
        bool swapped;

        for (int i = 0; i < length - 1; i++)
        {
            swapped = false;
            SetNarration($"Πέρασμα {i + 1}: σύγκριση στοιχείων.");

            for (int j = 0; j < length - i - 1; j++)
            {
                if (realItems[j] > realItems[j + 1])
                {
                    SetNarration($"Ανταλλαγή {realItems[j]} και {realItems[j + 1]}.");

                    swapped = true;
                    var temp = realItems[j];
                    realItems[j] = realItems[j + 1];
                    realItems[j + 1] = temp;

                    movements.Add(new MovementAnimation(j, j + 1));
                }
            }

            if (!swapped)
            {
                SetNarration("Δεν έγιναν άλλες ανταλλαγές. Ο πίνακας είναι ταξινομημένος.");
                break;
            }
        }

        SetNarration("Η Bubble Sort ολοκληρώθηκε.");
    }

/*
    void SelectionSortMinToMax()
    {
        int lenght = realItems.Count;
        for (int i = 0; i < lenght - 1; i++)
        {
            int minIdx = i;
            for (int j = i + 1; j < lenght; j++)
            {
                //items[minIdx].Highlight(false);
                //items[j].Highlight(true);
                //yield return WaitWhileNotPausedCoroutine(stepDelay);
                if (realItems[j] < realItems[minIdx])
                {
                    //items[minIdx].Highlight(false);
                    minIdx = j;
                }
                else
                {
                    //items[j].Highlight(false);
                }
            }
            if (minIdx != i)
            {
                //yield return StartCoroutine(AnimateSwap(items[i], items[minIdx]));
                var tmp = realItems[i];
                realItems[i] = realItems[minIdx];
                realItems[minIdx] = tmp;

                movements.Add(new MovementAnimation(i, minIdx));
                //items[i].index = i;
                //items[minIdx].index = minIdx;
            }
        }
    }*/
    void SelectionSortMinToMax()
    {
        SetNarration("Ξεκινάμε την Selection Sort. Σε κάθε βήμα βρίσκουμε το μικρότερο στοιχείο.");

        int length = realItems.Count;

        for (int i = 0; i < length - 1; i++)
        {
            int minIdx = i;
            SetNarration($"Θέση {i + 1}: αναζήτηση του μικρότερου στοιχείου.");

            for (int j = i + 1; j < length; j++)
            {
                if (realItems[j] < realItems[minIdx])
                {
                    minIdx = j;
                    SetNarration($"Νέο μικρότερο στοιχείο: {realItems[minIdx]}.");
                }
            }

            if (minIdx != i)
            {
                SetNarration($"Ανταλλαγή {realItems[i]} με {realItems[minIdx]}.");

                var tmp = realItems[i];
                realItems[i] = realItems[minIdx];
                realItems[minIdx] = tmp;

                movements.Add(new MovementAnimation(i, minIdx));
            }
        }

        SetNarration("Η Selection Sort ολοκληρώθηκε.");
    }

    /* void SelectionSortMaxToMin()
     {
         int lenght = realItems.Count;
         for (int i = lenght - 1; i > 0; i--)
         {
             int minIdx = 0;
             for (int j = 1; j <= i; j++)
             {
                 if (realItems[j] < realItems[minIdx])
                 {
                     minIdx = j;
                 }
                 else
                 {

                 }
             }

             if (minIdx != i)
             {
                 var tmp = realItems[i];
                 realItems[i] = realItems[minIdx];
                 realItems[minIdx] = tmp;

                 movements.Add(new MovementAnimation(i, minIdx));
             }
         }
     }
     */
    /*
    IEnumerator InsertionSort(List<VisualNumberItem> items)
    {
        int n = items.Count;
        for (int i = 1; i < n; i++)
        {
            var keyItem = items[i];
            int j = i - 1;
            keyItem.Highlight(true);
            yield return WaitWhileNotPausedCoroutine(stepDelay);
            while (j >= 0 && items[j].value > keyItem.value)
            {
                // shift right
                yield return StartCoroutine(AnimateMove(items[j], j + 1));
                items[j + 1] = items[j];
                items[j + 1].index = j + 1;
                j--;
            }
            // place key
            items[j + 1] = keyItem;
            keyItem.index = j + 1;
            keyItem.Highlight(false);
        }
    }
    IEnumerator InsertionSort(List<VisualNumberItem> items)
    {
        SetNarration("Ξεκινάμε την Insertion Sort.");

        for (int i = 1; i < items.Count; i++)
        {
            var key = items[i];
            int j = i - 1;

            SetNarration($"Επιλέγουμε το στοιχείο {key.value} για εισαγωγή.");

            while (j >= 0 && items[j].value > key.value)
            {
                SetNarration($"Το {items[j].value} μετακινείται δεξιά.");
                yield return AnimateMove(items[j], j + 1);
                j--;
            }

            SetNarration($"Τοποθέτηση του {key.value} στη σωστή θέση.");
        }

        SetNarration("Η Insertion Sort ολοκληρώθηκε.");
    }
    */
    void InsertionSortLogic() // Μετατροπή σε void
    {
        SetNarration("Ξεκινάμε την Insertion Sort.");
        int n = realItems.Count;
        for (int i = 1; i < n; i++)
        {
            int key = realItems[i];
            int j = i - 1;

            // Καταγραφή της κίνησης
            while (j >= 0 && realItems[j] > key)
            {
                movements.Add(new MovementAnimation(j + 1, j)); // Καταγραφή ανταλλαγής
                realItems[j + 1] = realItems[j];
                j--;
            }
            realItems[j + 1] = key;
        }
        SetNarration("Η Insertion Sort ολοκληρώθηκε.");
    }
    void MergeSortLogic(int left, int right)
    {
        if (left < right)
        {
            int mid = left + (right - left) / 2;

            SetNarration("Διαίρεση του πίνακα σε δύο μέρη.");

            MergeSortLogic(left, mid);
            MergeSortLogic(mid + 1, right);

            SetNarration("Συγχώνευση των ταξινομημένων υποπινάκων.");
            Merge(left, mid, right);
        }
    }
    void Merge(int left, int mid, int right)
    {
        SetNarration("Σύγκριση στοιχείων και συγχώνευση.");

        List<int> temp = new List<int>(realItems);
        int i = left, j = mid + 1, k = left;

        while (i <= mid && j <= right)
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
    void QuickSort(int low, int high)
    {
        if (low < high)
        {
            SetNarration("Επιλογή pivot και διαχωρισμός στοιχείων.");

            int pi = Partition(low, high);

            QuickSort(low, pi - 1);
            QuickSort(pi + 1, high);
        }
    }
    int Partition(int low, int high)
    {
        int pivot = realItems[high];
        SetNarration($"Pivot: {pivot}");

        int i = low - 1;

        for (int j = low; j < high; j++)
        {
            if (realItems[j] < pivot)
            {
                i++;
                movements.Add(new MovementAnimation(i, j));
            }
        }

        movements.Add(new MovementAnimation(i + 1, high));
        return i + 1;
    }

    IEnumerator WaitWhileNotPausedCoroutine(float t)
    {
        float remaining = t;
        while (remaining > 0)
        {
            if (!paused) remaining -= Time.deltaTime;
            yield return null;
        }
    }

    void HighlightPair(List<VisualNumberItem> items, int a, int b, bool on)
    {
        items[a].Highlight(on);
        items[b].Highlight(on);
    }

    IEnumerator AnimateSwap(VisualNumberItem a, VisualNumberItem b)
    {
        RectTransform ra = a.GetComponent<RectTransform>();
        RectTransform rb = b.GetComponent<RectTransform>();
        Vector3 pa = ra.localPosition;
        Vector3 pb = rb.localPosition;
        float t = 0; float dur = 0.25f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float f = t / dur;
            ra.localPosition = Vector3.Lerp(pa, pb, f);
            rb.localPosition = Vector3.Lerp(pb, pa, f);
            yield return null;
        }
        ra.localPosition = pa; rb.localPosition = pb; // keep original transforms as layout will handle positions
        // After animation, force UI text swap instead of transform swap (layout group will reposition)
        string ta = a.numberText.text;
        a.numberText.text = b.numberText.text;
        b.numberText.text = ta;
        int tmp = a.value; a.value = b.value; b.value = tmp;
    }

    IEnumerator AnimateMove(VisualNumberItem item, int targetIndex)
    {
        // simple placeholder: you can animate position; layout group may fight it.
        yield return WaitWhileNotPausedCoroutine(stepDelay * 0.5f);
    }
}
