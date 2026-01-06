using UnityEngine; 
using TMPro;     

public class ARNarrationController : MonoBehaviour
{
    public TextMeshPro narrationText;

    public void Show(string message)
    {
        narrationText.text = message;
    }

    public void SetText(string message)
    {
        narrationText.text = message;
    }

    public void Clear()
    {
        narrationText.text = "";
    }
}