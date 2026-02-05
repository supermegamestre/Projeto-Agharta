using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructibleObject : MonoBehaviour
{
    public void endOfLife() => StartCoroutine(endOfLifeRoutine());
    private IEnumerator endOfLifeRoutine()
    {
        yield return null;
    }
}
