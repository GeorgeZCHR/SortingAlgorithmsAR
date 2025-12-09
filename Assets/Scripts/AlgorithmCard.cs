// Assets/Scripts/AlgorithmCard.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class AlgorithmCard : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public Button selectButton;
    public string algorithmId;
    public Action<string> OnSelected;

    void Start()
    {
        if (selectButton != null)
            selectButton.onClick.AddListener(() => OnSelected?.Invoke(algorithmId));
    }

    public void SetTitle(string t)
    {
        if (titleText != null)
            titleText.text = t;
    }

    public void SetActiveVisual(bool active)
    {
        var img = GetComponent<Image>();
        if (img != null)
            img.color = active ? new Color(0.0f, 0.6f, 1.0f, 1f) : Color.white;

        transform.localScale = active ? Vector3.one * 1.05f : Vector3.one;
    }
}
