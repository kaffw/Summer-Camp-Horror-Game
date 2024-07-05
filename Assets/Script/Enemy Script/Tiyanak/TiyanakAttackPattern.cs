using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Cryptography;
using UnityEngine;

public class TiyanakAttackPattern : MonoBehaviour
{
    public bool playerAttackDetected = false;

    [SerializeField] public string difficulty;
    private int decision = 0;

    public Transform target;
    //private UnityEngine.AI.NavMeshAgent agent;

    public bool actionPhase = false;
    public bool enableMove = true;

    //temp lunge
    public Rigidbody rb;
    public bool isLunging = false;
    public Vector3 lungeTarget;
    public Vector3 jump;
    public float jumpForce = 12.0f;
    public float dashSpeed = 100f;
    public float dashTime = 0.5f;
    public float radius;
    public float distance;
    private float lungeStartTime = -1f;
    public bool playerDetected = false;
    public bool inLungeAttack = false;

    //NavMesh
    public UnityEngine.AI.NavMeshAgent agent;
    public float movementSpeed = 15f;
    public GameObject playerPos;
    public GameObject hostileRange;

    //timer di pa nagagamit
    public float timer = 0f;

    //Deal Damage
    public bool playerInRange = false;
    public EaseHealthBar healthBar;

    //Evade
    public float dashDistance = 15f;  // Distance to dash
    public float dashDuration = 0.2f;  // Duration of the dash
    private bool isDashing = false;
    private float dashTimeLeft;

    private Vector3 dashStartPos;
    private Vector3 dashEndPos;

    //anim
    public Animator anim;

    void Start()
    {
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        rb = GetComponent<Rigidbody>();

        difficulty = DifficultySelector.setDifficulty;
        if (difficulty == null) difficulty = "easy";

        //jump = new Vector3(0.0f, 2.0f, 0.0f);

        //hostileRange = GetComponent<GameObject>();
    }


    void Update()
    {
        radius = 5f;
        distance = Vector3.Distance(transform.position, playerPos.transform.position);

        if (playerAttackDetected && !actionPhase)
        {
            actionPhase = true;
            playerAttackDetected = false;
            enableMove = false;
            UnityEngine.Debug.Log(actionPhase);
            AttackDetected(difficulty);
            
        }
        
        if (enableMove)
        {
            //transform.LookAt(playerPos.transform.position);
            agent.isStopped = false;
            DefaultMovement();
        }
        else
        {
            agent.isStopped = true;
        }

        timer += Time.deltaTime;
        if (timer >= 3f)
        {
            StartCoroutine(tempCooldown(0.5f));
            timer = 0f;
        }
    }

    void AttackDetected(string difficulty)
    {
        switch (difficulty)
        {
            case "easy":
                if(!playerDetected) decision = UnityEngine.Random.Range(1, 4);
                else decision = UnityEngine.Random.Range(1, 7);

                //decision = UnityEngine.Random.Range(4, 6);

                dashSpeed = 100f; dashTime = 0.5f;
                
                switch (decision)
                {
                    case 1:
                        UnityEngine.Debug.Log("runs towards the player");                       
                        DefaultMovement();
                        StartCoroutine(tempCooldown(1));          
                    break;

                    case 2:
                    case 3:
                        UnityEngine.Debug.Log("temp Evade");
                        actionPhase = true;
                        StartCoroutine(Evade());
                    break;

                    case 4:
                    case 5:
                    case 6:
                        UnityEngine.Debug.Log("dashes towards the player");
                        inLungeAttack = true;
                        StartCoroutine(LungeAttack());
                    break;
                }

            break;
            
        }
        decision = 0;
    }

    void DefaultMovement()
    {
        anim.SetBool("Crawl", true);
        Vector3 newPosition = playerPos.transform.position;
        agent.SetDestination(newPosition);

    }

    void NormalAttack()
    {
        agent.destination = target.position;
    }

