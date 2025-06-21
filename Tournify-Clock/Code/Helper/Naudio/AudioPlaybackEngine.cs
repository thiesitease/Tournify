using NAudio.Wave.SampleProviders;
using NAudio.Wave;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemelo.Applications.Tournify.Clock.Code.Helper.Naudio
{
    public class AudioPlaybackEngine : IDisposable
    {
        private readonly IWavePlayer m_OutputDevice;
        private readonly MixingSampleProvider m_Mixer;

        public static readonly AudioPlaybackEngine Default = new AudioPlaybackEngine(48000, 2);

        public AudioPlaybackEngine(int sampleRate = 44100, int channelCount = 2)
        {
            m_OutputDevice = new WaveOutEvent();
            m_Mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channelCount));
            m_Mixer.ReadFully = true;
            m_OutputDevice.Init(m_Mixer);
            m_OutputDevice.Play();
        }


        public void PlaySound(string fileName)
        {
            var input = new AudioFileReader(fileName);
            AddMixerInput(new AutoDisposeFileReader(input));
        }

        private ISampleProvider ConvertToRightChannelCount(ISampleProvider input)
        {
            if (input.WaveFormat.Channels == m_Mixer.WaveFormat.Channels)
            {
                return input;
            }
            if (input.WaveFormat.Channels == 1 && m_Mixer.WaveFormat.Channels == 2)
            {
                return new MonoToStereoSampleProvider(input);
            }
            throw new NotImplementedException("Not yet implemented this channel count conversion");
        }

        public void PlaySound(CachedSound sound)
        {
            sound.IsStopPlayback = false;
            AddMixerInput(new CachedSoundSampleProvider(sound));
        }

        private void AddMixerInput(ISampleProvider input)
        {
            m_Mixer.AddMixerInput(ConvertToRightChannelCount(input));
        }

        public void Dispose()
        {
            m_OutputDevice.Dispose();
        }

    }
}
