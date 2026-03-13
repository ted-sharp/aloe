namespace Aloe.Apps.SyncBootBridgeLib.Services
{
    public interface ISyncBootBridgeBootstrapper
    {
        void Execute(string[] args);
        void ExecuteSyncOnly(string[] args);
    }
}
