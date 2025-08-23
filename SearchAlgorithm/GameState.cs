namespace SearchAlgorithm;

public struct GameState(Cyclo8.EntityStruct wheel0, Cyclo8.EntityStruct wheel1, Cyclo8.LinkStruct link, Cyclo8.ItemStruct[] items, bool isDead, bool isFinish)
{
    public Cyclo8.EntityStruct Wheel0 { get; set; } = wheel0;
    public Cyclo8.EntityStruct Wheel1 { get; set; } = wheel1;
    public Cyclo8.LinkStruct Link { get; set; } = link;
    public Cyclo8.ItemStruct[] Items { get; set; } = items;
    public bool IsDead { get; set; } = isDead;
    public bool IsFinish { get; set; } = isFinish;

    public string StateToString()
    {
        string str = " | ";
        Cyclo8.EntityStruct[] entities = [Wheel0, Wheel1];
        foreach (var entity in entities)
        {
            str += $"{
                entity.X}, {
                entity.Y}, {
                entity.Vx}, {
                entity.Vy}, {
                entity.Rot}, {
                entity.Vrot}, {
                Link.Length}, {
                Link.Dirx}, {
                Link.Diry}, {
                entity.Linkside} | ";
        }
        return str;
    }
}

