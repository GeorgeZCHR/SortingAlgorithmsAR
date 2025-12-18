// Assets/Scripts/VisualNumberItem.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using TMPro;

[RequireComponent(typeof(RectTransform))]
public class VisualNumberItem : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("UI References")]
    public TextMeshProUGUI numberText;
    public Image background;

    [Header("Data")]
    public int value;
    public int index; // θέση στον πίνακα

    // Events για να τις διαβάσει ο DigitManager
    public Action<int> OnPointerDownIndex;
    public Action<int> OnPointerUpIndex;

    public void SetValue(int v, int idx)
    {
        value = v;
        index = idx;
        if (numberText != null) numberText.text = v.ToString();
    }

    public void Highlight(bool on)
    {
        if (background != null) background.color = on ? Color.yellow : Color.white;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnPointerDownIndex?.Invoke(index);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        OnPointerUpIndex?.Invoke(index);
    }
}