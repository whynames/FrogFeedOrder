using Cysharp.Threading.Tasks;
using PrimeTween;
using UnityEngine;

public class Berry : MonoBehaviour, ICellOwner
{
    [SerializeField] private CellColor color;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private MeshRenderer meshRenderer;
    private SpriteProvider spriteProvider;

    private static readonly int MainTexProperty = Shader.PropertyToID("_MainTex");

    public CellColor Color => color;
    public Direction Facing => Direction.Up;
    public CellType Type => CellType.Berry;

    public void SetCellData(CellColor color, Direction facing, CellType type)
    {
        this.color = color;

    }

    public void Initialize(SpriteProvider spriteProvider)
    {
        this.spriteProvider = spriteProvider;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = spriteProvider.GetSprite(Type, color);
        }

        var mpb = new MaterialPropertyBlock();
        mpb.SetTexture(MainTexProperty, spriteProvider.GetSprite(Type, color).texture);
        meshRenderer.SetPropertyBlock(mpb);
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