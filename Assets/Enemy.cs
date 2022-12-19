using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Enemy : MonoBehaviour
{
    Player player;
    Rigidbody2D myRB;
    Animator animator;

    [SerializeField] public float runSpeed = 2.2f;
    public LayerMask playerLayer;
    public Transform attackPoint;
    public GameObject healthParent;
    public GameObject healthActive;
    public float attackRange;
    float MaxHealth = 100f;
    float myHealth;
    bool AttackDone = true;
    float attackTime;
    bool dead=false;
    Vector3 distance;
    GameController GC;
    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<Player>();
        myRB = GetComponent<Rigidbody2D>(); 
        animator = GetComponent<Animator>();
        myHealth = MaxHealth;
        GC = FindObjectOfType<GameController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!dead)
        {
            distance = player.transform.position - this.transform.position;
            Move();
            FlipSprite();
            Attack();
        }
        EnableHealthBar();
    }

    void EnableHealthBar()
    {
        if ((Mathf.Abs(distance.magnitude) < 1.0f || myHealth != MaxHealth) && !dead)
            healthParent.SetActive(true);
        else
            healthParent.SetActive(false); ;
    }

    void UpdateHealth()
    {
        float healthNormalised = myHealth / MaxHealth;
        if (healthNormalised < 0)
            healthNormalised = 0;

        Vector3 newpos = new Vector3(0,0,0);
        newpos.x -= (1.0f - healthNormalised)/2.0f ;
       
        healthActive.transform.localPosition = newpos;
        var scale = healthActive.transform.localScale;
        scale.x = healthNormalised;
        healthActive.transform.localScale = scale;
        if (healthNormalised <= .3f)
        {
            healthActive.GetComponent<SpriteRenderer>().color = Color.red;
        }
        
    }

    private void Move()
    {
        
        if (Mathf.Abs(distance.x) <= 4 && Mathf.Abs(distance.x) >= 1.2f && Mathf.Abs(distance.y) <= 3)
        {
            if (distance.x > 0)
            {
                Vector2 playerVelocity = new Vector2(runSpeed, myRB.velocity.y);
                myRB.velocity = playerVelocity;
            }
            else if (distance.x < 0)
            {
                Vector2 playerVelocity = new Vector2(-runSpeed, myRB.velocity.y);
                myRB.velocity = playerVelocity;
            }
        } 
        bool hasHorizontalVelocity = Mathf.Abs(myRB.velocity.x) > Mathf.Epsilon;
        if (!animator.GetBool("Hurt") )
        {

        animator.SetBool("Walk", hasHorizontalVelocity);
        }
    }

    private void FlipSprite()
    {
        bool hasHorizontalVelocity = Mathf.Abs(myRB.velocity.x) > .55f;
        if (hasHorizontalVelocity)
        {
            transform.localScale = new Vector2(
              Mathf.Sign(myRB.velocity.x),
              transform.localScale.y);
        }
    }

    private void Attack()
    {
        var currentAnim = animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
        if ( currentAnim != "Enemy_Attack" || currentAnim != "Enemy_Hurt" && !GC.gameOver)
        {
            Vector3 distance = player.transform.position - this.transform.position;
            if ((distance.x) <= 1 && distance.x >= .02f && Mathf.Abs(distance.y) <= 1)
                transform.localScale = new Vector2(1, transform.localScale.y);
            else if ((distance.x) >= -1 && distance.x <= -.02f && Mathf.Abs(distance.y) <= 1)
                transform.localScale = new Vector2(-1, transform.localScale.y);
            if (Random.Range(0, 1500) < 3 && Mathf.Abs(distance.x) <= 1 && Mathf.Abs(distance.x) >= .2f && Mathf.Abs(distance.y) <= 1)
            {
                animator.SetTrigger("Attack");
            }
        }
        if (currentAnim == "Enemy_Attack")
        {
           attackTime += Time.deltaTime;
           if (attackTime> .6 && !AttackDone)
            {
                HurtPlayer();
                AttackDone = true;
            }
        }
        else
        {
            attackTime = 0;
            AttackDone = false;
        }
    }


    public void HurtPlayer()
    {
        Collider2D[] hitPlayer = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, playerLayer);
        foreach (Collider2D player in hitPlayer)
        {
            player.GetComponent<Player>().HurtPlayerSelf(20f);
        }
    }

    public void HurtEnemySelf(float damage)
    {
        animator.SetTrigger("Hurt");
        myHealth -= damage;
        if (myHealth < 0)
            myHealth = 0;
        UpdateHealth();
        if(myHealth <= 0) { Die(); }
    }
    private void Die()
    {
        dead = true;
        var GC = FindObjectOfType<GameController>();
        GC.addScore();
        GC.activeEnemyCount--;
        animator.SetBool("Die",true);
        Collider2D box =  this.GetComponent<CapsuleCollider2D>();
        myRB.bodyType = RigidbodyType2D.Static ;
        box.enabled = false;
        StartCoroutine(DeleteEnemy());
    }

    IEnumerator DeleteEnemy()
    {
        //yield return new WaitForSeconds(1f);
        //healthParent.SetActive(false);
        yield return new WaitForSeconds(3.5f);
        Destroy(this.gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
