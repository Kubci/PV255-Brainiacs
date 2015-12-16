﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HUDBase : MonoBehaviour {

    protected PlayerBase player;
    protected WeaponHandling weaponHandling;
    protected SpriteRenderer renderer;
    protected string sprite;
    private string color;

    protected GameObject avatar;

    private Text name;
    private Text hp;
    private Text ammo;

    public int health;
    public int ammoActiveWeapon;

    bool ready = false;

    public void SetUp(PlayerBase p) {
        player = p;
        weaponHandling = p.weaponHandling;
        color = p.playInfo.playerColor;

        sprite = p.playInfo.charEnum.ToString() + "_portail";
        sprite = sprite.ToLower();
        SetupAvatar(sprite);
        ready = true;

        name = GameObject.Find("name_"+color).GetComponent<Text>();
        hp = GameObject.Find("hp_" + color).GetComponent<Text>();
        ammo = GameObject.Find("ammo_" + color).GetComponent<Text>();

        name.text = p.playInfo.playerName;
    }

	void SetupAvatar (string sprite) {
        avatar = transform.Find("Avatar_" + color).gameObject;
        renderer = avatar.GetComponent<SpriteRenderer>();
        //Debug.Log(renderer.sprite);
	    renderer.sprite = Resources.Load<Sprite>("Sprites/HUD/" + sprite);
        //Debug.Log(renderer.sprite);
    }
	
	// Update is called once per frame
	void Update () {
	    if (ready)
	    {
	        if (weaponHandling.activeWeapon.ready)
	        {
	            ammo.color = Color.black;
	            ammo.text = weaponHandling.activeWeapon.ammo.ToString();
	        }
	        else
	        {
	            ammo.color = Color.red;
	            string temp = (weaponHandling.activeWeapon.reloadTime - weaponHandling.activeWeapon.time).ToString();
	            int l = temp.Length;
	            if (l <= 3)
	            {
	                ammo.text = temp;
	            }
	            else
	            {
	                ammo.text = temp.Substring(0, 3);
	            }
	        }
	        hp.text = player.hitPoints.ToString();
	    }
	}
}
