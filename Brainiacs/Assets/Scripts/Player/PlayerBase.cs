﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerBase : MonoBehaviour
{
    //public CharacterEnum character;

    public int playerNumber { get; set; }

    public bool isClone = false;
    public bool isAi = false;

    // JP - farba spritu
    //   public string color { get; set; }

    public float posX;
    public float posY;

  //  public string playerName { get; set; }

    public float speed { get; set; }

    //rozměry mapy
    public float mapMinX;
    public float mapMinY;
    public float mapMaxX;
    public float mapMaxY;
    public float mapWidth;
    public float mapHeight;

    public float characterWidth = 1f;

    public PositionGenerator posGenerator;


    //////////////////////STATS/////////////////////////
    public int lifes;
    public int score;
    public GameInfo gameInfo;
    public GameManager gameManager;
    
    //////////////////////AUDIO SOURCE/////////////////////////
    public AudioClip playerEliminatedSound;
    public AudioClip playerHitSound;
    public AudioClip playerHitShieldSound;

    AudioClip powerupShieldSound;
    AudioClip powerupHealSound;
    AudioClip powerupAmmoSound;
    AudioClip powerupSpeedSound;
    AudioClip powerupSlowSpeedSound;
    AudioClip powerupDealDamageSound;

    //////////////////////ANIMATOR VARIABLES/////////////////////////
    public Animator characterAnimator;
    public bool walkUp = false;
    public bool walkRight = false;
    public bool walkDown = true;
    public bool walkLeft = false;
    public bool dead = false;

    // //////////////////////////////////////// HP /////////////////////////////////////////////
    //<<MG.. added
    private static int maxHP = 100;
    public int hitPoints = maxHP;
    private bool isShielded = false;

    public int GetMaxHp()
    {
        return maxHP;
    }
    
    // ///////////////////////////////////// POWER UPS ///////////////////////////////////////////
    private bool speedBuffIsActive = false;
    private bool slowDebuffIsActive = false;
    private float time = 0.0f;                  //for speed/slow
    private float speedAmount;                  //to remember if it is boost of slow
    private float speedBuffDuration = 10.0f;
    //..MG>>
    
    // /////////////////////////////////////// Movement ///////////////////////////////////////////
    public Vector2 direction;
    public Vector2 up = Vector2.up;
    public Vector2 down = Vector2.down;
    public Vector2 left = Vector2.left;
    public Vector2 right = Vector2.right;
    public Vector2 stop = Vector2.zero;
    // ///////////////////////////////////////////////////////////////////////////////////////////

    /// //////////////////////////////////// WEAPON HANDLING ///////////////////////////////////
    public WeaponHandling weaponHandling;
    public List<WeaponBase> inventory { get; set; }
    public WeaponBase activeWeapon { get; set; }
    public Dictionary<Vector2, int> directionMapping = new Dictionary<Vector2, int>();

    /////////////////////////////////////////////HANDS//////////////////////////////////////
    public SpriteRenderer handsRenderer;
    public Sprite[] handsSprites;
    /// //////////////////////////////////////// END ///////////////////////////////////////////


    // //////////////////////////////////////  Components ///////////////////////////////////////
    //rigid body of controlled object
    public Rigidbody2D rb2d { get; set; }
    public Components comp;
    /////////////////////////////////////////////////////////////////////////////////////////////
    public PlayerInfo playInfo;

    public void setUpPB(Components c, PlayerInfo p)
    {
        lifes = p.lifes;
        score = 0;

        comp = c;
        playInfo = p;
        weaponHandling = GetComponent<WeaponHandling>();
        
        setUpSprites();

        setUpSounds();

        weaponHandling.buletManager = transform.parent.GetComponent<BulletManager>();
        weaponHandling.buletManager.createBullets(this);

        characterAnimator = GetComponent<Animator>();

        //pičovina, pak to napojím na PLayerInfo a atribut playerNumber uplně smažu
        playerNumber = p.playerNumber;

        mapMinX = -4.75f;
        mapMinY = -4.75f;
        mapMaxX = 8.6f;
        mapMaxY = 4f;
        mapHeight = Math.Abs(mapMaxY - mapMinY);
        mapWidth = Math.Abs(mapMaxX - mapMinX);

        directionMapping.Add(up, 3);
        directionMapping.Add(down, 0);
        directionMapping.Add(left, 1);
        directionMapping.Add(right, 2);

        handsRenderer = transform.Find("hands_down").GetComponent<SpriteRenderer>(); //hands_down, protože sem zrovna na prefab přetáhl hands_down a nejde to změnit..normálně
        handsSprites = Resources.LoadAll<Sprite>("Sprites/Hands");

        weaponHandling.weaponRenderer = transform.Find("weapon").GetComponent<SpriteRenderer>();

        //setup position generator
        posGenerator = GameObject.Find("PositionGenerator").GetComponent<PositionGenerator>();

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    public void setUpSounds()
    {
        //string soundLoaderString = "Sounds/Player/" + playInfo.charEnum.ToString().ToLower() + "/";
        string soundLoaderString = "Sounds/Player/"; //for now
        
        playerEliminatedSound = 
            Resources.Load(soundLoaderString + "player_eliminated") as AudioClip;
        playerHitSound =
            Resources.Load(soundLoaderString + "player_hit") as AudioClip;
        playerHitShieldSound =
            Resources.Load(soundLoaderString + "player_shield_hit") as AudioClip;

        powerupShieldSound = Resources.Load<AudioClip>("Sounds/Items/powerup_shield");
        powerupHealSound = Resources.Load<AudioClip>("Sounds/Items/powerup_heal");
        powerupAmmoSound = Resources.Load<AudioClip>("Sounds/Items/powerup_ammo");
        powerupSpeedSound = Resources.Load<AudioClip>("Sounds/Items/powerup_speed");
        powerupSlowSpeedSound= Resources.Load<AudioClip>("Sounds/Items/powerup_slowSpeed");
        powerupDealDamageSound = Resources.Load<AudioClip>("Sounds/Items/powerup_dealDamage");
    }

    public void setUpWeapons(PlayerInfo pi)
    {

        weaponHandling.player = GetComponent<PlayerBase>();
        weaponHandling.specialWeapon = GetComponent<WeaponSpecial>();
        
        WeaponBase pistol = new WeaponPistol(pi.charEnum);
        WeaponBase sniper = new WeaponSniper();
        WeaponBase biogun = new WeaponBiogun();
        WeaponBase MP40 = new WeaponMP40();
        WeaponBase mine = new WeaponMine();
        WeaponBase flame = new WeaponFlamethrower();
        Transform childFlame = transform.Find("Flame");
        weaponHandling.flamethrower = childFlame.GetComponent<WeaponFlamethrowerLogic>();

        //WeaponBase specialCurie = new WeaponSpecialCurieLogic();
        // Tu sa vytvoria vsetky zbrane ktore sa priradia do weapon handling aby sa nemusel volat zbytocne load na sprajtoch
        //weaponHandling.weapons.Add(WeaponEnum.specialCurie, special);
        WeaponBase special = null;

        switch (pi.charEnum)
        {
            case CharacterEnum.Tesla:
                special = new WeaponTeslaSpecial();
                weaponHandling.specialWeapon.SetUp(pi, transform.parent.GetComponent<BulletManager>(), this, special);
                weaponHandling.weapons.Add(WeaponEnum.specialTesla, special);
                break;
            case CharacterEnum.Curie:
                special = new WeaponCurieSpecial();
                weaponHandling.specialWeapon.SetUp(pi, transform.parent.GetComponent<BulletManager>(), this, special);
                weaponHandling.weapons.Add(WeaponEnum.specialCurie, special);
                break;
            case CharacterEnum.DaVinci:
                special = new WeaponDaVinciSpecial();
                //special = new WeaponTeslaSpecial();
                weaponHandling.specialWeapon.SetUp(pi, transform.parent.GetComponent<BulletManager>(), this, special);
                weaponHandling.weapons.Add(WeaponEnum.specialDaVinci, special);
                //weaponHandling.specialWeapon.SetUp(pi, transform.parent.GetComponent<BulletManager>(), this, special);
                //weaponHandling.weapons.Add(WeaponEnum.specialTesla, special);
                break;
            case CharacterEnum.Einstein:
                special = new WeaponEinsteinSpecial();
                weaponHandling.specialWeapon.SetUp(pi, transform.parent.GetComponent<BulletManager>(), this, special);
                weaponHandling.weapons.Add(WeaponEnum.specialEinstein, special);
                break;
            case CharacterEnum.Nobel:
                special = new WeaponNobelSpecial();
                weaponHandling.specialWeapon.SetUp(pi, transform.parent.GetComponent<BulletManager>(), this, special);
                weaponHandling.weapons.Add(WeaponEnum.specialNobel, special);
                break;
        }

        weaponHandling.weapons.Add(WeaponEnum.sniper, sniper);
        weaponHandling.weapons.Add(WeaponEnum.pistol, pistol);
        weaponHandling.weapons.Add(WeaponEnum.biogun, biogun);
        weaponHandling.weapons.Add(WeaponEnum.MP40, MP40);
        weaponHandling.weapons.Add(WeaponEnum.mine, mine);
        weaponHandling.weapons.Add(WeaponEnum.flamethrower, flame);


        // Inicializacia prvej zbrane
        weaponHandling.inventory.Add(pistol);
        weaponHandling.inventory.Add(special);
        //weaponHandling.inventory.Add(flame);
        //weaponHandling.inventory.Add(sniper);
        //weaponHandling.inventory.Add(biogun);
        //weaponHandling.inventory.Add(MP40);
        //weaponHandling.inventory.Add(mine);
        
        weaponHandling.activeWeapon = weaponHandling.inventory[0];
    }

    private void setUpSprites() {
        string animControllerString = "";
        string character = playInfo.charEnum.ToString().ToLower();
        
        animControllerString +=  character + "_";
        animControllerString += playInfo.playerColor + "_";
        animControllerString += "override";
        animControllerString = "Animations/Characters/" + character + "/" + animControllerString;
        
        RuntimeAnimatorController rtac = Resources.Load(animControllerString) as AnimatorOverrideController;
        comp.animator.runtimeAnimatorController = rtac;
        
    }

    //TODO: transform.GetChild(1).GetComponent<SpriteRenderer>().sortingLayerName = "Hand_back";
    //až přídáme ruku
    /// <summary>
    /// změní pořádí vykreslování částí těla
    /// left,down,right: player-weapon-hand "weapon_front" + "hand_front"
    /// up: hand-weapon-player "weapon_back" + "hand_back"
    /// </summary>
    public void SortLayers()
    {
        if (direction == up)
        {
            weaponHandling.weaponRenderer.sortingLayerName = "Weapon_back";
            handsRenderer.sortingLayerName = "Hand_back";

            transform.GetChild(0).GetComponent<SpriteRenderer>().sortingLayerName = "Weapon_back";
        }
        else
        {
            weaponHandling.weaponRenderer.sortingLayerName = "Weapon_front";
            handsRenderer.sortingLayerName = "Hand_front";

            transform.GetChild(0).GetComponent<SpriteRenderer>().sortingLayerName = "Weapon_front";

        }
    }

    void FixedUpdate()
    {
        SpeedBuffChecker();
        UpdatePosition();
        GetComponent<SpriteRenderer>().color = Color.white;
    }
    
    public void RefreshAnimatorState()
    {
        if(direction == up)
        {
            UpdateAnimatorState(AnimatorStateEnum.walkUp);
        }
        else if (direction == right)
        {
            UpdateAnimatorState(AnimatorStateEnum.walkRight);
        }
        else if (direction == down)
        {
            UpdateAnimatorState(AnimatorStateEnum.walkDown);
        }
        else if (direction == left)
        {
            UpdateAnimatorState(AnimatorStateEnum.walkLeft);
        }
    }

    public void UpdateAnimatorState(AnimatorStateEnum state)
    {
        //Debug.Log(playerNumber + "to : " + state);
        switch (state)
        {
            case AnimatorStateEnum.walkUp:
                walkUp = true;
                walkRight = false;
                walkDown = false;
                walkLeft= false;
                break;
            case AnimatorStateEnum.walkRight:
                walkUp = false;
                walkRight = true;
                walkDown = false;
                walkLeft = false;
                break;
            case AnimatorStateEnum.walkDown:
                walkUp = false;
                walkRight = false;
                walkDown = true;
                walkLeft = false;
                break;
            case AnimatorStateEnum.walkLeft:
                walkUp = false;
                walkRight = false;
                walkDown = false;
                walkLeft = true;
                break;
            case AnimatorStateEnum.stop:
                walkUp = false;
                walkRight = false;
                walkDown = false;
                walkLeft = false;
                break;
            case AnimatorStateEnum.dead:
                walkUp = false;
                walkRight = false;
                walkDown = false;
                walkLeft = false;
                dead = true;
                break;
        }
        characterAnimator.SetBool("walkUp", walkUp);
        characterAnimator.SetBool("walkDown", walkDown);
        characterAnimator.SetBool("walkRight", walkRight);
        characterAnimator.SetBool("walkLeft", walkLeft);
        characterAnimator.SetBool("dead", dead);

        if (dead)
        {

        }
    }
    
    public void RandomizeDirection()
    {
        System.Random rnd = new System.Random();
        int randomInt = rnd.Next(1, 4);
        //Debug.Log(playerNumber + "rndDir:" + randomInt);
        switch (randomInt)
        {
            case 1:
                direction = up;
                break;
            case 2:
                direction = right;
                break;
            case 3:
                direction = down;
                break;
            case 4:
                direction = left;
                break;
            default:
                direction = left;
                break;
        }
    }

    /// <summary>
    /// momenátlně pouze na ruce + sortění LAYERS
    /// buď někam přesunout, nebo oatřičně přejmenovat
    /// </summary>
    public void UpdateDirection()
    {

        if (direction == down)
        {
            handsRenderer.sprite = handsSprites[0];
        }
        else if (direction == left)
        {
            handsRenderer.sprite = handsSprites[1];
        }
        else if (direction == right)
        {
            handsRenderer.sprite = handsSprites[2];
        }
        else if (direction == up)
        {
            handsRenderer.sprite = handsSprites[3];
        }
        

        SortLayers();

    }

    public void UpdatePosition()
    {
        posX = gameObject.transform.position.x;
        posY = gameObject.transform.position.y - 0.15f;
    }

    public void UpdateGameInfoDeaths(int playerNumber, int value)
    {
        if (isClone)
        {
            Debug.Log("Clone died");
            return;
        }

        switch (playerNumber)
        {
            case 1:
                gameInfo.player1deaths += value;
                break;
            case 2:
                gameInfo.player2deaths += value;
                break;
            case 3:
                gameInfo.player3deaths += value;
                break;
            case 4:
                gameInfo.player4deaths += value;
                break;
            default:
                Debug.Log("playerNumber not valid");
                break;
        }
    }

    ScoreboardManager scoreboard;

    public void IncreaseGameInfoScore(int playerNumber, int value)
    {
        if (isClone)
        {
            return;
        }
        if(playerNumber == this.playerNumber)
        {
            return;
        }

        if (scoreboard == null)
        {
            scoreboard = GameObject.Find("Scoreboard").GetComponent<ScoreboardManager>();
        }

        switch (playerNumber)
        {
            case 1:
                gameInfo.player1score += value;
                break;
            case 2:
                gameInfo.player2score += value;
                break;
            case 3:
                gameInfo.player3score += value;
                break;
            case 4:
                gameInfo.player4score += value;
                break;
            default:
                Debug.Log("playerNumber not valid");
                break;
        }

        if(scoreboard != null)
        {
            scoreboard.RefreshStats();
        }
    }

    //PowerUp and HP management - <<<MG...>>>

    //player receives damage
    public void ApplyDamage(int dmg, PlayerBase owner)
    {
        if (isShielded)
        {
            isShielded = false;
            SoundManager.instance.PlaySingle(playerHitShieldSound);
            //SHIELD
        }
        else
        {
            SoundManager.instance.PlaySingle(playerHitSound);

            if ((hitPoints - dmg) <= 0 && !dead) 
            {
                hitPoints = 0;
                comp.rb2d.velocity = stop;
                UpdateGameInfoDeaths(playerNumber,1);

                //add score to owner of bullet
                if (!isClone)
                {
                    owner.score++;
                }
                else
                {

                }

                IncreaseGameInfoScore(owner.playerNumber, 1);
                
                //check score
                if(!isClone && gameInfo.gameMode == GameModeEnum.Score && owner.score >= gameInfo.winScore)
                {
                    gameManager.EndGame();
                }

                if (!dead)
                {
                    StartCoroutine(Die());
                }

            }
            else
            {
                hitPoints -= dmg;
                GetComponent<SpriteRenderer>().color = Color.red;
            }
        }
    }

    IEnumerator Die()
    {
        dead = true;

        UpdateAnimatorState(AnimatorStateEnum.dead);
        //disable movement
        up = stop;
        right = stop;
        down= stop;
        left = stop;
        

        float deadTime = characterAnimator.GetCurrentAnimatorStateInfo(0).length;
        
        yield return new WaitForSeconds(deadTime);
        lifes--;

        transform.position = new Vector2(-666, 666);//move to hell

        if (!isClone && gameInfo.gameMode == GameModeEnum.Deathmatch && lifes <= 0)
        {
            weaponHandling.shootingEnabled = false;
            SoundManager.instance.PlaySingle(playerEliminatedSound);
            gameManager.CheckLifes();
        }
        else
        {
            dead = false;
            yield return new WaitForSeconds(1f);

            hitPoints = maxHP;
            Vector3 newRandomPosition = posGenerator.GenerateRandomPosition();
            UpdatePosition();
            transform.position = newRandomPosition;

            //enable movement
            up = Vector2.up;
            down = Vector2.down;
            left = Vector2.left;
            right = Vector2.right;

            //update weapon direction and stop player animation(AI reasons)
            RandomizeDirection();
            RefreshAnimatorState();
            UpdateDirection();
        }
    }

    /// ////////////////////////////////////// POWER UPS ///////////////////////////////////////////
    //player receives heal
    private void ApplyHeal(int heal)
    {
        if ((hitPoints + heal) > maxHP)
        {
            hitPoints = maxHP;
        }
        else
        {
            hitPoints += heal;
        }

    }

    //checks if speed/slow buff duration has expired
    private void SpeedBuffChecker()
    {
        if (speedBuffIsActive || slowDebuffIsActive)
        {
            time += Time.deltaTime;
            if (time > speedBuffDuration && speedBuffIsActive)
            {
                speedBuffIsActive = false;
                time = 0.0f;
                speed -= speedAmount;
            }
            if (time > speedBuffDuration && slowDebuffIsActive)
            {
                slowDebuffIsActive = false;
                time = 0.0f;
                speed -= speedAmount;
            }
        }
    }

    //receives speed buff
    private void ApplySpeed(float amount)
    {
        if (!speedBuffIsActive)
        {
            if (slowDebuffIsActive)
            {
                slowDebuffIsActive = false;
                speed -= speedAmount;
                return;
            }
            speedAmount = amount;
            speedBuffIsActive = true;
            speed += speedAmount;
        }
        else
        {
            time = 0.0f;
        }
    }

    private void ApplySlow(float amount)
    {
        if (!slowDebuffIsActive)
        {
            if (speedBuffIsActive)
            {
                speedBuffIsActive = false;
                speed -= speedAmount;
                return;
            }
            speedAmount = amount;
            slowDebuffIsActive = true;
            speed += speedAmount;
        }
        else
        {
            time = 0.0f;
        }
    }
    
    //this way player receives info from collision with power up
    public void AddPowerUp(PowerUpEnum en)
    {
        switch (en)
        {
            case PowerUpEnum.Shield:
                isShielded = true;
                SoundManager.instance.PlaySingle(powerupShieldSound);
                break;
            case PowerUpEnum.Heal:
                ApplyHeal(maxHP / 2);
                SoundManager.instance.PlaySingle(powerupHealSound);
                break;
            case PowerUpEnum.Ammo:
                weaponHandling.activeWeapon.reload();
                SoundManager.instance.PlaySingle(powerupAmmoSound);
                break;
            case PowerUpEnum.Speed:
                ApplySpeed(1.5f);
                SoundManager.instance.PlaySingle(powerupSpeedSound);
                break;
            case PowerUpEnum.Mystery:
                System.Random rnd = new System.Random();
                var v = Enum.GetValues(typeof(PowerUpEnum));
                AddPowerUp((PowerUpEnum)v.GetValue(rnd.Next(v.Length)));
                break;
            case PowerUpEnum.dealDamage:
                ApplyDamage(maxHP / 3, this); //hit yourself
                SoundManager.instance.PlaySingle(powerupDealDamageSound);
                break;
            case PowerUpEnum.slowSpeed:
                ApplySlow(-1.0f);
                SoundManager.instance.PlaySingle(powerupSlowSpeedSound);
                break;
            default:
                Debug.Log("ERROR: Unknown powerUp received.");
                break;
        }
    }
}

//proč mi to v Enums nefunguje?
public enum AnimatorStateEnum
{
    walkUp,
    walkDown,
    walkLeft,
    walkRight,
    stop,
    dead
}