    void Retreat(){}
    private IEnumerator Evade()
    {
        actionPhase = true;
        isDashing = true;
        transform.LookAt(playerPos.transform.position);

        Vector3 dashStartPos = transform.position;
        //Vector3 dashDirection = -transform.right;
        Vector3 dashDirection = UnityEngine.Random.Range(0, 2) == 0 ? transform.right : -transform.right;
        Vector3 dashEndPos = dashStartPos + dashDirection * dashDistance;

        float dashTimeLeft = dashDuration;

        while (dashTimeLeft > 0)
        {
            float dashProgress = 1 - (dashTimeLeft / dashDuration);
            transform.position = Vector3.Lerp(dashStartPos, dashEndPos, dashProgress);
            dashTimeLeft -= Time.deltaTime;
            yield return null;
        }

        isDashing = false;
        yield return StartCoroutine(tempCooldown(0.5f));
    }

    private IEnumerator LungeAttack()
    {
        yield return new WaitForSeconds(1f);

        // Rotate towards the player position
        transform.LookAt(playerPos.transform.position);
        yield return new WaitForSeconds(0.5f);

        // Calculate dash parameters based on distance
        float dist = Vector3.Distance(transform.position, playerPos.transform.position);
        float dashSpeedIncrement = 50f;
        float dashSpeed = 100f + Mathf.Floor(dist / 40f) * dashSpeedIncrement; // Increase dashSpeed by 50 for every 40 units away

        float dashTime = Mathf.Max(0.5f, dist / dashSpeed); // Calculate dashTime based on dashSpeed
        float startTime = Time.time;
        float jumpHeight = 6f;

        Vector3 startPosition = transform.position;
        Vector3 targetPosition = playerPos.transform.position;

        while (Time.time <= startTime + dashTime && Vector3.Distance(transform.position, playerPos.transform.position) + 6f > radius)
        {
            float verticalOffset = Mathf.Sin((Time.time - startTime) / dashTime * Mathf.PI) * jumpHeight;
            float progress = (Time.time - startTime) / dashTime;
            Vector3 newPosition = Vector3.Lerp(startPosition, targetPosition, progress);
            newPosition.y += verticalOffset;

            transform.position = newPosition;

            yield return null;
        }

        yield return StartCoroutine(tempCooldown(1));
    }




    private IEnumerator tempCooldown(float duration)
    {
        UnityEngine.Debug.Log("in cooldown");
        yield return new WaitForSeconds(duration);
        actionPhase = false;
        enableMove = true;

        //UnityEngine.Debug.Log("start count to 5 seconds");
        //yield return new WaitForSeconds(5f);
        //UnityEngine.Debug.Log("moving after 5 seconds");
        //DefaultMovement();
    }

}

/*
rb.AddForce(Vector3.forward * (dashSpeed * 10), ForceMode.Impulse);
case "normal":
                // UnityEngine.Debug.Log("difficulty is set to " + difficulty);
                decision = UnityEngine.Random.Range(1, 8);
                // UnityEngine.Debug.Log("decision = " + decision);
                switch(decision)
                {
                    case 1: case 2:
                        UnityEngine.Debug.Log("Do nothing");
                        lungeTarget = target.transform.position;
                        isLunging = true;
                        //LungeAttack();
                     break;
                    
                    case 3: case 4: 
                        UnityEngine.Debug.Log("Normal Attack");
                        //NormalAttack();
                        lungeTarget = target.transform.position;
                        isLunging = true;
                        //LungeAttack();   
                    break;


                    case 5: case 6:
                        UnityEngine.Debug.Log("Lunge Attack");
                        isLunging = true;
                        lungeTarget = target.transform.position;
                        //LungeAttack();
                        break;

                    case 7:
                        UnityEngine.Debug.Log("Retreat");
                        isLunging = true;
                        lungeTarget = target.transform.position;
                        //LungeAttack();
                        break;
                }
            break;

            case "hard":
                // UnityEngine.Debug.Log("difficulty is set to " + difficulty);
                decision = UnityEngine.Random.Range(1, 11);
                // UnityEngine.Debug.Log("decision = " + decision);
                switch(decision)
                {
                    case 1:
                        UnityEngine.Debug.Log("Do nothing");
                    break;
                    
                    case 2: case 3: case 4: 
                        UnityEngine.Debug.Log("Normal Attack");
                        NormalAttack();
                    break;

                    case 5: case 6: case 7:
                        UnityEngine.Debug.Log("Lunge Attack");
                    break;

                    case 8:
                        UnityEngine.Debug.Log("Retreat");
                    break;

                    case 9: case 10:
                        UnityEngine.Debug.Log("Evade");
                    break;
                }
            break;

*/