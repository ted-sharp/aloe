using Aloe.Apps.SyncBootBridgeLib.Models;

namespace Aloe.Apps.SyncBootBridgeLib.Services
{
    public interface IAppSelector
    {
        AppConfig SelectApp(string appId, SyncManifest manifest);
    }
}
