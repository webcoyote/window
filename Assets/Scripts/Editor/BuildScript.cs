// Copyright (C) 2019 One More Game - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace OMG.Tap.Editor {
    public static class BuildScript {
        [MenuItem("OMG/Build Win64 Player", priority = 1)]
        public static void BuildWin64Player () {
            _ = BuildPlayer(
                BuildTarget.StandaloneWindows64,
                Path.ChangeExtension(Application.productName, ".exe")
            ); 
        }

        private static BuildResult BuildPlayer (BuildTarget target, string appName) {
            Debug.Log($"BuildPlayer: ({target}, {appName})");

            if (EditorBuildSettings.scenes.Length == 0) {
                string error = "No scenes found in EditorBuildSettings.\nUse 'File Menu -> Build Settings' to add scenes";
                if (Application.isBatchMode) {
                    Debug.LogError(error);
                    EditorApplication.Exit(1);
                }
                _ = EditorUtility.DisplayDialog("Error", error, "OK");
                return BuildResult.Failed;
            }

            // Locate output directory
            string outDir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Build", target.ToString()));
            Debug.Log($"BuildPlayer: Writing build to {outDir}");

            // Collect scenes to include in the build
            var scenes = new List<string>(EditorBuildSettings.scenes.Length);
            foreach (var scene in EditorBuildSettings.scenes) {
                Debug.Log($"BuildPlayer: Including scene {scene.path}");
                scenes.Add(scene.path);
            }

            // Perform the build
            var options = new BuildPlayerOptions {
                scenes = scenes.ToArray(),
                locationPathName = Path.Combine(outDir, appName),
                target = target,
                options = BuildOptions.None
            };

            int exitCode = 0;
            Logger logger = Debug.Log;
            var summary = BuildPipeline.BuildPlayer(options).summary;
            if (summary.result == BuildResult.Failed) {
                exitCode = (int)ExitCodeReason.PLAYER_BUILD_FAILURE;
                logger = Debug.LogError;
            }

            logger($"BuildPlayer: {summary.result}, time(s) {summary.totalTime.TotalSeconds}, size {summary.totalSize}, errors {summary.totalErrors}, warnings {summary.totalWarnings}");
            if (Application.isBatchMode)
                EditorApplication.Exit(exitCode);

            return summary.result;
        }

        private delegate void Logger (string msg);

        // Exit Code definitions to assist with build automation failures.
        //
        // Rules to follow to ensure these are always useful:
        // * Numbers must be between 1-127.
        // * Do not change or swap codes for reasons. Do not re-use exit codes.
        //   This should be considered a protocol.
        private enum ExitCodeReason {
            PLAYER_BUILD_FAILURE = 1,
            PLAYER_CONTENT_BUILD_FAILURE = 2,
        }
    }
}
