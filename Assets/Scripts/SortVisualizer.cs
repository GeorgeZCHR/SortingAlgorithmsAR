// Assets/Scripts/SortVisualizer.cs
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SortVisualizer : MonoBehaviour
{
    public DigitManager digitManager;
    public float stepDelay = 0.6f;
    public Button startButton;
    public Button pauseButton;
    public Slider speedSlider;

    private Coroutine runningRoutine;
    private bool paused = false;

    private List<MovementAnimation> movements = new List<MovementAnimation>();

    void Start()
    {
        speedSlider.onValueChanged.AddListener((v) => stepDelay = Mathf.Lerp(0.1f, 1.0f, 1f - v)); // slider 0..1

        Debug.Log("SortVisualizer listeners assigned");
    }

    public void StartRun()
    {
        if (runningRoutine != null) StopCoroutine(runningRoutine);

        var items = digitManager.GetVisualItems();
        Debug.Log("Sort : " + Helper.GetSortName());

        //Debug.Log("--------------------------------------");
        //foreach (VisualNumberItem item in items)
        //    Debug.Log(item.value + ", ");
        //Debug.Log("--------------------------------------");

        movements.Clear();
        //digitManager.ChangeValueInArrayField(digitManager.ArrayField.text);

        //runningRoutine = StartCoroutine(RunAlgorithm(items));
        if (UIManager.IsFromMinToMax())
        {
            BubbleSortMinToMax(items);
        }
        else
        {
            BubbleSortMaxToMin(items);
        }

        runningRoutine = StartCoroutine(AnimateMovements(items));
    }

    // Bad practise i know
    public void ResetArrayField()
    {
        digitManager.ArrayField.text = digitManager.ArrayField.text + " ";
    }

    public void TogglePause() => paused = !paused;

    IEnumerator RunAlgorithm(List<VisualNumberItem> items)
    {
        switch (UIManager.SelectedSort)
        {
            //case Sort.Bubble:       yield return StartCoroutine(BubbleSort(items)); break;
            case Sort.Selection:    yield return StartCoroutine(SelectionSort(items)); break;
            case Sort.Insertion:    yield return StartCoroutine(InsertionSort(items)); break;
            //case Sort.Merge:        yield return StartCoroutine(MergeSort(items)); break;
            //case Sort.Quick:        yield return StartCoroutine(QuickSort(items)); break;
            //case Sort.Heap:         yield return StartCoroutine(HeapSort(items)); break;
            //default:                yield return StartCoroutine(BubbleSort(items)); break;
        }
    }

    IEnumerator AnimateMovements(List<VisualNumberItem> items)
    {
        for (int i = 0; i < movements.Count; i++)
        {
            yield return AnimateSwap(items[movements[i].from], items[movements[i].to]);
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

    void BubbleSortMinToMax(List<VisualNumberItem> items)
    {
        int length = items.Count;
        bool swapped;

        List<int> realItems = new List<int>();
        for (int i = 0; i < length; i++) realItems.Add(items[i].value);

        Debug.Log(string.Join(", ", realItems));

        for (int i = 0; i < length - 1; i++)
        {
            swapped = false;

            for (int j = 0; j < length - i - 1; j++)
            {
                //HighlightPair(items, j, j + 1, true);
                //yield return WaitWhileNotPausedCoroutine(stepDelay);
                //if (items[j].value > items[j + 1].value)
                //{
                //    yield return StartCoroutine(AnimateSwap(items[j], items[j + 1]));
                //    // swap in list
                //    var tmp = items[j];
                //    items[j] = items[j + 1];
                //    items[j + 1] = tmp;

                //    // update indexes
                //    items[j].index = j;
                //    items[j + 1].index = j + 1;

                //    swapped = true;
                //}
                //HighlightPair(items, j, j + 1, false);
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

        Debug.Log(string.Join(", ", realItems));
        Debug.Log("--------------------------------------");
    }

    void BubbleSortMaxToMin(List<VisualNumberItem> items)
    {
        int length = items.Count;
        bool swapped;

        List<int> realItems = new List<int>();
        for (int i = 0; i < length; i++) realItems.Add(items[i].value);

        Debug.Log(string.Join(", ", realItems));

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

        Debug.Log(string.Join(", ", realItems));
        Debug.Log("--------------------------------------");
    }

    IEnumerator SelectionSort(List<VisualNumberItem> items)
    {
        int n = items.Count;
        for (int i = 0; i < n - 1; i++)
        {
            int minIdx = i;
            for (int j = i + 1; j < n; j++)
            {
                items[minIdx].Highlight(false);
                items[j].Highlight(true);
                yield return WaitWhileNotPausedCoroutine(stepDelay);
                if (items[j].value < items[minIdx].value)
                {
                    items[minIdx].Highlight(false);
                    minIdx = j;
                }
                else
                {
                    items[j].Highlight(false);
                }
            }
            if (minIdx != i)
            {
                yield return StartCoroutine(AnimateSwap(items[i], items[minIdx]));
                var tmp = items[i]; items[i] = items[minIdx]; items[minIdx] = tmp;
                items[i].index = i; items[minIdx].index = minIdx;
            }
        }
    }

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