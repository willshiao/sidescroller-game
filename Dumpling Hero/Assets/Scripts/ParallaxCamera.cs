using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxCamera : MonoBehaviour
{
    public delegate void ParallaxCameraDelegate(float deltaXMovement, float deltaYMovement);
    public ParallaxCameraDelegate onCameraTranslate;
    private Vector3 oldPosition;
    void Start()
    {
        oldPosition = transform.position;
    }
    void Update()
    {
        if (transform.position != oldPosition)
        {
            if (onCameraTranslate != null)
            {
                float deltaX = oldPosition.x - transform.position.x;
                float deltaY = oldPosition.y - transform.position.y;
                onCameraTranslate(deltaX, deltaY);
            }
            oldPosition = transform.position;
        }
    }
}
