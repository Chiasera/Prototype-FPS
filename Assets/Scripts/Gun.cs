using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


public class Gun : MonoBehaviour
{
    [SerializeField]
    [Range(1f, 1000f)]
    private float fireSpeed;
    [SerializeField]
    [Range(0.01f, 10f)]
    private float fireRate = 1.0f;
    private bool fire = false;
    private bool canShoot = true;
    [SerializeField]
    protected GameObject bulletPrefab;
    [SerializeField]
    protected Transform baseTarget;
    [SerializeField]
    protected GameObject gunTip;
    protected Animator animator;
    protected Camera mainCamera;
    // Start is called before the first frame update
    void Awake()
    {
        animator = GetComponent<Animator>();
        mainCamera = Camera.main;  
    }

    //properties
    public Transform Target { get { return baseTarget; } }
    public Animator Animator { get { return animator; } }

    //While the gun holder can shool, allow them to shoot
    //A timer is set to prevent shooting too fast
    public void Fire()
    {
        fire = true;
        if (canShoot)
        {
            Shoot();
        }
    }

    //stop shooting
    public void Hold()
    {
        fire = false;
    }

    //Instantiate the bullet prefab at the tip of the gun and add velocity in its forward direction
    private async void Shoot()
    {
        while (fire && gunTip != null)
        {
            GameObject bullet = GameObject.Instantiate(bulletPrefab);
            bullet.transform.position = gunTip.transform.position;
            bullet.GetComponent<Rigidbody>().velocity = fireSpeed * gunTip.transform.forward;
            if(animator != null)
            animator.SetTrigger("fire");
            canShoot = false;
            await Task.Delay(TimeSpan.FromSeconds(fireRate));
            canShoot = true;
        }
    }

    private void OnDisable()
    {
        //Shoot is async, so since we don't use Unity's built in coroutines, make sure the async function is killed before editor mode
        fire = false;
    }
}
