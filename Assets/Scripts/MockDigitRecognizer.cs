// Assets/Scripts/MockDigitRecognizer.cs
using System;
using System.Collections.Generic;
using UnityEngine;

public class MockDigitRecognizer : MonoBehaviour
{
    // Event: μεταδίδει ανιχνευμένους αριθμούς (π.χ. [6,3,8])
    public Action<List<int>> OnDigitsRecognized;

    // For testing: initial set or generate random
    public List<int> initialDigits = new List<int> { 6, 3, 8 };

    void Start()
    {
        // μικρή καθυστέρηση για να φορτώσει σκηνή
        Invoke(nameof(EmitInitial), 0.5f);
    }

    void EmitInitial()
    {
        OnDigitsRecognized?.Invoke(new List<int>(initialDigits));
    }

    // Public method: call to re-scan (for UI button)
    public void Rescan()
    {
        // για το mock: απλά re-emit τα ίδια (ή random αν θες)
        OnDigitsRecognized?.Invoke(new List<int>(initialDigits));
    }

    // For convenience: set digits programmatically (όταν ενσωματώσεις OCR, θα καλέσεις αυτό)
    public void SetDigits(List<int> digits)
    {
        initialDigits = new List<int>(digits);
        OnDigitsRecognized?.Invoke(new List<int>(initialDigits));
    }
}
