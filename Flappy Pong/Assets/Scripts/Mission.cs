using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Mission : MonoBehaviour
{
    public GameController gameController;
    private TextMeshProUGUI rewardText;
    public Image rewardImage;
    public bool timed;
    public float timeLimit;
    public float distanceLimit; // if positive reach distance, if negative achieve before distance
    private TextMeshProUGUI missionText;
    private bool missionEnd;
    public enum MissionType
    {
        Distance, // walled or timed
        Coins, // collect or evade, timed or distanced
        Walls, // bounce or regular, timed or distanced
        Combos, // amount and length, timed or distanced
        PowerUps, // evade or collect, timed or distanced
        Revive, // amount, timed or distanced
        Hazards // amount with invincible, timed or distanced
    }
    public MissionType missionType;
    private int spentGems;

    //Mission Type Vars
    private int amountRequired;
    private int comboType;
    public int rewardType; // 1 = coins, 2 = gems, 3 = unlock
    public int itemRewardType;
    public int itemRewardID;
    public int rewardAmount;
    public string rewardItem = "";
    public Transform rewardButton;

    private void OnEnable()
    {
        rewardText = GameObject.Find("Mission Reward Text").GetComponent<TextMeshProUGUI>();
        rewardImage = GameObject.Find("Mission Reward Image").GetComponent<Image>();
        gameController = FindAnyObjectByType<GameController>();
        //spentGems = 100; // hardest
        //spentGems = Random.Range(0, gameController.totalShopGemPrice); // random
        spentGems = gameController.spentGems; // accurate
        missionText = GameObject.Find("Mission Text").GetComponent<TextMeshProUGUI>();
        int randomChoice = 0;
        int difficulty = 0;

        //Difficulty based on how much of the total shop has been bought out in thirds
        if (spentGems < gameController.totalShopGemPrice / 3)
        {
            if (Random.Range(0f, 1f) < 0.8f) // 80%
                difficulty = 1;
            else if (Random.Range(0f, 1f) < 0.8f) // 16%
                difficulty = 2;
            else // 4%
                difficulty = 3;
        }
        else if (spentGems >= gameController.totalShopGemPrice / 3 && spentGems < 2 * (gameController.totalShopGemPrice / 3))
        {
            if (Random.Range(0f, 1f) < 0.5f) // 50%
                difficulty = 1;
            else if (Random.Range(0f, 1f) < 0.8f) // 40%
                difficulty = 2;
            else // 10%
                difficulty = 3;
        }
        else
        {
            if (Random.Range(0f, 1f) < 0.3f) // 30%
                difficulty = 1;
            else if (Random.Range(0f, 1f) < 0.7f) // 49%
                difficulty = 2;
            else // 21%
                difficulty = 3;
        }

        if (difficulty == 1)
        {
            //easy
            rewardType = 1;
            rewardImage.sprite = gameController.coinSprite;
            rewardAmount = Random.Range(50, 201);
            rewardText.text = rewardAmount.ToString();
        }
        else if (difficulty == 2)
        {
            //medium
            if (Random.value > 0.5f)
                PickTimeLimit(60, 120);
            else
                PickDistanceLimit(100, 150, Random.value > 0.5f);
            rewardType = 2;
            rewardImage.sprite = gameController.gemSprite;
            rewardAmount = Random.Range(1, 4);
            rewardText.text = rewardAmount.ToString();
        }
        else
        {
            //hard
            PickTimeLimit(60, 120);
            PickDistanceLimit(100, 150, Random.value > 0.5f);
            rewardType = Random.Range(2,4);
            if (rewardType == 2)
            {
                rewardAmount = Random.Range(5, 9);
                rewardImage.sprite = gameController.gemSprite;
                rewardText.text = rewardAmount.ToString();
            }
            else // generate item to unlock, try 10 times, if nothing go to gems
            {
                rewardText.text = "";
                int attempts = 10;
                while (rewardItem == "" || (attempts > 0 && PlayerPrefs.GetInt(rewardItem) == 1))                             // NEED TO MAKE CASE IF ALL ITEMS ARE UNLOCKED, AND REMOVE LOCKED SYMBOL IN SHOP WHEN REWARDED
                {
                    attempts--;
                    itemRewardType = Random.Range(1, 6);

                    switch (itemRewardType)
                    {
                        case 1:
                            itemRewardID = Random.Range(1, 30);
                            rewardItem = "Skins" + itemRewardID.ToString() + "unlocked";
                            rewardImage.sprite = gameController.skinSprites[itemRewardID];
                            break;

                        case 2:
                            itemRewardID = Random.Range(1, 10);
                            rewardItem = "Trails" + itemRewardID.ToString() + "unlocked";
                            rewardImage.sprite = gameController.trailSprites[itemRewardID];
                            break;

                        case 3:
                            itemRewardID = Random.Range(1, 5);
                            rewardItem = "Themes" + itemRewardID.ToString() + "unlocked";
                            rewardImage.sprite = gameController.themeSprites[itemRewardID];
                            break;

                        case 4:
                            itemRewardID = Random.Range(0, 5);
                            rewardItem = "Power-Ups" + itemRewardID.ToString() + "unlocked";
                            rewardImage.sprite = gameController.powerUpSprites[itemRewardID];
                            break;

                        case 5:
                            itemRewardID = Random.Range(1, 6);
                            rewardItem = "Charms" + itemRewardID.ToString() + "unlocked";
                            rewardImage.sprite = gameController.charmSprites[itemRewardID];
                            break;
                    }
                }

                // failed to find locked item, go back to gems
                if (attempts == 0)
                {
                    rewardType = 2;
                    rewardAmount = Random.Range(5, 9);
                    rewardImage.sprite = gameController.gemSprite;
                    rewardText.text = rewardAmount.ToString();
                }
            }
        }

        randomChoice = Random.Range(1, 8);

        // generate mission
        switch (randomChoice)
        {
            case 1:
                missionType = MissionType.Distance;
                amountRequired = Random.Range(150, 351);
                missionText.text = "Reach " + amountRequired + "m";
                break;

            case 2:
                missionType = MissionType.Coins;
                amountRequired = Random.Range(10, 16);
                missionText.text = "Collect " + amountRequired + " coins";
                break;

            case 3:
                missionType = MissionType.Walls;
                amountRequired = Random.Range(20, 51);
                missionText.text = "Break " + amountRequired + " walls";
                break;

            case 4:
                missionType = MissionType.Combos;
                amountRequired = Random.Range(2, 5);
                comboType = Random.Range(2, 5);
                missionText.text = "Perform " + amountRequired;
                if (comboType == 2)
                    missionText.text += " double combos";
                else if (comboType == 3)
                    missionText.text += " triple combos";
                else if (comboType == 4)
                    missionText.text += " quadruple combos";
                break;

            case 5:
                missionType = MissionType.PowerUps;
                amountRequired = Random.Range(5, 11);
                missionText.text = "Collect " + amountRequired + " power-ups";
                break;

            case 6:
                missionType = MissionType.Revive;
                amountRequired = Random.Range(2, 5);
                missionText.text = "Revive " + amountRequired + " times";
                break;

            case 7:
                missionType = MissionType.Hazards;
                amountRequired = Random.Range(3, 6);
                missionText.text = "Hit " + amountRequired + " hazards while invincible";
                break;
        }

        if (timed && distanceLimit < 0)
            missionText.text += " before " + timeLimit.ToString("F0") + " seconds and before reaching " + Mathf.Abs(distanceLimit).ToString("F0") + "m";
        else if (timed)
            missionText.text += " before " + timeLimit.ToString("F0") + " seconds";
        else if (distanceLimit < 0)
            missionText.text += " before reaching " + Mathf.Abs(distanceLimit).ToString("F0") + "m";
    }

    void Update()
    {
        if (gameController.gameState == GameController.GameState.Playing && !missionEnd)
        {
            // check if time limit has passed before conditions met
            if (timed)
            {
                if (!ConditionsMet())
                {
                    if (gameController.seconds > timeLimit)
                    {
                        // mission failed
                        Debug.Log("Mission failed due to time");
                        missionEnd = true;
                    }
                }
                else
                {
                    // mission success
                    Debug.Log("Mission success");
                    gameController.missionAccomplished = true;
                    missionEnd = true;

                }
            }

            // check if distance limit has passed before conditions met
            if (distanceLimit < 0)
            {
                if (!ConditionsMet())
                {
                    if (gameController.maxHeight > Mathf.Abs(distanceLimit))
                    {
                        // mission failed
                        Debug.Log("Mission failed due to distance");
                        missionEnd = true;
                    }
                }
                else
                {
                    // mission success
                    Debug.Log("Mission success");
                    gameController.missionAccomplished = true;
                    missionEnd = true;
                }
            }
        } // game playing
    }

    private void PickTimeLimit(float lower, float upper)
    {
        timeLimit = Random.Range(lower, upper);
        timed = true;
    }

    private void PickDistanceLimit(float lower, float upper, bool negative)
    {
        if (negative)
            distanceLimit = -Random.Range(lower, upper);
        else
            distanceLimit = Random.Range(lower, upper);
    }

    public bool ConditionsMet()
    {
        if (missionType == MissionType.Distance && gameController.maxHeight >= amountRequired)
            return CheckDistance();
        else if (missionType == MissionType.Coins && gameController.coinsThisGame >= amountRequired)
            return CheckDistance();
        else if (missionType == MissionType.Walls && gameController.wallsBroke >= amountRequired)
            return CheckDistance();
        else if (missionType == MissionType.Combos)
        {
            if (comboType == 2 && gameController.doubleCombos >= amountRequired)
                return CheckDistance();
            else if (comboType == 3 && gameController.tripleCombos >= amountRequired)
                return CheckDistance();
            else if (comboType == 4 && gameController.quadCombos >= amountRequired)
                return CheckDistance();
            else
                return false;
        }
        else if (missionType == MissionType.PowerUps && gameController.powerUpsCollected >= amountRequired)
            return CheckDistance();
        else if (missionType == MissionType.Revive && gameController.revives >= amountRequired)
            return CheckDistance();
        else if (missionType == MissionType.Hazards && gameController.invincibleHazards >= amountRequired)
            return CheckDistance();
        else
            return false;
    }

    // checks if positive distance has been reached
    private bool CheckDistance()
    {
        if (distanceLimit > 0)
            return gameController.maxHeight >= distanceLimit;
        else
            return true;
    }
}
