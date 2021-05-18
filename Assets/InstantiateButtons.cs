using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiateButtons : MonoBehaviour
{
    private int i;
    // Start is called before the first frame update
    void Start()
    {
        i = 0;
        StartCoroutine(InstantiateButtons_());
    }

    IEnumerator InstantiateButtons_()
    {
        yield return new WaitForSeconds(1);
        
        i++;
        if (i < 7)
        {
            StartCoroutine(InstantiateButtons_());
        }
        else
            StopAllCoroutines();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
