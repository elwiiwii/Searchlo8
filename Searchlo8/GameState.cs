namespace Searchlo8;

public struct EntityState(int x, int y, int vx, int vy, int rot, int vrot, bool isFlying, int linkside)
{
    public int X = x;
    public int Y = y;
    public int Vx = vx;
    public int Vy = vy;
    public int Rot = rot;
    public int Vrot = vrot;
    public bool IsFlying = isFlying;
    public int Linkside = linkside;
}

public struct LinkState(int length, int dirx, int diry)
{
    public int Length = length;
    public int Dirx = dirx;
    public int Diry = diry;
}

public struct ItemState(int x, int y, bool active)
{
    public int X = x;
    public int Y = y;
    public bool Active = active;
}

public struct GameState
{
    public EntityState Wheel0, Wheel1;
    public LinkState Link;
    public ItemState[] Items;
    public bool IsDead, IsFinish;
    public GameState(Cyclo8.EntityClass wheel0, Cyclo8.EntityClass wheel1, Cyclo8.LinkClass link, List<Cyclo8.ItemClass> items, bool isDead, bool isFinish)
    {
        Wheel0 = new(wheel0.X.Raw, wheel0.Y.Raw, wheel0.Vx.Raw, wheel0.Vy.Raw, wheel0.Rot.Raw, wheel0.Vrot.Raw, wheel0.Isflying, wheel0.Linkside);
        Wheel1 = new(wheel1.X.Raw, wheel1.Y.Raw, wheel1.Vx.Raw, wheel1.Vy.Raw, wheel1.Rot.Raw, wheel1.Vrot.Raw, wheel1.Isflying, wheel1.Linkside);
        Link = new(link.Length.Raw, link.Dirx.Raw, link.Diry.Raw);
        Items = items;
        IsDead = isDead;
        IsFinish = isFinish;
    }
}

//public class GameState
//{
//    public List<Cyclo8.EntityClass> Entities { get; set; }
//    public Cyclo8.LinkClass Link { get; set; }
//    public List<Cyclo8.ItemClass> Items { get; set; }
//    public bool IsDead { get; set; }
//    public bool IsFinish { get; set; }
//    public GameState(List<Cyclo8.EntityClass> entities, Cyclo8.LinkClass link, List<Cyclo8.ItemClass> items, bool isDead, bool isFinish)
//    {
//        Entities = entities;
//        Link = link;
//        Items = items;
//        IsDead = isDead;
//        IsFinish = isFinish;
//    }
//    public string StateToString()
//    {
//        string str = " | ";
//        foreach (var entity in Entities)
//        {
//            str += $"{
//                entity.X}, {
//                entity.Y}, {
//                entity.Vx}, {
//                entity.Vy}, {
//                entity.Rot}, {
//                entity.Vrot}, {
//                entity.Link.Ent1}, {
//                entity.Link.Ent2}, {
//                entity.Link.Baselen}, {
//                entity.Link.Length}, {
//                entity.Link.Dirx}, {
//                entity.Link.Diry}, {
//                entity.Linkside} | ";
//        }
//        return str;
//    }
//}
