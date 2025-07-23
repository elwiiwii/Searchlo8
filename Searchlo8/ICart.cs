namespace Searchlo8;

public interface ICart
{
    public void Init();
    public void LoadLevel(int level);
    public void Update();
    public void Draw();
    public string SpriteData { get; }
    public string FlagData { get; }
    public string MapData { get; }
}
