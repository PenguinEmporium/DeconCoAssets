using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GreenBarrel : Block
{
    public override void DestroyBlock()
    {
        isBeingDestroyed = true;

        foreach (Block b in FindObjectsOfType<Block>())
        {
            if (b != this)
            {
                if (!b.isBeingDestroyed)
                {
                    b.DestroyBlock();
                }
            }
        }

        if (explode != null)
        {
            source.PlayOneShot(explode);
        }

        manager.OnBlockDestroyed(this);

        anim.SetTrigger("Destroy");
    }
}