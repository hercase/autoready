using System;
using System.IO;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;

namespace AutoReady
{
    // Avisos sonoros: notas suaves de una escala mayor (ataque lento, decaimiento tipo campanita), generadas al vuelo
    // como WAV en memoria: sin archivos ni recursos. Cada aviso tiene un dibujo distinto para no confundirlos.
    static class Sounds
    {
        const int Rate = 44100;

        static readonly object gate = new object();
        static SoundPlayer current;   // una referencia viva: si se recolecta, el sonido se corta

        // Aceptada / activo: mi -> sol.
        public static void Accepted() { Play(new double[][] { Note(659.25, 0, 450), Note(783.99, 140, 600) }, 800); }

        // Empezo la partida: arpegio mayor do-mi-sol.
        public static void MatchStarted() { Play(new double[][] { Note(523.25, 0, 450), Note(659.25, 130, 450), Note(783.99, 260, 700) }, 1000); }

        // Urgente (no pudo aceptar, o hay que aceptar a mano): la misma nota aguda tres veces, mas insistente que el resto.
        public static void Urgent() { Play(new double[][] { Note(987.77, 0, 220), Note(987.77, 260, 220), Note(987.77, 520, 420) }, 1000); }

        // Pausa: sol -> do, bajando.
        public static void Paused() { Play(new double[][] { Note(783.99, 0, 400), Note(523.25, 150, 600) }, 750); }

        // ---------- cable de audio virtual (para que el canal de voz escuche el aviso) ----------
        // Windows no deja meter audio en el microfono sin un dispositivo virtual (VB-Cable, Voicemeeter). AutoReady toca
        // la misma campanita de "aceptada" en ese dispositivo; el usuario lo mezcla con su micro (ver README).

        static readonly string[] CableNames = { "CABLE Input", "Voicemeeter Input", "Voicemeeter Aux Input" };

