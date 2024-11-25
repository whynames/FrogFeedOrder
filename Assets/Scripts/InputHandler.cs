using UnityEngine;
using VContainer;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using VContainer.Unity;

public class InputHandler
{
    private Camera mainCamera;
    private List<List<Node>> currentMap;
    private bool isInputEnabled;

    public void Initialize()
    {
        mainCamera = Camera.main;
        GameEvents.OnMapLoaded += HandleMapLoaded;
    }

    private void HandleMapLoaded(List<List<Node>> map)
    {
        currentMap = map;
    }



    public void UpdateInput()
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        if (Input.GetMouseButtonDown(0))
        {
            HandleClick();
        }
    }

    private void HandleClick()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {


            Cell cell = hit.collider.GetComponentInParent<Cell>();
            if (cell != null && cell.OwnerType == CellType.Frog)
            {


                Vector2Int position = GetNodePosition(cell);
                if (position != new Vector2Int(-1, -1))
                {
                    var frog = cell.GetComponentInChildren<Frog>();
                    if (frog != null && !frog.IsShootingTongue)
                    {

                        frog.ShootTongue(currentMap, position);
                    }
                }
            }
        }
    }

    private Vector2Int GetNodePosition(Cell cell)
    {
        if (currentMap == null) return new Vector2Int(-1, -1);

        Node parentNode = cell.GetComponentInParent<Node>();
        if (parentNode == null) return new Vector2Int(-1, -1);

        for (int row = 0; row < currentMap.Count; row++)
        {
            for (int col = 0; col < currentMap[row].Count; col++)
            {
                if (currentMap[row][col] == parentNode)
                {
                    return new Vector2Int(col, row);
                }
            }
        }

        return new Vector2Int(-1, -1);
    }
}