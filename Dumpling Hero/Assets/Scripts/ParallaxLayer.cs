using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    public float parallaxFactor;
    public float parallaxVerticalFactor;
    public void Move(float deltaX, float deltaY)
    {
        Vector3 newPos = transform.localPosition;
        newPos.x -= deltaX * parallaxFactor;
        newPos.y -= deltaY * parallaxVerticalFactor;
        transform.localPosition = newPos;
    }
}
