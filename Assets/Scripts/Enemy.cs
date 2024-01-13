using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    private Health health;
    public bool isActive = false;
    private Gun gun;
    [SerializeField]
    private Transform gunParent;
    [SerializeField]
    [Range(0.1f, 50f)]
    private float aimSensitivity = 1f;
    [SerializeField]
    float awakeRadius = 50f;

    public static bool canAwake = false;
    MainCharacterController mainCharacterController;

    // Start is called before the first frame update
    void Start()
    {
        //Initialize all the necessary fields
        mainCharacterController = FindObjectOfType<MainCharacterController>();
        gun = GetComponentInChildren<Gun>();
        health = GetComponent<Health>();
        health.OnHealthChanged += HandleDamage;
        health.OnHealthCleared += Die;
    }

    //Holds the enemy state for some time. For instance, when the player dies, we want the enemies to "Freeze"
    public async void Sleep(float seconds)
    {
        canAwake = false;
        isActive = false;
        gun.Hold();
        await Task.Delay(TimeSpan.FromSeconds(seconds));
        canAwake |= true;
    }

    //While active, turn towards the player and shoot them
    private void Update()
    {
        if (!isActive 
            && (gun.Target.position - transform.position).magnitude < awakeRadius
            && canAwake)
        {
            OnFireWait(1.0f);
            Activate();
        }
    }

    private async void OnFireWait(float delay)
    {
        await Task.Delay(TimeSpan.FromSeconds(delay));
        gun.Fire();
    }

    //Activate make the enemy tilt towards the player. We create a global rotation from the given direction and slowly lerp towards it
    //We don't want the enemies to lock their rotation with the player's. Not realistic and way to hard for the player
    private async void Activate()
    {
        isActive = true;
        while (isActive && gunParent != null)
        {
            //Rotation will point in this direction
            Vector3 targetDirection = (gun.Target.position - gunParent.position).normalized;
            //Rotation towards target in world space
            Quaternion weaponRotation = Quaternion.LookRotation(targetDirection);
            Quaternion bodyRotation = Quaternion.Euler(0, weaponRotation.eulerAngles.y, 0);

            //Take the angle between the enemy and the player. And slowly try to make it to 0
            float angleDelta = Vector3.Angle(gunParent.position, gun.Target.position);
            float clampedValue = 1.0f - (angleDelta / 180.0f);

            // Interpolate towards the target rotation smoothly using the clamped value
            gunParent.rotation = Quaternion.Slerp(gunParent.rotation, weaponRotation, clampedValue * aimSensitivity * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, bodyRotation, clampedValue * aimSensitivity * Time.deltaTime);

            await Task.Yield();
        }
    }

    private void HandleDamage(float damage)
    {
       //TODO: add VFX or some feedback
    }

    //Enemy simply dissapear, increasing the player's score in its death
    private void Die()
    {
        mainCharacterController.IncreaseScore();
        Destroy(gameObject);
    }

    //To avoid any potential null pointer exception during an async call 
    private void OnDisable()
    {
        isActive=false;
    }
}
