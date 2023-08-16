﻿using System.Runtime.CompilerServices;
using SDL2.Bindings;

namespace SDL2.NET.SDLMixer.Exceptions;

[Serializable]
public class MixerAudioChunkCreationException : MixerException
{
    public MixerAudioChunkCreationException() { }
    public MixerAudioChunkCreationException(string message) : base(message) { }
    public MixerAudioChunkCreationException(string message, Exception inner) : base(message, inner) { }
    protected MixerAudioChunkCreationException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowIfLessThan(int value, int comparison)
    {
        if (value < comparison)
            throw new MixerAudioChunkCreationException(SDL_mixer.Mix_GetError());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowIfEquals(int value, int comparison)
    {
        if (value == comparison)
            throw new MixerAudioChunkCreationException(SDL_mixer.Mix_GetError());
    }
}
