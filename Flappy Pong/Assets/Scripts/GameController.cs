using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using HutongGames.PlayMaker;
using UnityEngine.Audio;
using UnityEngine.Purchasing;

public class GameController : MonoBehaviour
{
    //References
    public Transform player;
    private SpriteRenderer playerSprite;
    private Rigidbody2D rb;
    private Transform hazardWalls;
    private TextMeshProUGUI heightText;
    [HideInInspector] public Transform runHolder;
    public Mission mission;
    private TextMeshProUGUI timerText;
    private Transform highscoreMarker;
    private AdsManager adsManager;
    private Transform shopPrompt;
    private Transform grantedPrompt;
    public AudioMixer audioMixer;
    public Slider sfxSlider;
    public Slider musicSlider;
    private GameObject pauseMenu;
    public GameObject runholderPrefab;
    public ParticleSystem confetti;

    //Optimization Caches
    private WaitForSeconds waitForSecondsClock = new WaitForSeconds(1);
    private WaitForSeconds waitForSecondsInitialWalls = new WaitForSeconds(0.15f);
    [HideInInspector] public Coroutine invincibleCoroutine;
    private float invincibleStartTime;
    private float invincibleDuration;
    private ObjectPool collectiblePool;
    private ObjectPool boosterPool;
    private ObjectPool cannonPool;
    private ObjectPool normalHazardPool;
    private ObjectPool rotHazardPool;
    private ObjectPool invincibleTrailPool;
    private ObjectPool SFXPool;
    private ObjectPool wallPool;
    public ObjectPool wallBreakPool;

    //States
    public enum GameState
    {
        Menu,
        Setup,
        Playing,
        Revive,
    }
    [HideInInspector] public GameState gameState;
    private float cameraPosY;
    [HideInInspector] public float direction = 1; // current horizontal direction
    public float currentSpeed;
    [HideInInspector] public float overrideSpeed = 0; // overrides speed when not 0
    private float normalSpeed; // what the normal speed would be if not overrided, higher normal speed means higher jump height and less chance for walls to spawn, speed gradually increases with your vertical height
    private float nextWallHeight = -14; // height player must reach to spawn next walls
    [HideInInspector] public int fsmLink; // var used to communicate with fsm
    public bool invincible;
    private bool preventDoubleObstacle; // bool that prevents obstacles and other things from spawning twice in a row
    private int leftWallCounter; // # of times wall has not spawned
    private int rightWallCounter; // # of times wall has not spawned
    public bool canJump = true;
    public bool paused;
    public bool clockActive;
    public int shields; // current num of shields
    private bool extraComboClick; // used to give the combo an extra click before resetting
    private bool coinTrailActive;
    private bool promptResponded;
    private bool promptResponse;
    public bool inCannon;
    public Transform cannon;
    public bool canMove = true;
    private float timeScaleBeforePause;
    private bool vibrationEnabled;
    private bool adsDisabled;

    //Cosmetics and Shop
    public int skinPrice;
    public int trailPrice;
    public int themePrice;
    public int powerUpPrice;
    public int charmPrice;
    public int totalShopGemPrice;
    public Sprite[] skinSprites = new Sprite[0];
    public Sprite coinSprite;
    public Sprite gemSprite;
    public GameObject[] trails = new GameObject[0];
    public Sprite[] trailSprites = new Sprite[0];
    public Sprite[] themeSprites = new Sprite[0]; // 0 = ???, 1 = forest, 2 = ???, 3 = ???, 4 = ???
    public Sprite[] powerUpSprites = new Sprite[0]; // 0 = invincibility, 1 = shields, 2 = coin trail, 3 = ???, 4 = cannon
    public Sprite[] charmSprites = new Sprite[0]; // 0 = nothing, 1 = bounce wall invincibility, 2 = gem wall, 3 = shrink, 4 = combo reset, 5 = 20% survival chance
    private int activeSkin;
    private int activeTrail;
    private int activeTheme;
    public int activeCharm = 0;
    public GameObject trail;
    private string[] powerUpDescriptions = new string[] {
            "Orbs that grant 5 seconds of invulnerability will appear",
            "Shields stackable up to 3 will appear",
            "Orbs that grant a shower of coins will appear",
            "Orbs that cause slow motion for 5 seconds will appear",
            "Cannons that give you a powerful boost will appear"};
    private string[] charmDescriptions = new string[] {
            "Bouncy walls will grant 2 seconds of invulnerability if equipped",
            "Special walls that grant a gem will appear if equipped",
            "The ball will become slightly smaller if equipped",
            "Combos will only reset after an extra jump if equipped",
            "You will have a 20% chance to survive when hitting a hazard if equipped" };

    //Stats
    public float defaultSpeed;

    [Header("Game Stats")]
    public int combo; // current combo
    public int highestCombo;
    public float maxHeight; // highest height for this run
    public int wallsBroke;
    public int seconds;
    public int doubleCombos;
    public int tripleCombos;
    public int quadCombos;
    public int coinsThisGame;
    public int invincibleHazards;
    public int revives;
    public bool missionAccomplished;
    public int powerUpsCollected;
    private bool newHighScore;

    [Header("Intergame Stats")]
    public int coins;
    public int gems;
    public float highscore;
    public int spentGems;
    public int totalNumRuns;

    //Audio
    [Header("SFX")]
    public AudioClip wallBreak;
    public AudioClip invincibleStart;
    public AudioClip bounce;
    public AudioClip hitHazardInvincible;
    public AudioClip hitHazard;
    public AudioClip coinCollect;
    public AudioClip jump;
    public AudioClip revive;
    public AudioClip powerUp;
    public AudioClip extraClick;
    public AudioClip shieldUp;
    public AudioClip shieldDown;
    public AudioClip cannonShoot;


