using UnityEngine;

public class UIManager : MonoBehaviour
{

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