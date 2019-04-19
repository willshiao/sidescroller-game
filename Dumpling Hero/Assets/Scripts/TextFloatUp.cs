using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextFloatUp : MonoBehaviour
{

    public float floatSpeed = 1.0F;
    public float floatTimeSec = 2.5F;
    public RectTransform rectTrans;

    // Update is called once per frame
    void Update()
    {
        rectTrans.Translate(rectTrans.up * floatSpeed * Time.deltaTime);
        Destroy(gameObject, floatTimeSec);
    }
}
