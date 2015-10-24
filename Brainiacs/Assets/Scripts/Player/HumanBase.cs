﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HumanBase : PlayerBase {
    public KeyCode keyUp { get; set; }
    public KeyCode keyLeft { get; set; }
    public KeyCode keyDown { get; set; }
    public KeyCode keyRight { get; set; }
    public KeyCode keyFire { get; set; }
    public KeyCode keySwitchWeapon { get; set; }


    private KeyCode lastPressed { get; set; }

    public List<KeyCode> pressedKeys { get; set; }
    // JP - pre alternatibny movement
    public Stack<KeyCode> pressed_keys = new Stack<KeyCode>();
    int pops = 0;

    public void SetControlKeys(KeyCode keyUp, KeyCode keyLeft, KeyCode keyDown, KeyCode keyRight, KeyCode keyFire, KeyCode keySwitchWeapon)
    {
        this.keyUp = keyUp;
        this.keyLeft = keyLeft;
        this.keyDown = keyDown;
        this.keyRight = keyRight;
        this.keyFire = keyFire;
        this.keySwitchWeapon = keySwitchWeapon;
    }

    // <<<MOVEMENT...>>> //
    protected void Movement()
    {
        if (Input.GetKeyDown(keyFire)) {
            fire();
        }

        if (Input.GetKeyDown(keySwitchWeapon))
        {
            SwitchWeapon();
        }

        if (Input.GetKeyDown(keyUp) && !PressedKeysContains(keyUp))
        {
            pressedKeys.Add(keyUp);
        }
        else if (Input.GetKeyDown(keyLeft) && !PressedKeysContains(keyLeft))
        {
            pressedKeys.Add(keyLeft);
        }
        else if (Input.GetKeyDown(keyDown) && !PressedKeysContains(keyDown))
        {
            pressedKeys.Add(keyDown);
        }
        else if (Input.GetKeyDown(keyRight) && !PressedKeysContains(keyRight))
        {
            pressedKeys.Add(keyRight);
        }

        if (GetLastPressed() == keyUp)
        {
            rb2d.velocity = up * speed;
            direction = up;
        }
        else if (GetLastPressed() == keyLeft)
        {
            rb2d.velocity = left * speed;
            direction = left;
            gameObject.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Tesla_left");

        }
        else if (GetLastPressed() == keyDown)
        {
            rb2d.velocity = down * speed;
            direction = down;
        }
        else if (GetLastPressed() == keyRight)
        {
            rb2d.velocity = right * speed;
            direction = right;
            gameObject.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Tesla_right");

        }
        else
        {
            rb2d.velocity = stop;
        }

        if (Input.GetKeyUp(keyUp) || Input.GetKeyUp(keyLeft) ||
            Input.GetKeyUp(keyDown) || Input.GetKeyUp(keyRight))
        {
            rb2d.velocity = stop;
            if (Input.GetKeyUp(keyUp))
            {
                RemoveKeyPressed(keyUp);
            }
            if (Input.GetKeyUp(keyLeft))
            {
                RemoveKeyPressed(keyLeft);
            }
            if (Input.GetKeyUp(keyDown))
            {
                RemoveKeyPressed(keyDown);
            }
            if (Input.GetKeyUp(keyRight))
            {
                RemoveKeyPressed(keyRight);
            }
        }

        CheckPressedKeys();

        SortLayers();
    }

   

    private void movement2()
    {

        //JP

        if (Input.GetKeyDown(keyUp) && pressed_keys.Peek() != keyUp)
        {
            pressed_keys.Push(keyUp);
        }
        else if (Input.GetKeyDown(keyLeft) && pressed_keys.Peek() != keyLeft)
        {
            pressed_keys.Push(keyLeft);
        }
        else if (Input.GetKeyDown(keyDown) && pressed_keys.Peek() != keyDown)
        {
            pressed_keys.Push(keyDown);
        }
        else if (Input.GetKeyDown(keyRight) && pressed_keys.Peek() != keyRight)
        {
            pressed_keys.Push(keyRight);
        }

        if (Input.GetKeyUp(keyUp))
        {
            if (keyUp == pressed_keys.Peek())
            {
                pressed_keys.Pop();
                pop();
            }
            else { pops++; }
        }
        else if (Input.GetKeyUp(keyLeft))
        {
            if (keyLeft == pressed_keys.Peek())
            {
                pressed_keys.Pop();
                pop();
            }
            else { pops++; }
        }
        else if (Input.GetKeyUp(keyDown))
        {
            if (keyDown == pressed_keys.Peek())
            {
                pressed_keys.Pop();
                pop();
            }
            else { pops++; }
        }
        else if (Input.GetKeyUp(keyRight))
        {
            if (keyRight == pressed_keys.Peek())
            {
                pressed_keys.Pop();
                pop();
            }
            else { pops++; }
        }

        if (pressed_keys.Peek() == keyUp)
        {
            rb2d.velocity = up * speed;
            direction = up;
            //gameObject.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Tesla_left");

        }
        else if (pressed_keys.Peek() == keyLeft)
        {
            rb2d.velocity = left * speed;
            direction = left;
            gameObject.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Tesla_left");

        }
        else if (pressed_keys.Peek() == keyDown)
        {
            rb2d.velocity = down * speed;
            direction = down;
        }
        else if (pressed_keys.Peek() == keyRight)
        {
            rb2d.velocity = right * speed;
            direction = right;
            gameObject.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Tesla_right");

        }
        else
        {
            rb2d.velocity = stop;
        }


    }

    private void pop()
    {

        for (int i = 0; i < pops; i++)
        {
            pressed_keys.Pop();
        }

        pops = 0;

    }

    /// <summary>
    /// last element of pressedKeys list
    /// </summary>
    /// <returns>defaul value (X) if no key was pressed</returns>
    public KeyCode GetLastPressed()
    {
        if (pressedKeys.Count < 1)
            return KeyCode.X;
        return pressedKeys[pressedKeys.Count - 1];
    }

    /// <summary>
    /// removes all keys in pressedKeys list
    /// </summary>
    /// <param name="key"></param>
    public void RemoveKeyPressed(KeyCode key)
    {
        for (int i = 0; i < pressedKeys.Count; i++)
        {
            if (pressedKeys[i] == key)
            {
                pressedKeys.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// debug
    /// </summary>
    /// <returns></returns>
    public string PrintPressedKeys()
    {
        string s = "";
        foreach (KeyCode k in pressedKeys)
        {
            s += k.ToString();
            s += ";";
        }
        return s;
    }

    /// <summary>
    /// prevents List from containg too many elements
    /// shouldn't be neccessary (RemoveKey should do the job)
    /// </summary>
    public void CheckPressedKeys()
    {
        if (pressedKeys.Count > 5)
        {
            pressedKeys.RemoveAt(1);
        }
    }

    public bool PressedKeysContains(KeyCode key)
    {
        foreach (KeyCode k in pressedKeys)
        {
            if (k == key)
                return true;
        }
        return false;
    }

    // <<<...MOVEMENT>>> //

}
