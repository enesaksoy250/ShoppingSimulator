using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class AnimalMove : MonoBehaviour
{

    [Header("Waypoints")]
    public Transform[] points;

    [Header("Player")]
    public Transform player;

    [Header("Speeds")]
    public float walkSpeed = 1.5f;
    public float runSpeed = 3f;
    public float followSpeed = 2.5f;

    [Header("Timers")]
    private float waitTime = 5f;
    public float followChance = 0.3f;

    private Animator animator;

    private AudioSource audioSource;

    private Transform currentTarget;
    private bool isWaiting = false;
    private bool isFollowingPlayer = false;
    private float currentSpeed;
    private float targetAnimatorSpeed;
    private bool hasWaitedThisApproach = false;
    private float followDistance = 1f;

    private Coroutine waitCoroutine;

    [SerializeField] TextMeshProUGUI fps;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
     
    }

    void Start()
    {
        DecideNextMove();
    }

    // Update is called once per frame
    private void Update()
    {
        //fps.text = Mathf.Round(1f / Time.deltaTime).ToString();

        Transform target = isFollowingPlayer ? player : currentTarget;
        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        float distance = direction.magnitude;

        if (distance <= 1f)
        {
          
            animator.SetFloat("Speed", 0f);
            targetAnimatorSpeed = 0f;

            if (waitCoroutine == null)
            {
                waitCoroutine = StartCoroutine(Wait());

                if (isFollowingPlayer) audioSource.Play();

            }
        }
        else
        {
            transform.position += direction.normalized * currentSpeed * Time.deltaTime;
            
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
            }
        }

        // Animator Speed Parametresi (blend tree kontrol)
        targetAnimatorSpeed = Mathf.Lerp(targetAnimatorSpeed, currentSpeed, Time.deltaTime * 5f);
        animator.SetFloat("Speed", targetAnimatorSpeed);
        animator.speed = 1.5f;
    }

    private IEnumerator Wait()
    {
        int waitTime = Random.Range(isFollowingPlayer ? 15 : 5,isFollowingPlayer ? 60 : 20);
        float timer = 0;
    
        while(timer < waitTime)
        {     
            timer += Time.deltaTime;
            yield return null;
        }

        StopCoroutine(waitCoroutine);
        waitCoroutine = null;
        DecideNextMove();    
    }

    private void DecideNextMove()
    {
       
        if (Random.value < followChance && player != null)
        {
            isFollowingPlayer = true;
            currentTarget = player;         
            currentSpeed = followSpeed;
        }
        else
        {
            isFollowingPlayer = false;
            ChooseNewTarget();
            currentSpeed = (Random.value < 0.5f) ? walkSpeed : runSpeed;
        
        }
    }

    private void ChooseNewTarget()
    {
        if (points.Length == 0)
        {
            Debug.LogWarning("Waypoint listesi boþ.");
            return;
        }

        Transform newTarget;
        do
        {
            newTarget = points[Random.Range(0, points.Length)];
        } while (newTarget == currentTarget);

        currentTarget = newTarget;
    }

}
