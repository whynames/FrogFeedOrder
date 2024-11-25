using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Linq;
using VContainer;
using PrimeTween;

public class Frog : MonoBehaviour, ICellOwner
{
    [SerializeField] private CellColor color;
    [SerializeField] private Direction facing;
    [SerializeField] private float tongueSpeed = 5f;
    [SerializeField] private float tongueMaxLength = 5f;
    [SerializeField] private SkinnedMeshRenderer meshRenderer;
    [SerializeField] private Tongue tongue;

    private Cell cell;
    private bool isActive = true;
    private SpriteProvider spriteProvider;
    private bool lastShootResult;
    private bool isShootingTongue = false;
    [Inject]
    private MoveManager moveManager;

    private static readonly int MainTexProperty = Shader.PropertyToID("_MainTex");

    public CellColor Color => color;
    public Direction Facing => facing;
    public CellType Type => CellType.Frog;
    public bool IsShootingTongue => isShootingTongue;

    public void SetCellData(CellColor color, Direction facing, CellType type)
    {
        this.color = color;
        this.facing = facing;
    }

    public void Initialize(SpriteProvider spriteProvider)
    {
        this.spriteProvider = spriteProvider;
        UpdateVisuals();
        UpdateRotation();
    }

    private void UpdateVisuals()
    {
        if (meshRenderer != null)
        {
            var mpb = new MaterialPropertyBlock();
            mpb.SetTexture(MainTexProperty, spriteProvider.GetSprite(Type, color).texture);
            meshRenderer.SetPropertyBlock(mpb);
        }
    }

    public async UniTaskVoid Open()
    {
        await Tween.Scale(transform, transform.localScale, Vector3.one, 0.3f, Ease.InOutSine).ToUniTask(coroutineRunner: this);

    }

    public async UniTaskVoid Close()
    {
        await Tween.Scale(transform, transform.localScale, Vector3.zero, 0.3f, Ease.OutBounce).ToUniTask(coroutineRunner: this);

    }

    private void UpdateRotation()
    {
        float rotation = facing switch
        {
            Direction.Up => 180f,
            Direction.Right => 90f,
            Direction.Left => -90f,
            _ => 0f // Down is default
        };
        transform.localRotation = Quaternion.Euler(0, rotation, 0);
    }

    public void ShootTongue(List<List<Node>> nodeMap, Vector2Int currentPosition)
    {
        if (!isActive || isShootingTongue || tongue == null) return;
        if (!moveManager.TryUseMove()) return;

        GameEvents.FrogShootStarted(this);
        ShootTongueAsync(nodeMap, currentPosition).Forget();
    }

    private async UniTaskVoid ShootTongueAsync(List<List<Node>> nodeMap, Vector2Int currentPosition)
    {
        isShootingTongue = true;
        Direction currentDirection = facing;
        List<Node> nodesInPath = GetNodesInPath(nodeMap, currentPosition, GetDirectionVector(currentDirection));

        lastShootResult = await tongue.ExtendAndCollect(nodesInPath, color, currentDirection, (newDirection, additionalNodes, node) =>
        {
            currentDirection = newDirection;
            List<Node> remainingNodes = GetNodesInPath(nodeMap, GetNodePosition(nodeMap, node), GetDirectionVector(newDirection));
            additionalNodes.AddRange(remainingNodes);
        });

        if (lastShootResult)
        {
            Node parentNode = transform.GetComponentInParent<Node>();
            var cell = await parentNode.RemoveTopCell();
        }

        GameEvents.FrogShootFinished(this);
        isShootingTongue = false;
    }

    private Vector2Int GetDirectionVector(Direction facing)
    {
        return facing switch
        {
            Direction.Up => new Vector2Int(0, -1),
            Direction.Right => new Vector2Int(1, 0),
            Direction.Down => new Vector2Int(0, 1),
            Direction.Left => new Vector2Int(-1, 0),
            _ => Vector2Int.zero
        };
    }

    private List<Node> GetNodesInPath(List<List<Node>> nodeMap, Vector2Int start, Vector2Int direction)
    {
        List<Node> path = new List<Node>();
        Vector2Int current = start;

        while (IsValidPosition(current, nodeMap))
        {
            path.Add(nodeMap[current.y][current.x]);
            current += direction;
        }

        return path;
    }

    private bool IsValidPosition(Vector2Int pos, List<List<Node>> nodeMap)
    {
        return pos.y >= 0 && pos.y < nodeMap.Count &&
               pos.x >= 0 && pos.x < nodeMap[pos.y].Count;
    }

    public void Deactivate(Cell cell)
    {
        isActive = false;
        cell.gameObject.SetActive(false);

    }

    private Vector2Int GetNodePosition(List<List<Node>> nodeMap, Node node)
    {
        for (int y = 0; y < nodeMap.Count; y++)
        {
            for (int x = 0; x < nodeMap[y].Count; x++)
            {
                if (nodeMap[y][x] == node)
                    return new Vector2Int(x, y);
            }
        }
        return Vector2Int.zero;
    }
}