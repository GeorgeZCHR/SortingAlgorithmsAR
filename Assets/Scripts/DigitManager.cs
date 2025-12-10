// Assets/Scripts/DigitManager.cs
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DigitManager : MonoBehaviour
{
    //public MockDigitRecognizer recognizer; // set in inspector
    public RectTransform numbersPanel; // parent for number items
    public GameObject visualNumberPrefab; // VisualNumberItem prefab
    public List<int> currentNumbers = new List<int>();
    public TMP_InputField ArrayField;

    private string inputText;
    private List<GameObject> spawnedItems = new List<GameObject>();

    // drag swap temporary
    private int pointerDownIndex = -1;

    //void OnEnable()
    //{
    //    recognizer.OnDigitsRecognized += OnDigitsRecognized;
    //}
    //void OnDisable()
    //{
    //    recognizer.OnDigitsRecognized -= OnDigitsRecognized;
    //}

    
    void Start()
    {
        ArrayField.onValueChanged.AddListener(value => {
            inputText = value;
            currentNumbers = Helper.GetNumbersFromString(inputText);
            Debug.Log(currentNumbers);
            RebuildUI();
        });
        //currentNumbers = recognizer.GetInitialDigits();
        ////Debug.Log("OnDigitsRecognized called: " + string.Join(",", currentNumbers));    
        //RebuildUI();
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
            Debug.Log(currentNumbers[i]);
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