    void Start()
    {
        if (PlayerPrefs.GetInt("freshgame") == 0)
        {
            PlayerPrefs.SetInt("freshgame", 1);
            // tutorial
            FindFSM(gameObject, "How To Play").SendEvent("global event");
        }
        else
        {
            sfxSlider.value = PlayerPrefs.GetFloat("sfxvolume");
            musicSlider.value = PlayerPrefs.GetFloat("musicvolume");
        }
        SetMusicVolume();
        SetSFXVolume();
        SaveSettings();
        collectiblePool = GameObject.Find("Collectible Pool").GetComponent<ObjectPool>();
        boosterPool = GameObject.Find("Booster Pool").GetComponent<ObjectPool>();
        cannonPool = GameObject.Find("Cannon Pool").GetComponent<ObjectPool>();
        normalHazardPool = GameObject.Find("Normal Hazard Pool").GetComponent<ObjectPool>();
        rotHazardPool = GameObject.Find("Rot Hazard Pool").GetComponent<ObjectPool>();
        invincibleTrailPool = GameObject.Find("Invincible Trail Pool").GetComponent<ObjectPool>();
        SFXPool = GameObject.Find("SFX Pool").GetComponent<ObjectPool>();
        wallPool = GameObject.Find("Wall Pool").GetComponent<ObjectPool>();
        wallBreakPool = GameObject.Find("Wall Break Pool").GetComponent<ObjectPool>();
        totalNumRuns = PlayerPrefs.GetInt("totalnumruns");
        vibrationEnabled = PlayerPrefs.GetInt("vibrationenabled") == 0;
        adsDisabled = PlayerPrefs.GetInt("adsdisabled") == 1;
        GameObject.Find("Vibrate Toggle").GetComponent<Toggle>().isOn = vibrationEnabled;
        adsManager = GetComponent<AdsManager>();
        shopPrompt = GameObject.Find("Shop Prompt").transform;
        grantedPrompt = GameObject.Find("Reward Granted Prompt").transform;
        pauseMenu = GameObject.Find("Pause Menu");
        shopPrompt.gameObject.SetActive(false);
        grantedPrompt.gameObject.SetActive(false);
        pauseMenu.SetActive(false);
        highscoreMarker = GameObject.Find("Highscore Marker").transform;
        timerText = GameObject.Find("Timer Text").GetComponent<TextMeshProUGUI>();
        timerText.gameObject.SetActive(false);
        spentGems = PlayerPrefs.GetInt("spentgems");
        highscore = PlayerPrefs.GetFloat("highscore");
        highscoreMarker.position = new Vector2(0, highscore);
        GameObject.Find("Highscore Text").GetComponent<TextMeshProUGUI>().text = "Highscore: \n" + highscore.ToString("F0") + "m";
        coins = PlayerPrefs.GetInt("coins");
        gems = PlayerPrefs.GetInt("gems");
        heightText = GameObject.Find("Height Text").GetComponent<TextMeshProUGUI>();
        hazardWalls = GameObject.Find("Hazard Walls").transform;
        confetti = hazardWalls.GetComponent<ParticleSystem>();
        player = GameObject.Find("Player").transform;
        rb = player.GetComponent<Rigidbody2D>();
        playerSprite = player.GetComponent<SpriteRenderer>();
        mission = gameObject.AddComponent<Mission>();
        InitializeCosmetics();
    }

