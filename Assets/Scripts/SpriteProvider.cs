using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using VContainer;
using Cysharp.Threading.Tasks;

public class SpriteProvider
{
    private readonly Dictionary<(CellType, CellColor), Sprite> spriteMap;
    private readonly Dictionary<(CellType, Direction), Sprite> directionSpriteMap;

    private bool isLoaded = false;
    public bool IsLoaded => isLoaded;

    public SpriteProvider()
    {
        spriteMap = new Dictionary<(CellType, CellColor), Sprite>();
        directionSpriteMap = new Dictionary<(CellType, Direction), Sprite>();
    }

    public async UniTask LoadSprites()
    {
        isLoaded = false;
        foreach (CellColor color in System.Enum.GetValues(typeof(CellColor)))
        {
            string colorName = color.ToString();

            // Load frog textures
            var frogTexture = await Addressables.LoadAssetAsync<Texture2D>($"Assets/FunradoGameDeveloperProject_Assets/Textures/FrogTexture/Frog{colorName}Texture.png").Task;
            Sprite frogSprite = Sprite.Create(frogTexture, new Rect(0, 0, frogTexture.width, frogTexture.height), new Vector2(0.5f, 0.5f));
            spriteMap[(CellType.Frog, color)] = frogSprite;

            // Load berry textures 
            var berryTexture = await Addressables.LoadAssetAsync<Texture2D>($"Assets/FunradoGameDeveloperProject_Assets/Textures/GrapeTexture/GrapeTexture{colorName}.png").Task;
            Sprite berrySprite = Sprite.Create(berryTexture, new Rect(0, 0, berryTexture.width, berryTexture.height), new Vector2(0.5f, 0.5f));
            spriteMap[(CellType.Berry, color)] = berrySprite;

            // Load arrow textures
            var squareTexture = await Addressables.LoadAssetAsync<Texture2D>($"Assets/FunradoGameDeveloperProject_Assets/Textures/CellTexture/Square{colorName}.png").Task;
            Sprite squareSprite = Sprite.Create(squareTexture, new Rect(0, 0, squareTexture.width, squareTexture.height), new Vector2(0.5f, 0.5f));
            spriteMap[(CellType.Square, color)] = squareSprite;

            // Load arrow textures for each direction
            foreach (Direction direction in System.Enum.GetValues(typeof(Direction)))
            {
                string directionName = direction.ToString();
                var arrowTexture = await Addressables.LoadAssetAsync<Texture2D>($"Assets/FunradoGameDeveloperProject_Assets/Textures/ArrowTexture/ArrowDown.png").Task;
                Sprite arrowSprite = Sprite.Create(arrowTexture, new Rect(0, 0, arrowTexture.width, arrowTexture.height), new Vector2(0.5f, 0.5f));
                directionSpriteMap[(CellType.Arrow, direction)] = arrowSprite;
            }
        }
        isLoaded = true;
    }

    public Sprite GetSprite(CellType type, CellColor color)
    {
        if (spriteMap.TryGetValue((type, color), out Sprite sprite))
        {
            return sprite;
        }
        Debug.LogError($"Sprite not found for {type} with color {color}");
        return null;
    }
    public Sprite GetSprite(CellType type, Direction direction)
    {
        if (directionSpriteMap.TryGetValue((type, direction), out Sprite sprite))
        {
            return sprite;
        }
        Debug.LogError($"Sprite not found for {type} with direction {direction}");
        return null;
    }
}