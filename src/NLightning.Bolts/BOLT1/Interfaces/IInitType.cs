namespace NLightning.Bolts.BOLT1.Interfaces;

public interface IInitType<T> where T : notnull
{
    byte Type { get; set; }
    T Data { get; set; }

    byte[] Serialize();
    void Deserialize(byte[] data);
}
