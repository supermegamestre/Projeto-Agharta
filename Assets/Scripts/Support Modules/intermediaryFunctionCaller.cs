using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class intermediaryFunctionCaller : MonoBehaviour
{
    [SerializeField]
    private UnityEvent functionCalled;

    public void callFunction()
    {
        functionCalled?.Invoke();
        Debug.Log("called");
    } 
    
}
