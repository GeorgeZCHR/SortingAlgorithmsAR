// Assets/Scripts/AlgorithmSelectionUI.cs
using UnityEngine;
using UnityEngine.UI;

public class AlgorithmSelectionUI : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button bubbleButton;
    [SerializeField] private Button selectionButton;
    [SerializeField] private Button insertionButton;
    [SerializeField] private Button mergeButton;
    [SerializeField] private Button quickButton;
    [SerializeField] private Button heapButton;

    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = new Color(0.85f, 0.95f, 1f, 1f);

    private void OnEnable()
    {
        UIManager.OnSelectionChanged += OnSelectionChanged;
        OnSelectionChanged(UIManager.SelectedSort, UIManager.LastSource);
    }

    private void OnDisable()
    {
        UIManager.OnSelectionChanged -= OnSelectionChanged;
    }

    private void OnSelectionChanged(Sort sort, UIManager.SelectionSource source)
    {
        // Highlight buttons based ONLY on SelectedSort (AR overrides will repaint here too)
        SetAllButtonsColor(normalColor);

        switch (sort)
        {
            case Sort.Bubble: SetButtonColor(bubbleButton, selectedColor); break;
            case Sort.Selection: SetButtonColor(selectionButton, selectedColor); break;
            case Sort.Insertion: SetButtonColor(insertionButton, selectedColor); break;
            case Sort.Merge: SetButtonColor(mergeButton, selectedColor); break;
            case Sort.Quick: SetButtonColor(quickButton, selectedColor); break;
            case Sort.Heap: SetButtonColor(heapButton, selectedColor); break;
        }
    }

    private void SetAllButtonsColor(Color c)
    {
        SetButtonColor(bubbleButton, c);
        SetButtonColor(selectionButton, c);
        SetButtonColor(insertionButton, c);
        SetButtonColor(mergeButton, c);
        SetButtonColor(quickButton, c);
        SetButtonColor(heapButton, c);
    }

    private void SetButtonColor(Button btn, Color c)
    {
        if (btn == null) return;
        var img = btn.GetComponent<Image>();
        if (img != null) img.color = c;
    }
}
