using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    int currentWaveIndex;// the index of the current wave [remember, a list starts from 0]
    int currentWaveSpawnCount = 0; //tracks how many enemies current wave has spawned

    public WaveData[] data;
    public Camera referenceCamera;

    [Tooltip("if there are more than this number of enemies, stop spawning any more. for performance.")]
    public int maximumEnemyCount = 300;
    float spawnTimer; // timer used to determine when to spawn the next group of enemy
    float currentWaveDuration = 0f;
    public bool boostedByCurse = true;

    public static SpawnManager instance;


    private void Start()
    {
        if (instance) Debug.LogWarning("there is more thatn 1 spawn manager in the Scene! please remove extras");
        instance = this;
    }

    private void Update()
    {
        //updates the spawn timer at every frame
        spawnTimer -= Time.deltaTime;
        currentWaveDuration += Time.deltaTime;

        if (spawnTimer <= 0f)
        {
            //check if we are ready to move on to the new wave.
            if (HasWaveEnded())
            {
                currentWaveIndex++;
                currentWaveDuration = currentWaveSpawnCount = 0;

                //if we have gone trough all the waves, disable this component.
                if (currentWaveIndex >= data.Length)
                {
                    Debug.Log("all waves have been spawned! Shutting down.", this);
                    enabled = false;
                }

                return;
            }

            if (!CanSpawn())
            {
                ActivateCooldown();
                return;
            }

            //get the array of enemies that we are spawning for this tick.
            GameObject[] spawns = data[currentWaveIndex].GetSpawns(EnemyStats.count);

            //loop trough and spawn all the prefabs
            foreach (GameObject prefab in spawns)
            {
                if(!CanSpawn()) continue;

                //spawn the enemy
                Instantiate(prefab, GeneratePosition(), Quaternion.identity);
                currentWaveSpawnCount++;
            }

            ActivateCooldown();
        }
    }

    //resets the spawn interval
    public void ActivateCooldown()
    {
        float curseBoost = boostedByCurse ? GameManager.GetCumulativeCurse() : 1;
        spawnTimer += data[currentWaveIndex].GetSpawnInterval() / curseBoost;
    }

    //do we meet the conditions to be able to continue spawning?
    public bool CanSpawn()
    {
        //don't spawn anymore if we exceed the max limit
        if(HasExceededMaxEnemies()) return false;

        //don't spawn if we exceeded the max spawns for the wave
        if(instance.currentWaveSpawnCount > instance.data[instance.currentWaveIndex].totalSpawns) return false;

        //don't spawn if we exceeded the wave's duration
        if(instance.currentWaveDuration > instance.data[instance.currentWaveIndex].duration) return false;

        return true;
    }

    public static bool HasExceededMaxEnemies()
    {
        if (!instance) return false; // if there is no spawn manager, don't limit max enemies
        if(EnemyStats.count > instance.maximumEnemyCount) return true;
        return false;
    }

    public bool HasWaveEnded()
    {
        WaveData currentWave = data[currentWaveIndex];

        //if waveDuration is one of the exit conditions, check how long the wave has been running.
        //if current wave duration is not greater than wave duration, do not exit yet.
        if((currentWave.exitConditions & WaveData.ExitCondition.waveDuration) > 0)
            if(currentWaveDuration < currentWave.duration) return false;

        //otherwise, if kill all is checked, we have to make sure there are no more enemies first
        if (currentWave.mustKillAll && EnemyStats.count > 0)
            return false;


        return true;
    }

    private void Reset()
    {
        referenceCamera = Camera.main;
    }

    //creates a new location where we can place the enemy at.
    public static Vector3 GeneratePosition()
    {
        //if there is no reference camera, then get one.
        if(!instance.referenceCamera) instance.referenceCamera = Camera.main;

        //give a warning if the camera is not orthographic
        if (!instance.referenceCamera.orthographic)
            Debug.LogWarning("the reference camera is not orthographic! this will cause enemy spawns to sometimes appear within camera boundaries!");

        //generate a position outside of camera boundaries using 2 random numbers
        float x = Random.Range(0f, 1f), y = Random.Range(0f, 1f);

        //then,randomly chose whether we want to round the x or the y value
        switch(Random.Range(0, 2))
        {
            case 0: default:
                return instance.referenceCamera.ViewportToWorldPoint(new Vector3(Mathf.Round(x), y));
            case 1:
                return instance.referenceCamera.ViewportToWorldPoint(new Vector3(x, Mathf.Round(y)));
        }
    }

    //checking if the enemy is within the camera's boundaries
    public static bool IsWithinBoundaries(Transform checkedObject)
    {
        //get the camera to check if we are withi boundaries
        Camera c = instance && instance.referenceCamera ? instance.referenceCamera : Camera.main;

        Vector2 viewport = c.WorldToViewportPoint(checkedObject.position);
        if(viewport.x < 0f || viewport.x >1f) return false;
        if(viewport.y < 0f || viewport.y >1f) return false;
        return true;
    }
}
