﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AiBase : PlayerBase
{

    public PlayerBase player1;
    public PlayerBase player2;
    public PlayerBase player3;
    public PlayerBase player4;

    Components comp;
    PlayerInfo playInfo;

    static int priority0 = 0;
    static int priority10 = 10;
    static int priority25 = 25;
    static int priority40 = 40;
    static int priority50 = 50;
    static int priority70 = 70;
    static int priority80 = 80;
    static int priority90 = 90;
    static int priority100 = 100;

    public int killPlayer1Priority;
    public int killPlayer2Priority;
    public int killPlayer3Priority;
    public int killPlayer4Priority;

    public int avoidBulletPriority;

    public Vector2 lastPosition;    //asi k ničemu - smazat
    public Vector2 preLastPosition; //asi k ničemu - smazat

    //////////////////////////////MAP shit
    public GameObject[] barriers;

    public List<GameObject> itemPowerUps;
    public List<GameObject> itemWeapons;

    public bool bulletIncoming;
    public Vector2 bulletFrom;

    /// //////////////////////////////////// CHARACTER COORDINATES /////////////////////////////
    Renderer aiRenderer;
    Collider2D collider;

    public Vector2 charBotLeft;
    public Vector2 charBotRight;
    public Vector2 charTopLeft;
    public Vector2 charTopRight;

    public Vector2 charBot;
    public Vector2 charTop;
    public Vector2 charLeft;
    public Vector2 charRight;

    public Vector2 currentTargetDestination;

    public AiActionEnum currentAction { get; set; }

    public void setUpAiB(Components c, PlayerInfo p)
    {
        comp = c;
        playInfo = p;
        //weaponHandling = GetComponent<WeaponHandling>();
        rb2d = gameObject.GetComponent<Rigidbody2D>();
        //Debug.Log(rb2d);
        aiRenderer = GetComponent<Renderer>();
        currentTargetDestination = new Vector2(0, 0);
        collider = GetComponent<Collider2D>();

        walkingFront = new List<Vector2>();

        barriers = GameObject.FindGameObjectsWithTag("Barrier");

        itemPowerUps = new List<GameObject>();
        itemWeapons = new List<GameObject>();
        pickWeaponPriority = priority0;
        pickPowerUpPriority = priority0;

        //Debug.Log(barriers[0].name);
        //Debug.Log(barriers[1].name);
        //Debug.Log(barriers[2].name);
        //Debug.Log(barriers[3].name);
        bulletIncoming = false;
        bulletFrom = new Vector2(0, 0);

        characterColliderHeight = gameObject.GetComponent<BoxCollider2D>().size.y;
        characterColliderWidth = gameObject.GetComponent<BoxCollider2D>().size.x;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.frameCount == 5)
        {
            SetPlayers();
            //Debug.Log("Other players set");
            lastPosition = new Vector2(0, 0);
            lastPosition = new Vector2(0, 1);
        }

        //////////CHEK EVERY FRAME
        
        
        

        //////////CHEK ONCE PER SECOND
        if (Time.frameCount % 30 == 6)
        {
            UpdatePriorities();
            /*
            Debug.Log("kill_1=" + killPlayer1Priority);
            Debug.Log("kill_2=" + killPlayer2Priority);
            Debug.Log("kill_3=" + killPlayer3Priority);
            Debug.Log("kill_4=" + killPlayer4Priority);
            */

            //dont kill yourself
            switch (playerNumber)
            {
                case 1:
                    killPlayer1Priority = 0;
                    break;
                case 2:
                    killPlayer2Priority = 0;
                    break;
                case 3:
                    killPlayer3Priority = 0;
                    break;
                case 4:
                    killPlayer4Priority = 0;
                    break;
                default:
                    Debug.Log(gameObject.ToString() + " has no player number");
                    break;
            }

            //Debug.Log("!");
            //PrintPriorities();
            //UpdateCurrentAction(); //has to be checked faster
            UpdateCurrentAction();

            UpdateLastPosition();


            
        }

        //////////CHECK 4 PER SECOND
        if (Time.frameCount % 8 == 0)
        {
           
        }

        if (Time.frameCount % 200 == 0)
        {
            //PrintPriorities();
            PrintAction();
        }

        //Debug.Log("currentAction:"+currentAction);

        //CheckAmmo(); //check when firing

        //UpdateCurrentAction(); //lagz
        DoCurrentAction();
        UpdateDirection();

        //PrintPriorities();
        ///PrintAction();


    }


    public void UpdateLastPosition()
    {
        preLastPosition = lastPosition;
        lastPosition = gameObject.transform.position;
    }

    public void PrintAction()
    {
        string message = "";
        message += "current action=" + currentAction;

        Debug.Log(message);
    }

    public void PrintPriorities()
    {
        string message = "priorities \n";
        message += "pickWeaponPriority=" + pickWeaponPriority + ",\n";
        message += "pickPowerUpPriority=" + pickPowerUpPriority + ",\n";

        message += "KillPlayer1=" + killPlayer1Priority;
        message += ",KillPlayer2=" + killPlayer2Priority;
        message += ",KillPlayer3=" + killPlayer3Priority;
        message += ",KillPlayer4=" + killPlayer4Priority;
        message += ",avoidBulletPriority=" + avoidBulletPriority;

        Debug.Log(message);
    }

    public void DoCurrentAction()
    {
        switch (currentAction)
        {
            case AiActionEnum.killPlayer1:
                KillPlayer(player1);
                break;
            case AiActionEnum.killPlayer2:
                KillPlayer(player2);
                break;
            case AiActionEnum.killPlayer3:
                KillPlayer(player3);
                break;
            case AiActionEnum.killPlayer4:
                KillPlayer(player4);
                break;
            case AiActionEnum.avoidBullet:
                AvoidBullet();
                break;
            case AiActionEnum.pickupPowerUp:
                PickUp(bestPowerUp);
                break;
            case AiActionEnum.pickupWeapon:
                PickUp(bestWeapon);
                break;
            default:
                Stand();
                break;
        }
        //Debug.Log("processing action: " + currentAction);
    }

    public bool CanShoot(Vector2 center, Vector2 direction)
    {

        Ray rayGun = new Ray(center, direction);

        float mapLenght = 15;
        RaycastHit2D[] hitGun = Physics2D.RaycastAll(rayGun.origin, direction, mapLenght);

        Debug.DrawRay(rayGun.origin, direction, Color.cyan);
        Debug.DrawRay(rayGun.origin, direction * -1, Color.red);

        if (hitGun.Length != 0)
        {
            //for (int i = 0; i < hitGun.Length; i++)
            //{
            //    Debug.Log("hit from " + center + " to " + hitGun[i].transform.name + " in dir:" + direction);
            //}
            //Debug.Log("---");
            //Borders have tag "Barrier" and it sometimes doesnt hit player first, but the border
            if (hitGun[0].transform.tag == "Barrier"
                && hitGun[0].transform.gameObject.layer != LayerMask.NameToLayer("Border"))
            {
                //Debug.Log("cant shoot");
                return false;
            }
        }
        return true;
    }

    public bool IsInPlayground(Vector2 point)
    {
        return mapMinX < point.x && point.x < mapMaxX && mapMinY < point.y && point.y < mapMaxY;
    }
    
    public void KillPlayer(PlayerBase player)
    {
        Vector2 targetPlayerPosition = player.transform.position;

        //move to same axis 
        float targetX;
        float targetY;

        Vector2 bestHorizontal = new Vector2(player.posX, player.posY);
        Vector2 bestVertical = new Vector2(player.posX, player.posY);

        Vector2 bestHorizontalUp = bestHorizontal;
        Vector2 bestHorizontalDown = bestHorizontal;

        Vector2 bestVerticalLeft = bestVertical;
        Vector2 bestVerticalRight = bestVertical;


        while (bestHorizontalDown.y > mapMinY && !CharacterCollidesBarrier(bestHorizontalDown) && bestHorizontalDown.y > posY)
        {
            bestHorizontalDown.y -= 0.1f;
        }

        //mapMaxY = 4f;
        
        while (bestHorizontalUp.y < mapMaxY && !CharacterCollidesBarrier(bestHorizontalUp) && bestHorizontalUp.y < posY)
        {
            bestHorizontalUp.y += 0.1f;
        }
        while (bestVerticalLeft.x > mapMinX && !CharacterCollidesBarrier(bestVerticalLeft) && bestVerticalLeft.x > posX)
        {
            bestVerticalLeft.x -= 0.1f;
        }
        while (bestVerticalRight.x < mapMaxX && !CharacterCollidesBarrier(bestVerticalRight) && bestVerticalRight.x < posX)
        {
            bestVerticalRight.x += 0.1f;
        }

        List<Vector2> possibleShootSpots = new List<Vector2>();
        possibleShootSpots.Add(bestHorizontalDown);
        possibleShootSpots.Add(bestHorizontalUp);
        possibleShootSpots.Add(bestVerticalLeft);
        possibleShootSpots.Add(bestVerticalRight);

        int spotIndex = 0;
        float distanceFromMe = 100; //big enough
        for (int i = 0; i < possibleShootSpots.Count; i++)
        {

            //Debug.Log(possibleShootSpots[i]);
            if (distanceFromMe > GetDistance(gameObject.transform.position, possibleShootSpots[i]))
            {
                spotIndex = i;
                distanceFromMe = GetDistance(gameObject.transform.position, possibleShootSpots[i]);
            }
        }

        //Debug.Log("move to: " + possibleShootSpots[spotIndex]);
        Debug.DrawRay(possibleShootSpots[spotIndex], up, Color.cyan);
        //MoveTo(possibleShootSpots[spotIndex]);


        //if you are on same axis -> turn his direction and shoot

        //if (AlmostEqual(posX, player.posX, 0.1) || AlmostEqual(posY, player.posY, 0.1))
        if(MoveTo(possibleShootSpots[spotIndex]))
        {
            //Debug.Log("i can shoot");
            //look at him (if you are not already looking at him)

            if(GetObjectDirection(player.gameObject) != direction)
                LookAt(player.gameObject);
            else
            {
                //UpdateAnimatorState(AnimatorStateEnum.stop);
            }

            if (CanShoot(transform.position, direction))
            {
                //Debug.Log("I can shoot from:" + transform.position + " to: " + direction);
                //chybí implementace kadence zbraně
                //CheckAmmo();
                weaponHandling.fire(direction);
            }
        }


    }

    public bool AlmostEqual(float pos1, float pos2, double e)
    {
        return pos2 - e < pos1 && pos1 < pos2 + e;
    }

    public Vector2 GetObjectDirection(GameObject obj)
    {
        double distanceX = Mathf.Abs(posX - obj.transform.position.x);
        double distanceY = Mathf.Abs(posY - obj.transform.position.y);

        //prefer more dominant axis
        if (distanceX > distanceY)
        {
            //it is on my left
            if (obj.transform.position.x < posX)
            {
                return left;
            }
            else
            {
                return right;
            }
        }
        else
        {
            //it is below me
            if (obj.transform.position.y < posY)
            {
                return down;
            }
            else
            {
                return up;
            }
        }
    }

    public void LookAt(GameObject obj)
    {

        double distanceX = Mathf.Abs(posX - obj.transform.position.x);
        double distanceY = Mathf.Abs(posY - obj.transform.position.y);

        //prefer more dominant axis
        if (distanceX > distanceY)
        {
            //it is on my left
            if (obj.transform.position.x < posX)
            {
                direction = left;
                UpdateAnimatorState(AnimatorStateEnum.walkLeft);
            }
            else
            {
                direction = right;
                UpdateAnimatorState(AnimatorStateEnum.walkRight);
            }
        }
        else
        {
            //it is below me
            if (obj.transform.position.y < posY)
            {
                direction = down;
                UpdateAnimatorState(AnimatorStateEnum.walkDown);
            }
            else
            {
                direction = up;
                UpdateAnimatorState(AnimatorStateEnum.walkUp);
            }
        }
        //UpdateAnimatorState(AnimatorStateEnum.stop);

    }

    


    public void Stand()
    {
        Stop();
    }

    // <<...COMMANDS>>

    public void UpdatePriorities()
    {
        //set kill priorities only when you can shoot
        if(CheckAmmo())
            SetKillPriorities();
        

        //register incoming bullets, powerups,...
        LookAroundYourself();

        SetPowerUpsPriority();

        SetWeaponsPriority();

        if (bulletIncoming)
        {
            SetAvoidBulletPriority(priority100);
        }

        //check bullets
        //CheckAmmo(); //has to be updated faster


        //PrintPriorities();

        //....
    }

    ////////////////////////CHECK AMMO
    public bool CheckAmmo()
    {
        bool canShoot = true;
        //Debug.Log(weaponHandling.activeWeapon.ammo);
        if(!weaponHandling.activeWeapon.ready || !weaponHandling.activeWeapon.kadReady)
        {
            //try switching to another fire weapon - TODO
            //Debug.Log("CANT shoot");

            canShoot = false;
        }

        //Debug.Log(canShoot);

        if (!canShoot)
        {

            killPlayer1Priority -= priority50;
            killPlayer2Priority -= priority50;
            killPlayer3Priority -= priority50;
            killPlayer4Priority -= priority50;
        }

        return canShoot;
    }




    /// ////////////////////////////AVOID BULLETS

    public void SetAvoidBulletPriority(int priority)
    {
        avoidBulletPriority = priority;

        killPlayer1Priority = priority0;
        killPlayer2Priority = priority0;
        killPlayer3Priority = priority0;
        killPlayer4Priority = priority0;
    }

    float characterColliderWidth;
    float characterColliderHeight;

    public bool RegisterBullets()
    {
        bulletIncoming = false;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, mapWidth / 2, bulletMask);



        foreach (Collider2D collider in colliders)
        {
            // enemies within 1m of the player
            //Debug.Log(collider.name);
            Vector2 bulletPosition = collider.transform.position;
            Vector2 bulletDirection = collider.GetComponent<Bullet>().direction;
            //Debug.Log(bulletPosition);
            //Debug.Log(bulletDirection);

            //Debug.Log(bulletPosition.y);
            //Debug.Log(posY);
            //Debug.Log(characterColliderHeight);



            if (AlmostEqual(bulletPosition.x, posX, characterColliderWidth))//bullet above or bellow
            {
                if (bulletPosition.y > posY) //bullet is above
                {
                    if (bulletDirection == down) //bullet is aiming down
                    {
                        bulletIncoming = true;
                        bulletFrom = up;
                    }
                }
                else //bullet is bellow
                {
                    if (bulletDirection == up) //bullet is aiming up
                    {
                        bulletIncoming = true;
                        bulletFrom = down;
                    }
                }
            }
            else if (AlmostEqual(bulletPosition.y, posY, characterColliderHeight))//bullet is on left or right
            {
                if (bulletPosition.x > posX) //bullet is on right
                {
                    if (bulletDirection == left) //bullet is aiming left
                    {
                        bulletIncoming = true;
                        bulletFrom = right;
                    }
                }
                else //bullet on left
                {
                    if (bulletDirection == right) //bullet is aiming right
                    {
                        bulletIncoming = true;
                        bulletFrom = left;
                    }
                }
            }


        }
        //Debug.Log(bulletIncoming);
        return bulletIncoming;
    }
    
    public bool decidedDirectionBool = false;
    public Vector2 decidedDirection;
    public void AvoidBullet()
    {
        //Debug.Log("decidedDirectionBool:"+ decidedDirectionBool);
        //Debug.Log("decidedDirection:" + decidedDirection);

        if (
            (decidedDirection == up && Collides(up,1))||
            (decidedDirection == right && Collides(right, 1)) ||
            (decidedDirection == down && Collides(down, 1)) ||
            (decidedDirection == left && Collides(left, 1))
            )
        {
            decidedDirectionBool = false;
        }

        if (decidedDirectionBool)
        {
            if(decidedDirection == up)
            {
                MoveTo(posX, posY+1);
            }
            else if (decidedDirection == right)
            {
                MoveTo(posX+1, posY);
            }
            else if (decidedDirection == down)
            {
                MoveTo(posX, posY-1);
            }
            else if (decidedDirection == left)
            {
                MoveTo(posX - 1, posY);
            }
            else
            {
                Debug.Log("fail direction");
            }
        }
        else
        {
            if (bulletFrom == up)
            {
                if (!Collides(left, 1))
                {
                    MoveTo(posX - 1, posY);
                    decidedDirection = left;
                }
                else if (!Collides(right, 1))
                {
                    MoveTo(posX + 1, posY);
                    decidedDirection = right;
                }
                else if (!Collides(down, 1))
                {
                    MoveTo(posX, posY - 1);
                    decidedDirection = down;
                }
            }
            else if (bulletFrom == right)
            {
                //Debug.Log("from right");
                if (!Collides(up, 1))
                {
                    MoveTo(posX, posY + 1);
                    decidedDirection = up;
                    Debug.Log("go up");
                }
                else if (!Collides(down, 1))
                {
                    Debug.Log("go down");
                    MoveTo(posX, posY - 1);
                    decidedDirection = down;
                }
                else if (!Collides(left, 1))
                {
                    Debug.Log("go left");
                    MoveTo(posX - 1, posY);
                    decidedDirection = left;
                }
                else
                {
                    Debug.Log("cant avoid");
                }
            }
            else if (bulletFrom == down)
            {
                if (!Collides(left, 1))
                {
                    MoveTo(posX - 1, posY);
                    decidedDirection = left;
                }
                else if (!Collides(right, 1))
                {
                    MoveTo(posX + 1, posY);
                    decidedDirection = right;
                }
                else if (!Collides(up, 1))
                {
                    MoveTo(posX, posY + 1);
                    decidedDirection = up;
                }
            }
            else if (bulletFrom == left)
            {
                //Debug.Log("from right");
                if (!Collides(up, 1))
                {
                    MoveTo(posX, posY + 1);
                    decidedDirection = up;
                }
                else if (!Collides(down, 1))
                {
                    MoveTo(posX, posY - 1);
                    decidedDirection = down;
                }
                else if (!Collides(right, 1))
                {
                    MoveTo(posX + 1, posY);
                    decidedDirection = right;
                }
            }
            decidedDirectionBool = true;
        }

        //chek if you avoided
        if (!RegisterBullets())
        {
            decidedDirectionBool = false;
            decidedDirection = down;
            Debug.Log("bullet avoided");
            SetAvoidBulletPriority(priority0);
            UpdateCurrentAction();
        }
    }

    public LayerMask bulletMask;
    public LayerMask itemMask;
    

    public void LookAroundYourself()
    {
        RegisterBullets();

        RegisterPowerUps();

        RegisterWeapons();
        

    }

    ////////////////////////WEAPONS
    public void RegisterWeapons()
    {
        itemWeapons.Clear();
        Collider2D[] items = Physics2D.OverlapCircleAll(transform.position, mapWidth / 2, itemMask);

        foreach (Collider2D item in items)
        {
            if (item.transform.tag == "Weapon" && item.gameObject.activeSelf)
            {
                itemWeapons.Add(item.gameObject);
            }
        }
    }
    GameObject bestWeapon;
    int pickWeaponPriority;

    public void SetWeaponsPriority()
    {
        List<int> weaponsPriorities = new List<int>();

        if (itemWeapons.Count == 0)
        {
            //Debug.Log("no items around");
            pickWeaponPriority = priority0;
            return;
        }

        int highestPriority = 0;
        foreach (GameObject weapon in itemWeapons)
        {
            //Debug.Log("I see " + powerUp.name);
            WeaponManager manager = weapon.GetComponent<WeaponManager>();
            float distanceFromMe = GetDistance(gameObject, weapon);
            float distanceFactor = GetDistanceFactor(distanceFromMe);

            int priority = 0;
            switch (manager.type)
            {

                case WeaponEnum.flamethrower:
                    float flamethrowerFactor = distanceFactor;// * weaponHandling.activeWeapon.ammo / weaponHandling.activeWeapon.clip;
                    /*if (HasWeapon(WeaponEnum.flamethrower))
                    {
                        //flamethrowerFactor /= ...
                    }
                    */

                    priority = (int)flamethrowerFactor * 10;
                    break;

                case WeaponEnum.shotgun:
                    float shotgunFactor = distanceFactor;// * weaponHandling.activeWeapon.ammo / weaponHandling.activeWeapon.clip;
                    /*if (HasWeapon(WeaponEnum.flamethrower))
                    {
                        //flamethrowerFactor /= ...
                    }
                    */

                    priority = (int)shotgunFactor * 10;
                    break;

                default:
                    priority = 0;
                    break;
            }
            if (priority == 0)
                priority = priority10;
            weaponsPriorities.Add(priority);

            //Debug.Log("setting: " + powerUpsPriorities[powerUpsPriorities.Count - 1]);

            if (weaponsPriorities[weaponsPriorities.Count - 1] > highestPriority)
            {
                highestPriority = weaponsPriorities[weaponsPriorities.Count - 1];
            }
        }

        bestWeapon = itemWeapons[weaponsPriorities.IndexOf(highestPriority)];

        pickWeaponPriority = highestPriority;
        //Debug.Log("pickPowerUpPriority:" + pickPowerUpPriority);


    }


    /// ////////////////////POWERUPS

    GameObject bestPowerUp;
    int pickPowerUpPriority;

    public void SetPowerUpsPriority()
    {
        List<int> powerUpsPriorities = new List<int>();

        if (itemPowerUps.Count == 0)
        {
            //Debug.Log("no items around");
            pickPowerUpPriority = priority0;
            return;
        }

        int highestPriority = 0;
        foreach (GameObject powerUp in itemPowerUps)
        {
            //Debug.Log("I see " + powerUp.name);
            PowerUpManager manager = powerUp.GetComponent<PowerUpManager>();
            if(manager == null)
            {
                Debug.Log(powerUp + " has no manager");
                return;
            }

            float distanceFromMe = GetDistance(gameObject, powerUp);
            float distanceFactor = GetDistanceFactor(distanceFromMe);

            int priority = 0;
            switch (manager.type)
            {

                case PowerUpEnum.Ammo:
                    float ammoFactor = distanceFactor * weaponHandling.activeWeapon.ammo / weaponHandling.activeWeapon.clip;
                    priority = (int)ammoFactor * 10;
                    break;
                case PowerUpEnum.Heal:
                    float healthFactor = distanceFactor * hitPoints / GetMaxHp();
                    priority = (int)healthFactor * 10;
                    break;
                case PowerUpEnum.Mystery:
                    int mysteryFactor = (int)distanceFactor * Random.Range(0, 80);
                    mysteryFactor += (int)distanceFactor * priority25;
                    priority = mysteryFactor;
                    break;
                case PowerUpEnum.Shield:
                    float shieldFactor = distanceFactor * (priority50 + Random.Range(0, 20));
                    priority = (int)shieldFactor;
                    break;
                case PowerUpEnum.Speed:
                    float speedFactor = distanceFactor * (priority50 + Random.Range(0, 20));
                    priority = (int)speedFactor;
                    break;
            }
            if (priority == 0)
                priority = priority10;
            powerUpsPriorities.Add(priority);

            //Debug.Log("setting: " + powerUpsPriorities[powerUpsPriorities.Count - 1]);

            if (powerUpsPriorities[powerUpsPriorities.Count - 1] > highestPriority)
            {
                highestPriority = powerUpsPriorities[powerUpsPriorities.Count - 1];
            }
        }

        bestPowerUp = itemPowerUps[powerUpsPriorities.IndexOf(highestPriority)];

        pickPowerUpPriority = highestPriority;
        //Debug.Log("pickPowerUpPriority:" + pickPowerUpPriority);


    }

    public void PickUp(GameObject obj)
    {
        //Debug.Log("picking up " + obj.name);
        if (MoveTo(obj.transform.position))
        {
            //Debug.Log("picked up");
            LookAroundYourself();
        }
    }

    public void RegisterPowerUps()
    {
        itemPowerUps.Clear();
        Collider2D[] items = Physics2D.OverlapCircleAll(transform.position, mapWidth / 2, itemMask);
        
        foreach (Collider2D item in items)
        {
            if(item.transform.tag == "PowerUp" && item.gameObject.activeSelf)
            {
                itemPowerUps.Add(item.gameObject);
            }
        }
    }

    
    /// ////////////////////////////.........

    public void UpdateCurrentAction()
    {
        int highestPriority = GetCurrentHighestPriority();

        if (killPlayer1Priority >= highestPriority)
        {
            currentAction = AiActionEnum.killPlayer1;
        }
        else if (killPlayer2Priority >= highestPriority)
        {
            currentAction = AiActionEnum.killPlayer2;
        }
        else if (killPlayer3Priority >= highestPriority)
        {
            currentAction = AiActionEnum.killPlayer3;
        }
        else if (killPlayer4Priority >= highestPriority)
        {
            currentAction = AiActionEnum.killPlayer4;
        }
        else if(avoidBulletPriority >= highestPriority)
        {
            currentAction = AiActionEnum.avoidBullet;
        }
        else if(pickPowerUpPriority >= highestPriority)
        {
            currentAction = AiActionEnum.pickupPowerUp;
        }
        else if (pickWeaponPriority >= highestPriority)
        {
            currentAction = AiActionEnum.pickupWeapon;
        }
        else
        {
            currentAction = AiActionEnum.stand;
        }

        //currentAction = AiActionEnum.stand;
    }

    public int GetCurrentHighestPriority()
    {
        int highestPriority = 0;
        if (killPlayer1Priority > highestPriority)
            highestPriority = killPlayer1Priority;
        if (killPlayer2Priority > highestPriority)
            highestPriority = killPlayer2Priority;
        if (killPlayer3Priority > highestPriority)
            highestPriority = killPlayer3Priority;
        if (killPlayer4Priority > highestPriority)
            highestPriority = killPlayer4Priority;
        if (avoidBulletPriority > highestPriority)
            highestPriority = avoidBulletPriority;
        if (pickPowerUpPriority > highestPriority)
            highestPriority = pickPowerUpPriority;

        //Debug.Log("highestPriority:" + highestPriority);
        if (highestPriority == 0)
            highestPriority++;
        return highestPriority;
    }

    /// <summary>
    /// nastaví kill priority podle zdálenosti
    /// 200 je cca max vzdálenost na mapě
    /// </summary>
    public void SetKillPriorities()
    {
        if (player1 != null && playerNumber != 1)
            killPlayer1Priority = (int) (priority90 * GetDistanceFactor(GetDistance(gameObject, player1.gameObject)));
        if(player2 != null && playerNumber != 2)
            killPlayer2Priority = (int) (priority90 * GetDistanceFactor(GetDistance(gameObject, player2.gameObject)));
        if (player3 != null && playerNumber != 3)
            killPlayer3Priority = (int) (priority90 * GetDistanceFactor(GetDistance(gameObject, player3.gameObject)));
        if (player4 != null && playerNumber != 4)
            killPlayer4Priority = (int) (priority90 * GetDistanceFactor(GetDistance(gameObject, player4.gameObject)));


    }

    public Vector3 GetMyPosition()
    {
        return gameObject.transform.position;
    }

    public float GetDistance(GameObject object1, GameObject object2)
    {
        return (object1.transform.position - object2.transform.position).sqrMagnitude;
    }

    public float GetDistance(Vector2 pos1, Vector2 pos2)
    {
        return (pos1 - pos2).sqrMagnitude;
    }

    public float GetDistanceFactor(float distance)
    {
        return (priority100 - distance)/priority100;
    }

    public List<PlayerBase> GetPlayers()
    {
        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
        List<PlayerBase> allPlayersBase = new List<PlayerBase>();
        foreach (GameObject obj in allPlayers)
        {
            allPlayersBase.Add(obj.GetComponent<PlayerBase>());
        }
        return allPlayersBase;
    }

    public void SetPlayers()
    {
        List<PlayerBase> allPlayers = new List<PlayerBase>();
        allPlayers = GetPlayers();
        foreach (PlayerBase player in allPlayers)
        {
            switch (player.playerNumber)
            {
                case 1:
                    player1 = player;
                    break;
                case 2:
                    player2 = player;
                    break;
                case 3:
                    player3 = player;
                    break;
                case 4:
                    player4 = player;
                    break;
                default:
                    Debug.Log(player.ToString() + " has no player number!");
                    break;
            }
            //Debug.Log("setting player: " + player.gameObject.name + ", number: " + player.playerNumber);
        }
    }



    /// <summary>
    /// funguje jen když je barrier trigger....asi vymyslím jinak
    /// </summary>
    /// <param name="coll"></param>
    void OnTriggerEnter2D(Collider2D coll)
    {
        //Debug.Log("col");
        if ((coll.gameObject.tag == "Barrier"))
        {
            //Debug.Log("Barrier");
            MoveDown();
            MoveLeft();
        }
    }




    /// <summary>
    /// řeší stav po skončení kolize
    /// bez toho by se zaseknul na jednom místě
    /// </summary>
    public void AfterCollision()
    {

        if (lastDirection == down || lastDirection == up)
        {
            if (!Collides(right))
            {
                MoveRight();
            }
            else if (!Collides(left))
            {
                MoveLeft();
            }
        }
        else if (lastDirection == left || lastDirection == right)
        {
            if (!Collides(up))
            {
                MoveUp();
            }
            else if (!Collides(down))
            {
                MoveDown();
            }
        }
    }


    public bool MoveTo(Vector2 position)
    {
        return MoveTo(position.x, position.y);
    }

    public List<Vector2> walkingFront;

    public List<Vector2> GetNodes()
    {
        List<Vector2> nodes = new List<Vector2>();
        nodes.Add(new Vector2(0, 0));
        nodes.Add(new Vector2(1, 0));
        nodes.Add(new Vector2(1, 1));
        nodes.Add(new Vector2(2, 1));
        return nodes;
    }

    /// <summary>
    /// node for pathfinding algorithm
    /// </summary>
    public class PathNode
    {
        public Vector2 node;
        public int parentIndex;

        public PathNode(Vector2 node, int parentIndex)
        {
            this.node = node;
            this.parentIndex = parentIndex;

        }

        public override bool Equals(object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            PathNode p = obj as PathNode;
            if ((System.Object)p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return node == p.node;
        }

        public override string ToString()
        {
            return node.ToString();
        }
    }

    public List<Vector2> GetPathTo(Vector2 target)
    {
        float step = characterColliderWidth;
        //float step = characterWidth/2;
        //Debug.Log(characterWidth);
        //um....wtf, proč když nenapíšu ručně 0.5f tak to nejde?

        List<Vector2> path = new List<Vector2>();
        PathNode startNode = new PathNode(new Vector2(posX, posY), 0);
        List<PathNode> visitedNodes = new List<PathNode>();
        //Debug.Log("start: " + startNode.node);

        bool found = false;
        int finalNodeIndex = 0;
        //trying 5 different starting points!!!!!!!!!!!!!!!
        for (int start = 0; start < 5; start++)
        {
            //Debug.Log("start:"+start);
            visitedNodes.Clear();
            switch (start)
            {
                case 0://center
                    startNode = new PathNode(new Vector2(posX, posY), 0);
                    break;
                case 1://left
                    startNode = new PathNode(new Vector2(posX - step / 2, posY), 0);
                    break;
                case 2://up
                    startNode = new PathNode(new Vector2(posX, posY + step / 2), 0);
                    break;
                case 3://right
                    startNode = new PathNode(new Vector2(posX + step / 2, posY), 0);
                    break;
                case 4://down
                    startNode = new PathNode(new Vector2(posX, posY - step / 2), 0);
                    break;
                default:
                    break;
            }
            //Debug.Log("startNode:"+ startNode);

            visitedNodes.Add(startNode);

            for (int i = 0; i < visitedNodes.Count; i++)
            {
                //Debug.Log(i);
                PathNode currentNode = visitedNodes[i];
                //end process when current node is close to target
                if (GetDistance(currentNode.node, target) < step)
                {
                    //Debug.Log("final = " + currentNode.node);
                    finalNodeIndex = i;
                    found = true;
                    break;
                }
                if (i > 5000)
                {
                    Debug.Log("FAIL");
                    finalNodeIndex = 100;
                    break;
                }
                

                //set neighbouring nodes
                PathNode nodeLeft = new PathNode(new Vector2(currentNode.node.x - step, currentNode.node.y), i);
                PathNode nodeUp = new PathNode(new Vector2(currentNode.node.x, currentNode.node.y + step), i);
                PathNode nodeRight = new PathNode(new Vector2(currentNode.node.x + step, currentNode.node.y), i);
                PathNode nodeDown = new PathNode(new Vector2(currentNode.node.x, currentNode.node.y - step), i);
                //add to list if there is no collision and they are not already in list
                if (!CharacterCollidesBarrier(nodeLeft.node) && !visitedNodes.Contains(nodeLeft))
                { visitedNodes.Add(nodeLeft); }
                if (!CharacterCollidesBarrier(nodeUp.node) && !visitedNodes.Contains(nodeUp))
                { visitedNodes.Add(nodeUp); }
                if (!CharacterCollidesBarrier(nodeRight.node) && !visitedNodes.Contains(nodeRight))
                { visitedNodes.Add(nodeRight); }
                if (!CharacterCollidesBarrier(nodeDown.node) && !visitedNodes.Contains(nodeDown))
                { visitedNodes.Add(nodeDown); }

                

            }

            if (found)
                break;
        }

        //reversly find the way back to starting node
        int index = finalNodeIndex;
        while (index != 0)
        {
            path.Add(visitedNodes[index].node);
            index = visitedNodes[index].parentIndex;
            //Debug.Log(visitedNodes[index].node);

        }
        //reverse path
        path.Reverse();




        return path;
    }

    /// <summary>
    /// gives commands to unit in order to get to the given coordinates
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public bool MoveTo(float x, float y)
    {
        //Debug.Log("destination:" + x + "," + y);
        //Debug.Log(GetMyPosition());
        if (ValueEquals(posX, x) && ValueEquals(posY, y))
        {
            Stand();
            //Debug.Log("you there");

            return true;
        }

        //refresh path only when target moves
        if (!ValueEquals(currentTargetDestination.x, x) || !ValueEquals(currentTargetDestination.y, y))
        {
            //Debug.Log("oldTarget:" + currentTargetDestination);
            //Debug.Log("newTarget:" + x + "," +y);
            //Debug.Log("recalculating");
            walkingFront = GetPathTo(new Vector2(x, y));
            currentTargetDestination = new Vector2(x, y);
        }

        if (Time.frameCount % 30 == 0)
        {
            //Debug.Log("walkFront:" + walkingFront.Count);
            foreach (Vector2 v in walkingFront)
            {
                //Debug.Log(v);
            }
        }
        if (walkingFront.Count == 0)
        {
            if (Time.frameCount % 30 < 15)
            {
                PreferX(x, y);
            }
            else
            {
                PreferY(x, y);
            }
        }
        else
        {
            //draw path
            for (int i = 0; i < walkingFront.Count; i++)
            {
                if (i + 1 != walkingFront.Count)
                    Debug.DrawLine(walkingFront[i], walkingFront[i + 1], Color.blue);
            }

            Vector2 currentNode = walkingFront[0];
            if (ValueEquals(gameObject.transform.position.y, currentNode.y) && ValueEquals(gameObject.transform.position.x, currentNode.x))
            {
                walkingFront.RemoveAt(0);
            }
            else
            {
                if (Time.frameCount % 30 < 15)
                {
                    PreferX(currentNode.x, currentNode.y);
                }
                else
                {
                    PreferY(currentNode.x, currentNode.y);
                }
            }
        }
        return false;
    }


    /// <summary>
    /// chekovat pouze jeden směr nestačí
    /// </summary>
    /// <param name="center"></param>
    /// <returns></returns>
    public bool CharacterCollides(Vector2 center)
    {
        Vector2 colliderOffset = GetComponent<Collider2D>().offset;
        float colliderWidth = GetComponent<Collider2D>().bounds.size.x;
        float distance = 0.1f;

        bool colRight = ObjectCollides(center + colliderOffset, colliderWidth, right, distance);
        //bool colLeft = ObjectCollides(center + colliderOffset, colliderWidth, left, distance);
        bool colUp = ObjectCollides(center + colliderOffset / 2, colliderWidth, up, distance);
        //bool colDown = ObjectCollides(center + colliderOffset, colliderWidth, down, distance);

        //return colRight || colLeft || colUp || colDown;
        //return colRight || colLeft;
        //return colRight;
        return colUp;
    }


    /// <summary>
    /// new collisioncheck using box collider bounds
    /// </summary>
    /// <returns></returns>
    public bool CharacterCollidesBarrier(Vector2 center)
    {
        //Debug.Log("colCheck");
        float width = characterColliderWidth/2;
        float height = characterColliderHeight/2;
        Vector2 colliderOffset = GetComponent<Collider2D>().offset / 2;
        float offset = 0.1f;

        Vector2 botLeft = new Vector2(center.x - width - offset, center.y - height - offset);
        Vector2 botRight = new Vector2(center.x + width + offset, center.y - height - offset);
        Vector2 topLeft = new Vector2(center.x - width - offset, center.y + height + offset);
        Vector2 topRight = new Vector2(center.x + width + offset, center.y + height + offset);



        for (int i = 0; i < barriers.Length; i++)
        {
            if (barriers[i].GetComponent<BoxCollider2D>().bounds.Contains(botLeft + colliderOffset))
                return true;
            if (barriers[i].GetComponent<BoxCollider2D>().bounds.Contains(botRight + colliderOffset))
                return true;
            if (barriers[i].GetComponent<BoxCollider2D>().bounds.Contains(topLeft + colliderOffset))
                return true;
            if (barriers[i].GetComponent<BoxCollider2D>().bounds.Contains(topRight + colliderOffset))
                return true;
        }

        //Debug.DrawLine(botLeft, botRight, Color.green, 2f);
        //Debug.DrawLine(botLeft, topLeft, Color.green, 2f);
        //Debug.DrawLine(topLeft, topRight, Color.green, 2f);
        //Debug.DrawLine(topRight, botRight, Color.green, 2f);

        return false;
    }


    /// <summary>
    /// bot, top, left, right might be obsolete
    /// </summary>
    /// <returns></returns>
    public bool ObjectCollides(Vector2 center, float width, Vector2 direction, float distance)
    {
        //Debug.Log("width:" + width);
        LayerMask layerMask = barrierMask;
        charBotLeft = new Vector2(center.x - width / 2, center.y - width / 2);
        charBotRight = new Vector2(center.x + width / 2, center.y - width / 2);
        charTopLeft = new Vector2(center.x - width / 2, center.y + width / 2);
        charTopRight = new Vector2(center.x + width / 2, center.y + width / 2);


        //charBot = new Vector2(center.x - width / 2, center.y - width / 2);
        //charTop = new Vector2(center.x + width / 2, center.y - width / 2);
        //charLeft = new Vector2(center.x - width / 2, center.y + width / 2);
        //charRight = new Vector2(center.x + width / 2, center.y + width / 2);


        /*
        for(int i = 0; i < barriers.Length; i++)
        {
            if (barriers[i].GetComponent<BoxCollider2D>().bounds.Contains(charBotLeft))
                return true;
            if (barriers[i].GetComponent<BoxCollider2D>().bounds.Contains(charBotRight))
                return true;
            if (barriers[i].GetComponent<BoxCollider2D>().bounds.Contains(charTopLeft))
                return true;
            if (barriers[i].GetComponent<BoxCollider2D>().bounds.Contains(charTopRight))
                return true;
        }
        */


        RaycastHit2D hitBotLeft;
        Ray rayBotLeft = new Ray(charBotLeft, direction);
        RaycastHit2D hitBotRight;
        Ray rayBotRight = new Ray(charBotRight, direction);
        RaycastHit2D hitTopLeft;
        Ray rayTopLeft = new Ray(charTopLeft, direction);
        RaycastHit2D hitTopRight;
        Ray rayTopRight = new Ray(charTopRight, direction);

        //RaycastHit2D hitTop;
        //Ray rayBot= new Ray(charBot, direction);
        //RaycastHit2D hitBot;
        //Ray rayTop = new Ray(charTop, direction);
        //RaycastHit2D hitLeft;
        //Ray rayLeft = new Ray(charLeft, direction);
        //RaycastHit2D hitRight;
        //Ray rayRight = new Ray(charRight, direction);


        hitBotLeft = Physics2D.Raycast(rayBotLeft.origin, direction, distance, layerMask);
        hitBotRight = Physics2D.Raycast(rayBotRight.origin, direction, distance, layerMask);
        hitTopLeft = Physics2D.Raycast(rayTopLeft.origin, direction, distance, layerMask);
        hitTopRight = Physics2D.Raycast(rayTopRight.origin, direction, distance, layerMask);

        //hitTop = Physics2D.Raycast(rayTop.origin, direction, distance, layerMask);
        //hitBot = Physics2D.Raycast(rayBot.origin, direction, distance, layerMask);        
        //hitLeft = Physics2D.Raycast(rayLeft.origin, direction, distance, layerMask);
        //hitRight = Physics2D.Raycast(rayRight.origin, direction, distance, layerMask);

        //Debug.DrawRay(currentTargetDestination, left, Color.red);
        //Debug.DrawRay(currentTargetDestination, up, Color.blue);

        //Debug.DrawRay(rayBotLeft.origin , direction, Color.blue);
        //Debug.DrawRay(rayBotRight.origin , direction, Color.red);


        if (hitBotLeft || hitBotRight || hitTopLeft || hitTopRight)
        {
            //Debug.DrawLine(charBotLeft, charBotRight, Color.red, 2f);
            //Debug.DrawLine(charBotLeft, charTopLeft, Color.red, 2f);
            //Debug.DrawLine(charBotRight, charTopRight, Color.red, 2f);
            //Debug.DrawLine(charTopLeft, charTopRight, Color.red, 2f);
            return true;
        }
        else
        {
            //Debug.DrawLine(charBotLeft, charBotRight, Color.green, 1f);
            //Debug.DrawLine(charBotLeft, charTopLeft, Color.green, 1f);
            //Debug.DrawLine(charBotRight, charTopRight, Color.green, 1f);
            //Debug.DrawLine(charTopLeft, charTopRight, Color.green, 1f);
        }

        return false;
    }

    public bool Collides(Vector2 direction, float distance)
    {
        LayerMask layerMask = barrierMask;

        charBotLeft = new Vector2(collider.bounds.min.x, collider.bounds.min.y);
        charBotRight = new Vector2(collider.bounds.max.x, collider.bounds.min.y);
        charTopLeft = new Vector2(collider.bounds.min.x, collider.bounds.max.y);
        charTopRight = new Vector2(collider.bounds.max.x, collider.bounds.max.y);

        RaycastHit2D hitBotLeft;
        Ray rayBotLeft = new Ray(charBotLeft, direction);
        RaycastHit2D hitBotRight;
        Ray rayBotRight = new Ray(charBotRight, direction);
        RaycastHit2D hitTopLeft;
        Ray rayTopLeft = new Ray(charTopLeft, direction);
        RaycastHit2D hitTopRight;
        Ray rayTopRight = new Ray(charTopRight, direction);


        hitBotLeft = Physics2D.Raycast(rayBotLeft.origin, direction, distance, layerMask);
        hitBotRight = Physics2D.Raycast(rayBotRight.origin, direction, distance, layerMask);
        hitTopLeft = Physics2D.Raycast(rayTopLeft.origin, direction, distance, layerMask);
        hitTopRight = Physics2D.Raycast(rayTopRight.origin, direction, distance, layerMask);

        //Debug.DrawRay(currentTargetDestination, left, Color.red);
        //Debug.DrawRay(currentTargetDestination, up, Color.blue);

        //Debug.DrawRay(botLeft, direction, Color.cyan);
        //Debug.DrawRay(botRight, direction, Color.green);
        //Debug.DrawRay(topLeft, direction, Color.yellow);
        //Debug.DrawRay(topRight, direction, Color.red);

        //if(hitTopRight.rigidbody != null)
        //Debug.Log(hitTopRight.rigidbody.transform.name);

        if (hitBotLeft || hitBotRight || hitTopLeft || hitTopRight)
            return true;

        return false;
    }

    public bool Collides(Vector2 direction)
    {
        return Collides(direction, 0.1f);
    }

    public bool Collides(float distance)
    {
        return Collides(left, distance) || Collides(up, distance) || Collides(right, distance) || Collides(down, distance);
    }

    //old
    public bool Collides(Vector2 center, float extens, float width, LayerMask layermask, Vector2 direction)
    {
        charBotLeft = new Vector2(center.x - extens + width, center.y - extens + width);
        charBotRight = new Vector2(center.x + extens - width, center.y - extens + width);
        charTopLeft = new Vector2(center.x - extens + width, center.y + extens - width);
        charTopRight = new Vector2(center.x + extens - width, center.y + extens - width);

        RaycastHit2D hitBotLeft;
        Ray rayBotLeft = new Ray(charBotLeft, direction);
        RaycastHit2D hitBotRight;
        Ray rayBotRight = new Ray(charBotRight, direction);
        RaycastHit2D hitTopLeft;
        Ray rayTopLeft = new Ray(charTopLeft, direction);
        RaycastHit2D hitTopRight;
        Ray rayTopRight = new Ray(charTopRight, direction);


        hitBotLeft = Physics2D.Raycast(rayBotLeft.origin, direction, width, layermask);
        hitBotRight = Physics2D.Raycast(rayBotRight.origin, direction, width, layermask);
        hitTopLeft = Physics2D.Raycast(rayTopLeft.origin, direction, width, layermask);
        hitTopRight = Physics2D.Raycast(rayTopRight.origin, direction, width, layermask);

        //Debug.DrawRay(currentTargetDestination, left, Color.red);
        //Debug.DrawRay(currentTargetDestination, up, Color.blue);

        // Debug.DrawRay(botLeft, direction, Color.cyan);
        //Debug.DrawRay(botRight, direction, Color.green);
        //Debug.DrawRay(topLeft, direction, Color.yellow);
        //Debug.DrawRay(topRight, direction, Color.red);

        if (hitBotLeft || hitBotRight || hitTopLeft || hitTopRight)
            return true;

        return false;
    }
    //old
    public bool Collides(Vector2 center, float extens, float width, LayerMask layermask)
    {
        charBotLeft = new Vector2(center.x - extens + width, center.y - extens + width);
        charBotRight = new Vector2(center.x + extens - width, center.y - extens + width);
        charTopLeft = new Vector2(center.x - extens + width, center.y + extens - width);
        charTopRight = new Vector2(center.x + extens - width, center.y + extens - width);

        RaycastHit2D hitBotLeft;
        Ray rayBotLeft = new Ray(charBotLeft, direction);
        RaycastHit2D hitBotRight;
        Ray rayBotRight = new Ray(charBotRight, direction);
        RaycastHit2D hitTopLeft;
        Ray rayTopLeft = new Ray(charTopLeft, direction);
        RaycastHit2D hitTopRight;
        Ray rayTopRight = new Ray(charTopRight, direction);

        Vector2 allDirection = new Vector2(0, 0);

        allDirection = left;

        hitBotLeft = Physics2D.Raycast(rayBotLeft.origin, allDirection, width, layermask);
        hitBotRight = Physics2D.Raycast(rayBotRight.origin, allDirection, width, layermask);
        hitTopLeft = Physics2D.Raycast(rayTopLeft.origin, allDirection, width, layermask);
        hitTopRight = Physics2D.Raycast(rayTopRight.origin, allDirection, width, layermask);

        if (hitBotLeft || hitBotRight || hitTopLeft || hitTopRight)
            return true;

        allDirection = up;

        hitBotLeft = Physics2D.Raycast(rayBotLeft.origin, allDirection, width, layermask);
        hitBotRight = Physics2D.Raycast(rayBotRight.origin, allDirection, width, layermask);
        hitTopLeft = Physics2D.Raycast(rayTopLeft.origin, allDirection, width, layermask);
        hitTopRight = Physics2D.Raycast(rayTopRight.origin, allDirection, width, layermask);

        if (hitBotLeft || hitBotRight || hitTopLeft || hitTopRight)
            return true;

        allDirection = right;

        hitBotLeft = Physics2D.Raycast(rayBotLeft.origin, allDirection, width, layermask);
        hitBotRight = Physics2D.Raycast(rayBotRight.origin, allDirection, width, layermask);
        hitTopLeft = Physics2D.Raycast(rayTopLeft.origin, allDirection, width, layermask);
        hitTopRight = Physics2D.Raycast(rayTopRight.origin, allDirection, width, layermask);

        if (hitBotLeft || hitBotRight || hitTopLeft || hitTopRight)
            return true;

        allDirection = down;

        hitBotLeft = Physics2D.Raycast(rayBotLeft.origin, allDirection, width, layermask);
        hitBotRight = Physics2D.Raycast(rayBotRight.origin, allDirection, width, layermask);
        hitTopLeft = Physics2D.Raycast(rayTopLeft.origin, allDirection, width, layermask);
        hitTopRight = Physics2D.Raycast(rayTopRight.origin, allDirection, width, layermask);

        if (hitBotLeft || hitBotRight || hitTopLeft || hitTopRight)
            return true;

        return false;
    }
    //old
    public bool Collides(Vector2 center, Vector2 direction)
    {
        return Collides(center, 0.5f, 0.3f, barrierMask, direction);
    }



    /// <summary>
    /// bacha, dělá to skoky po 1, přitom charWidth = 0.5..uvidí se, jak to pofachá
    /// </summary>
    /// <param name="node"></param>
    /// <param name="visited"></param>
    /// <returns></returns>
    public List<Vector2> GetPossibleDirections(Vector2 node, List<Vector2> visited)
    {
        List<Vector2> possibleDirections = new List<Vector2>();

        if (!CharacterCollides(node + left) && !visited.Contains(node + left))
        {
            Debug.Log("L");
            possibleDirections.Add(node + left);
        }
        if (!CharacterCollides(node + up) && !visited.Contains(node + up))
        {
            Debug.Log("U");
            possibleDirections.Add(node + up);
        }
        if (!CharacterCollides(node + right) && !visited.Contains(node + right))
        {
            Debug.Log("R");
            possibleDirections.Add(node + right);
        }
        if (!CharacterCollides(node + down) && !visited.Contains(node + down))
        {
            Debug.Log("D");
            possibleDirections.Add(node + down);
        }
        //Debug.Log("found directions:");
        //Debug.Log(PrintNodeList(possibleDirections));

        return possibleDirections;
    }

    public string PrintNodeList(List<Vector2> list)
    {
        string result = "";
        foreach (Vector2 node in list)
        {
            result += "[" + node.x + "," + node.y + "],";
        }
        return result;
    }

    private Vector2 lastDirection;

    public bool FindPath()
    {
        //Vector2 currentPosition = transform.position;

        if (Collides(up))
        {
            //Debug.Log("col-U");
            if (!Collides(right) && lastDirection != left)
            {
                MoveRight();
                lastDirection = right;
            }
            else if (!Collides(left) && lastDirection != right)
            {
                MoveLeft();
                lastDirection = left;
            }

            else if (lastDirection != up)
            {
                MoveDown();
                lastDirection = down;
            }
            //Debug.Log(lastDirection);
            return false;
        }
        if (Collides(right))
        {
            //Debug.Log("col-R");
            if (!Collides(up) && lastDirection != down)
            {
                MoveUp();
                lastDirection = up;
                //Debug.Log("goUp");
            }
            else if (!Collides(down) && lastDirection != up)
            {
                MoveDown();
                lastDirection = down;
                //Debug.Log("goDown");

            }

            else if (lastDirection != right)
            {
                MoveLeft();
                lastDirection = left;
                //Debug.Log("goLeft");
            }
            //Debug.Log(lastDirection);
            return false;
        }
        if (Collides(down))
        {
            //Debug.Log("col-D");

            if (!Collides(left) && lastDirection != right)
            {
                MoveLeft();
                lastDirection = left;
            }
            else if (!Collides(right) && lastDirection != left)
            {
                MoveRight();
                lastDirection = right;
            }

            else if (lastDirection != down)
            {
                MoveUp();
                lastDirection = up;
            }
            //Debug.Log(lastDirection);
            return false;
        }
        if (Collides(left))
        {
            //Debug.Log("col-L");
            if (!Collides(up) && lastDirection != down)
            {
                MoveUp();
                lastDirection = up;
            }
            else if (!Collides(down) && lastDirection != up)
            {
                MoveDown();
                lastDirection = down;
            }

            else if (lastDirection != left)
            {
                MoveRight();
                lastDirection = right;
            }
            //Debug.Log(lastDirection);
            return false;
        }

        //Debug.Log("---");
        return true;
    }

    //s tímto návrhem pohybu k ničemu
    /*
        for (int i=0;i<visited.Count;i++)
        {
            Vector2 node = visited[i];
            Debug.Log("visited:");
            Debug.Log(PrintNodeList(visited));
            if (GetDistance(node, currentTargetDestination) < characterWidth)
            {
                return true;
            }
            possibleDirections = GetPossibleDirections(node, visited);
            Debug.Log("possible:");
            foreach(Vector2 newNode in possibleDirections)
            {
                visited.Add(newNode);
            }
            possibleDirections.Clear();

        }
        */



    //detect only collision with barriers (assigned manually to prefab)
    public LayerMask barrierMask;
    public bool decided = false;

    /// <summary>
    /// returns false while there is still some collision with barrier
    /// </summary>
    /// <returns>state of avoiding barrier</returns>
    public bool AvoidBariers()
    {
        if (Collides(0.1f))
        {
            //Debug.Log("COL");
            return FindPath();
        }
        else
        {
            //Debug.Log("avoided");
            return true;
        }
    }



    /// <summary>
    /// moves to [x,y] preferably on x axis
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void PreferX(double x, double y)
    {
        if (ValueEquals(gameObject.transform.position.y, y)) { }
        else if (ValueSmaller(gameObject.transform.position.y, y)) { MoveUp(); }
        else { MoveDown(); }

        if (ValueEquals(gameObject.transform.position.x, x)) { }
        else if (ValueSmaller(gameObject.transform.position.x, x)) { MoveRight(); }
        else { MoveLeft(); }
    }
    /// <summary>
    /// moves  to [x,y] preferably on y axis
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void PreferY(double x, double y)
    {
        if (ValueEquals(gameObject.transform.position.x, x)) { }
        else if (ValueSmaller(gameObject.transform.position.x, x)) { MoveRight(); }
        else { MoveLeft(); }

        if (ValueEquals(gameObject.transform.position.y, y)) { }
        else if (ValueSmaller(gameObject.transform.position.y, y)) { MoveUp(); }
        else { MoveDown(); }
    }




    public void Move(Vector2 direction)
    {
        if (direction == left)
            MoveLeft();
        else if (direction == right)
            MoveRight();
        else if (direction == up)
            MoveUp();
        else if (direction == down)
            MoveDown();
    }
    public void MoveUp()
    {
        rb2d.velocity = Vector2.up * speed;
        direction = up;
        //Debug.Log("moving up");
        //Debug.Log("new dir:" + direction);
        UpdateAnimatorState(AnimatorStateEnum.walkUp);
    }
    public void MoveLeft()
    {
        rb2d.velocity = Vector2.left * speed;
        direction = left;
        UpdateAnimatorState(AnimatorStateEnum.walkLeft);
    }
    public void MoveDown()
    {
        rb2d.velocity = Vector2.down * speed;
        direction = down;
        UpdateAnimatorState(AnimatorStateEnum.walkDown);
    }
    public void MoveRight()
    {
        rb2d.velocity = Vector2.right * speed;
        direction = right;
        UpdateAnimatorState(AnimatorStateEnum.walkRight);
    }
    public void Stop()
    {
        rb2d.velocity = Vector2.zero;
        //UpdateAnimatorState(AnimatorStateEnum.stop);
    }

    private float e = 0.1f; //odchylka
    /// <summary>
    /// porovnání z důvodu odchylky způsobené pohybem (škubáním)
    /// </summary>
    /// <param name="value1"></param>
    /// <param name="value2"></param>
    /// <returns></returns>
    public bool ValueSmaller(double value1, double value2)
    {
        if (value1 - e < value2 || value1 + e < value2)
            return true;
        else
            return false;
    }

    public bool ValueEquals(double value1, double value2)
    {
        return ValueEquals(value1, value2, e);
    }

    public bool ValueEquals(double value1, double value2, float e)
    {
        if (value2 - e <= value1 && value1 <= value2 + e)
            return true;
        else
            return false;
    }
}
