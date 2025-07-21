using System.Collections.Generic;

namespace Searchlo8;

public class GameState
{
    public List<Cyclo8.EntityClass> Entities { get; set; }
    public Cyclo8.LinkClass Link { get; set; }
    public List<Cyclo8.ItemClass> Items { get; set; }
    public bool IsDead { get; set; }
    public bool IsFinish { get; set; }
    public GameState(List<Cyclo8.EntityClass> entities, Cyclo8.LinkClass link, List<Cyclo8.ItemClass> items, bool isDead, bool isFinish)
    {
        Entities = entities;
        Link = link;
        Items = items;
        IsDead = isDead;
        IsFinish = isFinish;
    }
    public string StateToString()
    {
        string str = " | ";
        foreach (var entity in Entities)
        {
            str += $"{
                entity.X}, {
                entity.Y}, {
                entity.Vx}, {
                entity.Vy}, {
                entity.Rot}, {
                entity.Vrot}, {
                entity.Link.Ent1}, {
                entity.Link.Ent2}, {
                entity.Link.Baselen}, {
                entity.Link.Length}, {
                entity.Link.Dirx}, {
                entity.Link.Diry}, {
                entity.Linkside} | ";
        }
        return str;
    }
}

