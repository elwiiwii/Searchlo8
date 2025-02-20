using FixMath;

namespace Searchlo8
{
    public class GameState
    {
        public List<Cyclo8.EntityClass> Entities { get; set; }
        public Cyclo8.LinkClass Link1 { get; set; }
        public List<Cyclo8.ItemClass> Items { get; set; }
        bool Isdead { get; set; }
        bool Isfinish { get; set; }
        (F32 X, F32 Y) Last_check { get; set; }

        public string StateToString(GameState state)
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
}
