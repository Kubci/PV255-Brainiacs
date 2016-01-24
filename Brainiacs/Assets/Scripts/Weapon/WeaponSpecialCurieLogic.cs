﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WeaponSpecialCurieLogic : MonoBehaviour
{
    private BulletManager bulletManager;
    private PlayerBase playerBase;

    private Dictionary<Vector2, Sprite> spriteMapping = new Dictionary<Vector2,Sprite>();
    private Dictionary<Vector2, RuntimeAnimatorController> crashMapping = new Dictionary<Vector2,RuntimeAnimatorController>();

    private Dictionary<Vector2, string> animatorDirectionMapping = new Dictionary<Vector2, string> { { Vector2.up, "up"}, { Vector2.right, "right" }, { Vector2.down, "down" }, { Vector2.left, "left" } };
    private Dictionary<Vector2, string> animatorCrashDirectionMapping = new Dictionary<Vector2, string> { { Vector2.up, "upCrash" }, { Vector2.right, "rightCrash" }, { Vector2.down, "downCrash" },
        { Vector2.left, "leftCrash" } };

    private SpriteRenderer renderer;
    private Animator animator;

    private FireProps fireProps;
    private bool update = false;

    private float traveledDistance = 0;
    private float clicksShooted = 0;
    private float shootEveryDst = 0.75f;

    /*
    public void SetUpGraphics()
    {
        renderer = GetComponent<SpriteRenderer>();
        
        RuntimeAnimatorController rtac;

        Debug.Log("Start from curie special");
        spriteMapping.Add(Vector2.up, Resources.LoadAll<Sprite>("Sprites/Special/bullet_curieSpecial_up")[0]);
        rtac = Resources.Load<RuntimeAnimatorController>("Animations/Special/bullet_curieSpecial_left_crash") as RuntimeAnimatorController;
        crashMapping.Add(Vector2.up, rtac);

        spriteMapping.Add(Vector2.left, Resources.LoadAll<Sprite>("Sprites/Special/bullet_curieSpecial_left")[0]);
        rtac = Resources.Load<RuntimeAnimatorController>("Animations/Special/bullet_curieSpecial_left_crash") as RuntimeAnimatorController;
        crashMapping.Add(Vector2.left, rtac);

        spriteMapping.Add(Vector2.right, Resources.LoadAll<Sprite>("Sprites/Special/bullet_curieSpecial_right")[0]);
        rtac = Resources.Load<RuntimeAnimatorController>("Animations/Special/bullet_curieSpecial_left_crash") as RuntimeAnimatorController;
        crashMapping.Add(Vector2.right, rtac);

        spriteMapping.Add(Vector2.down, Resources.LoadAll<Sprite>("Sprites/Special/bullet_curieSpecial_down")[0]);
        rtac = Resources.Load<RuntimeAnimatorController>("Animations/Special/bullet_curieSpecial_left_crash") as RuntimeAnimatorController;
        crashMapping.Add(Vector2.down, rtac);
    }*/

    public void SetUpVariables(PlayerBase pb, BulletManager bm)
    {
        playerBase = pb;
        bulletManager = bm;
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (update)
        {
            transform.position = new Vector2(transform.position.x + (fireProps.direction.x/10),
                transform.position.y + (fireProps.direction.y/10));

            traveledDistance += new Vector2((fireProps.direction.x / 10), (fireProps.direction.y / 10)).magnitude;

            if (traveledDistance > (clicksShooted * shootEveryDst))
            {
                Debug.Log("Cecky");

                if (fireProps.direction == Vector2.left || fireProps.direction == Vector2.right)
                {
                    bulletManager.fire(Vector2.up, transform.position, fireProps.animController, fireProps.bulletSpeed,
                        fireProps.damage, "sniper");
                    bulletManager.fire(Vector2.down, transform.position, fireProps.animController, fireProps.bulletSpeed,
                        fireProps.damage, "sniper");
                }
                else
                {
                    bulletManager.fire(Vector2.left, transform.position, fireProps.animController, fireProps.bulletSpeed,
                       fireProps.damage, "sniper");
                    bulletManager.fire(Vector2.right, transform.position, fireProps.animController, fireProps.bulletSpeed,
                        fireProps.damage, "sniper");
                }
                clicksShooted++;
            }
        }
    }

    public void fire(FireProps fp)
    {
        animator.SetBool("returnToOrig", true);
        nullAllAnimBools();

        fireProps = fp;
        moveInShootingDirection();
        update = true;
        //renderer.sprite = spriteMapping[fp.direction];
        setBoolAnimator(animatorDirectionMapping[fp.direction], true);

    }

    void moveInShootingDirection()
    {
        Vector2 pos = fireProps.position;

        if (fireProps.direction == Vector2.up)
        {
            transform.position = pos + new Vector2(0, 0.5f);
        }
        else if (fireProps.direction == Vector2.down)
        {
            transform.position = pos + new Vector2(0, -0.7f);
        }
        else if (fireProps.direction == Vector2.left)
        {
            transform.position = pos + new Vector2(-0.5f, 0);
        }
        else
        {
            transform.position = pos + new Vector2(0.5f, 0);
        }
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        if ((coll.gameObject.tag == "Barrier") || (coll.gameObject.tag == "Border"))
        {
            StartCoroutine(PlayCrash());
        }
        if ((coll.gameObject.tag == "Player"))
        {
            coll.gameObject.GetComponent<PlayerBase>().ApplyDamage(1000, playerBase);
            StartCoroutine(PlayCrash());
        }
    }

    IEnumerator PlayCrash()
    {
        //animator.runtimeAnimatorController = null;
        //animator.runtimeAnimatorController = crashMapping[fireProps.direction];
        setBoolAnimator(animatorCrashDirectionMapping[fireProps.direction], true);
        fireProps.direction = new Vector2(0, 0);
        yield return new WaitForSeconds(1.0f);                                               // tento prikaz kurvy cely kod
        animator.SetBool("returnToOrig", true);
        nullAllAnimBools();
        gameObject.SetActive(false);
        
        //animator.runtimeAnimatorController = null;
    }

    private void setBoolAnimator(string boolName, bool value)
    {
        animator.SetBool(boolName, value);
    }

    private void nullAllAnimBools()
    {
        animator.SetBool("up", false);
        animator.SetBool("right", false);
        animator.SetBool("down", false);
        animator.SetBool("left", false);
        
        animator.SetBool("upCrash", false);
        animator.SetBool("rightCrash", false);
        animator.SetBool("downCrash", false);
        animator.SetBool("leftCrash", false);
        animator.SetBool("returnToOrig", false);
    }
}