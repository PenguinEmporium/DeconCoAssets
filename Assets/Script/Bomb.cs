using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    public AudioSource source;

    public float timeTillExplode;

    Coroutine timerCoroutine;

    public int blastDistance;

    Animator anim;

    bool exploded = false;

    public void Awake()
    {
        if(source == null)
        {
            source = GetComponent<AudioSource>();
        }
        anim = GetComponent<Animator>();
        //EnableTimer();
    }

    public virtual void Explode()
    {
        if (exploded)
            return;

        exploded = true;

        RaycastHit2D[] leftResults = new RaycastHit2D[blastDistance];
        RaycastHit2D[] rightResults = new RaycastHit2D[blastDistance];
        RaycastHit2D[] upResults = new RaycastHit2D[blastDistance];
        RaycastHit2D[] downResults = new RaycastHit2D[blastDistance];

        //IDK what this is for
        ContactFilter2D contactFilter2D = new ContactFilter2D();

        if(Physics2D.Raycast(transform.position,-transform.right, contactFilter2D, leftResults, blastDistance) > 0)
        {
            foreach(RaycastHit2D hits in leftResults)
            {
                if (!hits)
                {
                    break;
                }

                if (hits.transform.TryGetComponent<Block>(out Block block))
                {
                    if (!block.isBeingDestroyed)
                    {
                        block.DestroyBlock();
                    }
                }
            }
        }
        if (Physics2D.Raycast(transform.position, transform.right, contactFilter2D, rightResults, blastDistance) > 0)
        {
            foreach (RaycastHit2D hits in rightResults)
            {
                if(!hits)
                {
                    break;
                }

                if (hits.transform.TryGetComponent<Block>(out Block block))
                {
                    if (!block.isBeingDestroyed)
                    {
                        block.DestroyBlock();
                    }
                }
            }
        }
        if (Physics2D.Raycast(transform.position, transform.up, contactFilter2D, upResults, blastDistance) > 0)
        {
            foreach (RaycastHit2D hits in upResults)
            {
                if (!hits)
                {
                    break;
                }

                if (hits.transform.TryGetComponent<Block>(out Block block))
                {
                    if (!block.isBeingDestroyed)
                    {
                        block.DestroyBlock();
                    }
                }
            }
        }
        if (Physics2D.Raycast(transform.position, -transform.up, contactFilter2D, downResults, blastDistance) > 0)
        {
            foreach (RaycastHit2D hits in downResults)
            {
                if (!hits)
                {
                    break;
                }

                if (hits.transform.TryGetComponent<Block>(out Block block))
                {
                    if (!block.isBeingDestroyed)
                    {
                        block.DestroyBlock();
                    }
                }
            }
        }

        anim.SetTrigger("Explode");
        //Debug.Log("Bomb exploded");
        source.Play();
    }

    public virtual void CleanUpObject()
    {
        Destroy(gameObject);
    }

    public virtual void EnableTimer()
    {
        timerCoroutine = StartCoroutine(Timer());
    }

    IEnumerator Timer()
    {
        float timer = 0f;
        while(timer < timeTillExplode)
        {
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        Explode();
    }

    //Create a function that checks the location of the bomb and set it to the grid once it's done moving


    private void OnDrawGizmos()
    {
        Debug.DrawRay(transform.position, -transform.right * blastDistance);
        Debug.DrawRay(transform.position, transform.right * blastDistance);
        Debug.DrawRay(transform.position, transform.up * blastDistance);
        Debug.DrawRay(transform.position, -transform.up * blastDistance);
    }
}
