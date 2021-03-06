﻿using UnityEngine;
using System.Collections;

public class QuitInGameButton : MonoBehaviour {

    public GUIStyle backTexture;

    void OnGUI()
    {
        float buttonX = (1920.0f - backTexture.normal.background.width) * Screen.width / 1920.0f;
        float buttonWidth = backTexture.normal.background.width * Screen.width / 1920.0f;
        float buttonHeight = backTexture.normal.background.height * Screen.height / 1080.0f;

        if (GUI.Button(new Rect(buttonX, 0.0f, buttonWidth, buttonHeight), "", backTexture))
        {
            //Application.LoadLevel("EndGame");
            GameManager gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
            gameManager.EndGame();
        }
    }
}
