using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] GameObject[,] grid;

    [SerializeField] int gridWidth = 5;
    [SerializeField] int gridHeight = 8;

    public Transform[] topDropPoints;
    public Transform[] middleDropPoints;
    public Transform[] bottomDropPoints;


    //Once it hits 40, spawn a green barrel to clear the board
    int numberOfBlocksSpawned = 0;
    float chanceOfRedBarrel = 0.25f;

    public GameObject[] blockTypes;

    private void Awake()
    {
        grid = new GameObject[gridWidth,gridHeight];
    }

    public void SpawnBlock(int offset, BlockPattern pattern, int angle)
    {
        BlockPattern.BlockAngle currentAnglePattern = pattern.blockAngles[angle];

        for(int b = 0; b < 3; b++)
        {
            int toSpawn = 0;

            if (currentAnglePattern.topRow[b])
            {
                int dropPoint = b + offset;
                if (dropPoint > 4)
                {
                    dropPoint -= 5;

                }

                if(numberOfBlocksSpawned > 40)
                {
                    numberOfBlocksSpawned = 0;
                    toSpawn = 2;
                }
                else if(Random.Range(0f,1f) < chanceOfRedBarrel)
                {
                    toSpawn = 1;
                }

                var newBlock = Instantiate(blockTypes[toSpawn], topDropPoints[dropPoint].position, transform.rotation, topDropPoints[dropPoint].parent);
                numberOfBlocksSpawned++;
            }
        }

        for (int b = 0; b < 3; b++)
        {
            int toSpawn = 0;

            if (currentAnglePattern.middleRow[b])
            {
                int dropPoint = b + offset;
                if (dropPoint > 4)
                {
                    dropPoint -= 5;

                }

                if (numberOfBlocksSpawned > 40)
                {
                    numberOfBlocksSpawned = 0;
                    toSpawn = 2;
                }
                else if (Random.Range(0f, 1f) < chanceOfRedBarrel)
                {
                    toSpawn = 1;
                }

                var newBlock = Instantiate(blockTypes[toSpawn], middleDropPoints[dropPoint].position, transform.rotation, middleDropPoints[dropPoint].parent);
                numberOfBlocksSpawned++;
            }
        }

        for (int b = 0; b < 3; b++)
        {
            int toSpawn = 0;

            if (currentAnglePattern.bottomRow[b])
            {
                int dropPoint = b + offset;
                if (dropPoint > 4)
                {
                    dropPoint -= 5;

                }

                if (numberOfBlocksSpawned > 40)
                {
                    numberOfBlocksSpawned = 0;
                    toSpawn = 2;
                }
                else if (Random.Range(0f, 1f) < chanceOfRedBarrel)
                {
                    toSpawn = 1;
                }

                var newBlock = Instantiate(blockTypes[toSpawn], bottomDropPoints[dropPoint].position, transform.rotation, bottomDropPoints[dropPoint].parent);
                numberOfBlocksSpawned++;
            }
        }
    }

    public Block SpawnGreen()
    {
        return Instantiate(blockTypes[2], topDropPoints[0].position, transform.rotation, topDropPoints[0].parent).GetComponent<GreenBarrel>();
    }

    //DoubleCheck if a spot
    public bool CheckSpotTaken(Vector2 cordSpot)
    {
        if (grid[Mathf.RoundToInt(cordSpot.x), Mathf.RoundToInt(cordSpot.y)] == null)
        {
            return false;
        }

        return true;
    }

    public GameObject GetObjectAtSpot(Vector2 cordSpot)
    {
        if (grid[Mathf.RoundToInt(cordSpot.x), Mathf.RoundToInt(cordSpot.y)] != null)
        {
            return grid[Mathf.RoundToInt(cordSpot.x), Mathf.RoundToInt(cordSpot.y)];
        }

        return null;
    }

    public Vector2 GetCordOfSpot(Vector3 position)
    {
        Vector2 closestGrid = new Vector2(Mathf.RoundToInt(position.x), -Mathf.RoundToInt(position.y));

        return closestGrid;
    }

    public void SetObjectLocation(GameObject go)
    {
        Vector2 currentLoc = GetCordOfSpot(go.transform.position);

        if (!CheckSpotTaken(currentLoc))
        {
            if (go.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
            {
                rb.simulated = false;
            }
            go.transform.SetPositionAndRotation(currentLoc, go.transform.rotation);

            grid[Mathf.RoundToInt(currentLoc.x), Mathf.RoundToInt(currentLoc.y)] = go;
        }
    }

    public void RemoveObjectAtLocation(Vector2Int cord)
    {
        if(grid[cord.x,cord.y] == null)
        {
            //Do nothing
            return;
        }
        else
        {
            Destroy(grid[cord.x, cord.y].gameObject);
            grid[cord.x, cord.y] = null;
        }
    }

}
