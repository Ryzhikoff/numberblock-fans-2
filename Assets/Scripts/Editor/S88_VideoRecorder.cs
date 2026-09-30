using System;
using System.IO;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Encoder;
using UnityEditor.Recorder.Input;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>Records the exact 18.4-second Scene 88 loop as 1,104 frames at 60 fps.</summary>
[InitializeOnLoad]
public static class S88_VideoRecorder
{
    public const int FrameRate = 60;
    public const int TotalFrames = 1104;
    public const float ExactDuration = (float)TotalFrames / FrameRate;

    private const string PendingKey = "S88.Movie.Pending";
    private const string CompletedKey = "S88.Movie.Completed";
    private const string FailedKey = "S88.Movie.Failed";
    private const string PresetPath = "Assets/Scenes/Scene_88_RecorderSettings.asset";

    private static RecorderController controller;
    private static RecorderControllerSettings settings;
    private static MovieRecorderSettings movie;
    private static bool recordingStarted;
    private static bool failed;
    private static double startTime;
    private static string outputBase;

    static S88_VideoRecorder()
    {
        EditorApplication.playModeStateChanged += OnPlayState;
    }

    [MenuItem("Tools/Scene_88/Create Exact Loop Recorder Preset")]
    public static void CreatePreset()
    {
        if (AssetDatabase.LoadAssetAtPath<RecorderControllerSettings>(PresetPath) != null)
        {
            return;
        }

        RecorderControllerSettings preset = MakeSettings();
        MovieRecorderSettings presetMovie = MakeMovie(
            "Recordings/Scene88/Scene88_x10_ExactLoop_<Take>");
        preset.AddRecorderSettings(presetMovie);
        AssetDatabase.CreateAsset(preset, PresetPath);
        AssetDatabase.AddObjectToAsset(presetMovie, preset);
        AssetDatabase.SaveAssets();
        Debug.Log("[S88_MOVIE] Exact-loop Recorder preset created: " + PresetPath);
    }

