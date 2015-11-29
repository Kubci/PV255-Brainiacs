﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AiPriorityLogic
{

    public AiBase aiBase;

    public int killPlayer1Priority;
    public int killPlayer2Priority;
    public int killPlayer3Priority;
    public int killPlayer4Priority;

    //logics
    public AiPowerUpLogic aiPowerUpLogic;
    public AiWeaponLogic aiWeaponLogic;
    public AiMovementLogic aiMovementLogic;
    public AiAvoidBulletLogic aiAvoidBulletLogic;
    //public AiPriorityLogic aiPriorityLogic;

    public AiPriorityLogic(AiBase aiBase)
    {
        this.aiBase = aiBase;
    }

    public void PrintPriorities()
    {
        string message = "priorities \n";
        message += "pickWeaponPriority=" + aiBase.aiWeaponLogic.pickWeaponPriority + ",\n";
        message += "pickPowerUpPriority=" + aiBase.aiPowerUpLogic.pickPowerUpPriority + ",\n";

        message += "KillPlayer1=" + killPlayer1Priority;
        message += ",KillPlayer2=" + killPlayer2Priority;
        message += ",KillPlayer3=" + killPlayer3Priority;
        message += ",KillPlayer4=" + killPlayer4Priority;
        message += ",avoidBulletPriority=" + aiBase.aiAvoidBulletLogic.avoidBulletPriority;

        Debug.Log(message);
    }


    public void UpdatePriorities()
    {
        //set kill priorities only when you can shoot
        if (aiBase.CheckAmmo())
            SetKillPriorities();


        //register incoming bullets, powerups,...
        aiBase.LookAroundYourself();

        SetPowerUpsPriority();

        SetWeaponsPriority();

        if (aiBase.aiAvoidBulletLogic.bulletIncoming)
        {
            //Debug.Log("avoiding");
            aiBase.aiAvoidBulletLogic.SetAvoidBulletPriority(100);
        }

        //check bullets
        //CheckAmmo(); //has to be updated faster


        //PrintPriorities();

        //....
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
        if (aiBase.aiAvoidBulletLogic.avoidBulletPriority > highestPriority)
            highestPriority = aiBase.aiAvoidBulletLogic.avoidBulletPriority;
        if (aiBase.aiPowerUpLogic.pickPowerUpPriority > highestPriority)
            highestPriority = aiBase.aiPowerUpLogic.pickPowerUpPriority;

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
        if (aiBase.player1 != null && aiBase.playerNumber != 1)
            killPlayer1Priority = (int)(90 * aiBase.GetDistanceFactor(aiBase.GetDistance(aiBase.gameObject, aiBase.player1.gameObject)));
        if (aiBase.player2 != null && aiBase.playerNumber != 2)
            killPlayer2Priority = (int)(90 * aiBase.GetDistanceFactor(aiBase.GetDistance(aiBase.gameObject, aiBase.player2.gameObject)));
        if (aiBase.player3 != null && aiBase.playerNumber != 3)
            killPlayer3Priority = (int)(90 * aiBase.GetDistanceFactor(aiBase.GetDistance(aiBase.gameObject, aiBase.player3.gameObject)));
        if (aiBase.player4 != null && aiBase.playerNumber != 4)
            killPlayer4Priority = (int)(90 * aiBase.GetDistanceFactor(aiBase.GetDistance(aiBase.gameObject, aiBase.player4.gameObject)));


    }

    public void SetWeaponsPriority()
    {
        List<int> weaponsPriorities = new List<int>();

        if (aiWeaponLogic.itemWeapons.Count == 0)
        {
            //Debug.Log("no items around");
            aiWeaponLogic.pickWeaponPriority = 0;
            return;
        }

        int highestPriority = 0;
        foreach (GameObject weapon in aiWeaponLogic.itemWeapons)
        {
            //Debug.Log("I see " + powerUp.name);
            WeaponManager manager = weapon.GetComponent<WeaponManager>();
            float distanceFromMe = aiBase.GetDistance(aiBase.gameObject, weapon);
            float distanceFactor = aiBase.GetDistanceFactor(distanceFromMe);

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
                /*
                                case WeaponEnum.shotgun:
                                    float shotgunFactor = distanceFactor;// * weaponHandling.activeWeapon.ammo / weaponHandling.activeWeapon.clip;
                                    /*if (HasWeapon(WeaponEnum.flamethrower))
                                    {
                                        //flamethrowerFactor /= ...
                                    }


                                    priority = (int)shotgunFactor * 10;
                                    break;
                */
                default:
                    priority = 0;
                    break;
            }
            if (priority == 0)
                priority = 10;
            weaponsPriorities.Add(priority);

            //Debug.Log("setting: " + powerUpsPriorities[powerUpsPriorities.Count - 1]);

            if (weaponsPriorities[weaponsPriorities.Count - 1] > highestPriority)
            {
                highestPriority = weaponsPriorities[weaponsPriorities.Count - 1];
            }
        }

        aiWeaponLogic.bestWeapon = aiWeaponLogic.itemWeapons[weaponsPriorities.IndexOf(highestPriority)];

        aiWeaponLogic.pickWeaponPriority = highestPriority;
        //Debug.Log("pickPowerUpPriority:" + pickPowerUpPriority);


    }


    public void SetPowerUpsPriority()
    {
        List<int> powerUpsPriorities = new List<int>();

        if (aiPowerUpLogic.itemPowerUps.Count == 0)
        {
            //Debug.Log("no items around");
            aiPowerUpLogic.pickPowerUpPriority = 0;
            return;
        }

        int highestPriority = 0;
        foreach (GameObject powerUp in aiPowerUpLogic.itemPowerUps)
        {
            //Debug.Log("I see " + powerUp.name);
            PowerUpManager manager = powerUp.GetComponent<PowerUpManager>();
            if (manager == null)
            {
                Debug.Log(powerUp + " has no manager");
                return;
            }

            float distanceFromMe = aiBase.GetDistance(aiBase.gameObject, powerUp);
            float distanceFactor = aiBase.GetDistanceFactor(distanceFromMe);

            int priority = 0;
            switch (manager.type)
            {

                case PowerUpEnum.Ammo:
                    float ammoFactor = distanceFactor * aiBase.weaponHandling.activeWeapon.ammo / aiBase.weaponHandling.activeWeapon.clip;
                    priority = (int)ammoFactor * 10;
                    break;
                case PowerUpEnum.Heal:
                    float healthFactor = distanceFactor * aiBase.hitPoints / aiBase.GetMaxHp();
                    priority = (int)healthFactor * 10;
                    break;
                case PowerUpEnum.Mystery:
                    int mysteryFactor = (int)distanceFactor * Random.Range(0, 80);
                    mysteryFactor += (int)distanceFactor * 25;
                    priority = mysteryFactor;
                    break;
                case PowerUpEnum.Shield:
                    float shieldFactor = distanceFactor * (50 + Random.Range(0, 20));
                    priority = (int)shieldFactor;
                    break;
                case PowerUpEnum.Speed:
                    float speedFactor = distanceFactor * (50 + Random.Range(0, 20));
                    priority = (int)speedFactor;
                    break;
            }
            if (priority == 0)
                priority = 10;
            powerUpsPriorities.Add(priority);

            //Debug.Log("setting: " + powerUpsPriorities[powerUpsPriorities.Count - 1]);

            if (powerUpsPriorities[powerUpsPriorities.Count - 1] > highestPriority)
            {
                highestPriority = powerUpsPriorities[powerUpsPriorities.Count - 1];
            }
        }

        aiPowerUpLogic.bestPowerUp = aiPowerUpLogic.itemPowerUps[powerUpsPriorities.IndexOf(highestPriority)];

        aiPowerUpLogic.pickPowerUpPriority = highestPriority;
        //Debug.Log("pickPowerUpPriority:" + pickPowerUpPriority);


    }

}