using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace CombatExtended.Framework.Data;

public class DataAsset<TD>
{
    
    public static Dictionary<string, TD> Data => _instance.InstanceData;
    
    protected Dictionary<string, TD>? _data;
    protected Dictionary<string, TD> InstanceData => _data ??= Helper.GameContent.Load<Dictionary<string, TD>>(_path);

    protected static DataAsset<TD> _instance;

    private readonly string _path;
    
    protected DataAsset(string path) => _path = path;

    protected static void RegisterAsset(IModHelper helper)
    {
        helper.Events.Content.AssetRequested += _instance.OnAssetRequested;
        helper.Events.Content.AssetReady += _instance.OnAssetReady;
        helper.Events.Content.AssetsInvalidated += _instance.OnAssetsInvalidated;
    }
    
    private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.NameWithoutLocale.IsEquivalentTo(_path))
        {
            Load(e);
            OnRequested();
        }
    }

    /// <summary>
    /// Load associated data asset. By default, empty dictionary is loaded.
    /// </summary>
    /// <param name="e"><see cref="AssetRequestedEventArgs"/>.</param>
    protected virtual void Load(AssetRequestedEventArgs e)
    {
        e.LoadFrom(() => new Dictionary<string, TD>(), AssetLoadPriority.Exclusive);
    }
    
    private void OnAssetReady(object? sender, AssetReadyEventArgs e)
    {
        if (e.NameWithoutLocale.IsEquivalentTo(_path))
        {
            _data = Helper.GameContent.Load<Dictionary<string, TD>>(_path);
            OnReady();
        }
    }

    private void OnAssetsInvalidated(object? sender, AssetsInvalidatedEventArgs e)
    {
        foreach (var assetName in e.NamesWithoutLocale)
        {
            if (assetName.IsEquivalentTo(_path))
            {
                Log.Trace($"Invalidated asset {assetName}");
                _data = null;
                OnInvalidated();
            }
        }
    }
    
    /// <summary>
    /// Raised when associated data asset is requested.
    /// </summary>
    protected virtual void OnRequested() {}
    
    /// <summary>
    /// Raised when associated data asset is ready.
    /// </summary>
    protected virtual void OnReady() {}
    
    /// <summary>
    /// Raised when associated data asset is invalidated.
    /// </summary>
    protected virtual void OnInvalidated() {}
}
