using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public enum GameState
    {
        Gameplay,
        Paused,
        GameOver,
        LevelUp,
        TreasureChest
    }

    PlayerCollector collector;

    public GameState currentState;
    public GameState previousState;


    [Header("Damage Text Settings")]
    public Canvas damageTextCanvas;
    public float textFontSize = 20;
    public TMP_FontAsset textFont;
    public Camera referenceCamera;


    [Header("Screens")]
    public GameObject pauseScreen;
    public GameObject resultsScreen;
    public GameObject levelUpScreen;
    int stackedLevelUps = 0;

    [Header("Current Stat Displays")]
    public TextMeshProUGUI currentHealthDisplay;
    public TextMeshProUGUI currentRecoveryDisplay;
    public TextMeshProUGUI currentMoveSpeedDisplay;
    public TextMeshProUGUI currentMightDisplay;
    public TextMeshProUGUI currentProjectileSpeedDisplay;
    public TextMeshProUGUI currentMagnetDisplay;

    [Header("Results Screen Displays")]
    public Image chosenCharacterImage;
    public TextMeshProUGUI chosenCharacterName;
    public TextMeshProUGUI levelReachedDisplay;
    public TextMeshProUGUI timeSurvivedDisplay;

    const float DEFAULT_TIME_LIMIT = 1800f;
    const float DEFAULT_CLOCK_SPEED = 1f;
    float ClockSpeed => UILevelSelector.currentLevel?.clockSpeed ?? DEFAULT_CLOCK_SPEED;
    float TimeLimit => UILevelSelector.currentLevel?.timeLimit ?? DEFAULT_TIME_LIMIT;

    [Header("Stopwatch")]
    public float timeLimit; // time limit in seconds
    private float stopwatchTime; //the current time elapsed since stopwatch started
    public TextMeshProUGUI stopwatchDisplay;

    bool levelEnded = false; // has the time limit been reached?
    public GameObject reaperPrefab; // spawns after time limit has been reached


    // flag to check if game is over
    public bool isGameOver { get { return currentState == GameState.GameOver; } }

    // flag to check if player is chosing their upgrades
    public bool chosingUpgrade { get { return currentState == GameState.LevelUp; } }

    public float GetElapsedTime() { return stopwatchTime; }

    //sums up the curse stat of all players and returns the value
    public static float GetCumulativeCurse()
    {
        if (!instance) return 1;

        float totalCurse = 0;
        foreach(PlayerStats p in instance.players)
        {
            totalCurse += p.Actual.curse;
        }
        return Mathf.Max(1, 1 + totalCurse);
    }

    //sum up the levels of all players and returns the value
    public static int GetCumulativeLevels()
    {
        if(!instance) return 1;
        int totalLevel = 0;
        foreach(PlayerStats p in instance.players)
        {
            totalLevel += p.level;
        }
        return Mathf.Max(1, totalLevel);
    }

    PlayerStats[] players; //tracks all players

    private void Awake()
    {
        collector = FindObjectOfType<PlayerCollector>();
        players = FindObjectsOfType<PlayerStats>();
        //set the level's time limit
        timeLimit = TimeLimit;
        //WARNING CHECK IF THERE IS ANY SINGLETON OF THIS KIND IN THE GAME
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogWarning("EXTRA " + this + "DELETED");
        }
        DisableScreens();
    }
    private void Update()
    {
        switch (currentState)
        {
            case GameState.Gameplay:
                CheckForPauseAndResume();
                UpdateStopwatch();
                break;

            case GameState.Paused:
                CheckForPauseAndResume();
                break;
            case GameState.TreasureChest:
                Time.timeScale = 0f;
                break;

            case GameState.GameOver:
            case GameState.LevelUp:
                break;

            default:
                Debug.LogWarning("State Does Not Exist!");
                break;
        }
    }


    IEnumerator GenerateFloatingTextCoroutine(string text, Transform target, float duration = 1f, float speed = 50f)
    {
        //start generating the floating text
        GameObject textObj = new GameObject("Damage Floating Text");
        RectTransform rect = textObj.AddComponent<RectTransform>();
        TextMeshProUGUI tmPro = textObj.AddComponent<TextMeshProUGUI>();
        tmPro.text = text;
        tmPro.horizontalAlignment = HorizontalAlignmentOptions.Center;
        tmPro.verticalAlignment = VerticalAlignmentOptions.Middle;
        tmPro.fontSize = textFontSize;
        if (textFont) tmPro.font = textFont;
        rect.position = referenceCamera.WorldToScreenPoint(target.position);

        //parent the text object to the canvas
        textObj.transform.SetParent(instance.damageTextCanvas.transform);
        textObj.transform.SetSiblingIndex(0);

        //pan the text upwards and fade it away over time
        WaitForEndOfFrame w = new WaitForEndOfFrame();
        float t = 0f;
        float yOffset = 0f;
        Vector3 lastKnownPosition = target.position;
        while (t < duration)
        {
            //if the rect object is missing for whatever reason, terminate this loop
            if(!rect) break;

            // fade the text to the right alpha value
            tmPro.color = new Color(tmPro.color.r, tmPro.color.g, tmPro.color.b, 1-t / duration);

            //if target exists, then save it's position
            if(target) lastKnownPosition = target.position;

            // pan the text upwards
            yOffset += speed * Time.deltaTime;
            if (target != null)
            {
                rect.position = referenceCamera.WorldToScreenPoint(lastKnownPosition + new Vector3(0, yOffset));
            }
            else
            {
                // If target is destroyed, just move the text upward from its last known position
                rect.position = new Vector3(rect.position.x, rect.position.y + speed * Time.deltaTime, rect.position.z);
            }

            // wait for end of frame and update the time
            yield return w;
            t += Time.deltaTime;
        }

        //destroy the text object after the animation is complete
        if (textObj != null)
        {
            Destroy(textObj);
        }
    }
    public static void GenerateFloatingText(string text, Transform target, float duration = 1f, float speed = 1f)
    {
        // if the canvas is not set, end the function, so we don't generate any floating text.
        if (!instance.damageTextCanvas) return;

        //find the relevant camera that we can use to convert the world position to screen position
        if (!instance.referenceCamera) instance.referenceCamera = Camera.main;
        instance.StartCoroutine(instance.GenerateFloatingTextCoroutine(text, target, duration, speed));
    }

    public void ChangeState(GameState newState)
    {
        previousState = currentState;
        currentState = newState;
    }
    public void PauseGame()
    {
        if (currentState != GameState.Paused)
        {
            ChangeState(GameState.Paused);
            Time.timeScale = 0f;
            pauseScreen.SetActive(true);
        }
    }

    public void ResumeGame()
    {
        if (currentState == GameState.Paused)
        {
            ChangeState(previousState);
            Time.timeScale = 1f;
            pauseScreen.SetActive(false);
        }
    }

    public void CheckForPauseAndResume()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentState == GameState.Paused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void DisableScreens()
    {
        pauseScreen.SetActive(false);
        resultsScreen.SetActive(false);
        levelUpScreen.SetActive(false);
    }

    public void GameOver()
    {
        timeSurvivedDisplay.text = stopwatchDisplay.text;
        ChangeState(GameState.GameOver);
        Time.timeScale = 0f;
        DisplayResults();

        //save all the coins of all the players to the save file
        foreach(PlayerStats p in players)
        {
            p.GetComponentInChildren<PlayerCollector>().SaveCoinsToStash();
        }

        //add all the player's coins to their save file, since the game has ended
        foreach(PlayerStats p in players)
        {
            if(p.TryGetComponent(out PlayerCollector c))
            {
                c.SaveCoinsToStash();
            }
        }
    }

    public void DisplayResults()
    {
        resultsScreen.SetActive(true);
    }

    public void AssignChosenCharacterUI(CharacterData chosenCharacterData)
    {
        chosenCharacterImage.sprite = chosenCharacterData.Icon;
        chosenCharacterName.text = chosenCharacterData.Name;
    }

    public void AssignLevelReachedUI(int levelReachedData)
    {
        levelReachedDisplay.text = levelReachedData.ToString();
    }


    public Vector2 GetRandomPlayerLocation()
    {
        int chosenPlayer = Random.Range(0, players.Length);
        return new Vector2(players[chosenPlayer].transform.position.x, players[chosenPlayer].transform.position.y);
    }


    private void UpdateStopwatch()
    {
        stopwatchTime += Time.deltaTime * ClockSpeed;
        UpdateStopwatchDisplay();
        if(stopwatchTime >= timeLimit && !levelEnded)
        {
            levelEnded = true;

            //set the enemy/event spawner GameObject inactive to stop enemies from spawning and kill the remaining enemies onscreen
            FindObjectOfType<SpawnManager>().gameObject.SetActive(false);
            foreach (EnemyStats e in FindObjectsOfType<EnemyStats>())
                e.SendMessage("Kill");

            //spawn the reaper off-camera
            Vector2 reaperOffset = Random.insideUnitCircle * 50f;
            Vector2 spawnPosition = GetRandomPlayerLocation() + reaperOffset;
            Instantiate(reaperPrefab, spawnPosition, Quaternion.identity);
        }
    }
    private void UpdateStopwatchDisplay()
    {
        //calculate the number of minutes and seconds that have elapsed
        int minutes = Mathf.FloorToInt(stopwatchTime / 60);
        int secnds = Mathf.FloorToInt(stopwatchTime % 60);

        //update the stopwatch text to display elapsed time
        stopwatchDisplay.text = string.Format("{0:00}:{1:00}", minutes,secnds);
    }

    public void StartLevelUp()
    {
        ChangeState(GameState.LevelUp);

        if(levelUpScreen.activeSelf) stackedLevelUps++;
        else
        {
            levelUpScreen.SetActive(true);
            Time.timeScale = 0f;
            foreach(PlayerStats p in players)
            {
                p.SendMessage("RemoveAndApplyUpgrades");
            }
        }
    }

    public void EndLevelUp()
    {
        Time.timeScale = 1f;
        levelUpScreen.SetActive(false);
        ChangeState(GameState.Gameplay);

        if(stackedLevelUps > 0)
        {
            stackedLevelUps--;
            StartLevelUp();
        }
    }
}
