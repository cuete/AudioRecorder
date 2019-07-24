using System;
using NAudio.Wave;
using NAudio.CoreAudioApi;
using System.Linq;
using System.Diagnostics;

namespace AudioRecorder
{
    class Program
    {
        public static WaveFileWriter writer = null;
        public static IWaveIn captureDevice;
        public static Stopwatch stopwatch = new Stopwatch();

        static void Main(string[] args)
        {
            int unixTimestamp = (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            MMDevice device = LoadWasapiDevice();
            string outputFilename = $"C:\\workspace\\hackathon\\recording{unixTimestamp.ToString()}.wav";
            captureDevice = CreateWaveInDevice();
            var waveIn = new WaveInEvent();
            writer = new WaveFileWriter(outputFilename, captureDevice.WaveFormat);
            Console.WriteLine("Press any key to start recording...");
            Console.ReadKey();
            stopwatch.Start();
            captureDevice.StartRecording();

            Console.WriteLine("Recording... Press any key to stop.");
            Console.ReadKey();

            StopRecording();
            Console.WriteLine($"File saved to {outputFilename}");
        }

        public static IWaveIn CreateWaveInDevice()
        {
            IWaveIn newWaveIn;
            newWaveIn = new WaveInEvent() { DeviceNumber = 0 };
            var sampleRate = 16000;
            var channels = 1;
            newWaveIn.WaveFormat = new WaveFormat(sampleRate, channels);

            //Alternative
            //MMDevice device = LoadWasapiDevice();
            //newWaveIn = new WasapiCapture(device);

            newWaveIn.DataAvailable += OnDataAvailable;
            newWaveIn.RecordingStopped += OnRecordingStopped;
            return newWaveIn;
        }

        private static MMDevice LoadWasapiDevice()
        {
            var deviceEnum = new MMDeviceEnumerator();
            var devices = deviceEnum.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active).ToList();
            return devices.First();
        }

        static void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            writer.Write(e.Buffer, 0, e.BytesRecorded);
            int secondsRecorded = (int)(writer.Length / writer.WaveFormat.AverageBytesPerSecond);
            if (secondsRecorded >= 30)
            {
                StopRecording();
            }
            else
            {
                Console.Write($"{String.Format("{0:00}:{1:00}", stopwatch.Elapsed.Minutes, stopwatch.Elapsed.Seconds)}\r");
            }
        }

        static void OnRecordingStopped(object sender, StoppedEventArgs e)
        {
            Console.WriteLine($"\nRecording stopped.");
        }

        static void StopRecording()
        {
            captureDevice?.StopRecording();
            Cleanup();
        }

        static void Cleanup()
        {
            if (captureDevice != null)
            {
                captureDevice.Dispose();
                captureDevice = null;
            }
            FinalizeWaveFile();
        }

        static void FinalizeWaveFile()
        {
            writer?.Dispose();
            writer = null;
        }

    }
}
