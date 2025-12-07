// Assets/Scripts/AlgorithmCardsManager.cs
using System;
using System.Collections.Generic;
using UnityEngine;

public class AlgorithmCardsManager : MonoBehaviour
{
    [Tooltip("All AlgorithmCard instances (children of CardsPanel). Order not important.")]
    public AlgorithmCard[] cards;

    void Start()
    {
        // Subscribe each card's OnSelected to our handler
        foreach (var c in cards)
        {
            // ensure not null
            if (c == null) continue;
            c.OnSelected += OnCardSelected;
        }

        // Optionally initialize selection to first card
        if (cards != null && cards.Length > 0 && cards[0] != null)
        {
            SetSelected(cards[0].algorithmId);
        }
    }

    void OnDestroy()
    {
        // Unsubscribe to avoid leaks
        foreach (var c in cards)
            if (c != null) c.OnSelected -= OnCardSelected;
    }

    private void OnCardSelected(string algorithmId)
    {
        SetSelected(algorithmId);
    }

    public void SetSelected(string algorithmId)
    {
        AlgorithmSelector.SelectedAlgorithm = algorithmId;
        // update visuals
        foreach (var c in cards)
        {
            if (c == null) continue;
            c.SetActiveVisual(c.algorithmId == algorithmId);
        }
        Debug.Log($"Algorithm selected: {algorithmId}");
    }

    // helper you can call from inspector via UnityEvent if desired
    public void SelectById(string id) => SetSelected(id);
}
