// Assets/Scripts/DigitManager.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DigitManager : MonoBehaviour
{
    public MockDigitRecognizer recognizer; // set in inspector
    public RectTransform numbersPanel; // parent for number items
    public GameObject visualNumberPrefab; // VisualNumberItem prefab
    public List<int> currentNumbers = new List<int>();
    private List<GameObject> spawnedItems = new List<GameObject>();

    // drag swap temporary
    private int pointerDownIndex = -1;

    void OnEnable()
    {
        recognizer.OnDigitsRecognized += OnDigitsRecognized;
    }
    void OnDisable()
    {
        recognizer.OnDigitsRecognized -= OnDigitsRecognized;
    }

    void OnDigitsRecognized(List<int> digits)
    {
        Debug.Log("OnDigitsRecognized called: " + string.Join(",", digits));
        currentNumbers = new List<int>(digits);
        RebuildUI();
    }

    void RebuildUI()
    {
        // clear old
        foreach (var g in spawnedItems) Destroy(g);
        spawnedItems.Clear();

        for (int i = 0; i < currentNumbers.Count; i++)
        {
            var go = Instantiate(visualNumberPrefab, numbersPanel);
            var v = go.GetComponent<VisualNumberItem>();
            v.SetValue(currentNumbers[i], i);
            int idxCopy = i;
            v.OnPointerDownIndex += OnItemPointerDown;
            v.OnPointerUpIndex += OnItemPointerUp;
            spawnedItems.Add(go);
        }
    }

    void OnItemPointerDown(int idx)
    {
        pointerDownIndex = idx;
    }

    void OnItemPointerUp(int idx)
    {
        if (pointerDownIndex == -1) return;
        if (pointerDownIndex == idx) { pointerDownIndex = -1; return; }
        // swap values in data
        int a = currentNumbers[pointerDownIndex];
        int b = currentNumbers[idx];
        currentNumbers[pointerDownIndex] = b;
        currentNumbers[idx] = a;

        // update UI text
        spawnedItems[pointerDownIndex].GetComponent<VisualNumberItem>().SetValue(currentNumbers[pointerDownIndex], pointerDownIndex);
        spawnedItems[idx].GetComponent<VisualNumberItem>().SetValue(currentNumbers[idx], idx);

        pointerDownIndex = -1;
    }

    // helper to get visual items list for visualizer
    public List<VisualNumberItem> GetVisualItems()
    {
        var list = new List<VisualNumberItem>();
        foreach (var g in spawnedItems) list.Add(g.GetComponent<VisualNumberItem>());
        return list;
    }
}
