using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class floorplangen : MonoBehaviour
{
    public bool debug = false;
    public Texture2D image;
    public int checkFuturePixelsLenght = 10;
    private Dictionary<Vector2Int, bool> marked = new Dictionary<Vector2Int, bool>();
    private Dictionary<Vector2Int, int[]> anchorPoints = new Dictionary<Vector2Int, int[]>();

    private enum directions {
        up = 0,
        right = 1,
        down = 2,
        left = 3
    }
    private bool[] triedDirections = new bool[4];

    void Start()
    {
        // Begin searching for an anchorPoint
        for (int i = 0; i < image.width; i++)
        {
            for (int j = 0; j < image.height; j++)
            {
                Color pixel = image.GetPixel(i, j);
                if (isBlack(pixel))
                {
                    Vector2Int initDiscoverPos = new Vector2Int(i, j);
                    discoverPaths(initDiscoverPos);
                    //disoverRooms()
                    return;
                }
            }
        }
    }

    void spawnAnchor(Vector3 pos, Color col)
    {
        GameObject anchorpoint = GameObject.CreatePrimitive(PrimitiveType.Cube);
        anchorpoint.GetComponent<Renderer>().materials[0].SetColor("_Color", col);
        anchorpoint.transform.position = pos;

        // Store the anchorpoint for later use
        if (col == Color.green)
        {
            Vector2Int key = new Vector2Int((int)pos.x, (int)pos.z);
            int[] value = new int[4] { 0, 0, 0, 0 };

            if (!anchorPoints.ContainsKey(key))
            {
                anchorPoints.Add(key, value);
                GameObject anchorpoint2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                anchorpoint2.GetComponent<Renderer>().materials[0].SetColor("_Color", Color.cyan);
                anchorpoint2.transform.position = pos;
            }
        }
    }

    bool isBlack(Color col)
    {
        float maxRange = 0.9f;
        float[] cols = { col.r, col.g, col.b };

        for(int i = 0; i < cols.Length; i++)
        {
            if (Mathf.Clamp(cols[i], 0, maxRange) == maxRange)
            {
                return false;
            }
        }

        return true;
    }

    void discoverPaths(Vector2Int initPos)
    {
        // Spawn init anchor.
        spawnAnchor(new Vector3(initPos.x, 0, initPos.y), Color.green);

        // Get tried directions of the current anchorpoint.
        int[] value;
        anchorPoints.TryGetValue(initPos, out value);

        // Follow Paths.
        if (triedDirections[(int)directions.up] == false && (int)value.GetValue((int)directions.up) == 0)
        {
            followPath(initPos, initPos.y + 1, image.height, (int)directions.up);
        }
        if (triedDirections[(int)directions.right] == false && (int)value.GetValue((int)directions.right) == 0)
        {
            followPath(initPos, initPos.x + 1, image.width, (int)directions.right);
        }
        if (triedDirections[(int)directions.down] == false && (int)value.GetValue((int)directions.down) == 0)
        {
            followPath(initPos, initPos.y - 1, image.height, (int)directions.down);
        }
        if (triedDirections[(int)directions.left] == false && (int)value.GetValue((int)directions.left) == 0)
        {
            followPath(initPos, initPos.x - 1, image.width, (int)directions.left);
        }

        // No more options on this anchorpoint. Check previous for undiscovered paths.
        bool found = false;
        Vector2Int unfinishedAnchor = new Vector2Int();
        foreach (KeyValuePair<Vector2Int, int[]> entry in anchorPoints)
        {
            for (int i = 0; i < 4;  i++)
            {
                if ((int)entry.Value.GetValue(i) == 0 && !found)
                {
                    triedDirections = new bool[4];
                    unfinishedAnchor = entry.Key;
                    found = true;
                }
            }
        }

        if (found)
        {
           discoverPaths(unfinishedAnchor);
        }
    }

    // Check the direction. If there is a cube on left or right side, we're dealing with a line-width pixel.
    bool checkBorderPixels(Vector2Int proposedPos, int dir)
    {
        Vector2Int positionPos = proposedPos;
        Vector2Int positionNeg = proposedPos;

        // Up Down
        if (dir == 0 || dir == 2)
        {
            positionPos = new Vector2Int(proposedPos.x + 1, proposedPos.y);
            positionNeg = new Vector2Int(proposedPos.x - 1, proposedPos.y);
        }

        // Left Right
        if (dir == 1 || dir == 3)
        {
            positionPos = new Vector2Int(proposedPos.x, proposedPos.y + 1);
            positionNeg = new Vector2Int(proposedPos.x, proposedPos.y - 1);
        }

        if (marked.ContainsKey(positionPos) || marked.ContainsKey(positionNeg))
        {
            spawnAnchor(new Vector3(positionPos.x, 0, positionPos.y), Color.black);
            spawnAnchor(new Vector3(positionNeg.x, 0, positionNeg.y), Color.black);
            return true;
        }

        return false;
    }

    void followPath(Vector2Int initPos, int axis, int imageProp, int dir)
    {
        // Add direction to the current anchor point.
        if (anchorPoints.ContainsKey(initPos))
        {
            anchorPoints[initPos].SetValue(1, dir);
        }

        int x2 = 0;
        int y2 = 0;

        if (dir == (int)directions.up || dir == (int)directions.right)
        {
            for (int i = axis; i < imageProp; i++)
            {
                
                Vector3 anchorPos = (dir == (int)directions.up)
                    ? new Vector3(initPos.x, 0, i)
                    : new Vector3(i, 0, initPos.y);

                if (i == axis && debug)
                {
                    spawnAnchor(anchorPos, Color.yellow);
                }

                Color pixel = image.GetPixel((int)anchorPos.x, (int)anchorPos.z);

                if (!isBlack(pixel) || i == imageProp || marked.ContainsKey(new Vector2Int((int)anchorPos.x, (int)anchorPos.z)))
                {
                    if (debug)
                    {
                        spawnAnchor(anchorPos, Color.red);
                    }
                    triedDirections[dir] = true;
                    break;
                }
                else
                {
                    // Check if we just caught a border pixel.
                    if (i == axis)
                    {
                        if (checkBorderPixels(new Vector2Int((int)anchorPos.x, (int)anchorPos.z), dir)) {
                            triedDirections[dir] = true;
                            break;
                        }

                        // Check for the future x pixels to determine if we actually need to build the wall
                        Vector3 checkFuturePixels = (dir == (int)directions.up)
                            ? new Vector3(anchorPos.x, 0, anchorPos.z + checkFuturePixelsLenght)
                            : new Vector3(anchorPos.x + checkFuturePixelsLenght, 0, anchorPos.z);

                        Color futurePixel = image.GetPixel((int)checkFuturePixels.x, (int)checkFuturePixels.z);
                        if (!isBlack(futurePixel))
                        {
                            spawnAnchor(new Vector3(checkFuturePixels.x, 0, checkFuturePixels.z), Color.blue);
                            break;
                        }
                    }

                    // Reset directions.
                    triedDirections = new bool[4];

                    // We are at the edge of the image. Create a new anchorpoint?
                    if (i + 1 == imageProp)
                    {
                        discoverPaths(new Vector2Int((int)anchorPos.x, (int)anchorPos.z));
                        break;
                    }

                    x2 = (int)anchorPos.x;
                    y2 = (int)anchorPos.z;
                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.transform.position = anchorPos;
                    marked.Add(new Vector2Int((int)anchorPos.x, (int)anchorPos.z), true);
                }
            }
        }
        else
        {
            for (int i = axis; i > 0; i--)
            {
                {
                    Vector3 anchorPos = (dir == (int)directions.down)
                        ? new Vector3(initPos.x, 0, i)
                        : new Vector3(i, 0, initPos.y);

                    if (i == axis && debug)
                    {
                        spawnAnchor(anchorPos, Color.yellow);
                    }

                    Color pixel = image.GetPixel((int)anchorPos.x, (int)anchorPos.z);

                    if (!isBlack(pixel) || i < 0 || marked.ContainsKey(new Vector2Int((int)anchorPos.x, (int)anchorPos.z)))
                    {
                        if (debug)
                        {
                            spawnAnchor(anchorPos, Color.red);
                        }
                        triedDirections[dir] = true;
                        break;
                    }
                    else
                    {
                        // Check if we just caught a border pixel.
                        if (i == axis)
                        {
                            if (checkBorderPixels(new Vector2Int((int)anchorPos.x, (int)anchorPos.z), dir))
                            {
                                triedDirections[dir] = true;
                                break;
                            }

                            // Check for the future x pixels to determine if we actually need to build the wall
                            Vector3 checkFuturePixels = (dir == (int)directions.down)
                                ? new Vector3(anchorPos.x, 0, anchorPos.z - checkFuturePixelsLenght)
                                : new Vector3(anchorPos.x - checkFuturePixelsLenght, 0, anchorPos.z);

                            Color futurePixel = image.GetPixel((int)checkFuturePixels.x, (int)checkFuturePixels.z);
                            if (!isBlack(futurePixel))
                            {
                                spawnAnchor(new Vector3(checkFuturePixels.x, 0, checkFuturePixels.z), Color.blue);
                                break;
                            }
                        }

                        // Reset directions.
                        triedDirections = new bool[4];

                        // We are at the edge of the image. Create a new anchorpoint?
                        if (i - 1 == imageProp)
                        {
                            discoverPaths(new Vector2Int((int)anchorPos.x, (int)anchorPos.z));
                            break;
                        }

                        x2 = (int)anchorPos.x;
                        y2 = (int)anchorPos.z;
                        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        cube.transform.position = anchorPos;
                        marked.Add(new Vector2Int((int)anchorPos.x, (int)anchorPos.z), true);
                    }
                }
            }
        }

        if (x2 != 0 && y2 != 0)
        {
            discoverPaths(new Vector2Int(x2, y2));
        }
    }
}
