using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepObjects : MonoBehaviour
{
    private static KeepObjects instance;
    void Awake()
    {
            DontDestroyOnLoad(gameObject);
    }
}
