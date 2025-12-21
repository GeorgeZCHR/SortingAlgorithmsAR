using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    public Toggle MinOrMax;

    void Awake()
    {
        Instance = this;
    }

    public static bool IsFromMinToMax()
    {
        return Instance.MinOrMax.isOn;
    }

    public static Sort SelectedSort;
    void Start()
    {
        SelectedSort = Sort.Bubble; //Sort.NotSelected; I want to start with BubbleSort if not selected
    }

    public void BubbleSortSelected() => SelectedSort = Sort.Bubble;

    public void SelectionSortSelected() => SelectedSort = Sort.Selection;

    public void InsertionSortSelected() => SelectedSort = Sort.Insertion;

    public void MergeSortSelected() => SelectedSort = Sort.Merge;

    public void QuickSortSelected() => SelectedSort = Sort.Quick;

    public void HeapSortSelected() => SelectedSort = Sort.Heap;
}