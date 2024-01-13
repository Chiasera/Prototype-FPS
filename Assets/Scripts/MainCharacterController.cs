using System;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class MainCharacterController : MonoBehaviour
{
    private Rigidbody rb;
    private float m_horizontal;
    private float m_vertical;
    [Range(1, 100)]
    public float m_speed;
    private float normalSpeed;
    private bool stopJump = false;
    private CinemachineVirtualCamera mainCamera;
    [Range(0.01f, 100f)]
    public float mouseSensitivity = 10f;
    private float xRotation = 0;
    private PlayerGun gunController;
    [Range(1.0f, 5000f)]
    public float jumpForceUp = 10f;
    [Range(1, 100)]
    public float jumpForceDown = 10f;
    public float2 mouseInput;
    [Range(0.1f, 5.0f)]
    public float jumpTime = 0.5f;
    public bool isGrounded = true;
    private int currentScore;
    private Health health;
    private CinemachineBasicMultiChannelPerlin m_BasicMultiChannelPerlin;
    [SerializeField]
    private Animator startBridgeAnimator;
    [SerializeField]
    public FloatingStone triggerStone;
    public TextMeshProUGUI score;
    [SerializeField]
    private Transform root;
    public Image image;
    public Transform[] respawnPoints;
    public Canvas canvas;
    private bool isDead = false;
    private bool canMove = true;
    private bool canFire = true;
    public bool canRespawn=true;
    private bool isJumping = false;
    private int groundMask;

    //Initialize all the necessarry variables
    //Lock the cursor and susbcribe to health callback methods
    private void Awake()
    {
        currentScore = 0;
        score.text = currentScore.ToString();
        rb = GetComponent<Rigidbody>();
        mainCamera = FindObjectOfType<CinemachineVirtualCamera>();
        m_BasicMultiChannelPerlin = mainCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        Cursor.lockState = CursorLockMode.Locked;
        gunController = GetComponentInChildren<PlayerGun>();
        gunController.gameObject.SetActive(false);
        health = GetComponent<Health>();
        // Subscribe to the OnHealthChanged event.
        if (health != null)
        {
            health.OnHealthChanged += HandleHealthChanged;
            health.OnHealthCleared += Die;
        }
        normalSpeed = m_speed;
        groundMask = LayerMask.GetMask("Ground");
    }

    private void Update()
    {
        UpdateCamera(mouseInput);
    }

    //Code to trigge the win screen
    public void WinGame()
    {
        rb.constraints = RigidbodyConstraints.FreezeAll;
        canFire= false;
        canMove= false;
        Cursor.lockState = CursorLockMode.Confined;
        canvas.GetComponent<Animator>().SetTrigger("win");
    }

    //Callback for when the player takes damage
    private void HandleHealthChanged(float damage)
    {
        ShakeCamera(0.4f, 2.0f, 1.0f);
        image.fillAmount = health.CurrentHealth/ health.MaxHealth;
    }

    //A simple camera shake function that uses built in cinemachine noise shake 
    private async void ShakeCamera(float shakeDuration, float amplitude, float frequencyGain)
    {
        float shakeTime = 0;
        while(shakeTime < shakeDuration)
        {
            m_BasicMultiChannelPerlin.m_AmplitudeGain = amplitude;
            m_BasicMultiChannelPerlin.m_FrequencyGain = frequencyGain;
            shakeTime += Time.deltaTime;
            await Task.Yield();
        }
        m_BasicMultiChannelPerlin.m_AmplitudeGain = 0;
        m_BasicMultiChannelPerlin.m_FrequencyGain = 0;
    }

    //Close the game
    public void Quit()
    {
        Application.Quit();
    }

    //Update the score and display it in the UI
    public void IncreaseScore()
    {
        score.text = (++currentScore).ToString();
    }

    //In case the player dies before the canyon, possibility to respawn.
    //Once the player steps onto the second stone, it's over for them, they have to continue
    private async void Die()
    {
        if (!isDead)
        {
            canFire = false;
            canMove = false;
            //prevent the player to fire and move. In theory we should also block inputs and aim
            gunController.Hold();
            isDead = true;
            if (canRespawn)
            {
                canvas.GetComponent<Animator>().SetTrigger("gameOver");
            } else
            {
                canvas.GetComponent<Animator>().SetTrigger("endScreen");
            }
            foreach (Enemy enemy in FindObjectsOfType<Enemy>())
            {
                enemy.Sleep(3600f);
            }
            //Make the cursor appear so that the player can choose an option
            Cursor.lockState = CursorLockMode.Confined;
            await Task.Delay(2000);
        }
    }

    //The code for respawning
    public async void Respawn()
    {
        if(canRespawn)
        {
            //reset the health bar in the UI
            image.fillAmount = 1;
            isDead = false;
            canvas.GetComponent<Animator>().SetTrigger("respawn");
            await Task.Delay(500);
            //There are two potential respawn points, one close to the canyon, and one close to the begining, chose the closest.
            Vector3 closestPoint = respawnPoints.OrderBy(point => (transform.position - point.position).magnitude)
                .First().position;
            transform.position = closestPoint;
            canFire = true;
            canMove = true;
            //Update the backend part of the health as well
            health.CurrentHealth = health.MaxHealth;
            foreach (Enemy enemy in FindObjectsOfType<Enemy>())
            {
                enemy.Sleep(4.0f);
            }
            Cursor.lockState = CursorLockMode.Locked;
        }   
    }

    //Update speed when shift is pressed
    public void OnSprintTrigger()
    {
        m_speed *= 1.5f;
    }

    //Regress the speed when shit is released
    public void OnSprintRelease()
    {
        m_speed = normalSpeed;
    }

    //Get the movement values from the WASD keyboard input. Using Unity's new Input System here
    public void OnMovement(InputValue value)
    {
        Vector2 inputValue = value.Get<Vector2>();
        m_vertical = inputValue.y;
        m_horizontal = inputValue.x;
    }

    private void FixedUpdate()
    {
        if (Physics.Raycast(transform.position, -transform.up, 2.0f, groundMask) || isJumping)
        {
            WASD();
        }
    }

    //Handle the WASD displacement
    private void WASD()
    {
        if (rb != null && canMove)
        {       
            //When player in the air, can't go left or right easily mid air
            float displacementModifier = isGrounded ? 1.0f : 0.6f;
            //Update the velocity of the rigidbody
            float3 newVelocity = (transform.right * m_horizontal * displacementModifier + transform.forward * m_vertical) * m_speed;
            rb.velocity = new Vector3(newVelocity.x, rb.velocity.y, newVelocity.z);
            // Check if the gunController is active
            if (gunController.gameObject.activeInHierarchy)
            {
                // Set the "isWalking" parameter based on velocity magnitude
                bool isWalking = rb.velocity.magnitude >= 0.1f;
                //Small detail, for the gun animation
                gunController.Animator.SetBool("isWalking", isWalking);
            }
        }
    }

    //Get the mouse input from the Input system
    public void OnLook(InputValue value)
    {
        mouseInput = value.Get<Vector2>();
        if(gunController.gameObject.activeInHierarchy)
        {
            gunController.SetMousePosition(mouseInput);
        }
    }

    //Jump
    public void OnJumpPress()
    {
        if (isGrounded)
        {
            Jump();
        }
    }

    //Stop jump
    public void OnJumpRelease()
    {
        stopJump = true;
    }

    //coroutine that handles jumping. If release mid air, loses momentum.
    //So the longer you press, the higher and farther you go.
    private async void Jump()
    {
        isJumping = true;
        stopJump = false;
        isGrounded = false;
        float jumptDelay = 0;
        float downForceAscend = 0;
        float downForce = 0;
        while (!isGrounded)
        {
            //one can press the space bar for up to jumpDelay seconds. While do so, go up
            if(jumptDelay < jumpTime && !stopJump)
            {
                // Apply the jump force to the Rigidbody's Y-axis.
                rb.velocity = new Vector3(rb.velocity.x, jumpForceUp - downForceAscend, rb.velocity.z);
                await Task.Yield();
                jumptDelay += Time.deltaTime;
                downForceAscend += Time.deltaTime * jumpForceUp;
            //When you start falling, update the velocity accordingly
            } else
            {
                // Apply the jump force to the Rigidbody's Y-axis.
                rb.velocity = new Vector3(rb.velocity.x, -downForce, rb.velocity.z);
                downForce += Time.deltaTime * jumpForceDown;
                await Task.Yield();
            }
        }
        //player is no longer falling
        stopJump = false;
        isJumping = false;
    }

    //Input for gun fire
    public void OnFirePressed()
    {
        if (gunController.gameObject.activeInHierarchy && canFire)
            gunController.Fire();
    }

    //Input for gun trigger release
    public void OnFireReleased()
    {
        if (gunController.gameObject.activeInHierarchy)
            gunController.Hold();
    }

    //I decided to implement the camera movement myself, as I never did it and it's a good opportunity to learn
    private void UpdateCamera(Vector2 input)
    {
        if (Mouse.current != null)
        {
            //What happens here is basically, we rotate our player, the camera follows them
            //And the camera only rotate arround the local x axis, as if it was a head (but player is only a capsule for now)
            xRotation -= (input.y * Time.deltaTime) * mouseSensitivity;
            //Clamp because the neck can only extend upwards up to a certain angle
            xRotation = Mathf.Clamp(xRotation, -80f, 80f);
            //Update the camera by adjusting its xRotation
            mainCamera.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
            //Update the player transform by rotation arround the y axis, and with it the child Camera
            transform.Rotate(Vector3.up * (input.x * Time.deltaTime) * mouseSensitivity);
        }
    }

    //Handle the Collision. "Gun" refers to the prop the player needs to pick up after the transition phase
    private async void OnTriggerEnter(Collider other)
    {       
        if (other.tag == "Gun" )
        {
            gunController.gameObject.SetActive(true);
            TriggerBridgeDescent();
            Destroy(other.gameObject);
        }
        //The terrain itself, set to grounded when the player is on it
        if (other.tag == "Floor")
        {
            await Task.Yield();
            isGrounded = true;
        }
    }

    //Set grounded to false when exit the terrain. Note our player is a capsule, which makes for bad collision detection
    //So we designed a small cube towards the player's "feet" that will handle the ground state of the player
    //See player prefab for more details 
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Floor")
        {
            if (!Physics.Raycast(transform.position, -transform.up, 2.0f, groundMask) || isJumping)
            {
                isGrounded = false;
            }
        }
    }

    //This code triggers the bridge from the first transition phase into the battlefield. It makes it sink slowly into the river
    // Camera will shake while it descends
    private async void TriggerBridgeDescent()
    {
        startBridgeAnimator.enabled = true;
        ShakeCamera(7.0f, 0.5f, 1.0f);
        await Task.Delay(TimeSpan.FromSeconds(7.0f));
        Enemy.canAwake = true;
    }
}