    [MenuItem("Tools/Scene_88/Record Exact Loop — 1080x1920 60fps")]
    public static void RecordExactLoop()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            throw new InvalidOperationException(
                "[S88_MOVIE] Start recording from Edit Mode so frame zero is captured.");
        }
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        S88_SceneSetup.ValidateScene();
        CreatePreset();
        SessionState.SetBool(CompletedKey, false);
        SessionState.SetBool(FailedKey, false);
        SessionState.SetBool(PendingKey, true);
        EditorApplication.isPlaying = true;
    }

    private static RecorderControllerSettings MakeSettings()
    {
        RecorderControllerSettings result = ScriptableObject.CreateInstance<RecorderControllerSettings>();
        result.FrameRate = FrameRate;
        result.FrameRatePlayback = FrameRatePlayback.Constant;
        result.CapFrameRate = false;
        result.ExitPlayMode = false;
        result.SetRecordModeToFrameInterval(0, TotalFrames - 1);
        return result;
    }

    private static MovieRecorderSettings MakeMovie(string path)
    {
        MovieRecorderSettings result = ScriptableObject.CreateInstance<MovieRecorderSettings>();
        result.name = "Scene88 Vertical Exact Loop — 1104 Frames";
        result.Enabled = true;
        result.OutputFile = path;
        result.CaptureAudio = true;
        result.CaptureAlpha = false;
        result.ImageInputSettings = new CameraInputSettings
        {
            Source = ImageSource.MainCamera,
            OutputWidth = 1080,
            OutputHeight = 1920,
            CaptureUI = true
        };
        result.EncoderSettings = new CoreEncoderSettings
        {
            Codec = CoreEncoderSettings.OutputCodec.MP4,
            EncodingQuality = CoreEncoderSettings.VideoEncodingQuality.High
        };
        return result;
    }

    private static void OnPlayState(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(PendingKey, false))
        {
            SessionState.SetBool(PendingKey, false);
            Directory.CreateDirectory("Recordings/Scene88");
            outputBase = "Recordings/Scene88/Scene88_x10_ExactLoop_" +
                DateTime.Now.ToString("yyyyMMdd_HHmmss");
            settings = MakeSettings();
            movie = MakeMovie(outputBase);
            settings.AddRecorderSettings(movie);
            controller = new RecorderController(settings);
            recordingStarted = false;
            failed = false;
            startTime = EditorApplication.timeSinceStartup;
            Application.logMessageReceived += OnLog;
            EditorApplication.update += Tick;

            try
            {
                controller.PrepareRecording();
                recordingStarted = controller.StartRecording();
                if (!recordingStarted)
                {
                    throw new InvalidOperationException("[S88_MOVIE] Recorder could not start.");
                }
                Debug.Log(
                    "[S88_MOVIE] Recording exactly 1,104 frames: 1080x1920, 60fps, " +
                    "18.4 seconds, UI and audio → " + outputBase + ".mp4");
            }
            catch (Exception exception)
            {
                failed = true;
                Debug.LogException(exception);
                Stop();
            }
        }
        else if (state == PlayModeStateChange.ExitingPlayMode && controller != null)
        {
            controller.StopRecording();
        }
        else if (state == PlayModeStateChange.EnteredEditMode &&
                 SessionState.GetBool(CompletedKey, false))
        {
            failed = SessionState.GetBool(FailedKey, false);
            SessionState.EraseBool(CompletedKey);
            SessionState.EraseBool(FailedKey);
            Application.logMessageReceived -= OnLog;
            EditorApplication.update -= Tick;
            if (movie != null) UnityEngine.Object.DestroyImmediate(movie);
            if (settings != null) UnityEngine.Object.DestroyImmediate(settings);
            controller = null;
            movie = null;
            settings = null;
            if (Application.isBatchMode) EditorApplication.Exit(failed ? 1 : 0);
        }
    }

    private static void OnLog(string message, string stack, LogType type)
    {
        if ((type == LogType.Error || type == LogType.Exception || type == LogType.Assert) &&
            (message.Contains("[S88") || stack.Contains("S88_")))
        {
            failed = true;
        }
    }

    private static void Tick()
    {
        if (recordingStarted && controller != null && !controller.IsRecording())
        {
            S88_Main director = UnityEngine.Object.FindAnyObjectByType<S88_Main>();
            failed |= director == null ||
                director.SequenceTime < S88_Main.ReturnCompleteTime ||
                !director.FirstPressTriggered ||
                !director.TransformationTriggered ||
                !director.DivisionTriggered ||
                !director.SummonStoppedAtTransformation ||
                !director.MusicConfigured ||
                director.VisibleThousandCount != 1;

            File.WriteAllText(
                outputBase + ".txt",
                "Scene 88 exact seamless loop\n" +
                $"Duration: {ExactDuration:F1} seconds\n" +
                $"Frames: {TotalFrames} at {FrameRate} fps\n" +
                "Resolution: 1080x1920 (9:16)\n" +
                "Audio: captured\n" +
                "Music: 487685__gr8horizon__dragon-power-training-loop, two 9.2s cycles\n" +
                "Loop music options: 18.4s once, 9.2s twice, or 4.6s four times\n");
            Stop();
        }
        else if (failed || EditorApplication.timeSinceStartup - startTime > 180d)
        {
            failed = true;
            Stop();
        }
    }

    private static void Stop()
    {
        SessionState.SetBool(CompletedKey, true);
        SessionState.SetBool(FailedKey, failed);
        EditorApplication.update -= Tick;
        if (controller != null) controller.StopRecording();
        Debug.Log((failed ? "[S88_MOVIE] FAILED " : "[S88_MOVIE] SAVED ") + outputBase + ".mp4");
        EditorApplication.isPlaying = false;
    }
}
