﻿using SDL2.NET.SDLFont;
using SDL2.NET.SDLMixer;
using Serilog.Events;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDL2.NET.SDLImage;

namespace SDL2.NET.Tests;

[SDLTestRepository("General Tests")]
public static class GeneralTests
{
    private static RunControl Run;
    private static readonly List<IDisposable> Disposables = new(10);
    private static AudioChunk Pew;

    [SDLTestMethod("Basic General Test")]
    public static void BasicGeneralTest(RunControl run)
    {
        Run = run;
        var app = SDLDefaultApp.Instance
            .InitializeVideo()
            .InitializeAudio()
            .InitializeTTF()
            .LaunchWindow("SDL2.NET Test", 800, 600, rendererFlags: RendererFlags.Accelerated | RendererFlags.PresentVSync);

        if (!AudioMixer.IsInitialized)
            AudioMixer.InitAudioMixer(MixerInitFlags.OGG);
        if (!AudioMixer.IsOpen)
            AudioMixer.OpenAudioMixer();

        Log.Debug("Succesfully launched app and Window");

        var window = app.MainWindow;
        var renderer = app.MainRenderer;

        {
            var icon = Image.Load("Resources\\Icon.png");
            window.SetIcon(icon);
            icon.Dispose();  // Even if it isn't disposed, it'll be finalized and freed. Still better to dispose, though
            Log.Verbose("Set Window Icon");
        }

        // log some renderer information
        {
            var info = renderer.RendererInfo;
            Log.Verbose("Rendering using: {currentVideoDriver}", info.Name);
        }

        Pew = new AudioChunk("Resources\\laserpew.ogg");
        var music = new Song("Resources\\loop.wav");
        Disposables.Add(music);
        Disposables.Add(Pew);

        Music.VolumePercentage = .5;
        music.FadeIn(TimeSpan.FromMilliseconds(200), AudioLoop.Infinite);

        Texture deer = Image.LoadTexture(renderer, "Resources\\deer.png");

        var deerDstBox = deer.GetRectangle(128, 128);
        Disposables.Add(deer);

        var VCRFont = new TTFont("Resources\\VCR_OSD_MONO_1.001.ttf", 32);
        var fontColor = Colors.Red;
        Disposables.Add(VCRFont);

        Texture deerText;
        {
            var _dt = VCRFont.RenderTextSolid("This is a deer!", fontColor, EncodingType.Latin1);
            deerText = new Texture(renderer, _dt);
            _dt.Dispose();
        }
        Disposables.Add(deerText);

        var deerTextDstBox = deerText.GetRectangle(128, 128 + 35);

        SDLApplication.Quitting += App_Quitting;
        window.KeyPressed += Window_KeyPressed;
        window.KeyReleased += Window_KeyReleased;
        window.Resized += Window_Resized;
        window.TextInput += Window_TextInput;

        // main loop
        while (run.IsRunning)
        {
            ulong start = Performance.PerformanceCounter;

            SDLApplication.UpdateEvents();

            renderer.Clear(Colors.CornflowerBlue);

            deer.Render(null, deerDstBox);
            deerText.Render(null, deerTextDstBox);

            // underline the font with SDL_gfx
            //SDL_gfx.lineColor(Renderer, (short)FontTarget.x, (short)(FontTarget.y + FontTarget.h + 10), (short)(FontTarget.x + FontTarget.w), (short)(FontTarget.y + FontTarget.h + 10), SolidRed);

            // end render batch
            renderer.Present();

            // limit framerate to ~120 fps when vSync is disabled
            if (!renderer.IsVSyncEnabled)
            {
                ulong end = Performance.PerformanceCounter;
                double elapsed = (end - start) / (double)Performance.PerformanceFrequency * 1000.0f;
                double delay = Math.Floor(8.333f - elapsed);

                // this check avoids a huge delay when the window can't be drawn during for example moving it
                if (delay < 32)
                    SDLApplication.Delay(TimeSpan.FromMilliseconds(delay));
            }
        } // end of main loop

        for (int i = 0; i < Disposables.Count; i++)
            Disposables[i].Dispose();

        SDLApplication.Quitting -= App_Quitting;
        window.KeyPressed -= Window_KeyPressed;
        window.KeyReleased -= Window_KeyReleased;
        window.Resized -= Window_Resized;
        window.TextInput -= Window_TextInput;
        
        app.Quit(SDLSubSystem.Video | SDLSubSystem.Audio);
        AudioMixer.Quit();

        Log.Information("Program ended, bye!");
    }

    private static void Window_TextInput(Window sender, TimeSpan timestamp, ReadOnlySpan<char> text)
    {
        Log.Debug("Window receiving text input: {text}", new string(text));
    }

    private static void Window_Resized(Window sender, TimeSpan timestamp, Size newsize)
    {
        Log.Debug("Window resized to {newsize.Width} and {newsize.Height}. Resetting", newsize);
        if (sender.Size is not { Width: 640, Height: 480 })
        {
            sender.Size = new(640, 480);
        }
    }

    private static void Window_KeyReleased(Window sender, TimeSpan timestamp, Scancode scancode, Keycode key, KeyModifier modifiers, bool isPressed, bool repeat, uint unicode)
    {
        Log.Debug("The key {key} of scancode {scancode} with modifiers {modifiers} was released. Repeat? {repeat}", key, scancode, modifiers, repeat);
        if (scancode is Scancode.Return or Scancode.Return2)
            Pew.Play();
    }

    private static void App_Quitting()
    {
        Log.Information("Quitting");
        Run.IsRunning = false;
    }

    private static void Window_KeyPressed(Window sender, TimeSpan timestamp, Scancode scancode, Keycode key, KeyModifier modifiers, bool isPressed, bool repeat, uint unicode)
    {
        Log.Debug("The key {key} of scancode {scancode} with modifiers {modifiers} was pressed. Repeat? {repeat}", key, scancode, modifiers, repeat);
        if (scancode is Scancode.Escape)
        {
            Log.Information("Quitting");
            Run.IsRunning = false;
        }
    }
}
