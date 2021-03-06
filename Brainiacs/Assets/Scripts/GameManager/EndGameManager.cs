﻿using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class EndGameManager : MonoBehaviour
{

    private GameInfo gameInfo;
    private TextDisplay textDisplay;


    GameObject prefab;

    GameObject player1;
    GameObject player2;
    GameObject player3;
    GameObject player4;

    Player player1Comp;
    Player player2Comp;
    Player player3Comp;
    Player player4Comp;

    GameObject firstPlace;
    GameObject secondPlace;
    GameObject thirdPlace;
    GameObject fourthPlace;
    
    PlayerInfo player1Info = new PlayerInfo();
    PlayerInfo player2Info = new PlayerInfo();
    PlayerInfo player3Info = new PlayerInfo();
    PlayerInfo player4Info = new PlayerInfo();

    SpriteRenderer firstScoreSingle;
    SpriteRenderer firstScoreTenField;
    SpriteRenderer firstScoreUnitField;
    SpriteRenderer firstDeathSingle;
    SpriteRenderer firstDeathTenField;
    SpriteRenderer firstDeathUnitField;
    SpriteRenderer firstSlash;

    SpriteRenderer secondScoreSingle;
    SpriteRenderer secondScoreTenField;
    SpriteRenderer secondScoreUnitField;
    SpriteRenderer secondDeathSingle;
    SpriteRenderer secondDeathTenField;
    SpriteRenderer secondDeathUnitField;
    SpriteRenderer secondSlash;
    
    SpriteRenderer thirdScoreSingle;
    SpriteRenderer thirdScoreTenField;
    SpriteRenderer thirdScoreUnitField;
    SpriteRenderer thirdDeathSingle;
    SpriteRenderer thirdDeathTenField;
    SpriteRenderer thirdDeathUnitField;
    SpriteRenderer thirdSlash;

    SpriteRenderer fourthScoreSingle;
    SpriteRenderer fourthScoreTenField;
    SpriteRenderer fourthScoreUnitField;
    SpriteRenderer fourthDeathSingle;
    SpriteRenderer fourthDeathTenField;
    SpriteRenderer fourthDeathUnitField;
    SpriteRenderer fourthSlash;

    // Use this for initialization
    void Start()
    {
        try
        {
            gameInfo = GameObject.Find("GameInfo").GetComponent<GameInfo>();
        }
        catch (Exception e)
        {

        }

        if (gameInfo == null)
        {
            GameObject gameInfoObj = new GameObject("GameInfo");
            gameInfoObj.AddComponent<GameInfo>();
            gameInfo = gameInfoObj.GetComponent<GameInfo>();
        }
        
        gameInfo.RefreshInactiveStats();

        textDisplay = new TextDisplay();
        InitializeEndGameVariables();

        LoadPlayers();

        SortPlayers();

        DisplayResults();


        SetupPlayerPositions();
        AssignCharacters();
        Run();
    }

    List<PlayerResult> playersOrder;
    public void SortPlayers()
    {
        PlayerResult player1Result = new PlayerResult(1,gameInfo.player1score, gameInfo.player1deaths);
        PlayerResult player2Result = new PlayerResult(2,gameInfo.player2score, gameInfo.player2deaths);
        PlayerResult player3Result = new PlayerResult(3,gameInfo.player3score, gameInfo.player3deaths);
        PlayerResult player4Result = new PlayerResult(4,gameInfo.player4score, gameInfo.player4deaths);

        playersOrder = new List<PlayerResult>();
        playersOrder.Add(player1Result);
        playersOrder.Add(player2Result);
        playersOrder.Add(player3Result);
        playersOrder.Add(player4Result);

        playersOrder.Sort();
        
        switch (playersOrder[0].playerNumber)
        {
            case 1:
                firstPlace = player1;
                break;
            case 2:
                firstPlace = player2;
                break;
            case 3:
                firstPlace = player3;
                break;
            case 4:
                firstPlace = player4;
                break;
        }

        switch (playersOrder[1].playerNumber)
        {
            case 1:
                secondPlace = player1;
                break;
            case 2:
                secondPlace = player2;
                break;
            case 3:
                secondPlace = player3;
                break;
            case 4:
                secondPlace = player4;
                break;
        }

        switch (playersOrder[2].playerNumber)
        {
            case 1:
                thirdPlace = player1;
                break;
            case 2:
                thirdPlace = player2;
                break;
            case 3:
                thirdPlace = player3;
                break;
            case 4:
                thirdPlace = player4;
                break;
        }

        switch (playersOrder[3].playerNumber)
        {
            case 1:
                fourthPlace = player1;
                break;
            case 2:
                fourthPlace = player2;
                break;
            case 3:
                fourthPlace = player3;
                break;
            case 4:
                fourthPlace = player4;
                break;
        }

    }

    void DisplayResults()
    {

        int firstScore = playersOrder[0].score;
        int firstDeaths = playersOrder[0].deaths;

        int secondScore = playersOrder[1].score;
        int secondDeaths = playersOrder[1].deaths;

        int thirdScore = playersOrder[2].score;
        int thirdDeaths = playersOrder[2].deaths;

        int fourthScore = playersOrder[3].score;
        int fourthDeaths = playersOrder[3].deaths;


        SetEndGameValues(
            firstScore, firstDeaths, 
            secondScore, secondDeaths, 
            thirdScore, thirdDeaths, 
            fourthScore, fourthDeaths);

        RefreshSlashes();

    }

    public void RefreshSlashes()
    {
        Sprite slash = Resources.Load<Sprite>("Sprites/HUD/numbers/number_score_field");

        if (playersOrder[0].score < 0)
            firstSlash.sprite = null;
        else
            firstSlash.sprite = slash;

        if (playersOrder[1].score < 0)
            secondSlash.sprite = null;
        else
            secondSlash.sprite = slash;

        if (playersOrder[2].score < 0)
            thirdSlash.sprite = null;
        else
            thirdSlash.sprite = slash;

        if (playersOrder[3].score < 0)
            fourthSlash.sprite = null;
        else
            fourthSlash.sprite = slash;

    }


    public void LoadPlayers()
    {
        prefab = (GameObject)Resources.Load("Prefabs/PlayerManagment");
        
        player1 = Instantiate(prefab);
        player1Comp = player1.transform.GetChild(0).GetComponent<Player>();
        player1.transform.localScale = new Vector3(2, 2, 0);
        player1.SetActive(false);

        player2 = Instantiate(prefab);
        player2Comp = player2.transform.GetChild(0).GetComponent<Player>();
        player2.transform.localScale = new Vector3(2, 2, 0);
        player2.SetActive(false);

        player3 = Instantiate(prefab);
        player3Comp = player3.transform.GetChild(0).GetComponent<Player>();
        player3.transform.localScale = new Vector3(2, 2, 0);
        player3.SetActive(false);

        player4 = Instantiate(prefab);
        player4Comp = player4.transform.GetChild(0).GetComponent<Player>();
        player4.transform.localScale = new Vector3(2, 2, 0);
        player4.SetActive(false);
    }

    public void SetupPlayerPositions()
    {
        Vector2 firstPlaceposition = new Vector2(-0.75f, 1.1f);
        //if (gameInfo.player1type != PlayerTypeEnum.None)
            firstPlace.transform.position = firstPlaceposition;

        Vector2 secondPlaceposition = new Vector2(-2.9f, 0.1f);
        //if (gameInfo.player2type != PlayerTypeEnum.None)
            secondPlace.transform.position = secondPlaceposition;

        Vector2 thirdPlaceposition = new Vector2(1.45f, -0.9f);
        //if (gameInfo.player3type != PlayerTypeEnum.None)
            thirdPlace.transform.position = thirdPlaceposition;

        Vector2 fourthPlaceposition = new Vector2(3.6f, -1.9f);
        //if (gameInfo.player4type != PlayerTypeEnum.None)
            fourthPlace.transform.position = fourthPlaceposition;
    }

    public void AssignCharacters()
    {
        if (gameInfo.player1type != PlayerTypeEnum.None)
            SetUpPlayer(1, gameInfo.player1char);
        if (gameInfo.player2type != PlayerTypeEnum.None)
            SetUpPlayer(2, gameInfo.player2char);
        if (gameInfo.player3type != PlayerTypeEnum.None)
            SetUpPlayer(3, gameInfo.player3char);
        if (gameInfo.player4type != PlayerTypeEnum.None)
            SetUpPlayer(4, gameInfo.player4char);
    }
    
    bool player1Active = false;
    bool player2Active = false;
    bool player3Active = false;
    bool player4Active = false;

    public void SetUpPlayer(int playerNumber, CharacterEnum charEnum)
    {
        if (playerNumber < 1 || playerNumber > 4) return;
        switch (playerNumber)
        {
            case 1:
                player1Info.playerNumber = 1;
                player1Info.charEnum = charEnum;
                player1Info.playerColor = "red";
                player1Info.lifes = gameInfo.lifes;
                switch (gameInfo.player1type)
                {
                    case PlayerTypeEnum.None:
                        player1Active = false;
                        break;
                    default:
                        ControlKeys ck = new ControlKeysP1();
                        ck.keyFire = KeyCode.None;
                        player1Comp.SetUpPlayer(player1Info, ck);
                        player1Active = true;
                        break;
                    
                }
                break;
            case 2:
                player2Info.playerNumber = 2;
                player2Info.charEnum = charEnum;
                player2Info.playerColor = "green";
                player2Info.lifes = gameInfo.lifes;
                switch (gameInfo.player2type)
                {
                    case PlayerTypeEnum.None:
                        player2Active = false;
                        break;
                    default:
                        ControlKeys ck = new ControlKeysP2();
                        ck.keyFire = KeyCode.None;
                        player2Comp.SetUpPlayer(player2Info, ck);
                        player2Active = true;
                        break;
                    
                }
                break;
            case 3:
                player3Info.playerNumber = 3;
                player3Info.charEnum = charEnum;
                player3Info.playerColor = "blue";
                player3Info.lifes = gameInfo.lifes;
                switch (gameInfo.player3type)
                {
                    case PlayerTypeEnum.None:
                        player3Active = false;
                        break;
                    default:
                        ControlKeys ck = new ControlKeysP3();
                        ck.keyFire = KeyCode.None;
                        player3Comp.SetUpPlayer(player3Info, ck);
                        player3Active = true;
                        break;

                }
                break;
            case 4:
                player4Info.playerNumber = 4;
                player4Info.charEnum = charEnum;
                player4Info.playerColor = "yellow";
                player4Info.lifes = gameInfo.lifes;
                switch (gameInfo.player4type)
                {
                    case PlayerTypeEnum.None:
                        player4Active = false;
                        break;
                    default:
                        ControlKeys ck = new ControlKeysP4();
                        ck.keyFire = KeyCode.None;
                        player4Comp.SetUpPlayer(player4Info, ck);
                        player4Active = true;
                        break;

                }
                break;
        }
    }

    public void Run()
    {
        if (player1Active) player1.SetActive(true);
        if (player2Active) player2.SetActive(true);
        if (player3Active) player3.SetActive(true);
        if (player4Active) player4.SetActive(true);
    }


    public void InitializeEndGameVariables()
    {
        firstScoreSingle = GameObject.Find("first_score_single_field").GetComponent<SpriteRenderer>();
        firstScoreTenField = GameObject.Find("first_score_ten_field").GetComponent<SpriteRenderer>();
        firstScoreUnitField = GameObject.Find("first_score_unit_field").GetComponent<SpriteRenderer>();
        firstDeathSingle = GameObject.Find("first_death_single_field").GetComponent<SpriteRenderer>();
        firstDeathTenField = GameObject.Find("first_death_ten_field").GetComponent<SpriteRenderer>();
        firstDeathUnitField = GameObject.Find("first_death_unit_field").GetComponent<SpriteRenderer>();
        firstSlash = GameObject.Find("first_slash").GetComponent<SpriteRenderer>();

        secondScoreSingle = GameObject.Find("second_score_single_field").GetComponent<SpriteRenderer>();
        secondScoreTenField = GameObject.Find("second_score_ten_field").GetComponent<SpriteRenderer>();
        secondScoreUnitField = GameObject.Find("second_score_unit_field").GetComponent<SpriteRenderer>();
        secondDeathSingle = GameObject.Find("second_death_single_field").GetComponent<SpriteRenderer>();
        secondDeathTenField = GameObject.Find("second_death_ten_field").GetComponent<SpriteRenderer>();
        secondDeathUnitField = GameObject.Find("second_death_unit_field").GetComponent<SpriteRenderer>();
        secondSlash = GameObject.Find("second_slash").GetComponent<SpriteRenderer>();

        thirdScoreSingle = GameObject.Find("third_score_single_field").GetComponent<SpriteRenderer>();
        thirdScoreTenField = GameObject.Find("third_score_ten_field").GetComponent<SpriteRenderer>();
        thirdScoreUnitField = GameObject.Find("third_score_unit_field").GetComponent<SpriteRenderer>();
        thirdDeathSingle = GameObject.Find("third_death_single_field").GetComponent<SpriteRenderer>();
        thirdDeathTenField = GameObject.Find("third_death_ten_field").GetComponent<SpriteRenderer>();
        thirdDeathUnitField = GameObject.Find("third_death_unit_field").GetComponent<SpriteRenderer>();
        thirdSlash = GameObject.Find("third_slash").GetComponent<SpriteRenderer>();

        fourthScoreSingle = GameObject.Find("fourth_score_single_field").GetComponent<SpriteRenderer>();
        fourthScoreTenField = GameObject.Find("fourth_score_ten_field").GetComponent<SpriteRenderer>();
        fourthScoreUnitField = GameObject.Find("fourth_score_unit_field").GetComponent<SpriteRenderer>();
        fourthDeathSingle = GameObject.Find("fourth_death_single_field").GetComponent<SpriteRenderer>();
        fourthDeathTenField = GameObject.Find("fourth_death_ten_field").GetComponent<SpriteRenderer>();
        fourthDeathUnitField = GameObject.Find("fourth_death_unit_field").GetComponent<SpriteRenderer>();
        fourthSlash = GameObject.Find("fourth_slash").GetComponent<SpriteRenderer>();

    }

    public void SetEndGameValues(
        int firstScore, int firstDeaths,
        int secondScore, int secondDeaths,
        int thirdScore, int thirdDeaths,
        int fourthScore, int fourthDeaths)
    {
        textDisplay.DisplayNumberOn(firstScoreSingle, firstScoreTenField, firstScoreUnitField, firstScore);
        textDisplay.DisplayNumberOn(firstDeathSingle, firstDeathTenField, firstDeathUnitField, firstDeaths);

        textDisplay.DisplayNumberOn(secondScoreSingle, secondScoreTenField, secondScoreUnitField, secondScore);
        textDisplay.DisplayNumberOn(secondDeathSingle, secondDeathTenField, secondDeathUnitField, secondDeaths);

        textDisplay.DisplayNumberOn(thirdScoreSingle, thirdScoreTenField, thirdScoreUnitField, thirdScore);
        textDisplay.DisplayNumberOn(thirdDeathSingle, thirdDeathTenField, thirdDeathUnitField, thirdDeaths);

        textDisplay.DisplayNumberOn(fourthScoreSingle, fourthScoreTenField, fourthScoreUnitField, fourthScore);
        textDisplay.DisplayNumberOn(fourthDeathSingle, fourthDeathTenField, fourthDeathUnitField, fourthDeaths);

    }



}
