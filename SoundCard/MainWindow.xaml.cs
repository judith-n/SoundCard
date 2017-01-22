using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SlimDX.DirectSound;
using SlimDX.Multimedia;
using System.Windows.Interop;
using NAudio.Wave;

namespace SoundCard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SoundPlayer sound;
        WavHeader Header;
        AxWMPLib.AxWindowsMediaPlayer axWmp;
        public MainWindow()
        {
            InitializeComponent();
            Header = new WavHeader();
        }

        private void PlaySound_Click(object sender, RoutedEventArgs e)
        {
            if (PlaySound.Content.ToString() == "Play Sound")
            {
                var ofd = new OpenFileDialog()
                {
                    Multiselect = false,
                    Title = "Choose file to play"
                };
                var result = ofd.ShowDialog();
                sound = new SoundPlayer(ofd.FileName);
                sound.Play();

                PlaySound.Content = "Stop";
            }
            else
            {
                sound.Stop();
                PlaySound.Content = "Play Sound";
            }
        }

        private void getWavHeader()
        {
            var ofd = new OpenFileDialog()
            {
                Multiselect = false,
                Title = "Choose file to get header"
            };
            var result = ofd.ShowDialog();
            FileStream fileStream = new FileStream(ofd.FileName, FileMode.Open, FileAccess.Read);
            BinaryReader binaryReader = new BinaryReader(fileStream);
            Header.riffID = binaryReader.ReadBytes(4);
            Header.size = binaryReader.ReadUInt32();
            Header.wavID = binaryReader.ReadBytes(4);
            Header.fmtID = binaryReader.ReadBytes(4);
            Header.fmtSize = binaryReader.ReadUInt32();
            Header.format = binaryReader.ReadUInt16();
            Header.channels = binaryReader.ReadUInt16();
            Header.sampleRate = binaryReader.ReadUInt32();
            Header.bytePerSec = binaryReader.ReadUInt32();
            Header.blockSize = binaryReader.ReadUInt16();
            Header.bit = binaryReader.ReadUInt16();
            Header.dataID = binaryReader.ReadBytes(4);
            Header.dataSize = binaryReader.ReadUInt32();
            binaryReader.Close();
            fileStream.Close();
        }

        private void HeaderButton_Click(object sender, RoutedEventArgs e)
        {
            getWavHeader();
            hContent.Text =
                  "riffID:\t\t" + System.Text.Encoding.UTF8.GetString(Header.riffID) + "\n"
                + "size:\t\t" + Header.size + "\n"
                + "wavID:\t\t" + System.Text.Encoding.UTF8.GetString(Header.wavID) + "\n"
                + "fmtID:\t\t" + System.Text.Encoding.UTF8.GetString(Header.fmtID) + "\n"
                + "fmtSize:\t\t" + Header.fmtSize + "\n"
                + "format:\t\t" + Header.format + "\n"
                + "channels:\t" + Header.channels + "\n"
                + "sampleRate:\t" + Header.sampleRate + "\n"
                + "bytePerSec:\t" + Header.bytePerSec + "\n"
                + "blockSize:\t" + Header.blockSize + "\n"
                + "bit:\t\t" + Header.bit + "\n"
                + "dataID:\t\t" + System.Text.Encoding.UTF8.GetString(Header.dataID) + "\n"
                + "dataSize:\t\t" + Header.dataSize;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Create the interop host control.
            System.Windows.Forms.Integration.WindowsFormsHost host =
                new System.Windows.Forms.Integration.WindowsFormsHost();

            // Create the ActiveX control.
            axWmp = new AxWMPLib.AxWindowsMediaPlayer();

            // Assign the ActiveX control as the host control's child.
            host.Child = axWmp;

            // Add the interop host control to the Grid
            // control's collection of child controls.
            this.ActiveXPlayer.Children.Add(host);

            // Play a .wav file with the ActiveX control.

        }

        private void ActiveXButton_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog()
            {
                Multiselect = false,
                Title = "Choose file to play"
            };
            var result = ofd.ShowDialog();
            axWmp.URL = ofd.FileName;
            axWmp.Refresh();
        }

        private void DirectSoundButton_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog()
            {
                Multiselect = false,
                Title = "Choose file to play"
            };
            var result = ofd.ShowDialog();
            var m_DirectSound = new DirectSound();
            System.Windows.Interop.WindowInteropHelper helper = new System.Windows.Interop.WindowInteropHelper(this);

            m_DirectSound.SetCooperativeLevel(helper.Handle, CooperativeLevel.Priority);
            m_DirectSound.IsDefaultPool = false;

            using (SlimDX.Multimedia.WaveStream waveFile = new SlimDX.Multimedia.WaveStream(ofd.FileName))
            {
                SoundBufferDescription desc = new SoundBufferDescription();
                desc.SizeInBytes = (int)waveFile.Length;
                desc.Flags = BufferFlags.None;
                desc.Format = waveFile.Format;

                var m_DSoundBuffer = new SecondarySoundBuffer(m_DirectSound, desc);
                byte[] data = new byte[desc.SizeInBytes];
                waveFile.Read(data, 0, (int)waveFile.Length);
                m_DSoundBuffer.Write(data, 0, LockFlags.None);
                m_DSoundBuffer.Play(0, 0);
            }
        }

        internal class WavHeader
        {
            public byte[] riffID;
            public uint size;
            public byte[] wavID;
            public byte[] fmtID;
            public uint fmtSize;
            public ushort format;
            public ushort channels;
            public uint sampleRate;
            public uint bytePerSec;
            public ushort blockSize;
            public ushort bit;
            public byte[] dataID;
            public uint dataSize;
        }

        private void WaveOutButton_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog()
            {
                Multiselect = false,
                Title = "Choose file to play"
            };
            var result = ofd.ShowDialog();
            var waveReader = new WaveFileReader(ofd.FileName);
            var waveOut = new WaveOut();
            waveOut.Init(waveReader);
            waveOut.Play();
        }

        private void RecordButton_Click(object sender, RoutedEventArgs e)
        {
            var waveIn = new WaveIn();
            if (RecordButton.Content.ToString() == "Record Sound")
            {
                RecordButton.Content = "Stop";
                var wfw = new WaveFileWriter(@"record.wav", waveIn.WaveFormat);
                //wfw.Write(e.Buffer, 0, bytesRecorded);
                waveIn.StartRecording();
                wfw.Dispose();
            }
            else
            {
                RecordButton.Content = "Record Sound";
                waveIn.StopRecording();
            }
        }
    }
}
