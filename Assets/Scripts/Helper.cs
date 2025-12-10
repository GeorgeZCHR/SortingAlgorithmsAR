using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Windows;

public static class Helper
{
    public static string GetSortName()
    {
        switch (UIManager.SelectedSort)
        {
            case Sort.Bubble:       return "Bubble";
            case Sort.Selection:    return "Selection";
            case Sort.Insertion:    return "Insertion";
            case Sort.Merge:        return "Merge";
            case Sort.Quick:        return "Quick";
            case Sort.Heap:         return "Heap";
            default:                return "NotSelected";
        }
    }

    public static List<int> GetNumbersFromString(string text)
    {
        if (string.IsNullOrEmpty(text))
            return new List<int>();

        return text
            .Split(',')                       // split by comma
            .Select(s => s.Trim())            // remove spaces
            .Where(s => s.Length > 0)         // ignore empty entries
            .Select(int.Parse)                // convert to int (throws if invalid)
            .ToList();
    }
}
