using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Obsolete("replaced by spawn manager")]
public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public string name;
        public List<EnemyGroup> enemyGroups; // a list of group of enemies to spawn in this wave
        public int waveQuota;  //the total number of enemies to spawn in this wave
        public float spawnInterval; //interval at which to spawn enemies
        public int spawnCount; // number of enemies allready spawned in this wave
    }

    [System.Serializable]
    public class EnemyGroup
    {
        public string name;
        public int enemyCount; // the number of enemies to spawn in this wave
        public int spawnCount; // the number of enemies of this type  allready spawned in this wave
        public GameObject enemyPrefab;
    }

    public List<Wave> waves; // a list of all the waves in the game.
    public int currentWaveCount; //the index of the current wave [remember, a list starts at 0]


    [Header("Spawner Attributes")]
    float spawnTimer; // timer use to determine when to spawn next enemy
    public int enemiesAlive;
    public int maxEnemiesAllowed; // maximum enemies allowed at once on the map
    public bool maxEnemiesReached = false;
    public float waveInterval; // the interval between each wave.
    bool isWaveActive = false;


    [Header("Spawn Positions")]
    public List<Transform> relativeSpawnPoints; //a list to store all the relative spawn points of enemies

    Transform player;
    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<PlayerStats>().transform;
        CalculateWaveQuota();
    }

    // Update is called once per frame
    void Update()
    {
        //check if the wave ended and next wave should start
        if(currentWaveCount < waves.Count && waves[currentWaveCount].spawnCount == 0 && !isWaveActive)
        {
            StartCoroutine(BeginNextWave());
        }


        spawnTimer += Time.deltaTime;

        // check if it's time to spawn next enemy.
        if(spawnTimer >= waves[currentWaveCount].spawnInterval)
        {
            spawnTimer = 0f;
            SpawnEnemies();
        }
    }

    IEnumerator BeginNextWave()
    {
        isWaveActive = true;
        yield return new WaitForSeconds(waveInterval); // wait for the interval before starting the next wave


        if (currentWaveCount < waves.Count - 1) // if there are more waves to spawn
        {
            isWaveActive = false;
            currentWaveCount++;
            CalculateWaveQuota();
        }
    }

    void CalculateWaveQuota()
    {
        int currentWaveQuota = 0;
        foreach(var enemyGroup in waves[currentWaveCount].enemyGroups)
        {
            currentWaveQuota += enemyGroup.enemyCount;
        }
        waves[currentWaveCount].waveQuota = currentWaveQuota;
    }


    /// <summary>
    /// this method will stop spawning enemies if the amount reaches max enemies allowed on the map
    /// the method will only spawn enemies in a particular wave until it is time for next wave's enemies to be spawned
    /// </summary>
    void SpawnEnemies()
    {
        //check if the minimum number of enemies in the wave have been spawned
        if (waves[currentWaveCount].spawnCount < waves[currentWaveCount].waveQuota && !maxEnemiesReached)
        {
            // spawn each type of enemy until quota is filled
            foreach(var enemyGroup in waves[currentWaveCount].enemyGroups)
            {
                //check if minimum number of enemies of this type have been spawned
                if (enemyGroup.spawnCount < enemyGroup.enemyCount)
                {

                    //spawn enemy at a random position close to the player
                    Instantiate(enemyGroup.enemyPrefab, player.position + relativeSpawnPoints[Random.Range(0, relativeSpawnPoints.Count)].position, Quaternion.identity);
                    enemyGroup.spawnCount++;
                    waves[currentWaveCount].spawnCount++;
                    enemiesAlive++;

                    // limit the number of enemies that can be spawned at once
                    if (enemiesAlive >= maxEnemiesAllowed)
                    {
                        maxEnemiesReached = true;
                        return;
                    }
                }
            }
        }
    }

    // call this method when enemy dies
    public void OnEnemyKilled()
    {
        // decrement the number of enemies allive
        enemiesAlive--;
        // reset maxEnemiesReached flag if the number of enemies alive droped bellow the maximum amount
        if (enemiesAlive < maxEnemiesAllowed)
        {
            maxEnemiesReached = false;
        }
    }
}
