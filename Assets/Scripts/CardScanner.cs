
using UnityEngine;
public class CardScanner : MonoBehaviour
{
    public SortVisualizer sortVisualizer;

    public void OnCardDetected(string letter)
    {
        Debug.Log("Scanned card: " + letter);
        sortVisualizer.StartSortFromCard(letter);
    }
}
