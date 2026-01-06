using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DigitManager : MonoBehaviour
{
    public RectTransform numbersPanel;            // parent for number items
    public GameObject visualNumberPrefab;         // VisualNumberItem prefab
    public List<int> currentNumbers = new List<int>();
    public TMP_InputField ArrayField;

    [Header("Limits")]
    public int maxNumbers = 12;

    private List<GameObject> spawnedItems = new List<GameObject>();

    // drag swap temporary
    private int pointerDownIndex = -1;

    void Start()
    {
        if (ArrayField != null)
            ArrayField.onValueChanged.AddListener(ChangeValueInArrayField);
    }

    public void ChangeValueInArrayField(string value)
    {
        // Parse as before
        var parsed = Helper.GetNumbersFromString(value);

        // Limit amount (DO NOT modify the input text)
        if (parsed.Count > maxNumbers)
            parsed = parsed.GetRange(0, maxNumbers);

        currentNumbers = parsed;

        RebuildUI();
    }

    void RebuildUI()
    {
        // clear old
        foreach (var g in spawnedItems)
            Destroy(g);

        spawnedItems.Clear();

        if (numbersPanel == null || visualNumberPrefab == null) return;

        for (int i = 0; i < currentNumbers.Count; i++)
        {
            var go = Instantiate(visualNumberPrefab, numbersPanel);
            var v = go.GetComponent<VisualNumberItem>();
            if (v != null)
            {
                v.SetValue(currentNumbers[i], i);
                v.OnPointerDownIndex += OnItemPointerDown;
                v.OnPointerUpIndex += OnItemPointerUp;
            }
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
        var va = spawnedItems[pointerDownIndex].GetComponent<VisualNumberItem>();
        var vb = spawnedItems[idx].GetComponent<VisualNumberItem>();

        if (va != null) va.SetValue(currentNumbers[pointerDownIndex], pointerDownIndex);
        if (vb != null) vb.SetValue(currentNumbers[idx], idx);

        pointerDownIndex = -1;
    }

    // helper to get visual items list for visualizer
    public List<VisualNumberItem> GetVisualItems()
    {
        var list = new List<VisualNumberItem>();
        foreach (var g in spawnedItems)
        {
            var v = g.GetComponent<VisualNumberItem>();
            if (v != null) list.Add(v);
        }
        return list;
    }
}
