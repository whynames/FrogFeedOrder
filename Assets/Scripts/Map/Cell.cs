using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using PrimeTween;
public interface ICellOwner
{
    CellColor Color { get; }
    Direction Facing { get; }
    CellType Type { get; }
    void Initialize(SpriteProvider spriteProvider);
    void SetCellData(CellColor color, Direction facing, CellType type);


    UniTaskVoid Open();
    UniTaskVoid Close();
}

public class Cell : MonoBehaviour
{
    [SerializeField] private CellColor color;
    [SerializeField] private Direction facing;
    [SerializeField] private CellType cellType;
    [SerializeField] private MeshRenderer meshRenderer;

    private static readonly int MainTexProperty = Shader.PropertyToID("_MainTex");

    private ICellOwner owner;
    private SpriteProvider spriteProvider;
    public CellColor Color => owner?.Color ?? color;
    public Direction Facing => owner?.Facing ?? facing;
    public CellType Type => CellType.Square;
    public CellType OwnerType => owner != null ? owner.Type : Type;

    public T GetOwner<T>() where T : MonoBehaviour
    {
        return owner as T;
    }
    public bool TryGetOwner<T>() where T : MonoBehaviour
    {
        if (this.owner.GetType() == typeof(T))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public Action OnOpened;
    public void Initialize(CellColor color, Direction facing, SpriteProvider spriteProvider)
    {
        this.color = color;
        this.facing = facing;
        this.spriteProvider = spriteProvider;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (meshRenderer != null && spriteProvider != null)
        {
            var sprite = spriteProvider.GetSprite(Type, owner.Color);
            var mpb = new MaterialPropertyBlock();
            mpb.SetTexture(MainTexProperty, sprite.texture);
            meshRenderer.SetPropertyBlock(mpb);
        }
    }

    public void SetOwner(ICellOwner owner)
    {
        this.owner = owner;
        UpdateVisuals();
    }

    public void ClearOwner()
    {
        owner = null;
        UpdateVisuals();
    }

    public async UniTask Open()
    {
        Activate();
        owner.Open().Forget();
        await Tween.Scale(transform, transform.localScale, Vector3.one, 0.3f, Ease.InOutSine);

        // Add any open animation/effects here
    }

    public async UniTask Close()
    {
        owner.Close().Forget();
        await Tween.Scale(transform, transform.localScale, Vector3.zero, 0.3f, Ease.Default).ToUniTask(coroutineRunner: this);
        Deactivate();
        // Add any close animation/effects here
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);

    }

    public void Activate()
    {
        gameObject.SetActive(true);
        OnOpened?.Invoke();

    }
}

public enum CellType
{
    Frog,
    Arrow,
    Berry,
    Square
}