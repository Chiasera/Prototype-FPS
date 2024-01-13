using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGun : Gun
{
    [SerializeField]
    [Range(1.0f, 50.0f)]
    private float swayAmplitude = 1.0f;
    [SerializeField]
    [Range(1.0f, 50.0f)]
    private float swayDampening = 1.0f;
    private float2 mouseInput;
    Quaternion Xrotation;
    Quaternion Yrotation;
    private int ignoreCollisionMask;
    
    private void Update()
    {
        SwayWeapon();
        AdjustAim();
        //Debug.Log(mainCamera.transform.rotation);
    }

    private void Start()
    {
        //A layer we will ommit during raycast while aiming
        ignoreCollisionMask = ~LayerMask.GetMask("IgnoreBullet");
    }

    //Update mouse position if needed
    public void SetMousePosition(float2 mousePos)
    {
        mouseInput = mousePos;
    } 

    //Catching the delta for the mouse to rotate arround x and y axis our weapon, to add a natural feel
    //Spherical interpolation looks better and linearly 
    private void SwayWeapon()
    {
        Xrotation = Quaternion.AngleAxis(Mathf.Clamp(mouseInput.x, -1, 1) * swayAmplitude, transform.up);
        Yrotation = Quaternion.AngleAxis(Mathf.Clamp(mouseInput.y, -1, 1) * swayAmplitude, transform.right);
        Quaternion targetRotation = Xrotation * Yrotation;
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, swayDampening * Time.deltaTime);
    }

    private void AdjustAim()
    {
        if(gunTip != null)
        {
            // Calculate a ray from the center of the camera
            Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

            // Create a RaycastHit variable to store hit information
            RaycastHit hit;

            // Perform the raycast
            if (Physics.Raycast(ray, out hit, float.MaxValue, ignoreCollisionMask))
            {
                //Aim at target 
                gunTip.transform.LookAt(hit.point);
            } else
            {
                //Should not happen if the level is built correctly
                //This aim point is not accurate and does not reflect a correct aim with the crossair
                gunTip.transform.LookAt(baseTarget);
            }
        }     
    }
}
