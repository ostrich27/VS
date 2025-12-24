using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapController : MonoBehaviour
{
    public List<GameObject> terrainChunks;
    public GameObject currentChunk;
    public GameObject player;
    public float checkerRadius;
    public LayerMask terrainMask;
    Vector3 playerLastPosition;

    [Header("Optimization")]
    public List<GameObject> spawnedChunks;
    public GameObject latestChunk;
    public float maxOpDist;
    float opDist;
    float optimizerCooldown;
    public float optimizerCooldownDuration;
    void Start()
    {
        playerLastPosition = player.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        ChunkChecker();
        ChunkOptimizer();
    }

    void ChunkChecker()
    {
        if (!currentChunk)
        {
            return;
        }
        Vector3 moveDir = player.transform.position - playerLastPosition;
        playerLastPosition = player.transform.position;
        string directionName = GetDirectionName(moveDir);
        CheckAndSpawnChunk(directionName);

        //check aditional adjacent chunks if moving diagonally
        if (directionName.Contains("Up"))
        {
            CheckAndSpawnChunk("Up");
        }
        if (directionName.Contains("Down"))
        {
            CheckAndSpawnChunk("Down");
        }
        if (directionName.Contains("Left"))
        {
            CheckAndSpawnChunk("Left");
        }
        if (directionName.Contains("Right"))
        {
            CheckAndSpawnChunk("Right");
        }
    }

    void CheckAndSpawnChunk(string direction)
    {
        if (!Physics2D.OverlapCircle(currentChunk.transform.Find(direction).position, checkerRadius, terrainMask))
        {
            SpawnChunk(currentChunk.transform.Find(direction).position);
        }
    }

    string GetDirectionName(Vector3 direction)
    {
        direction = direction.normalized;
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            //moving horizontally more than vertically
            if (direction.y > 0.5f)
            {
                //also moving upwards
                return direction.x > 0 ? "Right_Up" : "Left_Up";
            }
            else if (direction.y < -0.5f)
            {
                //also moving downwards
                return direction.x > 0 ? "Right_Down" : "Left_Down";

            }
            else
            {
                //moving perfectly horizontally
                return direction.x > 0 ? "Right" : "Left";

            }
        }
        else
        {
            {
                //moving vertically more than horizontally
                if (direction.x > 0.5f)
                {
                    //also moving right
                    return direction.y > 0 ? "Right_Up" : "Right_Down";
                }
                else if (direction.x < -0.5f)
                {
                    //also moving left
                    return direction.y > 0 ? "Left_Up" : "Left_Down";
                }
                else
                {
                    //moving perfectly vertically
                    return direction.y > 0 ? "Up" : "Down";

                }
            }
        }
    }
        void SpawnChunk(Vector3 spawnPosition)
            {
                int rand = Random.Range(0, terrainChunks.Count);
                latestChunk = Instantiate(terrainChunks[rand], spawnPosition, Quaternion.identity);
                spawnedChunks.Add(latestChunk);
            }

    void ChunkOptimizer()
    {
        optimizerCooldown -= Time.deltaTime;
        if(optimizerCooldown <= 0f)
        {
            optimizerCooldown = optimizerCooldownDuration;
        }
        else
        {
            return;
        }
            foreach (GameObject chunk in spawnedChunks)
            {
                opDist = Vector3.Distance(player.transform.position, chunk.transform.position);
                if (opDist > maxOpDist)
                {
                    chunk.SetActive(false);
                }
                else
                {
                    chunk.SetActive(true);
                }
            }
    }
}
