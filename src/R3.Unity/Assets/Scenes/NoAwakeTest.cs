using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoAwakeTest : MonoBehaviour
{
    private void OnDestroy()
    {
        Debug.Log("Destroy");
    }
}
