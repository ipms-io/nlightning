using NullReferenceException = System.NullReferenceException;

namespace NLightning.Blazor.Tests.Helpers;

public static class StaticAssetsHelper
{
    public static List<StaticAssetInfo> GetRootLevelEntries(string file)
    {
        var jsonString = File.ReadAllText(file);
        var assets = JsonHelper.Deserialize<StaticAssets>(jsonString);

        if (assets?.Root?.Children == null)
        {
            throw new Exception("The file has an incorrect format.");
        }

        var assetPaths = new List<StaticAssetInfo>();

        // Only process the root-level children (no recursion)
        foreach (var rootChild in assets.Root.Children)
        {
            // Handle directories and files at the root level
            AddRootLevelEntry(rootChild.Key, rootChild.Value, assets, assetPaths);
        }

        return assetPaths;
    }

    // Function to handle top-level entries (files or directories)
    private static void AddRootLevelEntry(string key, AssetNode node, StaticAssets assets, List<StaticAssetInfo> assetPaths)
    {
        // If the node has children, it represents a directory
        if (node.Children != null)
        {
            // Handle special cases like _content
            if (key == "_content")
            {
                // Recursively add _content and its first-level subdirectories
                foreach (var child in node.Children)
                {
                    AddDirectoryEntry(key, child.Key, child.Value, assets, assetPaths);
                }
            }
            else
            {
                // Add the top-level directory (like css or _framework)
                var firstAsset = node.Children.FirstOrDefault(c => c.Value.Asset != null).Value?.Asset;
                if (firstAsset == null)
                {
                    return;
                }

                if (!assetPaths.Any(x => x.ContentRoot.Equals(assets.ContentRoots?[firstAsset.ContentRootIndex])))
                {
                    assetPaths.Add(new StaticAssetInfo
                    {
                        ContentRoot = assets.ContentRoots?[firstAsset.ContentRootIndex] ?? throw new NullReferenceException("assets.ContentRoots[firstAsset.ContentRootIndex] was null."),
                        RelativePath = string.Empty
                    });
                }
            }
        }
        else
        {
            // If the node has no children, it's a file, so we add the parent directory path (or root)
            var asset = node.Asset;
            if (asset == null)
            {
                return;
            }

            // Files at the root level should use an empty relative path
            if (!assetPaths.Any(x => x.RelativePath.Equals("") && x.ContentRoot.Equals(assets.ContentRoots?[asset.ContentRootIndex])))
            {
                assetPaths.Add(new StaticAssetInfo
                {
                    ContentRoot = assets.ContentRoots?[asset.ContentRootIndex] ?? throw new NullReferenceException("assets.ContentRoots[asset.ContentRootIndex] was null."),
                    RelativePath = string.Empty
                });
            }
        }
    }

    // Function to add directories under _content
    private static void AddDirectoryEntry(string parentKey, string key, AssetNode node, StaticAssets assets,
        List<StaticAssetInfo> assetPaths)
    {
        // Construct the _content/[LibraryName] path
        var fullRelativePath = $"/{parentKey}/{key}";

        var firstAsset = node.Children?.FirstOrDefault(c => c.Value.Asset != null).Value?.Asset;
        if (firstAsset != null)
        {
            assetPaths.Add(new StaticAssetInfo
            {
                ContentRoot = assets.ContentRoots?[firstAsset.ContentRootIndex] ?? throw new NullReferenceException("assets.ContentRoots[firstAsset.ContentRootIndex] was null."),
                RelativePath = fullRelativePath
            });
        }
    }
}

// Model classes for the deserialized static assets
public class StaticAssets
{
    public List<string>? ContentRoots { get; set; }
    public AssetRoot? Root { get; set; }
}

public class AssetRoot
{
    public Dictionary<string, AssetNode>? Children { get; set; }
}

public class AssetNode
{
    public Dictionary<string, AssetNode>? Children { get; set; }
    public Asset? Asset { get; set; }
}

public class Asset
{
    public int ContentRootIndex { get; set; }
    public string? SubPath { get; set; }
}

public class StaticAssetInfo
{
    public string ContentRoot { get; set; } = string.Empty; // Content root directory
    public string RelativePath { get; set; } = string.Empty; // Relative path (directory)
}