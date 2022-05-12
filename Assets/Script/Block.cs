using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public bool isBeingDestroyed;

    public int pointValue;

    public AudioSource source;
    public AudioClip fall;
    public AudioClip explode;

    public ParticleSystem blockDestructionParticles;

    public float speed = 1;
    public float doubleCheckDist = 1;

    public bool doneFalling;

    public Animator anim;

    bool isOverlappingWarning = false;

    protected GameManager manager;

    void Awake()
    {
         manager = FindObjectOfType<GameManager>();

        if (source == null)
        {
            source = GetComponent<AudioSource>();
        }
        anim = GetComponent<Animator>();

        speed += manager.currentRoundTimeInSeconds * manager.blockSpeedDifficultyRate;
    }

    void OnEnable()
    {

    }

    void OnDisable()
    {

    }

    //Create events to raise for each block destroyed;

    public virtual void DestroyBlock()
    {
        isBeingDestroyed = true;

        if (blockDestructionParticles != null)
        {
            ParticleSystem clone = Instantiate(blockDestructionParticles);
            clone.transform.parent = gameObject.transform.parent;
            clone.Play();
        }
        if (explode != null)
        {
            source.PlayOneShot(explode);
        }

        manager.OnBlockDestroyed(this);

        anim.SetTrigger("Destroy");
    }

    public virtual void CleanUpObject()
    {
        Destroy(gameObject);
    }

    public void Update()
    {
        if (doneFalling)
            return;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, -transform.up, doubleCheckDist);
        RaycastHit2D leftSide = Physics2D.Raycast(transform.position + new Vector3(-0.32f,0,0), -transform.up, doubleCheckDist);
        RaycastHit2D rightSide = Physics2D.Raycast(transform.position + new Vector3(0.32f, 0, 0), -transform.up, doubleCheckDist);

        if (hit)
        {
            if ((hit.transform.tag == "Block") || (hit.transform.tag == "Barrier"))// || (hit.transform.tag == "Bomb"))
            {
                if(hit.transform.tag == "Block")
                {
                    if(hit.transform.GetComponent<Block>().doneFalling)
                    {
                        doneFalling = true;
                        DoneFalling();
                    }
                }
                else
                {
                    doneFalling = true;
                    DoneFalling();
                }

                
                //If going to round to nearest, remember to use local position
                /*Vector3 pos = transform.position;
                pos.x = Mathf.Ceil(pos.x);
                pos.y = Mathf.Ceil(pos.y);

                transform.LocalSetPositionAndRotation(pos, transform.rotation);*/
            }
            else if ((hit.transform.tag == "Bomb"))
            {
                hit.transform.GetComponent<Bomb>().Explode();
            }
            else if((hit.transform.tag == "Player"))
            {
                PlayerControl playerControl = hit.transform.GetComponent<PlayerControl>();
                if(playerControl.DetectGround())
                {
                    playerControl.Die();
                }
            }
        }
        else if(leftSide)
        {
            if ((leftSide.transform.tag == "Block") || (leftSide.transform.tag == "Barrier"))// || (hit.transform.tag == "Bomb"))
            {
                if (leftSide.transform.tag == "Block")
                {
                    if (leftSide.transform.GetComponent<Block>().doneFalling)
                    {
                        doneFalling = true;
                        DoneFalling();
                    }
                }
                else
                {
                    doneFalling = true;
                    DoneFalling();
                }


                //If going to round to nearest, remember to use local position
                /*Vector3 pos = transform.position;
                pos.x = Mathf.Ceil(pos.x);
                pos.y = Mathf.Ceil(pos.y);

                transform.LocalSetPositionAndRotation(pos, transform.rotation);*/
            }
            else if ((leftSide.transform.tag == "Bomb"))
            {
                leftSide.transform.GetComponent<Bomb>().Explode();
            }
            else if ((leftSide.transform.tag == "Player"))
            {
                PlayerControl playerControl = leftSide.transform.GetComponent<PlayerControl>();
                if (playerControl.DetectGround())
                {
                    playerControl.Die();
                }
            }
        }
        else if(rightSide)
        {
            if ((rightSide.transform.tag == "Block") || (rightSide.transform.tag == "Barrier"))// || (hit.transform.tag == "Bomb"))
            {
                if (rightSide.transform.tag == "Block")
                {
                    if (rightSide.transform.GetComponent<Block>().doneFalling)
                    {
                        doneFalling = true;
                        DoneFalling();
                    }
                }
                else
                {
                    doneFalling = true;
                    DoneFalling();
                }


                //If going to round to nearest, remember to use local position
                /*Vector3 pos = transform.position;
                pos.x = Mathf.Ceil(pos.x);
                pos.y = Mathf.Ceil(pos.y);

                transform.LocalSetPositionAndRotation(pos, transform.rotation);*/
            }
            else if ((rightSide.transform.tag == "Bomb"))
            {
                rightSide.transform.GetComponent<Bomb>().Explode();
            }
            else if ((rightSide.transform.tag == "Player"))
            {
                PlayerControl playerControl = rightSide.transform.GetComponent<PlayerControl>();
                if (playerControl.DetectGround())
                {
                    playerControl.Die();
                }
            }
        }

        if (doneFalling)
            return;
        Falling();
    }

    public void DoneFalling()
    {
        source.PlayOneShot(fall);

        Vector3 pos = transform.localPosition;
        pos.x = Mathf.Round(Mathf.Ceil(pos.x));
        pos.y = Mathf.Round(Mathf.Ceil(pos.y));

        transform.localPosition.Set(pos.x,pos.y,0f);

        if (isOverlappingWarning)
        {
            FindObjectOfType<GameManager>().WarningLineHit();
        }
    }

    public virtual void Falling()
    {
        transform.Translate(-transform.up*speed*Time.deltaTime);
    }

    private void OnDrawGizmos()
    {
        Debug.DrawRay(transform.position, -transform.up * doubleCheckDist);
        Debug.DrawRay(transform.position + new Vector3(0.32f,0,0), -transform.up * doubleCheckDist);
        Debug.DrawRay(transform.position + new Vector3(-0.32f, 0, 0), -transform.up * doubleCheckDist);
    }


    public void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "WarningLine")
        {
            isOverlappingWarning = true;
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "WarningLine")
        {
            isOverlappingWarning = false;
        }
    }

}
