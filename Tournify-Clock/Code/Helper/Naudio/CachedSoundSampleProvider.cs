
using Gemelo.Applications.Tournify.Clock.Code.Enumerations;

using NAudio.Wave;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemelo.Applications.Tournify.Clock.Code.Helper.Naudio
{
    class CachedSoundSampleProvider : ISampleProvider
    {
        private readonly CachedSound m_CachedSound;
        private long m_Position;

        public SoundIds? SoundId => m_CachedSound?.SoundId;

        public CachedSoundSampleProvider(CachedSound cachedSound)
        {
            this.m_CachedSound = cachedSound;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            if (m_CachedSound.IsStopPlayback) return 0;

            var availableSamples = m_CachedSound.AudioData.Length - m_Position;
            var samplesToCopy = Math.Min(availableSamples, count);
            Array.Copy(m_CachedSound.AudioData, m_Position, buffer, offset, samplesToCopy);
            m_Position += samplesToCopy;
            return (int)samplesToCopy;
        }

        public WaveFormat WaveFormat { get { return m_CachedSound.WaveFormat; } }
    }
}
