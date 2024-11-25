using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System;
using System.Linq;

public class Tongue : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float extendSpeed = 5f;
    [SerializeField] private float retractSpeed = 7f;
    [SerializeField] private float tongueWidth = 0.1f;
    [SerializeField] private float tongueHeight = 0.2f; // Height above ground

    private List<Vector3> tonguePoints = new List<Vector3>();
    private Transform frogTransform;

    private void Awake()
    {
        frogTransform = transform;
        InitializeLineRenderer();
    }

    private void InitializeLineRenderer()
    {
        lineRenderer.startWidth = tongueWidth;
        lineRenderer.endWidth = tongueWidth * 0.5f;
        lineRenderer.positionCount = 0;
    }

    public async UniTask<bool> ExtendAndCollect(List<Node> nodesInPath, CellColor frogColor, Direction initialDirection, Action<Direction, List<Node>, Node> onDirectionChange)
    {
        Direction currentDirection = initialDirection;
        List<Cell> cellsToRemove = new List<Cell>();
        List<Cell> collectedBerries = new List<Cell>();
        bool success = true;

        tonguePoints.Clear();
        tonguePoints.Add(frogTransform.position + Vector3.up * tongueHeight);
        UpdateLineRenderer();

        List<Node> additionalNodes = new List<Node>();

        foreach (Node node in nodesInPath)
        {
            Vector3 targetPosition = node.transform.position + Vector3.up * tongueHeight;
            await ExtendToPosition(targetPosition);
            Cell topCell = node.GetTopCell();

            if (topCell == null)
            {
                break;
            }

            if (topCell.OwnerType == CellType.Arrow)
            {
                if (topCell.Color != frogColor)
                {
                    success = false;
                    break;
                }

                Arrow arrow = topCell.GetComponentInChildren<Arrow>();
                if (arrow != null)
                {
                    currentDirection = arrow.Facing;
                    onDirectionChange?.Invoke(currentDirection, additionalNodes, node);
                    cellsToRemove.Add(topCell);
                    Debug.Log(topCell.transform.position + " " + tonguePoints[tonguePoints.Count - 1] + " " + topCell.OwnerType);

                    break;
                }

                ;

            }
            else if (topCell.OwnerType == CellType.Berry)
            {
                if (topCell.Color != frogColor)
                {
                    success = false;
                    break;
                }
                GameEvents.BerryCollected();

                collectedBerries.Add(topCell);
                cellsToRemove.Add(topCell);
            }
        }

        // Process additional nodes after direction change
        foreach (Node node in additionalNodes)
        {
            Vector3 targetPosition = node.transform.position + Vector3.up * tongueHeight;
            await ExtendToPosition(targetPosition);

            Cell topCell = node.GetTopCell();
            if (topCell == null) continue;

            if (topCell.OwnerType == CellType.Berry)
            {
                if (topCell.Color != frogColor)
                {
                    success = false;
                    break;
                }
                collectedBerries.Add(topCell);
                cellsToRemove.Add(topCell);
            }
        }

        if (success)
        {
            // Pull berries toward frog
            int index = 0;
            List<UniTask> berryTasks = collectedBerries.Select(async cell =>
            {
                var berry = cell.GetOwner<Berry>();
                var berryStartPos = berry.transform.position;
                var elapsed = 0f;
                var duration = 0.5f + (float)(index / 20);

                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    var t = elapsed / duration;
                    berry.transform.position = Vector3.Lerp(berryStartPos, frogTransform.position, t);
                    berry.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, t);
                    await UniTask.Yield();
                }
                index++;
            }).ToList();
            berryTasks.Add(RetractTongue());
            await UniTask.WhenAll(berryTasks);
            GameEvents.CollectionFinished();


            // Remove all cells after animation
            foreach (var cell in cellsToRemove)
            {
                Node parentNode = cell.transform.GetComponentInParent<Node>();
                parentNode.RemoveTopCell();
            }
        }
        else
        {
            await RetractTongue();
        }

        // Clear line renderer
        lineRenderer.positionCount = 0;
        tonguePoints.Clear();

        return success;
    }

    private async UniTask ExtendToPosition(Vector3 targetPosition)
    {
        tonguePoints.Add(targetPosition);
        float distance = Vector3.Distance(tonguePoints[tonguePoints.Count - 2], targetPosition);
        float duration = distance / extendSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            Vector3 currentEnd = Vector3.Lerp(tonguePoints[tonguePoints.Count - 2], targetPosition, t);
            tonguePoints[tonguePoints.Count - 1] = currentEnd;
            UpdateLineRenderer();
            await UniTask.Yield();
        }

        tonguePoints[tonguePoints.Count - 1] = targetPosition;
        UpdateLineRenderer();
    }

    private async UniTask RetractTongue()
    {
        while (tonguePoints.Count > 1)
        {
            float distance = Vector3.Distance(tonguePoints[tonguePoints.Count - 1], tonguePoints[tonguePoints.Count - 2]);
            float duration = distance / retractSpeed;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                Vector3 currentEnd = Vector3.Lerp(tonguePoints[tonguePoints.Count - 1], tonguePoints[tonguePoints.Count - 2], t);
                tonguePoints[tonguePoints.Count - 1] = currentEnd;
                UpdateLineRenderer();
                await UniTask.Yield();
            }

            tonguePoints.RemoveAt(tonguePoints.Count - 1);
            UpdateLineRenderer();
        }
    }

    private void UpdateLineRenderer()
    {
        lineRenderer.positionCount = tonguePoints.Count;
        lineRenderer.SetPositions(tonguePoints.ToArray());
    }
}