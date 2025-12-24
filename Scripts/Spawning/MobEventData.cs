using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "Mob Event Data", menuName = "2D Top-Down Rogue-Like/Event Data/Mob")]

public class MobEventData : EventData
{
    [Header("Mob Data")]
    [Range(0f, 360f)] public float possibleAngles = 360f;
    [Min(0)] public float spawnRadius = 2f, spawnDistance = 20f;
    [Header("Plant Wave Data")]
    public bool isCircleSpawning = false;
    public float plantWaveDuration;

    public override bool Activate(PlayerStats player = null)
    {
        //only activate this if the player is present
        if (player)
        {
            //otherwise, we spawn a mob outside of the screen and move it towards the player
            float randomAngle = Random.Range(0, possibleAngles) * Mathf.Deg2Rad;
            if (!isCircleSpawning)
            {
                foreach (GameObject o in GetSpawns())
                {
                    Instantiate(o, player.transform.position + new Vector3(
                        (spawnDistance + Random.Range(-spawnRadius, spawnRadius)) * Mathf.Cos(randomAngle),
                        (spawnDistance + Random.Range(-spawnRadius, spawnRadius)) * Mathf.Sin(randomAngle)
                        ), Quaternion.identity);
                }
                return true;
            }
            else
            {
                GameObject[] spawns = GetSpawns();
                int count = spawns.Length;
                float angleStep = possibleAngles / count;

                int index = 0;
                foreach (GameObject o in spawns)
                {
                    float angleRad = (angleStep * index) * Mathf.Deg2Rad;

                    Vector3 pos =
                        player.transform.position +
                        new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * spawnDistance;

                    Destroy(Instantiate(o, pos, Quaternion.identity), plantWaveDuration);

                    index++;
                }
                return true;
            }
        }
        return false;
    }
}
