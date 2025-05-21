using com.nampstudios.bumper.Zone;

[System.Serializable]
public struct ZoneData
{
    public float Length;
    public ZoneController ZonePrefab;
}
public enum PowerupType
{
    SpeedBoost,
    PowerBoost,
    Invincibility
}
public class PooledItem<T>
{
    public T item;
    public bool isUsed;
}
public enum Formation 
{ 
    Circle,
    Cube, 
    Triangle, 
    Army 
}