    private void InitializeCosmetics()
    {
        activeSkin = PlayerPrefs.GetInt("activeskin");
        activeTrail = PlayerPrefs.GetInt("activetrail");
        activeTheme = PlayerPrefs.GetInt("activetheme");
        activeCharm = PlayerPrefs.GetInt("activecharm");

        if (activeSkin == 0)
            PlayerPrefs.SetInt("Skins0unlocked", 1);
        if (activeTrail == 0)
            PlayerPrefs.SetInt("Trails0unlocked", 1);
        if (activeTheme == 0)
            PlayerPrefs.SetInt("Themes0unlocked", 1);

        GameObject skins = GameObject.Find("Skins");
        skins.transform.GetChild(activeSkin).GetComponent<Image>().color = Color.gray;
        GameObject trails = GameObject.Find("Trails");
        trails.transform.GetChild(activeTrail).GetComponent<Image>().color = Color.gray;
        GameObject themes = GameObject.Find("Themes");
        themes.transform.GetChild(activeTheme).GetComponent<Image>().color = Color.gray;
        GameObject powerUps = GameObject.Find("Power-Ups");
        GameObject charms = GameObject.Find("Charms");
        if(activeCharm != 0)
            charms.transform.GetChild(activeCharm - 1).GetComponent<Image>().color = Color.gray;

        GameObject.Find("Shop Coin Text").GetComponent<TextMeshProUGUI>().text = ": " + coins;
        GameObject.Find("Shop Gem Text").GetComponent<TextMeshProUGUI>().text = ": " + gems;
        SetCosmetics();

        int index = 0;
        //Initialize shop buttons
        foreach(Transform child in skins.transform)
        {
            child.GetChild(0).GetComponent<Image>().sprite = skinSprites[index];
            if (PlayerPrefs.GetInt("Skins" + child.name + "unlocked") == 1)
                child.GetChild(1).gameObject.SetActive(false);
            else
                child.GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>().text = skinPrice.ToString();   //shopPrices[index].ToString();
            index++;
        }

        index = 0;
        foreach (Transform child in trails.transform)
        {
            child.GetChild(0).GetComponent<Image>().sprite = trailSprites[index];
            if (PlayerPrefs.GetInt("Trails" + child.name + "unlocked") == 1)
                child.GetChild(1).gameObject.SetActive(false);
            else
                child.GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>().text = trailPrice.ToString(); //shopPrices[index].ToString();
            index++;
        }

        index = 0;
        foreach (Transform child in themes.transform)
        {
            child.GetChild(0).GetComponent<Image>().sprite = themeSprites[index];
            if (PlayerPrefs.GetInt("Themes" + child.name + "unlocked") == 1)
                child.GetChild(1).gameObject.SetActive(false);
            else
                child.GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>().text = themePrice.ToString(); //shopPrices[index].ToString();
            index++;
        }

        index = 0;
        foreach (Transform child in powerUps.transform)
        {
            child.GetChild(0).GetComponent<Image>().sprite = powerUpSprites[index];

            if (index == 0) // unlock 1st power up by default
            {
                child.GetChild(1).gameObject.SetActive(false);
                PlayerPrefs.SetInt("Power-Ups0unlocked", 1);
            }
            else
            {
                if (PlayerPrefs.GetInt("Power-Ups" + child.name + "unlocked") == 1)
                    child.GetChild(1).gameObject.SetActive(false);
                else
                    child.GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>().text = powerUpPrice.ToString(); //shopPrices[index].ToString();
            }
            index++;
        }

        index = 0;
        foreach (Transform child in charms.transform)
        {
            child.GetChild(0).GetComponent<Image>().sprite = charmSprites[index];
            if (PlayerPrefs.GetInt("Charms" + child.name + "unlocked") == 1)
                child.GetChild(1).gameObject.SetActive(false);
            else
                child.GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>().text = charmPrice.ToString(); //shopPrices[index].ToString();
            index++;
        }
    }

    //Called at start and when exiting shop
    public void SetCosmetics()
    {
        playerSprite.sprite = skinSprites[activeSkin];

        if (trail != null)
            Destroy(trail);
        trail = Instantiate(trails[activeTrail], player.position, Quaternion.identity, player);

        SoloActivate(Camera.main.transform.GetChild(0).GetChild(activeTheme).gameObject);
    }

    //Prepares game for next run
    public void SetupGame()
    {
        if (!adsDisabled)
        {
            //adsManager.LoadInterstitial(); disabled for prototype
        }
        player.gameObject.layer = 3;
        totalNumRuns++;
        PlayerPrefs.SetInt("totalnumruns", totalNumRuns);
        transform.GetChild(0).GetComponent<AudioSource>().Play();
        ClearGameStats();
        if (mission.timed)
        {
            timerText.text = mission.timeLimit.ToString("F0");
            timerText.gameObject.SetActive(true);
        }
        fsmLink = 0;
        nextWallHeight = -14;
        currentSpeed = defaultSpeed;
        direction = 1;
        runHolder = Instantiate(runholderPrefab).transform;
        StartCoroutine(InitialWalls());
    }

    //Resets all run stats at beginning of run
    private void ClearGameStats()
    {
        highestCombo = 0;
        maxHeight = 0;
        wallsBroke = 0;
        seconds = 0;
        doubleCombos = 0;
        tripleCombos = 0;
        quadCombos = 0;
        coinsThisGame = 0;
        invincibleHazards = 0;
        revives = 0;
        missionAccomplished = false;
        powerUpsCollected = 0;
        newHighScore = false;
    }

    //Spawns first walls and enables the first input
    IEnumerator InitialWalls()
    {
        yield return waitForSecondsInitialWalls;
        if (player.position.y > nextWallHeight)
        {
            SpawnWalls(1); // chance between 0 and 1
            nextWallHeight += 2;
            StartCoroutine(InitialWalls());
        }
        else
            gameState = GameState.Setup;
    }

    //Counts seconds
    IEnumerator Clock()
    {
        seconds++;
        yield return waitForSecondsClock;
        if (mission.timed)
            timerText.text = (Mathf.Clamp(mission.timeLimit - seconds, 0, 1000)).ToString("F0");
        if (clockActive)
            StartCoroutine(Clock());
    }

    //Called if the player chooses to revive
    public void Revive()
    {
        player.gameObject.layer = 3;
        transform.GetChild(0).GetComponent<AudioSource>().Play();
        PlaySound(revive, 1, 1.4f);
        revives++;
        combo = 0;
        direction = 1;
        fsmLink = 0;
        rb.bodyType = RigidbodyType2D.Static;
        player.position = new Vector2(0, Camera.main.transform.position.y);
        gameState = GameState.Setup;
    }

    //Called if player dies and does not revive or player quits
    public void EndGame()
    {
        collectiblePool.DeactivateAll();
        boosterPool.DeactivateAll();
        cannonPool.DeactivateAll();
        normalHazardPool.DeactivateAll();
        rotHazardPool.DeactivateAll();
        invincibleTrailPool.DeactivateAll();
        SFXPool.DeactivateAll();
        wallPool.DeactivateAll();
        transform.GetChild(0).GetComponent<AudioSource>().Stop();
        timerText.gameObject.SetActive(false);
        clockActive = false;
        trail.GetComponent<PlayMakerFSM>().SendEvent("global off");
        combo = 0;
        shields = 0;
        heightText.text = "0m";
        gameState = GameState.Menu;
        Destroy(runHolder.gameObject);
        rb.bodyType = RigidbodyType2D.Static;
        player.position = new Vector3(0, 0, 0);
        hazardWalls.position = new Vector3(0, 0, 0);
        cameraPosY = 0;
        Camera.main.transform.position = new Vector3(0, 0, -10);

        // run interstitial
        if (!adsDisabled)
        {
            if (revives == 0 && maxHeight >= 20 && totalNumRuns >= 6 && totalNumRuns % 2 == 0)
            {
                //adsManager.ShowInterstitial(); disabled for prototype
            }
        }

        if (!mission.timed && mission.distanceLimit >= 0)
            missionAccomplished = mission.ConditionsMet();

        StartCoroutine(NewMission());
    }

