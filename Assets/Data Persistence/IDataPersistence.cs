public interface IDataPersistence
{
    void LoadData(GameData data);
    void SaveData(ref GameData data); // Uses 'ref' to modify the centralized data source
}