using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI")]
    public Toggle MinOrMax;

    public static Sort SelectedSort;

    void Awake()
    {
        // singleton safety
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Duplicate UIManager found. Destroying the new one.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (SelectedSort == 0) // αν το enum ξεκινάει από 0, αλλιώς βγάλε το
            SelectedSort = Sort.Bubble;

        if (MinOrMax == null)
            Debug.LogError("UIManager: MinOrMax Toggle is not assigned in Inspector!");
    }

    public static bool IsFromMinToMax()
    {
        if (Instance == null || Instance.MinOrMax == null)
        {
            Debug.LogWarning("UIManager.IsFromMinToMax(): UIManager or Toggle missing. Defaulting to true.");
            return true; // default: Min→Max
        }
        return Instance.MinOrMax.isOn;
    }

    public void BubbleSortSelected() => SelectedSort = Sort.Bubble;
    public void SelectionSortSelected() => SelectedSort = Sort.Selection;
    public void InsertionSortSelected() => SelectedSort = Sort.Insertion;
    public void MergeSortSelected() => SelectedSort = Sort.Merge;
    public void QuickSortSelected() => SelectedSort = Sort.Quick;
    public void HeapSortSelected() => SelectedSort = Sort.Heap;
}
