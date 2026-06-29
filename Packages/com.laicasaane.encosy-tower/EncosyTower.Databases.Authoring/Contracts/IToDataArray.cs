namespace EncosyTower.Databases.Authoring
{
    public interface IToDataArray<out T>
    {
        T[] ToDataArray();
    }
}
