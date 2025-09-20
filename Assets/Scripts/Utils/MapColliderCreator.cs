#if UNITY_EDITOR

using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class MapColliderCreator : MonoBehaviour
{
    private LevelMapData levelMapData;

    [SerializeField] private bool createColliders;

    private void Awake()
    {
        Destroy(this);
    }

    private void CreateColliders()
    {
        Tilemap tilemap = GetComponent<Tilemap>();
        levelMapData = GetComponentInParent<LevelMapData>();

        List<Vector2Int> CreateTilesIndices()
        {
            BoundsInt bounds = tilemap.cellBounds;
            TileBase[] allTiles = tilemap.GetTilesBlock(bounds);

            List<Vector2Int> indices = new List<Vector2Int>();

            for (int x = 0; x < bounds.size.x; x++)
            {
                for (int y = 0; y < bounds.size.y; y++)
                {
                    TileBase tile = allTiles[x + y * bounds.size.x];
                    if (tile != null)
                    {
                        indices.Add(new Vector2Int(x, y));
                    }
                }
            }
            return indices;
        }

        List<List<Vector2Int>> CreateRectanglesBlocks(List<Vector2Int> indices)
        {
            List<List<Vector2Int>> lstRectangleIndices = new List<List<Vector2Int>>();
            List<Vector2Int> currentSquare = new List<Vector2Int>();

            while (indices.Count > 0)
            {
                bool Touch(in Vector2Int tile)
                {
                    foreach (Vector2Int t2 in currentSquare)
                    {
                        if ((Mathf.Abs(t2.x - tile.x) <= 1 && Mathf.Abs(t2.y - tile.y) <= 1) && (t2.x == tile.x || t2.y == tile.y))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                currentSquare.Add(indices[0]);
                indices.RemoveAt(0);

                while (true)
                {
                    bool find = false;

                    for (int i = indices.Count - 1; i >= 0; i--)
                    {
                        if (Touch(indices[i]))
                        {
                            find = true;
                            currentSquare.Add(indices[i]);
                            indices.RemoveAt(i);
                        }
                    }

                    if (!find)
                    {
                        lstRectangleIndices.Add(currentSquare.Clone());
                        currentSquare.Clear();
                        break;
                    }
                }
            }

            return lstRectangleIndices;
        }

        List<(Vector2 center, Vector2 size)> CreateRectangles(List<List<Vector2Int>> lstRectangleIndices)
        {
            List<(Vector2 center, Vector2 size)> rectangles = new List<(Vector2 center, Vector2 size)>();
            Vector2 offset = new Vector2(tilemap.cellBounds.position.x, tilemap.cellBounds.position.y);

            foreach (List<Vector2Int> currentRectangle in lstRectangleIndices)
            {
                while (currentRectangle.Count > 0)
                {
                    Vector2Int topLeft = currentRectangle[0];
                    for (int i = 1; i < currentRectangle.Count; i++)
                    {
                        if (currentRectangle[i].y >= topLeft.y)
                        {
                            if (currentRectangle[i].y > topLeft.y)
                            {
                                topLeft = currentRectangle[i];
                            }
                            else if (currentRectangle[i].x < topLeft.x)
                            {
                                topLeft = currentRectangle[i];
                            }
                        }
                    }

                    int xLength = 1, yLength = 1;
                    bool tryX = true, tryY = true, xOk = true, yOk = true;
                    while (true)
                    {
                        xOk = tryX;
                        if (tryX)
                        {
                            for (int i = 0; i < yLength; i++)
                            {
                                if (!currentRectangle.Contains(new Vector2Int(topLeft.x + xLength, topLeft.y - i)))
                                {
                                    xOk = false;
                                    break;
                                }
                            }
                        }

                        yOk = tryY;
                        if (tryY)
                        {
                            for (int i = 0; i < xLength; i++)
                            {
                                if (!currentRectangle.Contains(new Vector2Int(topLeft.x + i, topLeft.y - yLength)))
                                {
                                    yOk = false;
                                    break;
                                }
                            }
                        }

                        if (xOk && yOk)
                        {
                            if (currentRectangle.Contains(new Vector2Int(topLeft.x + xLength, topLeft.y - yLength)))
                            {
                                xLength++;
                                yLength++;
                            }
                            else
                            {
                                yLength++;
                                tryX = false;
                            }
                        }
                        else if (xOk || yOk)
                        {
                            if (xOk)
                            {
                                xLength++;
                                tryY = false;
                            }
                            else
                            {
                                yLength++;
                                tryX = false;
                            }
                        }
                        else
                        {
                            break;
                        }

                        if (!tryX && !tryY)
                        {
                            break;
                        }
                    }

                    for (int x = 0; x < xLength; x++)
                    {
                        for (int y = 0; y < yLength; y++)
                        {
                            currentRectangle.Remove(new Vector2Int(topLeft.x + x, topLeft.y - y));
                        }
                    }

                    Vector2 size = new Vector2(xLength * levelMapData.cellSize.x, yLength * levelMapData.cellSize.y);
                    Vector2 center = topLeft * levelMapData.cellSize + new Vector2(size.x * 0.5f, -size.y * 0.5f + levelMapData.cellSize.y) + offset * levelMapData.cellSize;

                    rectangles.Add((center, size));
                }
            }

            return rectangles;
        }

        void AddHitbox(List<(Vector2 center, Vector2 size)> rectangles)
        {
            foreach ((Vector2 center, Vector2 size) in rectangles)
            {
                BoxCollider2D boxCollider = gameObject.AddComponent<BoxCollider2D>();
                boxCollider.offset = center;
                boxCollider.size = size;
            }
        }

        List<Vector2Int> tilesIndices = CreateTilesIndices();
        List<List<Vector2Int>> rectangleIndices = CreateRectanglesBlocks(tilesIndices);
        List<(Vector2 center, Vector2 size)> rec = CreateRectangles(rectangleIndices);
        AddHitbox(rec);
    }

    private void OnValidate()
    {
        if (createColliders)
        {
            createColliders = false;

            CreateColliders();
            DestroyImmediate(this);
            Debug.ClearDeveloperConsole();
        }
    }
}

#endif