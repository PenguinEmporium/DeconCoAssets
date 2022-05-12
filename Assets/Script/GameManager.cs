using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{

    [SerializeField] int targetScore;
    [SerializeField] int currentScore;
    [SerializeField] int highScore;

    [SerializeField] public int lives = 3;
    [SerializeField] public int numOfBombs = 3;

    [SerializeField] float bombRegenRate = 50f;
    [SerializeField] float bombPercentage = 300;
    [SerializeField] public float blockSpeedDifficultyRate = 0.004f;

    public float currentRoundTimeInSeconds = 0f;


    float nextSpawnTime = 3f;

    float timeBetweenBlockSpawns = 5f;

    bool playerAlive = true;
    bool respawningPlayer = false;
    bool checkingWin = false;

    UIManager uiManager;

    GridManager grid;

    AudioManager audioManager;

    //0-4 (1-5)
    int currentSpot = 0;
    //0-3 (0, 90, 180, 270)
    int currentAngle = 0;
    //(0-5) (single, 2-line, 3-line, 3-L, Square)
    int currentBlock = 0;
    bool[] blocksUsed;
    [SerializeField] BlockPattern[] blockPatterns;

    bool isProcessingMenus = false;
    bool receivingBlocks = true;

    public enum GameState
    {
        MainMenu,
        LoadingGame,
        InGame,
        GameWin,
        GameOver
    }

    public enum GameType
    {
        Infinite,
        ScoreMore
    }

    Coroutine gamePrep;

    [SerializeField] GameState currentGameState;

    GameType currentGameType;

    GameObject playerGO;
    PlayerControl playerControl;
    public GameObject playerDeathObject;
    public GameObject respawnPoint;
    Coroutine playerRespawn;

    int reserve = 0;

    public void Awake()
    {
        audioManager = GetComponent<AudioManager>();

        audioManager.PlayLoop("Drums");
        audioManager.PlayLoop("Organ");
        audioManager.PlayLoop("Piano");

        audioManager.FadeLoop("Drums",2f,0.5f);

        blocksUsed = new bool[blockPatterns.Length];
        for(int b = 0; b < blockPatterns.Length; b++)
        {
            blocksUsed[b] = false;
        }

        currentGameType = GameType.Infinite;

        //Here for testing purposes
        ChangeGameState(GameState.MainMenu);

        playerGO = GameObject.FindGameObjectWithTag("Player");
        playerControl = playerGO.GetComponent<PlayerControl>();


        grid = FindObjectOfType<GridManager>();
        uiManager = FindObjectOfType<UIManager>();

        highScore = PlayerPrefs.GetInt("Highscore");

        uiManager.UpdateHighscore(highScore);

    }

    public void Update()
    {
        switch(currentGameState)
        {
            case GameState.MainMenu:
                //Play some menu animations
                break;
            case GameState.LoadingGame:
                playerControl.PlayerUpdate();

                break;
            case GameState.InGame:
                if (playerAlive)
                {
                    playerControl.PlayerUpdate();
                }

                //Run timer
                //Currently in coroutine but probably can be brought to here

                //Check if time to give the player a new bomb
                ProcessBombs();

                //Spawn new blocks as appropriately
                ProcessBlocks();

                //Keep a watch on score

                //If player is close to losing, warn them. If they did a big combo, cheer

                break;
            case GameState.GameWin:
                //Victory dance
                //Calculate final score
                //Reset meter for next level
                //Give player option for prompt

                break;
            case GameState.GameOver:
                //Start gameOverTimer
                //If timer is up, send back to main menu
                //Respawn player for reset prompt
                break;
        }

        if(Input.GetKeyDown(KeyCode.Escape) && currentGameState == GameState.InGame)
        {
            CheckWin();
        }
    }

    private void ChangeGameState(GameState newState)
    {
        currentGameState = newState;
        switch (currentGameState)
        {
            case GameState.MainMenu:
                Cursor.visible = true;
                audioManager.FadeLoop("Drums", 0.5f, 0.5f);
                audioManager.FadeLoop("Piano", 0.5f, 0f);
                audioManager.FadeLoop("Organ", 0.5f, 0f);
                break;
            case GameState.LoadingGame:
                Cursor.visible = false;

                break;
            case GameState.InGame:
                audioManager.FadeLoop("Drums", 0.5f, 0f);
                audioManager.FadeLoop("Organ", 0.5f, 0f);
                audioManager.FadeLoop("Piano", 0.5f, 0.5f);
                StartCoroutine(GameTimer());
                break;
            case GameState.GameWin:
                audioManager.FadeLoop("Drums", 0.5f, 0f);
                audioManager.FadeLoop("Organ", 0.5f, 0f);
                audioManager.FadeLoop("Piano", 0.5f, 0f);
                GameWin();
                break;
            case GameState.GameOver:
                audioManager.FadeLoop("Drums", 0.5f, 0f);
                audioManager.FadeLoop("Organ", 0.5f, 0f);
                audioManager.FadeLoop("Piano", 0.5f, 0f);
                GameOver();
                break;
        }
    }

    void GameWin()
    {
        StartCoroutine(IE_GameWin());
    }

    IEnumerator IE_GameWin()
    {
        //Use this to clear the field
        receivingBlocks = false;
        grid.SpawnGreen().DestroyBlock();
        yield return new WaitWhile(() => respawningPlayer);
        yield return new WaitForSeconds(1.5f);
        receivingBlocks = true;
        playerGO.SetActive(true);
        playerGO.transform.SetPositionAndRotation(respawnPoint.transform.position, respawnPoint.transform.rotation);

        uiManager.SetStatus(true, "NEW RECORD!",4.5f, null);

        audioManager.Play("Win");

        playerGO.GetComponent<Animator>().SetBool("Win", true);

        PlayerPrefs.SetInt("Highscore", highScore);

        //Show NEW RECORD!

        //Go back to main menu

        yield return new WaitForSeconds(3f);
        uiManager.SetStatus(false, "",0, null);
        playerGO.GetComponent<Animator>().SetBool("Win", false);
        StartCoroutine(ReturnToMenu());
    }

    void GameOver()
    {
        StartCoroutine(IE_GameLose());
    }

    IEnumerator IE_GameLose()
    {
        //Use this to clear the field
        receivingBlocks = false;
        grid.SpawnGreen().DestroyBlock();
        yield return new WaitWhile(() => respawningPlayer);

        audioManager.Play("Lose");


        yield return new WaitForSeconds(1.5f);

        receivingBlocks = true;
        //Show GAME OVER
        uiManager.SetStatus(true, "GAME OVER",4f, null);

        yield return new WaitForSeconds(3f);
        uiManager.SetStatus(false, "", 0, null);

        StartCoroutine(ReturnToMenu());
    }

    IEnumerator GameTimer()
    {
        currentRoundTimeInSeconds = 0f;

        while(currentGameState == GameState.InGame)
        {
            currentRoundTimeInSeconds++;
            yield return new WaitForSeconds(1f);
        }

        //As the game goes on, slowly increase bomb respawn timer,increase block rate, or decrease red barrel rate.

    }

    public void PlayerKilled()
    {
        if(playerRespawn != null)
        {
            StopCoroutine(playerRespawn);
        }

        playerAlive = false;
        lives--;

        lives = Mathf.Clamp(lives, 0, 10);

        uiManager.SetLives(lives);

        if(lives <= 0)
        {
            CheckWin();
        }

        playerRespawn = StartCoroutine(RespawnTimer(true));
    }

    IEnumerator RespawnTimer(bool respawnPlayer)
    {
        respawningPlayer = true;
        var deadPlayer = Instantiate(playerDeathObject, playerGO.transform.position, playerGO.transform.rotation);
        playerGO.SetActive(false);

        yield return new WaitForSeconds(0.5f);

        //Clear board
        //Make sure the player doesn't get points for the death. Or do and make it tech
        receivingBlocks = false;
        grid.SpawnGreen().DestroyBlock();

        yield return new WaitForSeconds(1f);
        receivingBlocks = true;
        //Set the player position to middle of board
        playerGO.transform.SetPositionAndRotation(respawnPoint.transform.position, respawnPoint.transform.rotation);

        playerGO.SetActive(respawnPlayer);
        playerAlive = respawnPlayer;

        //Make the player flash into being
        SpriteRenderer playerSR = playerGO.GetComponent<SpriteRenderer>();

        yield return new WaitForSeconds(2f);
        Destroy(deadPlayer);
        respawningPlayer = false;
    }

    public void WarningLineHit()
    {
        CheckWin();
    }

    void CheckWin()
    {
        if (checkingWin)
            return;

        checkingWin = true;

        if (currentGameType == GameType.Infinite)
        {
            if (currentScore > highScore)
            {
                highScore = currentScore;
                PlayerPrefs.SetFloat("Highscore", highScore);
                uiManager.UpdateHighscore(highScore);
                ChangeGameState(GameState.GameWin);
            }
            else
            {
                ChangeGameState(GameState.GameOver);
            } 
        }

    }

    public void PullOutBomb()
    {
        numOfBombs--;
        bombPercentage -= 100f;
        //Update UI;

        ProcessBombs();
    }

    void ProcessBombs()
    {
        bombPercentage += bombRegenRate * Time.deltaTime;

        bombPercentage = Mathf.Clamp(bombPercentage, 0f, 300f);

        numOfBombs = Mathf.FloorToInt(bombPercentage/100);

        uiManager.UpdateBombProgress(bombPercentage);
    }

    void ProcessBlocks()
    {
        if (currentRoundTimeInSeconds > nextSpawnTime)
        {
            nextSpawnTime = currentRoundTimeInSeconds + timeBetweenBlockSpawns;

            bool reset = true;
            for (int b = 0; b < blockPatterns.Length; b++)
            {
                if (!blocksUsed[b])
                {
                    reset = false;
                    break;
                }
            }

            if (reset)
            {
                for (int b = 0; b < blockPatterns.Length; b++)
                {
                    blocksUsed[b] = false;
                }
            }


            bool canSpawn = false;
            while (!canSpawn)
            {
                currentBlock = Random.Range(0, 5);
                if (!blocksUsed[currentBlock])
                {
                    canSpawn = true;
                    blocksUsed[currentBlock] = true;
                }
            }

            currentSpot++;
            if (currentSpot > 4)
            {
                currentSpot -= 4;
            }
            currentAngle++;
            if (currentAngle > 3)
            {
                currentAngle = 0;
            }


            grid.SpawnBlock(currentSpot, blockPatterns[currentBlock], currentAngle);
        }
    }

    IEnumerator ReturnToMenu()
    {
        StopCoroutine(gamePrep);

        isProcessingMenus = true;
        uiManager.SetMenu(true);
        yield return new WaitForSeconds(1.5f);
        ChangeGameState(GameState.MainMenu);
        isProcessingMenus = false;
    }
    
    public void OnBlockDestroyed(Block blockDestroyed)
    {
        if (!receivingBlocks)
            return;

        //Just get the points value and do some basic calculations

        currentScore += blockDestroyed.pointValue;

        //if(!uiManager.processingScore)
        {
            //currentScore += reserve;
            reserve = 0;
            uiManager.SetScore(currentScore);
        }

        if (currentScore > highScore)
        {
            //if (!uiManager.processingHighscore)
            {
                uiManager.SetHighscore(currentScore);
            }
        }
    }

    public void PrepareGame()
    {
        if (!isProcessingMenus)
        {
            ChangeGameState(GameState.LoadingGame);

            isProcessingMenus = true;

            currentScore = 0;
            lives = 3;

            nextSpawnTime = timeBetweenBlockSpawns;

            currentRoundTimeInSeconds = 0f;

            checkingWin = false;
            uiManager.SetStatus(false, "", 0, null);
            uiManager.SetScore(currentScore);
            uiManager.SetLives(lives);
            uiManager.SetHighscore(highScore);

            gamePrep = StartCoroutine(IE_GamePrep());
        }
    }

    IEnumerator IE_GamePrep()
    {
        uiManager.SetMenu(false);

        yield return new WaitForSeconds(2.5f);

        //Show a READY, START!
        uiManager.SetStatus(true, "Ready?",3f, null);

        yield return new WaitForSeconds(1.5f);

        uiManager.SetStatus(true, "Go!",3f, null);

        ChangeGameState(GameState.InGame);

        yield return new WaitForSeconds(1.5f);

        uiManager.SetStatus(false, "",0, null);

        isProcessingMenus = false;
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ResetHighscore()
    {
        PlayerPrefs.SetInt("Highscore", 0);
        highScore = 0;
        uiManager.UpdateHighscore(0);
    }

    public void SetMouseLook(bool enabled)
    {
        playerControl.SetMouseLook(enabled);
    }

    public void ButtonPressed()
    {
        audioManager.Play("Select");
    }
}
