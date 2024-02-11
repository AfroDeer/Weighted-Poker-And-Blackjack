//Nicholas Johnson - 2024

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForcedAspectRatio : MonoBehaviour
{
    void Start()
    {
        Camera camera = GetComponent<Camera>();
        float targetRatio = 9.0f / 16.0f;
        float currentRatio = (float)Screen.width / (float)Screen.height;
        float heightScale = currentRatio / targetRatio;
        
        if (heightScale < 1.0f)
        {
            Rect rect = camera.rect;

            rect.width = 1.0f;
            rect.height = heightScale;
            rect.x = 0;
            rect.y = (1.0f - heightScale) / 2.0f;

            camera.rect = rect;
        }
        else
        {
            float scalewidth = 1.0f / heightScale;

            Rect rect = camera.rect;

            rect.width = scalewidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scalewidth) / 2.0f;
            rect.y = 0;

            camera.rect = rect;
        }
    }
}
