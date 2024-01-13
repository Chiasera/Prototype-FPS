using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathCollider : MonoBehaviour
{
    //Makes a given target take 100 damage, which is the max damage => causes deaths
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Target")
        {
            other.GetComponent<Health>().TakeDamage(100);

        }
    }
}
