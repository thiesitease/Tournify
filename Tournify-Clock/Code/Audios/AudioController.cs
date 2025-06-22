using Gemelo.Applications.Tournify.Clock.Code.Enumerations;
using Gemelo.Applications.Tournify.Clock.Code.Helper.Naudio;
using Gemelo.Components.Common.Settings;
using Gemelo.Components.Common.Wpf.Threading;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;

namespace Gemelo.Applications.Tournify.Clock.Code.Audios;
public class AudioController
{
    public static AudioController Default { get; } = new AudioController();

    private SpeechSynthesizer m_SpeechSynthesizer;

    public AudioController()
    {
        m_DictCachedSounds = new Dictionary<SoundIds, CachedSound>();
        m_FolderSounds = Directories.GetDirectoryInApplicationDirectory(@"Data\Sounds");
        m_DelayedCalls = new List<DelayedCall>();

        //m_SpeechSynthesizer = new SpeechSynthesizer();
        m_SpeechSynthesizer?.SetOutputToDefaultAudioDevice();
    }

    public void InitAndWelcome()
    {
        m_SpeechSynthesizer?.Speak("Willkommen beim Kiwi Cup");

        InitAllSounds();
    }

    public void SpeakAsync(string text)
    {
        try
        {
            m_SpeechSynthesizer?.SpeakAsync(text);
        }
        catch (Exception ex)
        {
            Debugger.Break();
        }
    }

    //public bool UseKitOutput { get; set; }

    public void Speak(AudioOutputcontent outputcontent, string text = null)
    {
        if (outputcontent == AudioOutputcontent.FreeText)
        {
            SpeakAsync(text);
        }
        else
        {
            switch (outputcontent)
            {
                case AudioOutputcontent.OneMinuteTooPlay:
                    PlaySound(SoundIds.OneMinuteTooPlay);
                    break;
                case AudioOutputcontent.OneMinuteToStart:
                    PlaySound(SoundIds.OneMinuteToStart);
                    break;
                case AudioOutputcontent.Anpfiff:
                    PlaySound(SoundIds.Anpfiff);
                    break;
                case AudioOutputcontent.Abpfiff:
                    PlaySound(SoundIds.Abpfiff);
                    break;
                default:
                    break;
            }
        }
    }

    public const string TextOneMinuteToPlay = "Noch eine Minute zu Spielen!";
    public const string TextOneMinuteToStart = "Noch eine Minute bis zum Start!";
    //public const string Text = "";
    //public const string Text = "";
    //public const string Text = "";
    //public const string Text = "";

    // https://markheath.net/post/fire-and-forget-audio-playback-with

    #region private Member

    private DirectoryInfo m_FolderSounds;
    private Dictionary<SoundIds, CachedSound> m_DictCachedSounds;

    private List<DelayedCall> m_DelayedCalls;

    #endregion private Member


    #region öffentliche Methoden

    private async Task InitAllSounds()
    {
        try
        {
            foreach (SoundIds id in Enum.GetValues(typeof(SoundIds)))
            {
                await InitSound(id);
                //PlaySound(id);
            }
        }
        catch (Exception ex)
        {
            Debugger.Break();
        }
        try
        {
            //PlaySound(SoundIds.Anpfiff);
            //await Task.Delay(15000);
            //PlaySound(SoundIds.OneMinuteTooPlay);
            //await Task.Delay(15000);
            //PlaySound(SoundIds.Abpfiff);
            //await Task.Delay(5000);
            //PlaySound(SoundIds.OneMinuteToStart);
        }
        catch (Exception ex)
        {
            Debugger.Break();
        }

    }

    public void StopDelayedCalls()
    {
        foreach (DelayedCall dc in m_DelayedCalls)
        {
            if (!dc.IsLaunched) dc.Cancel();
        }
        m_DelayedCalls.Clear();
    }

    public void PlaySoundDelayed(SoundIds sound, TimeSpan delay)
    {
        DelayedCall dc = DelayedCall.Start(delay, () => PlaySound(sound));
        m_DelayedCalls.Add(dc);
    }

    public void PlaySound(SoundIds sound)
    {
        //Debug.WriteLine($"PlaySound {sound}");
        if (m_DictCachedSounds.ContainsKey(sound))
        {
            AudioPlaybackEngine.Default.PlaySound(m_DictCachedSounds[sound]);
        }
    }

    public void StopSounds(params SoundIds[] ids)
    {
        if (ids != null)
        {
            foreach (var kvp in m_DictCachedSounds)
            {
                if (ids.Contains(kvp.Key))
                {
                    kvp.Value.IsStopPlayback = true;
                }
            }
        }
    }

    #endregion öffentliche Methoden

    #region private Methoden

    private async Task InitSound(SoundIds id)
    {
        string path = GetPathFor(id);
        if (File.Exists(path))
        {
            m_DictCachedSounds[id] = new CachedSound(path, id);
        }
        else
        {
            Debugger.Break();
        }
    }

    private string GetPathFor(SoundIds id)
    {
        string fileName;
        switch (id)
        {
            case SoundIds.Abpfiff:
                fileName = @"Abpiff.wav";
                break;
            case SoundIds.Anpfiff:
                fileName = @"Anpfiff2.wav";
                break;
            case SoundIds.OneMinuteToStart:
                fileName = @"Nächstes Spiel in einer Minute.wav";
                break;
            case SoundIds.OneMinuteTooPlay:
                fileName = @"nur noch eine Minute zu spielen.wav";
                break;

            default:
                fileName = "";
                break;
        }

        string result = Path.Combine(m_FolderSounds.FullName, fileName);


        return result;
    }


    #endregion private Methoden
}

