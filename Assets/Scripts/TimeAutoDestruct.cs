using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class TimeAutoDestruct : MonoBehaviour
{
    // Start is called before the first frame update
    private void Awake()
    {
        OnWaitDestroy(5.0f);
    }

    private void Update()
    {
        if(transform.position.y > 200)
        {
            Destroy(gameObject);
        }
    }

    //Destruct this object after set amount of time
    private async void OnWaitDestroy(float delay)
    {
        await Task.Delay(TimeSpan.FromSeconds(delay));
        try
        {
            Destroy(gameObject);
        } catch (Exception e)
        {
            //ignore
        }
    }
}
