using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Reborn.EditorTools
{
    /// <summary>
    /// Web-Demo bauen (Meilenstein 10c): WebGL mit eigener Ladeseite (Assets/WebGLTemplates/Reborn), Brotli mit Entpacken
    /// im Browser (laeuft auf jedem Hoster, z. B. Netlify, ohne Extra-Einstellungen), Kennzeichen REBORN_DEMO, ohne Frostgipfel.
    /// Aufruf: WebBuild.BuildDemo("Builds/WebDemo") - per CLI ueber Tools/Build/build_web.ps1.
    /// Danach wird wieder auf Windows umgeschaltet, damit der Editor so bleibt wie vorher.
    /// </summary>
    public static class WebBuild
    {
        static readonly string[] DemoExcluded = { "Frost" };

        const string PrevTargetKey = "Reborn.WebBuild.PrevTarget";

        public static string BuildDemo(string outDir)
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
                SessionState.SetInt(PrevTargetKey, (int)EditorUserBuildSettings.activeBuildTarget);
            var named = NamedBuildTarget.WebGL;
            PlayerSettings.WebGL.template = "PROJECT:Reborn";
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
            PlayerSettings.WebGL.decompressionFallback = true;
            PlayerSettings.WebGL.dataCaching = true;
            PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;
            PlayerSettings.WebGL.nameFilesAsHashes = false;
            PlayerSettings.WebGL.showDiagnostics = false;
            PlayerSettings.SetManagedStrippingLevel(named, ManagedStrippingLevel.Medium);
            PlayerSettings.stripEngineCode = true;
            PlayerSettings.runInBackground = false;

            var scenes = EditorBuildSettings.scenes
                .Where(s => s.enabled && !DemoExcluded.Contains(Path.GetFileNameWithoutExtension(s.path)))
                .Select(s => s.path).ToArray();

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outDir,
                target = BuildTarget.WebGL,
                targetGroup = BuildTargetGroup.WebGL,
                options = BuildOptions.None,
                extraScriptingDefines = new[] { "REBORN_DEMO" }
            };
            if (Directory.Exists(outDir)) Directory.Delete(outDir, true);
            var report = BuildPipeline.BuildPlayer(options);
            var s = report.summary;
            string result = $"WebGL {s.result}: {scenes.Length} Szenen, {s.totalSize / (1024f * 1024f):0.0} MB, {s.totalTime.TotalSeconds:0} s, Fehler {s.totalErrors}";
            if (s.result != BuildResult.Succeeded)
            {
                foreach (var step in report.steps)
                    foreach (var m in step.messages)
                        if (m.type == LogType.Error || m.type == LogType.Exception) result += "\n" + m.content;
            }
            return result;
        }

        /// <summary>Zurueck auf die Plattform von vorher (Standard: Windows)</summary>
        public static string SwitchBack()
        {
            var target = (BuildTarget)SessionState.GetInt(PrevTargetKey, (int)BuildTarget.StandaloneWindows64);
            var group = BuildPipeline.GetBuildTargetGroup(target);
            bool ok = EditorUserBuildSettings.SwitchActiveBuildTarget(group, target);
            return target + " aktiv: " + ok;
        }
    }
}
