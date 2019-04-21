using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextFloatUp : MonoBehaviour
{

    public float floatSpeed;
    public float floatTimeSec;
    public RectTransform rectTrans;

    // Update is called once per frame
    void Update()
    {
        rectTrans.Translate(rectTrans.up * floatSpeed * Time.deltaTime);
        Destroy(gameObject, floatTimeSec);
    }
}
