using Aloe.Apps.SyncBootBridgeLib.Models;

namespace Aloe.Apps.SyncBootBridgeLib.Repositories
{
    public interface IManifestRepository
    {
        SyncManifest LoadManifest();
    }
}
