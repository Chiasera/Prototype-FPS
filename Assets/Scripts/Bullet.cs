using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField]
    [Range(1.0f, 20f)]
    private float damage = 5f;

    private void OnTriggerEnter(Collider other)
    {
        //Target is either player or an enemy. Call the TakeDamage method on the health component of the target
        //Will call back the specific implementation by said target (see health class)
        if(other.tag == "Target")
        {
            other.gameObject.GetComponent<Health>().TakeDamage(damage);
        }
        Destroy(gameObject);
    }

    //Same but with collision instead of trigger. Triggers are used  For enemy or player, collision for environment
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Target")
        {
            collision.gameObject.GetComponent<Health>().TakeDamage(damage);
        }
        Destroy(gameObject);
    }
}
