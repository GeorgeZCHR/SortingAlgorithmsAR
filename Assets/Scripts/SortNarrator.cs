using UnityEngine;
using TMPro;

public class SortNarrator : MonoBehaviour
{
    public TextMeshProUGUI narrationText;

    public void SetText(string text)
    {
        narrationText.text = text;
    }

    public void Clear()
    {
        narrationText.text = "";
    }
}
