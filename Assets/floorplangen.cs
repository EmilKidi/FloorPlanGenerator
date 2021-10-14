using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class floorplangen : MonoBehaviour
{
    public Texture2D image;
    private Dictionary<Vector2Int, bool> marked = new Dictionary<Vector2Int, bool>();

    void Start()
    {
        // Begin searching for an anchorPoint
        for (int i = 0; i < image.width; i++)
        {
            for (int j = 0; j < image.height; j++)
            {
                Color pixel = image.GetPixel(i, j);
                if (pixel == Color.black)
                {
                    Vector2Int initDiscoverPos = new Vector2Int(i, j);
                    discoverPaths(initDiscoverPos);
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
    }

    void discoverPaths(Vector2Int initPos, string cameFrom = "any")
    {
        // Spawn init anchor.
        spawnAnchor(new Vector3(initPos.x, 0, initPos.y), Color.green);

        Color pixel = image.GetPixel(initPos.x, initPos.y);
        if (pixel != Color.black)
        {
            return;
        }

        // Follow Paths.
        if (cameFrom != "down")
        {
            followPath(initPos, initPos.y + 2, image.height, "up");
        }
        if (cameFrom != "left")
        {
            followPath(initPos, initPos.x + 2, image.width, "right");
        }
        if (cameFrom != "up")
        {
            followPath(initPos, initPos.y - 2, image.height, "down");
        }
        if (cameFrom != "right")
        {
            followPath(initPos, initPos.x - 2, image.width, "left");
        }
    }

    void followPath(Vector2Int initPos, int axis, int imageProp, string dir)
    {
        if (dir == "up" || dir == "right")
        {
            for (int i = axis; i < imageProp; i++)
            {
                Vector3 anchorPos = (dir == "up")
                    ? new Vector3(initPos.x, 0, i)
                    : new Vector3(i, 0, initPos.y);

                if (i == axis)
                {
                    spawnAnchor(anchorPos, Color.yellow);
                    continue;
                }

                Color pixel = image.GetPixel((int)anchorPos.x, (int)anchorPos.z);

                if (pixel != Color.black || i == imageProp)
                {
                    spawnAnchor(anchorPos, Color.red);
                    int x2 = (dir == "up") ? (int)anchorPos.x : (int)anchorPos.x - 1;
                    int y2 = (dir == "up") ? (int)anchorPos.z - 1 : (int)anchorPos.z;
                    discoverPaths(new Vector2Int(x2, y2), dir);
                    break;
                }
                else
                {
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
                    Vector3 anchorPos = (dir == "down")
                        ? new Vector3(initPos.x, 0, i)
                        : new Vector3(i, 0, initPos.y);

                    if (i == axis)
                    {
                        spawnAnchor(anchorPos, Color.yellow);
                        continue;
                    }

                    Color pixel = image.GetPixel((int)anchorPos.x, (int)anchorPos.z);

                    if (pixel != Color.black || i < 0)
                    {
                        spawnAnchor(anchorPos, Color.red);
                        int x2 = (dir == "down") ? (int)anchorPos.x : (int)anchorPos.x + 1;
                        int y2 = (dir == "down") ? (int)anchorPos.z + 1 : (int)anchorPos.z;
                        discoverPaths(new Vector2Int(x2, y2), dir);
                        break;
                    }
                    else
                    {
                        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        cube.transform.position = anchorPos;
                    }
                }
            }
        }
    }
}