    //Shows mission accomplished and creates new mission
    private IEnumerator NewMission()
    {
        if (missionAccomplished)
        {
            promptResponded = false;
            grantedPrompt.GetChild(3).GetComponent<Image>().sprite = mission.rewardImage.sprite;

            // set prompt text 
            switch (mission.rewardType)
            {
                case 1:
                    grantedPrompt.GetChild(2).GetComponent<TextMeshProUGUI>().text = mission.rewardAmount.ToString() + " Coins";
                    break;
                case 2:
                    grantedPrompt.GetChild(2).GetComponent<TextMeshProUGUI>().text = mission.rewardAmount.ToString() + " Gems";
                    break;
                case 3:

                    switch (mission.itemRewardType)
                    {
                        case 1:
                            grantedPrompt.GetChild(2).GetComponent<TextMeshProUGUI>().text = "New Skin";
                            break;

                        case 2:
                            grantedPrompt.GetChild(2).GetComponent<TextMeshProUGUI>().text = "New Trail";
                            break;

                        case 3:
                            grantedPrompt.GetChild(2).GetComponent<TextMeshProUGUI>().text = "New Theme";
                            break;

                        case 4:
                            grantedPrompt.GetChild(2).GetComponent<TextMeshProUGUI>().text = powerUpDescriptions[mission.itemRewardID];
                            break;

                        case 5:
                            grantedPrompt.GetChild(2).GetComponent<TextMeshProUGUI>().text = charmDescriptions[mission.itemRewardID - 1];
                            break;
                    }
                    break;
            }

            grantedPrompt.gameObject.SetActive(true);
            yield return new WaitUntil(() => promptResponded);
            grantedPrompt.gameObject.SetActive(false);

            //give reward
            switch (mission.rewardType)
            {
                case 1:
                    coins += mission.rewardAmount;
                    break;
                case 2:
                    gems += mission.rewardAmount;
                    break;
                case 3:
                    PlayerPrefs.SetInt(mission.rewardItem, 1);
                    //here is where shop button locked symbol would be removed
                    break;
            }
        }
        
        Destroy(mission);
        mission = gameObject.AddComponent<Mission>();
        Save();
    }

