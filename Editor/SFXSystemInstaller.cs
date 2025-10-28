#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class SFXSystemInstaller
{
    // ======= CONFIG =======
    private const string TargetAssetsFolder = "Assets/SFXSystem";
    // Relative to the package root:
    private static readonly string[] RuntimeCandidates = new[]
    {
        "Runtime/SFXSystem.Runtime.dll",
        "Runtime/SFXSystem.Runtime.pdb",
        "Runtime/SFXSystem.Runtime.xml"
    };

    private static readonly string[] EditorCandidates = new[]
    {
        "Editor/SFXSystem.Editor.dll",
        "Editor/SFXSystem.Editor.pdb",
        "Editor/SFXSystem.Editor.xml"
    };
    // =======================

    [MenuItem("Tools/SFX System/Install to Assets")]
    public static void InstallToAssets()
    {
        string packageRoot = ResolveThisPackageRoot();
        if (string.IsNullOrEmpty(packageRoot))
        {
            EditorUtility.DisplayDialog("SFX System", "Could not resolve package root. Make sure this installer script is inside the package's Editor/ folder.", "OK");
            return;
        }

        if (!Directory.Exists(TargetAssetsFolder))
        {
            Directory.CreateDirectory(TargetAssetsFolder);
        }

        // List files to copy
        var filesToCopy = RuntimeCandidates
            .Concat(EditorCandidates)
            .Select(rel => Path.Combine(packageRoot, rel))
            .Where(File.Exists)
            .ToArray();

        if (filesToCopy.Length == 0)
        {
            EditorUtility.DisplayDialog("SFX System", "No DLLs found in the package. Make sure your compiled files are in Runtime/ and Editor/.", "OK");
            return;
        }

        // Confirm overwrite if any destination exists
        bool willOverwrite = filesToCopy.Any(src => File.Exists(ToAssetsPath(src, packageRoot)));
        if (willOverwrite)
        {
            bool ok = EditorUtility.DisplayDialog("SFX System",
                "Files already exist in Assets/SFXSystem.\n\nDo you want to overwrite them?",
                "Overwrite", "Cancel");
            if (!ok) return;
        }

        try
        {
            foreach (var src in filesToCopy)
            {
                var dst = ToAssetsPath(src, packageRoot);
                CopyFile(src, dst);
            }

            AssetDatabase.Refresh();

            // Apply importer settings
            ApplyImporterSettings();

            EditorUtility.DisplayDialog("SFX System", "Installed to Assets/SFXSystem successfully.", "OK");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SFXSystemInstaller] Install failed: {ex}");
            EditorUtility.DisplayDialog("SFX System", "Install failed, see Console for details.", "OK");
        }
    }

    [MenuItem("Tools/SFX System/Update from Package")]
    public static void UpdateFromPackage()
    {
        // Same as install but without overwrite prompt (always overwrite)
        string packageRoot = ResolveThisPackageRoot();
        if (string.IsNullOrEmpty(packageRoot))
        {
            EditorUtility.DisplayDialog("SFX System", "Could not resolve package root.", "OK");
            return;
        }

        if (!Directory.Exists(TargetAssetsFolder))
        {
            Directory.CreateDirectory(TargetAssetsFolder);
        }

        var filesToCopy = RuntimeCandidates
            .Concat(EditorCandidates)
            .Select(rel => Path.Combine(packageRoot, rel))
            .Where(File.Exists)
            .ToArray();

        if (filesToCopy.Length == 0)
        {
            EditorUtility.DisplayDialog("SFX System", "No DLLs found in the package.", "OK");
            return;
        }

        try
        {
            foreach (var src in filesToCopy)
            {
                var dst = ToAssetsPath(src, packageRoot);
                CopyFile(src, dst);
            }

            AssetDatabase.Refresh();
            ApplyImporterSettings();

            EditorUtility.DisplayDialog("SFX System", "Updated from package successfully.", "OK");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SFXSystemInstaller] Update failed: {ex}");
            EditorUtility.DisplayDialog("SFX System", "Update failed, see Console for details.", "OK");
        }
    }

    [MenuItem("Tools/SFX System/Uninstall from Assets")]
    public static void UninstallFromAssets()
    {
        if (!Directory.Exists(TargetAssetsFolder))
        {
            EditorUtility.DisplayDialog("SFX System", "Assets/SFXSystem is not present.", "OK");
            return;
        }

        bool ok = EditorUtility.DisplayDialog("SFX System",
            "Delete Assets/SFXSystem (DLLs and related files)?",
            "Delete", "Cancel");
        if (!ok) return;

        try
        {
            FileUtil.DeleteFileOrDirectory(TargetAssetsFolder);
            FileUtil.DeleteFileOrDirectory(TargetAssetsFolder + ".meta");
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("SFX System", "Uninstalled from Assets.", "OK");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SFXSystemInstaller] Uninstall failed: {ex}");
            EditorUtility.DisplayDialog("SFX System", "Uninstall failed, see Console for details.", "OK");
        }
    }

    // ---------- helpers ----------

    private static string ResolveThisPackageRoot()
    {
        // Try to locate this script asset path
        var script = FindThisScriptAsset();
        if (script == null) return null;

        var scriptPath = AssetDatabase.GetAssetPath(script); // e.g. Packages/com.yourname.sfxsystem/Editor/SFXSystemInstaller.cs
        if (string.IsNullOrEmpty(scriptPath)) return null;

        // Go up from /Editor/ to package root
        var dir = Path.GetDirectoryName(scriptPath)?.Replace('\\', '/');
        if (string.IsNullOrEmpty(dir)) return null;

        // If scriptPath ends with /Editor/..., remove /Editor and anything after it
        int editorIdx = dir.LastIndexOf("/Editor", StringComparison.OrdinalIgnoreCase);
        string packageRootAssetPath = editorIdx >= 0 ? dir.Substring(0, editorIdx) : Path.GetDirectoryName(dir)?.Replace('\\', '/');

        if (string.IsNullOrEmpty(packageRootAssetPath)) return null;

        // Convert to absolute on disk
        // For Packages/, Application.dataPath points to /Project/Assets; we need to go up one
        string projectRoot = Directory.GetParent(Application.dataPath).FullName.Replace('\\', '/');
        if (packageRootAssetPath.StartsWith("Packages/", StringComparison.OrdinalIgnoreCase))
        {
            string absolute = Path.Combine(projectRoot, packageRootAssetPath).Replace('\\', '/');
            return absolute;
        }
        // Fallback: if embedded as source under Assets, still handle
        if (packageRootAssetPath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
        {
            string absolute = Path.Combine(projectRoot, packageRootAssetPath).Replace('\\', '/');
            return absolute;
        }
        return null;
    }

    private static UnityEngine.Object FindThisScriptAsset()
    {
        // Find by class name to be resilient to file rename
        string[] guids = AssetDatabase.FindAssets("SFXSystemInstaller t:Script");
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var text = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
            if (text != null && text.GetClass() == typeof(SFXSystemInstaller))
                return text;
        }
        return null;
    }

    private static string ToAssetsPath(string packageFileAbsolute, string packageRootAbsolute)
    {
        string rel = packageFileAbsolute.Replace('\\', '/').Substring(packageRootAbsolute.Replace('\\', '/').Length).TrimStart('/');
        string fileName = Path.GetFileName(rel);
        string dst = Path.Combine(TargetAssetsFolder, fileName).Replace('\\', '/');
        return dst;
    }

    private static void CopyFile(string src, string dst)
    {
        var dstDir = Path.GetDirectoryName(dst);
        if (!Directory.Exists(dstDir)) Directory.CreateDirectory(dstDir);
        File.Copy(src, dst, true);
        // ensure Unity sees it as an asset
        var rel = dst.Replace(Application.dataPath, "Assets");
        // no-op here; AssetDatabase.Refresh will pick it up
    }

    private static void ApplyImporterSettings()
    {
        // Runtime DLL: included in builds + in editor
        ConfigurePluginAt($"{TargetAssetsFolder}/SFXSystem.Runtime.dll",
            setAnyPlatform: true,
            editorOnly: false);

        // Editor DLL: editor-only
        ConfigurePluginAt($"{TargetAssetsFolder}/SFXSystem.Editor.dll",
            setAnyPlatform: false,
            editorOnly: true);
    }

    private static void ConfigurePluginAt(string assetPath, bool setAnyPlatform, bool editorOnly)
    {
        var importer = AssetImporter.GetAtPath(assetPath) as PluginImporter;
        if (!importer) return;

        if (editorOnly)
        {
            importer.SetCompatibleWithAnyPlatform(false);
            importer.SetCompatibleWithEditor(true);
        }
        else
        {
            importer.SetCompatibleWithAnyPlatform(true);
            importer.SetCompatibleWithEditor(true);
        }

        // Optional: you can fine-tune specific platforms here if needed
        importer.SaveAndReimport();
    }
}
#endif
