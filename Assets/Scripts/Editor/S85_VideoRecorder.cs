using System;
using System.IO;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Encoder;
using UnityEditor.Recorder.Input;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>Records the entire variable-duration Scene 85 sequence, including every complete escape.</summary>
[InitializeOnLoad]
public static class S85_VideoRecorder
{
    private const string PendingKey = "S85.Movie.Pending";
    private const string CompletedKey = "S85.Movie.Completed";
    private const string FailedKey = "S85.Movie.Failed";
    private const string PresetPath = "Assets/Scenes/Scene_85_RecorderSettings.asset";
    private static RecorderController controller;
    private static RecorderControllerSettings settings;
    private static MovieRecorderSettings movie;
    private static bool failed;
    private static double startTime;
    private static string outputBase;

    static S85_VideoRecorder()
    {
        EditorApplication.playModeStateChanged += OnPlayState;
    }

    public static void CreatePreset()
    {
        if (AssetDatabase.LoadAssetAtPath<RecorderControllerSettings>(PresetPath) != null) return;
        RecorderControllerSettings preset = MakeSettings();
        MovieRecorderSettings presetMovie = MakeMovie("Recordings/Scene85/Scene85_10_to_1_Million_<Take>");
        preset.AddRecorderSettings(presetMovie);
        AssetDatabase.CreateAsset(preset, PresetPath);
        AssetDatabase.AddObjectToAsset(presetMovie, preset);
        AssetDatabase.SaveAssets();
    }

    [MenuItem("Tools/Scene_85/Record Complete Landscape Video — 1080p 30fps")]
    public static void RecordCompleteVideo()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Start recording from Edit Mode so the opening is captured.");
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        S85_SceneSetup.ValidateScene();
        CreatePreset();
        SessionState.SetBool(CompletedKey, false);
        SessionState.SetBool(FailedKey, false);
        SessionState.SetBool(PendingKey, true);
        EditorApplication.isPlaying = true;
    }

    private static RecorderControllerSettings MakeSettings()
    {
        RecorderControllerSettings result = ScriptableObject.CreateInstance<RecorderControllerSettings>();
        result.FrameRate = 30f;
        result.FrameRatePlayback = FrameRatePlayback.Constant;
        result.CapFrameRate = false;
        result.ExitPlayMode = false;
        result.SetRecordModeToManual();
        return result;
    }

    private static MovieRecorderSettings MakeMovie(string path)
    {
        MovieRecorderSettings result = ScriptableObject.CreateInstance<MovieRecorderSettings>();
        result.name = "Scene85 Landscape 1080p — Original Sizes";
        result.Enabled = true;
        result.OutputFile = path;
        result.CaptureAudio = true;
        result.CaptureAlpha = false;
        result.ImageInputSettings = new CameraInputSettings
        {
            Source = ImageSource.MainCamera,
            OutputWidth = 1920,
            OutputHeight = 1080,
            CaptureUI = false
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
            Directory.CreateDirectory("Recordings/Scene85");
            outputBase = "Recordings/Scene85/Scene85_10_to_1_Million_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
            settings = MakeSettings();
            movie = MakeMovie(outputBase);
            settings.AddRecorderSettings(movie);
            controller = new RecorderController(settings);
            failed = false;
            startTime = EditorApplication.timeSinceStartup;
            Application.logMessageReceived += OnLog;
            EditorApplication.update += Tick;
            try
            {
                controller.PrepareRecording();
                if (!controller.StartRecording()) throw new InvalidOperationException("Recorder could not start.");
                Debug.Log("[S85_MOVIE] Recording 1920x1080 / 30fps / audio to " + outputBase + ".mp4");
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
        else if (state == PlayModeStateChange.EnteredEditMode && SessionState.GetBool(CompletedKey, false))
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
        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) failed = true;
    }

    private static void Tick()
    {
        S85_Main director = UnityEngine.Object.FindAnyObjectByType<S85_Main>();
        if (director != null && director.SequenceComplete)
        {
            failed |= director.CompletedRounds != 10 || director.EscapedNumbers.Count != 10 ||
                !director.ContactIsValid || !director.ScalesAreOriginal || director.UnitBurst.ActiveUnitCount != 0 ||
                !director.MusicIsPlaying || (director.SequenceTime > director.MusicClip.length + 0.5f && director.MusicLoopCount < 1);
            File.WriteAllText(outputBase + ".txt", $"1920x1080, 30fps, audio; duration {director.SequenceTime:F2}s\n" +
                $"Completed stomps: {director.CompletedRounds}; completed escapes: {director.EscapedNumbers.Count}\n" +
                $"Original scales: {director.ScalesAreOriginal}; correct contacts: {director.ContactIsValid}\n" +
                $"Visible unit cap: {S85_UnitBurst.MaximumVisibleUnits}; no per-unit GameObjects or Rigidbody\n" +
                $"Music: {director.MusicClip.name}; volume {director.MusicVolume:F2}; loop count {director.MusicLoopCount}; playing {director.MusicIsPlaying}\n" +
                $"Effect levels: approach {director.ApproachVolume:F2}; stomp {director.StompVolume:F2}; run {director.RunVolume:F2}\n");
            Stop();
        }
        else if (failed || EditorApplication.timeSinceStartup - startTime > 2400d)
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
        Debug.Log((failed ? "[S85_MOVIE] FAILED " : "[S85_MOVIE] SAVED ") + outputBase + ".mp4");
        EditorApplication.isPlaying = false;
    }
}
