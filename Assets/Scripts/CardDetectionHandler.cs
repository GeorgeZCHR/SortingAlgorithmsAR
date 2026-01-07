using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class CardDetectionHandler : MonoBehaviour
{
    public ARTrackedImageManager imageManager;
    public CardScanner cardScanner;

    void OnEnable() => imageManager.trackedImagesChanged += OnChanged;
    void OnDisable() => imageManager.trackedImagesChanged -= OnChanged;

    void OnChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var newImage in eventArgs.added)
        {
            // Το name είναι το όνομα που έδωσες στο Reference Image Library (π.χ. "B")
            cardScanner.OnCardDetected(newImage.referenceImage.name);
        }

        foreach (var updatedImage in eventArgs.updated)
        {
            if (updatedImage.trackingState == UnityEngine.XR.ARSubsystems.TrackingState.Tracking)
            {
                cardScanner.OnCardDetected(updatedImage.referenceImage.name);
            }
        }
    }
}