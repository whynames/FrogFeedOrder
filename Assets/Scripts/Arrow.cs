using Cysharp.Threading.Tasks;
using PrimeTween;
using UnityEngine;

public class Arrow : MonoBehaviour, ICellOwner
{
    [SerializeField] private CellColor color;
    [SerializeField] private Direction facing;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private MeshRenderer meshRenderer;
    private SpriteProvider spriteProvider;

    private static readonly int MainTexProperty = Shader.PropertyToID("_MainTex");

    public CellColor Color => color;
    public Direction Facing => facing;
    public CellType Type => CellType.Arrow;

    public void Initialize(SpriteProvider spriteProvider)
    {
        this.spriteProvider = spriteProvider;
        UpdateVisuals();
        UpdateRotation();
    }

    public void SetCellData(CellColor color, Direction facing, CellType type)
    {
        this.color = color;
        this.facing = facing;
    }

    private void UpdateVisuals()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = spriteProvider.GetSprite(Type, facing);
            spriteRenderer.color = GetColor(color);
        }

        if (meshRenderer != null)
        {
            var mpb = new MaterialPropertyBlock();
            mpb.SetTexture(MainTexProperty, spriteProvider.GetSprite(Type, facing).texture);
            mpb.SetColor("_Color", GetColor(color));
            meshRenderer.SetPropertyBlock(mpb);
        }
    }

    private Color GetColor(CellColor cellColor)
    {
        return cellColor switch
        {
            CellColor.Red => UnityEngine.Color.red,
            CellColor.Blue => UnityEngine.Color.blue,
            CellColor.Green => UnityEngine.Color.green,
            CellColor.Yellow => UnityEngine.Color.yellow,
            _ => UnityEngine.Color.white
        };
    }

    private void UpdateRotation()
    {
        float rotation = facing switch
        {
            Direction.Down => -180f,
            Direction.Right => -90f,
            Direction.Left => 90f,
            _ => 0f // Down is default
        };
        transform.localRotation = Quaternion.Euler(0, rotation, 0);
    }

    public async UniTaskVoid Open()
    {
        await Tween.Scale(transform, transform.localScale, Vector3.one, 0.3f, Ease.InOutSine).ToUniTask(coroutineRunner: this);

    }

    public async UniTaskVoid Close()
    {
        await Tween.Scale(transform, transform.localScale, Vector3.zero, 0.2f, Ease.Linear).ToUniTask(coroutineRunner: this);

    }
}