    void Update()
    {
        //Initial jump and revive jump
        if (gameState == GameState.Setup)
        {
            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    trail.GetComponent<PlayMakerFSM>().SendEvent("global on");
                    rb.bodyType = RigidbodyType2D.Dynamic;
                    rb.AddForce(new Vector2(0, 7 + defaultSpeed), ForceMode2D.Impulse);
                    gameState = GameState.Playing;
                    SetInvincible(3);
                    StartCoroutine(Clock());
                    clockActive = true;
                }
            }

        } //Game is set up

        if (gameState == GameState.Playing && !paused)
        {
            //Jump
            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    if (!inCannon)
                    {
                        if (canJump)
                        {
                            if (combo > 1 && activeCharm != 4) // extra combo not active and combo over 1
                            {
                                SetInvincible(combo - 1);
                                if (combo == 2)
                                    doubleCombos++;
                                else if (combo == 3)
                                    tripleCombos++;
                                else if (combo >= 4)
                                    quadCombos++;
                                combo = 0;
                            }
                            else if (combo > 1 && activeCharm == 4) // extra combo active and combo over 1
                            {
                                if (extraComboClick) // extra click used
                                {
                                    {
                                        SetInvincible(combo - 1);
                                        if (combo == 2)
                                            doubleCombos++;
                                        else if (combo == 3)
                                            tripleCombos++;
                                        else if (combo >= 4)
                                            quadCombos++;
                                    }
                                    combo = 0;
                                    extraComboClick = false;
                                }
                                else // give extra click
                                {
                                    extraComboClick = true;
                                    PlaySound(extraClick, .8f, 1);
                                }
                            }
                            else
                            {
                                combo = 0;
                            }

                            PlaySound(jump, .5f, Random.Range(.7f, .8f));
                            rb.velocity = new Vector2(0, 0);
                            rb.AddForce(new Vector2(0, 7 + normalSpeed), ForceMode2D.Impulse);
                        } // can jump
                    } // not in cannon
                    else // cannon launch, works if you cant jump
                    {
                        trail.GetComponent<PlayMakerFSM>().SendEvent("global on");
                        PlaySound(cannonShoot, 1, 1);
                        player.parent = null;
                        if (cannon.rotation.z > 0)
                            direction = -1;
                        else
                            direction = 1;
                        rb.bodyType = RigidbodyType2D.Dynamic;
                        StartCoroutine(CannonLaunch(1));
                    }
                }
            }

            //Horizontal Movement
            normalSpeed = defaultSpeed + (Mathf.Abs(player.position.y) / 300); // speed increase rate, also how long a run is

            if (overrideSpeed != 0)
                currentSpeed = direction * overrideSpeed;
            else
                currentSpeed = direction * normalSpeed;

            if (!inCannon)
            {
                if (canMove)
                    rb.velocity = new Vector2(currentSpeed, rb.velocity.y);

                //Fall Detector
                if (rb.velocity.y < 0)
                {
                    rb.gravityScale = 3;
                    if (rb.velocity.y < -12)
                        rb.velocity = new Vector2(rb.velocity.x, -12); // cap fall speed
                }
                else
                {
                    rb.gravityScale = 2;
                }
            }

            //Camera Follow
            cameraPosY = Mathf.Lerp(Camera.main.transform.position.y, player.position.y + 2.5f, 0.05f);
            Camera.main.transform.position = new Vector3(0, cameraPosY, -10);
            

            //Hazard Follow
            hazardWalls.position = new Vector2(0, player.position.y);

            //Spawn Walls
            if (player.position.y > nextWallHeight)
            {
                SpawnWalls(1 - (normalSpeed / 10)); // chance between 0 and 1
                nextWallHeight += 2;
            }

            //Height Tracker
            heightText.text = player.position.y.ToString("F0") + "m";
            if (player.position.y > maxHeight)
                maxHeight = player.position.y;

            //Combo
            if (combo > highestCombo)
                highestCombo = combo;

            //New Highscore
            if (!newHighScore && player.position.y > highscoreMarker.position.y)
            {
                confetti.Play();
                newHighScore = true;
            }

        } //Game is playing
    } //Update

    //Spawns walls, obstacles, and collectibles above the player at a certain height
    private void SpawnWalls(float chance)
    {
        float randomValue = Random.value;
        if (!preventDoubleObstacle)
        {
            if (randomValue < 0.05f && (leftWallCounter <= 2 || rightWallCounter <= 2)) // 5% chance to spawn hazard on left or right, wont spawn after gap of 3
            {
                chance = 1;
                preventDoubleObstacle = true;
                if (Random.value < 0.5f)
                    Reuse(normalHazardPool, new Vector3(-1, nextWallHeight + 8, 0), Quaternion.identity);
                else
                    Reuse(normalHazardPool, new Vector3(1, nextWallHeight + 8, 0), Quaternion.identity);
            }
            else if (randomValue > 0.95f) // if hazard does not spawn, 5% chance for booster to spawn
            {
                Reuse(boosterPool, new Vector3(Random.Range(-1, 2), nextWallHeight + 9, 0), Quaternion.identity);
                preventDoubleObstacle = true;
            }
            else if (randomValue < 0.1f) // if hazard and booster do not spawn, 5% chance for power-up to spawn
            {
                GameObject powerUpObject = Reuse(collectiblePool, new Vector3(Random.Range(-1, 2), nextWallHeight + 9, 0), Quaternion.identity);
                if (powerUpObject != null)
                {
                    powerUpObject.GetComponent<Collectible>().SetPowerUp();
                    preventDoubleObstacle = true;
                }
            }
            else if (randomValue > 0.9f) // if nothing else, 5% rotating hazard
            {
                Reuse(rotHazardPool, new Vector3(0, nextWallHeight + 7, 0), Quaternion.identity);
                preventDoubleObstacle = true;
            }
            else if (randomValue < 0.15f && PlayerPrefs.GetInt("Power-Ups4unlocked") == 1) // if nothing else, 5% canon if unlocked
            {
                Reuse(cannonPool, new Vector3(0, nextWallHeight + 8, 0), Quaternion.identity);
                preventDoubleObstacle = true;
            }
        }
        else
            preventDoubleObstacle = false;

        // chance of spawning wall decreases with height
        if (Random.value < chance || rightWallCounter > 2)
        {
            Reuse(wallPool, new Vector3(2, nextWallHeight + 8, 0), Quaternion.identity);
            rightWallCounter = 0;
        }
        else
            rightWallCounter++;

        if (Random.value < chance || leftWallCounter > 2)
        {
            Reuse(wallPool, new Vector3(-2, nextWallHeight + 8, 0), Quaternion.identity);
            leftWallCounter = 0;
        }
        else
            leftWallCounter++;

        if (randomValue > 0.8 && !preventDoubleObstacle) // 10% chance for coin to spawn if nothing else spawned
        {
            GameObject coinObject = Reuse(collectiblePool, new Vector3(Random.Range(-1, 2), nextWallHeight + 8, 0), Quaternion.identity);
            if (coinObject != null)
                coinObject.GetComponent<Collectible>().SetCoin();
        }
    }

    //Horizontal bounce that occurs when you hit a bounce wall
    public void Bounce()
    {
        if (canMove)
            rb.velocity = new Vector2(0, 0);
        rb.AddForce(new Vector2(0, (7 + normalSpeed) * 1.5f), ForceMode2D.Impulse);
        SetDisableJump(0.2f);
        DOTween.To(() => normalSpeed * 3, x => overrideSpeed = x, normalSpeed, 0.25f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => overrideSpeed = 0);  
    }

    //Makes the player invincible for a duration
    public void SetInvincible(float time)
    {
        if (invincibleCoroutine != null)
        {
            if ((invincibleDuration - (Time.time - invincibleStartTime)) < time) // if remaining time is < new time, start new coroutine
            {
                StopCoroutine(invincibleCoroutine);
                invincibleCoroutine = StartCoroutine(Invincible(time));
            }
        }
        else
            invincibleCoroutine = StartCoroutine(Invincible(time));
    }

    private IEnumerator Invincible(float wait)
    {
        if (!invincible)
            PlaySound(invincibleStart, 1, 1);

        invincibleStartTime = Time.time;
        invincibleDuration = wait;

        //float elapsedTime = 0f;
        invincible = true;

        yield return new WaitForSeconds(wait);

        /*while (elapsedTime < wait)
        {
            elapsedTime += Time.deltaTime;
            invincible = true; // Ensure it's set to true every frame
            yield return null; // Wait until the next frame
        }*/

        invincible = false; // Set to false after the wait time has elapsed
    }

    //Makes the player unable to jump for a duration
    public void SetDisableJump(float time)
    {
        StartCoroutine(DisableJump(time));
    }

    private IEnumerator DisableJump(float wait)
    {
        float elapsedTime = 0f;
        canJump = false;

        while (elapsedTime < wait)
        {
            elapsedTime += Time.deltaTime;
            canJump = false;
            yield return null;
        }

        canJump = true;
    }

    private IEnumerator CannonLaunch(float wait)
    {
        Vibrate();
        Collider2D collider = cannon.GetComponent<Collider2D>();
        collider.enabled = false;
        inCannon = false;

        cannon.GetChild(1).gameObject.SetActive(true); // enable force which disables itself
        //rb.AddForce(new Vector2(0, 50), ForceMode2D.Impulse);

        float elapsedTime = 0f;
        SetInvincible(wait + 1);
        SetDisableJump(wait);

        while (elapsedTime < wait)
        {
            elapsedTime += Time.deltaTime;
            canMove = false;
            yield return null; // Wait until the next frame
        }

        canMove = true;
        collider.enabled = true;
    }

    //Pause game
    public void Pause()
    {
        if (gameState == GameState.Playing)
        {
            if (Time.timeScale == 0)
            {
                paused = false;
                Time.timeScale = timeScaleBeforePause;
            }
            else
            {
                pauseMenu.SetActive(true);
                timeScaleBeforePause = Time.timeScale;
                Time.timeScale = 0;
                paused = true;
            }
        }
    }

    //Saves playerprefs after game. Updates coins, gems, shop text, and highscore stuff
    private void Save()
    {
        PlayerPrefs.SetInt("coins", coins);
        PlayerPrefs.SetInt("gems", gems);
        GameObject.Find("Shop Coin Text").GetComponent<TextMeshProUGUI>().text = ": " + coins;
        GameObject.Find("Shop Gem Text").GetComponent<TextMeshProUGUI>().text = ": " + gems;
        if (highscore < maxHeight)
        {
            highscore = maxHeight;
            PlayerPrefs.SetFloat("highscore", maxHeight);
            GameObject.Find("Highscore Text").GetComponent<TextMeshProUGUI>().text = "Highscore:\n" + maxHeight.ToString("F0") + "m";
            highscoreMarker.position = new Vector2(0, highscore);
        }
    }

    //Called when player attempts to purchase something with in game currency
    public bool Spend(int currency, int cost, bool complete)
    {
        if (currency == 1) // coins
        {
            if (coins < cost)
                return false;
            else
            {
                if (complete)
                {
                    coins -= cost;
                    GameObject.Find("Shop Coin Text").GetComponent<TextMeshProUGUI>().text = ": " + coins;
                    PlayerPrefs.SetInt("coins", coins);
                }
                return true;
            }
        }
        else // gems
        {
            if (gems < cost)
                return false;
            else
            {
                if (complete)
                {
                    gems -= cost;
                    spentGems += cost;
                    PlayerPrefs.SetInt("spentgems", spentGems);
                    GameObject.Find("Shop Gem Text").GetComponent<TextMeshProUGUI>().text = ": " + gems;
                    PlayerPrefs.SetInt("gems", gems);
                }
                return true;
            }
        }
    }

    //Called by pressing shop buttons
    public void ShopButton()
    {
        StartCoroutine(ShopButtonPressed(null));
    }

    private IEnumerator ShopButtonPressed(GameObject lastClicked)
    {
        promptResponded = false;
        if (lastClicked == null)
            lastClicked = EventSystem.current.currentSelectedGameObject;
        string category = lastClicked.transform.parent.name;
        int price = 0;
        switch (category)
        {
            case "Skins":
                price = skinPrice;
                break;

            case "Trails":
                price = trailPrice;
                break;

            case "Themes":
                price = themePrice;
                break;

            case "Power-Ups":
                price = powerUpPrice;
                break;

            case "Charms":
                price = charmPrice;
                break;
        }

        if (PlayerPrefs.GetInt(lastClicked.transform.parent.name + lastClicked.name + "unlocked") == 1)
        {
            // selecting unlocked
            switch (category)
            {
                case "Skins":
                    CreatePrompt("Equip Skin?", false, "", lastClicked.transform.GetChild(0).GetComponent<Image>().sprite);

                    yield return new WaitUntil(() => promptResponded);
                    shopPrompt.gameObject.SetActive(false);
                    if (promptResponse)
                    {
                        lastClicked.transform.parent.GetChild(activeSkin).GetComponent<Image>().color = Color.white;
                        lastClicked.GetComponent<Image>().color = Color.grey;
                        activeSkin = int.Parse(lastClicked.name);
                        PlayerPrefs.SetInt("activeskin", activeSkin);
                    }
                    break;

                case "Trails":
                    CreatePrompt("Equip Trail?", false, "", lastClicked.transform.GetChild(0).GetComponent<Image>().sprite);

                    yield return new WaitUntil(() => promptResponded);
                    shopPrompt.gameObject.SetActive(false);
                    if (promptResponse)
                    {
                        lastClicked.transform.parent.GetChild(activeTrail).GetComponent<Image>().color = Color.white;
                        lastClicked.GetComponent<Image>().color = Color.grey;
                        activeTrail = int.Parse(lastClicked.name);
                        PlayerPrefs.SetInt("activetrail", activeTrail);
                    }
                    break;

                case "Themes":
                    CreatePrompt("Equip Theme?", false, "", lastClicked.transform.GetChild(0).GetComponent<Image>().sprite);

                    yield return new WaitUntil(() => promptResponded);
                    shopPrompt.gameObject.SetActive(false);
                    if (promptResponse)
                    {
                        lastClicked.transform.parent.GetChild(activeTheme).GetComponent<Image>().color = Color.white;
                        lastClicked.GetComponent<Image>().color = Color.grey;
                        activeTheme = int.Parse(lastClicked.name);
                        PlayerPrefs.SetInt("activetheme", activeTheme);
                    }
                    break;

                case "Power-Ups":
                    CreatePrompt("Do You Understand?", true, powerUpDescriptions[int.Parse(lastClicked.name)], lastClicked.transform.GetChild(0).GetComponent<Image>().sprite);

                    yield return new WaitUntil(() => promptResponded);
                    shopPrompt.gameObject.SetActive(false);
                    break;

                case "Charms":
                    CreatePrompt("Equip Charm?", true, charmDescriptions[int.Parse(lastClicked.name)], lastClicked.transform.GetChild(0).GetComponent<Image>().sprite);

                    yield return new WaitUntil(() => promptResponded);
                    shopPrompt.gameObject.SetActive(false);
                    if (promptResponse)
                    {
                        lastClicked.transform.parent.GetChild(Mathf.Clamp(activeCharm - 1, 0, 5)).GetComponent<Image>().color = Color.white;
                        lastClicked.GetComponent<Image>().color = Color.grey;
                        activeCharm = int.Parse(lastClicked.name) + 1;
                        PlayerPrefs.SetInt("activecharm", activeCharm);
                    }
                    break;
            }

        }
        else if (Spend(2, price, false))
        {
            // can purchase
            switch (category)
            {
                case "Skins":
                    CreatePrompt("Buy Skin for " + price.ToString() + " Gems?", false, "", lastClicked.transform.GetChild(0).GetComponent<Image>().sprite);
                    break;

                case "Trails":
                    CreatePrompt("Buy Trail for " + price.ToString() + " Gems?", false, "", lastClicked.transform.GetChild(0).GetComponent<Image>().sprite);
                    break;

                case "Themes":
                    CreatePrompt("Buy Theme for " + price.ToString() + " gems?", false, "", lastClicked.transform.GetChild(0).GetComponent<Image>().sprite);
                    break;

                case "Power-Ups":
                    CreatePrompt("Buy Power-Up for " + price.ToString() + " gems?", true, powerUpDescriptions[int.Parse(lastClicked.name)], lastClicked.transform.GetChild(0).GetComponent<Image>().sprite);
                    break;

                case "Charms":
                    CreatePrompt("Buy Charm for " + price.ToString() + " gems?", true, charmDescriptions[int.Parse(lastClicked.name)], lastClicked.transform.GetChild(0).GetComponent<Image>().sprite);
                    break;
            }

            yield return new WaitUntil(() => promptResponded);
            if (promptResponse)
            {
                Spend(2, price, true);
                PlayerPrefs.SetInt(lastClicked.transform.parent.name + lastClicked.name + "unlocked", 1);
                lastClicked.transform.GetChild(1).gameObject.SetActive(false);
                StartCoroutine(ShopButtonPressed(lastClicked));
            }
            else
                shopPrompt.gameObject.SetActive(false);
        }
        else
        {
            // locked and cannot buy
            switch (category)
            {
                case "Skins":
                    CreatePrompt("Cannot Afford Skin", false, "", lastClicked.transform.GetChild(0).GetComponent<Image>().sprite);
                    break;

                case "Trails":
                    CreatePrompt("Cannot Afford Trail", false, "", lastClicked.transform.GetChild(0).GetComponent<Image>().sprite);
                    break;

                case "Themes":
                    CreatePrompt("Cannot Afford Theme", false, "", lastClicked.transform.GetChild(0).GetComponent<Image>().sprite);
                    break;

                case "Power-Ups":
                    CreatePrompt("Cannot Afford Power-Up", true, powerUpDescriptions[int.Parse(lastClicked.name)], lastClicked.transform.GetChild(0).GetComponent<Image>().sprite);
                    break;

                case "Charms":
                    CreatePrompt("Cannot Afford Charm", true, charmDescriptions[int.Parse(lastClicked.name)], lastClicked.transform.GetChild(0).GetComponent<Image>().sprite);
                    break;
            }

            yield return new WaitUntil(() => promptResponded);
            shopPrompt.gameObject.SetActive(false);
        }
    }

    public void PromptResponseYes()
    {
        promptResponse = true;
        promptResponded = true;
    }

    public void PromptResponseNo()
    {
        promptResponse = false;
        promptResponded = true;
    }

    private void CreatePrompt(string question, bool incAddInfo, string addInfo, Sprite sprite)
    {
        shopPrompt.GetChild(1).GetComponent<TextMeshProUGUI>().text = question;
        if (incAddInfo)
        {
            shopPrompt.GetChild(2).GetChild(1).GetComponent<TextMeshProUGUI>().text = addInfo;
            shopPrompt.GetChild(2).GetChild(1).gameObject.SetActive(true);
        }
        else
            shopPrompt.GetChild(2).GetChild(1).gameObject.SetActive(false);
        shopPrompt.GetChild(2).GetChild(0).GetComponent<Image>().sprite = sprite;
        shopPrompt.gameObject.SetActive(true);
    }

    public IEnumerator CoinTrail(float wait)
    {
        coinTrailActive = true;
        StartCoroutine(SpawnCoin(.2f));
        yield return new WaitForSeconds(wait);
        coinTrailActive = false;
    }

    private IEnumerator SpawnCoin(float wait)
    {
        if (runHolder != null)
        {
            GameObject coin = Reuse(collectiblePool, new Vector3(Random.Range(-1f, 2f), nextWallHeight + 8, 0), Quaternion.identity);
            if (coin != null)
            {
                coin.GetComponent<Collectible>().SetCoin();
                coin.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
            }
            yield return new WaitForSeconds(wait);
            if (coinTrailActive)
                StartCoroutine(SpawnCoin(.2f));
        }
    }

    public void ConvertToGem()
    {
        if(Spend(1, 100, true))
        {
            gems += 1;
            PlayerPrefs.SetInt("gems", gems);
            GameObject.Find("Shop Gem Text").GetComponent<TextMeshProUGUI>().text = ": " + gems;
        }
    }

    //Sound effect
    public void PlaySound(AudioClip clip, float volume, float pitch)
    { 
        AudioSource soundEffect = Reuse(SFXPool, new Vector3(), Quaternion.identity).GetComponent<AudioSource>();
        if (soundEffect != null)
        {
            soundEffect.clip = clip;
            soundEffect.volume = volume;
            soundEffect.pitch = pitch;
            soundEffect.Play();
        }
    }

    public void SoloActivate(GameObject child)
    {
        Transform parent = child.transform.parent;
        if (parent != null)
        {
            foreach (Transform sibling in parent)
                sibling.gameObject.SetActive(false);
        }
        child.SetActive(true);
    }

    public PlayMakerFSM FindFSM(GameObject gameObject, string name)
    {
        // Get all PlayMakerFSM components on the GameObject
        PlayMakerFSM[] fsms = gameObject.GetComponents<PlayMakerFSM>();

        // Loop through each FSM to find the one with the specified name
        foreach (PlayMakerFSM fsm in fsms)
        {
            if (fsm.FsmName == name)
            {
                return fsm; // Return the FSM if the name matches
            }
        }

        return null; // Return null if no FSM was found with the specified name
    }

    public void Vibrate()
    {
        if (vibrationEnabled)
            Handheld.Vibrate();
    }

    // Settings

    public void VibrateToggle()
    {
        if (GameObject.Find("Vibrate Toggle").GetComponent<Toggle>().isOn)
        {
            PlayerPrefs.SetInt("vibrationenabled", 0);
            vibrationEnabled = true;
        }
        else
        {
            PlayerPrefs.SetInt("vibrationenabled", 1);
            vibrationEnabled = false;
        }
    }

    public void SetSFXVolume()
    {
        if (sfxSlider.value == 0)
            audioMixer.SetFloat("sfxVolume", -80);
        else
            audioMixer.SetFloat("sfxVolume", Mathf.Log10(sfxSlider.value) * 20); // Adjust SFX channel
    }

    public void SetMusicVolume()
    {
        if (musicSlider.value == 0)
            audioMixer.SetFloat("musicVolume", -80);
        else
            audioMixer.SetFloat("musicVolume", Mathf.Log10(musicSlider.value) * 20); // Adjust Music channel
    }

    // called when exiting settings menu
    public void SaveSettings()
    {
        PlayerPrefs.SetFloat("sfxvolume", sfxSlider.value);
        PlayerPrefs.SetFloat("musicvolume", musicSlider.value);
    }

    public void EnableAds()
    {
        adsDisabled = false;
        PlayerPrefs.SetInt("adsdisabled", 0);
    }

    public void IAP(Product product)
    {
        Debug.Log("IAP Called");
        switch (product.definition.id)
        {
            case "gems100":
                AddGems(100);
                break;
            case "gems300":
                AddGems(300);
                break;
            case "gems700":
                AddGems(700);
                break;
            case "disableads":
                adsDisabled = true;
                PlayerPrefs.SetInt("adsdisabled", 1);
                ///disable remove ad button if they are already disabled from gem purchase
                break;
        }
    }

    private void AddGems(int count)
    {
        Debug.Log("AddGems Called");
        gems += count;
        GameObject.Find("Shop Gem Text").GetComponent<TextMeshProUGUI>().text = ": " + gems;
        PlayerPrefs.SetInt("gems", gems);
        if (!adsDisabled)
        {
            adsDisabled = true;
            PlayerPrefs.SetInt("adsdisabled", 1);
        }
    }
    
    public void SetTrailRenderer(TrailRenderer trail, bool emitting)
    {
        trail.emitting = emitting;
    }

    //Use an object pool to get and prepare the next object
    public GameObject Reuse(ObjectPool pool, Vector3 pos, Quaternion rot)
    {
        GameObject entity = pool.GetPooledObject();
        if (entity != null)
        {
            entity.transform.position = pos;
            entity.transform.rotation = rot;
            //entity.transform.parent = parent;
            entity.SetActive(true);
            return entity;
        }
        else
            return null;
    }

    public void InvincibleTrail()
    {
        Reuse(invincibleTrailPool, player.position, Quaternion.identity);
    }

} //GameController