using System;
using System.Collections;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Scene = UnityEngine.SceneManagement.Scene;

public class Player : MonoBehaviour
{
    //Config
    [SerializeField] float runSpeed = 5f;
    [SerializeField] float jumpStrength = 5f;
    //[SerializeField] float climbSpeed = 3f;
    [SerializeField] float customGravityScale = 8f;
    [SerializeField] float attackRange = .5f;
    [SerializeField] Vector2 deathkick;
    public Scrollbar health;
    public Image healthColor;
    public float maxHealth = 100f;
    private float myHealth ;

    //Caches
    public Transform attackPoint;
    public LayerMask enemyLayer;
    Rigidbody2D myRigidbody;
    Animator animator;
    CapsuleCollider2D myBodyCollider;
    BoxCollider2D myFeet;

    //State
    bool isAlive = true;
    bool Attack1Done = false;
    bool Attack2Done = false;
    bool Attack3Done = false;

    // Start is called before the firsft frame update
    void Start()
    {
        myHealth = maxHealth;
        health.size = myHealth / maxHealth;
        myRigidbody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        myBodyCollider = GetComponent<CapsuleCollider2D>();
        myFeet = GetComponent<BoxCollider2D>();
        myRigidbody.gravityScale = customGravityScale;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isAlive) { return; }
        Run();
        //ClimbLadder();
        Jump();
        FlipSprite();
        Attack();
        UpdateHealth();
        
    }

    void UpdateHealth()
    {
        health.size = myHealth / maxHealth;
        if (health.size < .25f)
            healthColor.color = Color.red;
        else
            healthColor.color = Color.green;
    }

    private void Run()
    {
        float controlThrow = Input.GetAxisRaw("Horizontal") * runSpeed ;
        //Debug.Log(Input.GetButtonDown("A"));
        Vector2 playerVelocity = new Vector2(controlThrow, myRigidbody.velocity.y);
        myRigidbody.velocity = playerVelocity;

        bool hasHorizontalVelocity = Mathf.Abs(myRigidbody.velocity.x) > Mathf.Epsilon;
        animator.SetBool("isRunning", hasHorizontalVelocity);
    }


    private void Jump()
    {
        bool hasNegVerticalVelocity = myRigidbody.velocity.y < -Mathf.Epsilon;
        animator.SetBool("isFalling", hasNegVerticalVelocity);
        if (!myFeet.IsTouchingLayers(LayerMask.GetMask("Ground"))) { return; }

        if (Input.GetButtonDown("Jump"))
        {
            Vector2 jumpVelocityToAdd = new Vector2(0f, jumpStrength);
            myRigidbody.velocity += jumpVelocityToAdd;
        }
        bool hasPosVerticalVelocity = myRigidbody.velocity.y > Mathf.Epsilon;
        animator.SetBool("isJumping", hasPosVerticalVelocity);
    }


    private void FlipSprite()
    {
        bool hasHorizontalVelocity = Mathf.Abs(myRigidbody.velocity.x) > Mathf.Epsilon;
        if (hasHorizontalVelocity)
        {
            transform.localScale = new Vector2(
              Mathf.Sign(myRigidbody.velocity.x),
              transform.localScale.y);
        }
    }

    private void Die()
    {
        animator.SetBool("Dead",true);
        FindObjectOfType<GameController>().gameOver = true;
        StartCoroutine(DisplayGameOver());
    }

    private void Attack()
    {
        AttackComboLogic();
        if (!Input.GetButtonDown("Fire1")) { return; }
        animator.SetTrigger("attacking");
    }

    private void AttackComboLogic()
    {
      
      string currentAnim = animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
      if (currentAnim == "Player_Attack1")
        {
            float val = animator.GetFloat("Attack1") + Time.deltaTime;
            animator.SetFloat("Attack1", val);
            if (val > 0.015 && !Attack1Done)
            {
                HurtEnemy();  
                Attack1Done = true;
            }
        }
        else if (currentAnim == "Player_Attack2")
        {
            float val = animator.GetFloat("Attack2") + Time.deltaTime;
            animator.SetFloat("Attack2", val);
            if (val > 0.008 && !Attack2Done)
            {
                HurtEnemy();
                Attack2Done = true;
            }
        }
        else if (currentAnim == "Player_Attack3")
        {
            float val = animator.GetFloat("Attack3") + Time.deltaTime;
            animator.SetFloat("Attack3", val);
            if (val > 0.018 && !Attack3Done)
            {
                HurtEnemy();
                Attack3Done = true;
            }
        }
        if (currentAnim != "Player_Attack1")
        { animator.SetFloat("Attack1", 0);
            Attack1Done = false;
        }
        if (currentAnim != "Player_Attack2")
        {
            animator.SetFloat("Attack2", 0);
            Attack2Done = false;
        }
        if (currentAnim != "Player_Attack3")
        { animator.SetFloat("Attack3", 0);
            Attack3Done = false;
        }
    }



    void HurtEnemy()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);
        foreach (Collider2D enemy in hitEnemies)
        {
            enemy.GetComponent<Enemy>().HurtEnemySelf(40f);
        }
    }

    public void HurtPlayerSelf(float damage)
    {
        //animator.SetTrigger("Hurt");
        myHealth -= damage;
        Debug.Log("player hurt");
        if (myHealth <= 0) { Die(); }
    }

    //IEnumerator Reload()
    //{
    //    yield return new WaitForSecondsRealtime(5);
    //    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    //}

    //public void ChangeMaterial()
    //{
    //    myBodyCollider.sharedMaterial = deathMaterial;
    //}

    //IEnumerator NewSpawn()
    //{

    //    yield return new WaitForSeconds(5f);
    //    //FindObjectOfType<PlayerSpawner>().BirthNewPlayer();

    //}

    IEnumerator DisplayGameOver()
    {
        yield return new WaitForSeconds(5f);
        FindObjectOfType<GameController>().gameOverText.enabled = true;
        yield return new WaitForSeconds(5f);
        Scene scene = SceneManager.GetActiveScene(); 
        SceneManager.LoadScene(scene.name);
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}