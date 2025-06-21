using Gemelo.Applications.Tournify.Clock.Code.Enumerations;

using NAudio.Wave;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemelo.Applications.Tournify.Clock.Code.Helper.Naudio
{
   public class CachedSound
    {
        public SoundIds SoundId {get; }

        public float[] AudioData { get; private set; }
        public WaveFormat WaveFormat { get; private set; }

        public bool IsStopPlayback { get; set; }

        public CachedSound(string audioFileName, SoundIds soundID)
        {
            SoundId = soundID;
            using (var audioFileReader = new AudioFileReader(audioFileName))
            {
                // TODO: could add resampling in here if required
                WaveFormat = audioFileReader.WaveFormat;
                var wholeFile = new List<float>((int)(audioFileReader.Length / 4));
                var readBuffer = new float[audioFileReader.WaveFormat.SampleRate * audioFileReader.WaveFormat.Channels];
                int samplesRead;
                while ((samplesRead = audioFileReader.Read(readBuffer, 0, readBuffer.Length)) > 0)
                {
                    wholeFile.AddRange(readBuffer.Take(samplesRead));
                }
                AudioData = wholeFile.ToArray();
            }
        }
    }
}
