using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    public GameObject bomb_pf;
    public List<Bomb> bombs;

    Rigidbody2D rb2d;

    public Transform holdPosition;
    public GameObject heldBomb;
    bool bombHeld;
    bool grounded;
    bool bombJump;

    [SerializeField] float normalPitch;
    [SerializeField] float pitchAmount = 0.5f;
    float pitchShift;

    public AudioSource source;
    public AudioClip pullOutBomb;
    public AudioClip throwBomb;
    Animator anim;

    [SerializeField] float groundDetectDistance;

    [SerializeField] Vector2 lastMovedDirection;
    [SerializeField] float throwPower;
    [SerializeField] float verticalPowerMultiplier;
    [SerializeField] float jumpPower;

    [SerializeField] float currentHorzVel;
    [SerializeField] float maxSpeed;
    [SerializeField] float moveSpeed;
    [SerializeField] float airMoveSpeed;

    [SerializeField] SpriteRenderer charksSprite;

    GameManager gameManager;

    public bool usingMouse = false;

    public GameObject mousePivot;
    public GameObject mouseScale;

    void Awake()
    {
        if (source == null)
        {
            source = GetComponent<AudioSource>();
        }

        normalPitch = source.pitch;
        pitchShift = normalPitch + pitchAmount;

        charksSprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        gameManager = FindObjectOfType<GameManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        lastMovedDirection = Vector2.right;

        rb2d = GetComponent<Rigidbody2D>();
        bombHeld = false;

        mousePivot.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (gameManager != null)
            return;

        PlayerUpdate();
    }

    public void PlayerUpdate()
    {
        grounded = DetectGround();

        if(usingMouse)
        {
            Vector3 point = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 direction = mousePivot.transform.position - point;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            mousePivot.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            lastMovedDirection = -mousePivot.transform.right;
            lastMovedDirection.Normalize();
            mousePivot.SetActive(bombHeld);
        }
        /*else
        {
            mousePivot.transform.rotation = Quaternion.LookRotation(lastMovedDirection, mousePivot.transform.right);
            
        }*/

       

        if (heldBomb == null)
        {
            bombHeld = false;
            anim.SetBool("HoldingBomb", bombHeld);
        }

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (bombHeld)
            {
                ThrowBomb();
            }
            else
            {
                if (gameManager.numOfBombs > 0)
                {
                    PullOutBomb();
                }
                else
                {
                    //Error
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            TriggerExplosions();
        }

        Move();
    }

    private void LateUpdate()
    {
        if (heldBomb != null)
        {
            heldBomb.transform.SetPositionAndRotation(holdPosition.position, heldBomb.transform.rotation);
        }
    }

    public void Die()
    {
        FindObjectOfType<GameManager>().PlayerKilled();
    }

    public void PullOutBomb()
    {
        gameManager.PullOutBomb();

        source.PlayOneShot(pullOutBomb, 0.8f);

        bombHeld = true;
        anim.SetBool("HoldingBomb", bombHeld);
        GameObject newBomb = Instantiate(bomb_pf,holdPosition.position, holdPosition.transform.rotation);
        heldBomb = newBomb;

        //Maybe dont disable and move the bomb to higher over the player
        //heldBomb.GetComponent<Collider2D>().enabled = false;
        bombs.Add(newBomb.GetComponent<Bomb>());
    }

    public void ThrowBomb()
    {
        if(heldBomb == null)
        {
            bombHeld = false;
            anim.SetBool("HoldingBomb", bombHeld);
            return;
        }

        source.PlayOneShot(throwBomb, 0.4f);

        bombHeld = false;
        anim.SetBool("HoldingBomb", bombHeld);
        //heldBomb.GetComponent<Collider2D>().enabled = false;

        if(Input.GetKey(KeyCode.S))
        {
            lastMovedDirection = new Vector2(0, -1);
        }

        if (lastMovedDirection.y > 0)
        {
            lastMovedDirection.y *= verticalPowerMultiplier;
        }

        heldBomb.GetComponent<Rigidbody2D>().AddForce(lastMovedDirection * throwPower);

        heldBomb = null;
        if (lastMovedDirection.y < 0 && Mathf.Abs(lastMovedDirection.x) < 0.5f)
        {
            bombJump = true;
        }
    }

    public void TriggerExplosions()
    {
        bombHeld = false;
        heldBomb = null;
        anim.SetBool("HoldingBomb", bombHeld);

        if (bombs.Count == 0)
        {
            //Click sound
            return;
        }

        foreach (Bomb bm in bombs)
        {
            if(bm != null)
                bm.Explode();
        }

        bombs.Clear();
    }

    public void Move()
    {
        float horz = Input.GetAxis("Horizontal");
        float vert = Input.GetAxis("Vertical");
        bool jump = Input.GetKeyDown(KeyCode.Space);

        if ((jump && grounded) || bombJump) // && Physics2D.Raycast(transform.position,-transform.up,0.5f))
        {
            if(bombJump)
            {
                source.pitch = pitchShift;
            }
            source.Play();

            source.pitch = normalPitch;

            bombJump = false;
            rb2d.AddForce(transform.up*jumpPower,ForceMode2D.Force);
        }

        currentHorzVel = rb2d.velocity.x;
       

        if (Mathf.Abs(horz) > 0.05f)
        {

            anim.SetFloat("Horizontal", Mathf.Abs(horz));

            if (horz > 0)
            {
                charksSprite.flipX = false;
            }
            else if(horz < 0)
            {
                charksSprite.flipX = true;
            }


            if (Mathf.Abs(currentHorzVel) < maxSpeed)
            {
                if (grounded)
                {
                    rb2d.AddForce(transform.right * horz * moveSpeed * Time.deltaTime);
                }
                else
                {
                    rb2d.AddForce(transform.right * horz * airMoveSpeed * Time.deltaTime);
                }
            }
        }
        else
        {

            Vector2 vel = rb2d.velocity;
            vel.y = vel.y * 0;
            rb2d.AddForce(-vel);
        }

        if (!usingMouse)
        {
            if (Mathf.Abs(horz) > 0.1f || Mathf.Abs(vert) > 0.1f)
            {
                lastMovedDirection = new Vector2(horz, vert);
                lastMovedDirection.Normalize();
            }
        }
    }

    public bool DetectGround()
    {
        Vector3 leftSide = transform.position + new Vector3(0.2f,0,0);
        Vector3 rightSide = transform.position + new Vector3(-0.2f, 0, 0);

        RaycastHit2D hit = Physics2D.Raycast(transform.position, -transform.up, groundDetectDistance);
        RaycastHit2D leftSideCast = Physics2D.Raycast(leftSide, -transform.up, groundDetectDistance);
        RaycastHit2D rightSideCast = Physics2D.Raycast(rightSide, -transform.up, groundDetectDistance);

        if (hit && hit.transform.tag != "Bomb")
        {
            anim.SetBool("Airborne", false);
            return true;
        }
        else if(leftSideCast && leftSideCast.transform.tag != "Bomb")
        {
            anim.SetBool("Airborne", false);
            return true;
        }
        else if (rightSideCast && rightSideCast.transform.tag != "Bomb")
        {
            anim.SetBool("Airborne", false);
            return true;
        }

        anim.SetBool("Airborne", true);
        return false;
    }

    public void SetMouseLook(bool enabled)
    {
        usingMouse = enabled;
    }

    private void OnDrawGizmos()
    {
        Debug.DrawRay(transform.position, -transform.up * groundDetectDistance, Color.red);
        Debug.DrawRay(transform.position + new Vector3(0.2f, 0, 0), -transform.up * groundDetectDistance, Color.red);
        Debug.DrawRay(transform.position + new Vector3(-0.2f, 0, 0), -transform.up * groundDetectDistance, Color.red);
    }
}
