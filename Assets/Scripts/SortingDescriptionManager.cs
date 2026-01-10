using UnityEngine;
using TMPro; // Required for TextMeshPro

public class SortingDescriptionManager : MonoBehaviour
{
    public TextMeshProUGUI descriptionText;

    public void UpdateDescription(string algorithmName)
    {
        switch (algorithmName)
        {
            case "Bubble":
                descriptionText.text = "<b>Bubble Sort:</b> Repeatedly steps through the list, compares adjacent elements and swaps them if they are in the wrong order.";
                break;
            case "Selection":
                descriptionText.text = "<b>Selection Sort:</b> Finds the smallest element from the unsorted part and puts it at the beginning.";
                break;
            case "Insertion":
                descriptionText.text = "<b>Insertion Sort:</b> Builds the final sorted array one item at a time by inserting elements into their correct position.";
                break;
            case "Merge":
                descriptionText.text = "<b>Merge Sort:</b> A 'divide and conquer' algorithm that splits the array in half, sorts the halves, and merges them back together.";
                break;
            case "Quick":
                descriptionText.text = "<b>Quick Sort:</b> Picks a 'pivot' element and partitions the array so that smaller elements are on the left and larger ones are on the right.";
                break;
            case "Heap":
                descriptionText.text = "<b>Heap Sort:</b> Organizes data into a binary tree structure (a heap) to repeatedly find and remove the largest element.";
                break;
            default:
                descriptionText.text = "Select an algorithm to see its description.";
                break;
        }
    }
}