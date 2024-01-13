using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class FloatingStone : MonoBehaviour
{
    private Renderer _renderer;
    public Animator animator;
    private bool isStable = true;
    private bool hasActivated = false;
    private PathGenerator _pathGenerator;
    public static List<FloatingStone> activeStones;
    
    // Start is called before the first frame update
    void Awake()
    {
        _pathGenerator = FindObjectOfType<PathGenerator>();
        _renderer = GetComponent<Renderer>();
        if(activeStones == null)
        {
            activeStones = new List<FloatingStone>();
        }
    }

    public bool IsStable { set { isStable = value; } }

    private void OnStepEvent()
    {
        if(activeStones.Contains(this)) activeStones.Remove(this);
        foreach(FloatingStone stone in activeStones)
        {
            if(stone != null)
            {
                stone.Dissolve();
            }
        }
        if (!isStable)
        {
            Dissolve();
        }
        activeStones.Add(this);
        animator.SetTrigger("stepFeedback");
        _pathGenerator.GenerateBranch(transform.position);
        
    }

    public void MakeUnstable()
    {
        animator.SetTrigger("wobble");
        isStable = false;
    }


    private async void Dissolve()
    {
        float dissolveThreshold = 0f;
        float dissolveSpeed = 1.5f;
        while(dissolveThreshold < 1.1f)
        {
            dissolveThreshold += Time.deltaTime * dissolveSpeed;
            //In theory, one would either use Shader.PropertyToId for global shader fields
            //Or create a seperate class for the material properties that hold a reference to the material properties and thus shader
            //Because here Unity hashes the string each time we make this call, and it can become expensive if many scripts reference materials this way
            _renderer.material.SetFloat("_Dissolve_threshold", dissolveThreshold);
            await Task.Yield();
        }
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "MainCharacter")
        {
            if (!hasActivated)
            {
                hasActivated = true;
                OnStepEvent();
            }
        }
    }


    private void OnTriggerExit(Collider other)
    {
        
    }
}
