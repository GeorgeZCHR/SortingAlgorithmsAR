using System.Collections.Generic;
using System.Linq;

public static class Helper
{
    public static string GetSortName()
    {
        switch (UIManager.SelectedSort)
        {
            case Sort.Bubble: return "Bubble";
            case Sort.Selection: return "Selection";
            case Sort.Insertion: return "Insertion";
            case Sort.Merge: return "Merge";
            case Sort.Quick: return "Quick";
            case Sort.Heap: return "Heap";
            default: return "NotSelected";
        }
    }

    public static List<int> GetNumbersFromString(string text)
    {
        if (string.IsNullOrEmpty(text))
            return new List<int>();

        return text
            .Split(',')
            .Select(s => s.Trim())
            .Where(s => s.Length > 0)
            .Select(int.Parse)
            .ToList();
    }
}