        // Los nombres de winmm se cortan a 31 caracteres ("CABLE Input (VB-Audio Virtual C").
        internal static bool IsCableName(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            foreach (string c in CableNames)
                if (name.StartsWith(c, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        // Numero del primer dispositivo de salida que es un cable virtual; -1 si no hay.
        public static int FindCable()
        {
            try
            {
                int n = waveOutGetNumDevs();
                for (int i = 0; i < n; i++)
                {
                    WAVEOUTCAPS caps = new WAVEOUTCAPS();
                    if (waveOutGetDevCaps((IntPtr)i, ref caps, Marshal.SizeOf(typeof(WAVEOUTCAPS))) == 0 && IsCableName(caps.szPname)) return i;
                }
            }
            catch (Exception ex) { Log.Write("No pude listar los dispositivos de audio: " + ex.Message); }
            return -1;
        }

        // La campanita de aceptada en el cable virtual (si no hay, no hace nada).
        public static bool AcceptedToCable()
        {
            int dev = FindCable();
            if (dev < 0) return false;
            byte[] wav = Synthesize(new double[][] { Note(659.25, 0, 450), Note(783.99, 140, 600) }, 800);
            System.Threading.ThreadPool.QueueUserWorkItem(_ => PlayOnDevice(dev, wav));
            return true;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct WAVEFORMATEX { public ushort wFormatTag, nChannels; public uint nSamplesPerSec, nAvgBytesPerSec; public ushort nBlockAlign, wBitsPerSample, cbSize; }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct WAVEOUTCAPS
        {
            public ushort wMid, wPid; public uint vDriverVersion;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string szPname;
            public uint dwFormats; public ushort wChannels, wReserved1; public uint dwSupport;
        }

        [DllImport("winmm.dll")] static extern int waveOutGetNumDevs();
        [DllImport("winmm.dll", CharSet = CharSet.Unicode)] static extern int waveOutGetDevCaps(IntPtr device, ref WAVEOUTCAPS caps, int size);
        [DllImport("winmm.dll")] static extern int waveOutOpen(out IntPtr h, int device, ref WAVEFORMATEX fmt, IntPtr callback, IntPtr instance, int flags);
        [DllImport("winmm.dll")] static extern int waveOutPrepareHeader(IntPtr h, IntPtr hdr, int size);
        [DllImport("winmm.dll")] static extern int waveOutWrite(IntPtr h, IntPtr hdr, int size);
        [DllImport("winmm.dll")] static extern int waveOutUnprepareHeader(IntPtr h, IntPtr hdr, int size);
        [DllImport("winmm.dll")] static extern int waveOutClose(IntPtr h);

        // Toca un WAV (mono, 16 bits, 44,1 kHz, el que arma Synthesize) en un dispositivo concreto y espera a que termine.
        static void PlayOnDevice(int device, byte[] wav)
        {
            IntPtr h = IntPtr.Zero, data = IntPtr.Zero, hdr = IntPtr.Zero;
            bool prepared = false;
            int p = IntPtr.Size, hdrSize = 4 * p + 16, offFlags = 2 * p + 8;
            try
            {
                int len = wav.Length - 44;
                WAVEFORMATEX fmt = new WAVEFORMATEX { wFormatTag = 1, nChannels = 1, nSamplesPerSec = Rate, nAvgBytesPerSec = Rate * 2, nBlockAlign = 2, wBitsPerSample = 16 };
                if (waveOutOpen(out h, device, ref fmt, IntPtr.Zero, IntPtr.Zero, 0) != 0) { h = IntPtr.Zero; return; }
                data = Marshal.AllocHGlobal(len);
                Marshal.Copy(wav, 44, data, len);
                hdr = Marshal.AllocHGlobal(hdrSize);
                for (int i = 0; i < hdrSize; i++) Marshal.WriteByte(hdr, i, 0);
                Marshal.WriteIntPtr(hdr, 0, data);
                Marshal.WriteInt32(hdr, p, len);
                if (waveOutPrepareHeader(h, hdr, hdrSize) != 0) return;
                prepared = true;
                if (waveOutWrite(h, hdr, hdrSize) != 0) return;
                for (int waited = 0; waited < 4000 && (Marshal.ReadInt32(hdr, offFlags) & 1) == 0; waited += 20)   // WHDR_DONE
                    System.Threading.Thread.Sleep(20);
            }
            catch (Exception ex) { Log.Write("No se pudo tocar el sonido en el cable virtual: " + ex.Message); }
            finally
            {
                if (prepared) waveOutUnprepareHeader(h, hdr, hdrSize);
                if (h != IntPtr.Zero) waveOutClose(h);
                if (hdr != IntPtr.Zero) Marshal.FreeHGlobal(hdr);
                if (data != IntPtr.Zero) Marshal.FreeHGlobal(data);
            }
        }

        static double[] Note(double freq, double startMs, double durMs) { return new double[] { freq, startMs, durMs }; }

        static void Play(double[][] notes, int totalMs)
        {
            try
            {
                byte[] wav = Synthesize(notes, totalMs);
                lock (gate)
                {
                    if (current != null) current.Stop();
                    current = new SoundPlayer(new MemoryStream(wav));
                    current.Play();   // en su propio hilo
                }
            }
            catch (Exception ex) { Log.Write("No se pudo reproducir el sonido: " + ex.Message); }
        }

        internal static byte[] Synthesize(double[][] notes, int totalMs)
        {
            int n = (int)((long)Rate * totalMs / 1000);
            double[] buf = new double[n];
            foreach (double[] note in notes)
            {
                int start = (int)(Rate * note[1] / 1000);
                int len = (int)(Rate * note[2] / 1000);
                for (int i = 0; i < len && start + i < n; i++)
                {
                    double t = (double)i / Rate;
                    double attack = Math.Min(1.0, i / 440.0);
                    double decay = Math.Exp(-5.0 * i / len);
                    double wave = Math.Sin(2 * Math.PI * note[0] * t)
                                + 0.25 * Math.Sin(4 * Math.PI * note[0] * t)
                                + 0.08 * Math.Sin(6 * Math.PI * note[0] * t);
                    buf[start + i] += 0.35 * attack * decay * wave;
                }
            }

            MemoryStream ms = new MemoryStream();
            BinaryWriter w = new BinaryWriter(ms);
            int dataLen = n * 2;
            w.Write(Encoding.ASCII.GetBytes("RIFF")); w.Write(36 + dataLen);
            w.Write(Encoding.ASCII.GetBytes("WAVEfmt ")); w.Write(16); w.Write((short)1); w.Write((short)1);
            w.Write(Rate); w.Write(Rate * 2); w.Write((short)2); w.Write((short)16);
            w.Write(Encoding.ASCII.GetBytes("data")); w.Write(dataLen);
            foreach (double v in buf)
                w.Write((short)(Math.Max(-1.0, Math.Min(1.0, v)) * 28000));
            w.Flush();
            return ms.ToArray();
        }
    }
}
