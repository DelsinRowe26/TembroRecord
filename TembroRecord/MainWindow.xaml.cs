using CSCore.CoreAudioAPI;
using CSCore;
using System;
using System.Collections.Generic;
using System.Linq;
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
using CSCore.SoundOut;
using CSCore.SoundIn;
using System.Diagnostics;
using System.IO;
using CSCore.Codecs;
using CSCore.Codecs.WAV;
using static System.Net.WebRequestMethods;
using System.Threading;
using File = System.IO.File;

namespace TembroRecord
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        string langindex, fileDeleteRec1, fileDeleteCutRec1, fileDeleteRec2, fileDeleteCutRec2;
        private int SampleRate, SoundClick;
        string myfile;
        string cutmyfile;

        private WasapiOut mSoundOut;
        private WasapiCapture mSoundIn;
        private ISampleSource mMp3;
        private SimpleMixer mMixer;
        private IWaveSource mSource;
        private MMDeviceCollection mOutputDevices;
        private MMDeviceCollection mInputDevices;
        private SampleDSPTurbo mDspTurbo;

        private FileInfo fileInfo1 = new FileInfo("Data_Load.tmp");
        private FileInfo FileLanguage = new FileInfo("Data_Language.tmp");
        private FileInfo fileinfo = new FileInfo("DataTemp.tmp");

        int ImgBtnRecordClick = 0, ImgBtnPlayClick = 0, ImgBtnTembroClick = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Mixer()
        {
            try
            {

                mMixer = new SimpleMixer(1, SampleRate) //стерео, 44,1 КГц
                {
                    //Right = true,
                    //Left = true,
                    FillWithZeros = true,
                    DivideResult = true, //Для этого установлено значение true, чтобы избежать звуков тиков из-за превышения -1 и 1.
                };
            }
            catch (Exception ex)
            {
                if (langindex == "0")
                {
                    string msg = "Ошибка в Mixer: \r\n" + ex.Message;
                    MessageBox.Show(msg);
                    Debug.WriteLine(msg);
                }
                else
                {
                    string msg = "Error in Mixer: \r\n" + ex.Message;
                    MessageBox.Show(msg);
                    Debug.WriteLine(msg);
                }
            }
        }

        private void Languages()
        {
            try
            {
                StreamReader FileLanguage = new StreamReader("Data_Language.tmp");
                File.WriteAllText("Data_Load.tmp", "1");
                File.WriteAllText("DataTemp.tmp", "0");
                langindex = FileLanguage.ReadToEnd();
                if(langindex == "0")
                {
                    lbRecordPB.Content = "Идёт запись...";
                    btnRecord.ToolTip = "Запись";
                }
                else
                {
                    lbRecordPB.Content = "Recording in progress...";
                    btnRecord.ToolTip = "Record";
                }
            }
            catch (Exception ex)
            {
                if (langindex == "0")
                {
                    string msg = "Ошибка в Languages: \r\n" + ex.Message;
                    LogClass.LogWrite(msg);
                    MessageBox.Show(msg);
                    Debug.WriteLine(msg);
                }
                else
                {
                    string msg = "Error in Languages: \r\n" + ex.Message;
                    LogClass.LogWrite(msg);
                    MessageBox.Show(msg);
                    Debug.WriteLine(msg);
                }
            }
        }

        private void btnRecord_Click(object sender, RoutedEventArgs e)
        {
            YesNoWin win = new YesNoWin();
            win.ShowDialog();
            ImgBtnRecordClick = 1;
            string uri = @"TembroRecord\Button\button-record-active.png";
            ImgBackRecord.ImageSource = new ImageSourceConverter().ConvertFromString(uri) as ImageSource;
            Recording1();
            btnPlay.IsEnabled = false;
            btnTembro.IsEnabled = false;
            btnRecord.IsEnabled = false;
            btnStop.IsEnabled = false;

        }

        private void btnRecord_MouseMove(object sender, MouseEventArgs e)
        {
            string uri = @"TembroRecord\Button\button-record-hover.png";
            ImgBackRecord.ImageSource = new ImageSourceConverter().ConvertFromString(uri) as ImageSource;
        }

        private void btnRecord_MouseLeave(object sender, MouseEventArgs e)
        {
            if (ImgBtnRecordClick == 1)
            {
                string uri = @"TembroRecord\Button\button-record-active.png";
                ImgBackRecord.ImageSource = new ImageSourceConverter().ConvertFromString(uri) as ImageSource;
            }
            else
            {
                string uri = @"TembroRecord\Button\button-record-inactive.png";
                ImgBackRecord.ImageSource = new ImageSourceConverter().ConvertFromString(uri) as ImageSource;
            }
        }

        private void SoundIn()
        {
            try
            {
                mSoundIn = new WasapiCapture(/*false, AudioClientShareMode.Exclusive, 1*/);
                Dispatcher.Invoke(() => mSoundIn.Device = mInputDevices[cmbInput.SelectedIndex]);
                mSoundIn.Initialize();
                mSoundIn.Start();
            }
            catch (Exception ex)
            {
                if (langindex == "0")
                {
                    string msg = "Ошибка в SoundIn: \r\n" + ex.Message;
                    LogClass.LogWrite(msg);
                    MessageBox.Show(msg);
                    Debug.WriteLine(msg);
                }
                else
                {
                    string msg = "Error in SoundIn: \r\n" + ex.Message;
                    LogClass.LogWrite(msg);
                    MessageBox.Show(msg);
                    Debug.WriteLine(msg);
                }
            }
        }

        private void Stop()
        {
            try
            {
                if (mMixer != null)
                {
                    mMixer.Dispose();
                    //mMp3.ToWaveSource(32).Loop().ToSampleSource().Dispose();
                    mMixer = null;
                }
                if (mSoundOut != null)
                {
                    mSoundOut.Stop();
                    mSoundOut.Dispose();
                    mSoundOut = null;
                }
                if (mSoundIn != null)
                {
                    mSoundIn.Stop();
                    mSoundIn.Dispose();
                    mSoundIn = null;
                }
                if (mSource != null)
                {
                    mSource.Dispose();
                    mSource = null;
                }
                if (mMp3 != null)
                {
                    /*if (mDspRec != null)
                    {
                        mDspRec.Dispose();
                    }*/
                    mMp3.Dispose();
                    mMp3 = null;
                }

            }
            catch (Exception ex)
            {
                /*if (langindex == "0")
                {
                    string msg = "Ошибка в Stop: \r\n" + ex.Message;
                    MessageBox.Show(msg);
                    Debug.WriteLine(msg);
                }
                else
                {
                    string msg = "Error in Stop: \r\n" + ex.Message;
                    MessageBox.Show(msg);
                    Debug.WriteLine(msg);
                }*/
            }
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            Stop();
            string uri1 = @"TembroRecord\Button\button-turbo-inactive.png";
            ImgBackTembro.ImageSource = new ImageSourceConverter().ConvertFromString(uri1) as ImageSource;
            ImgBtnPlayClick = 1;
            ImgBtnTembroClick = 0;
            string uri = @"TembroRecord\Button\button-play-active.png";
            ImgBackPlay.ImageSource = new ImageSourceConverter().ConvertFromString(uri) as ImageSource;
            btnPlay.IsEnabled = false;
            btnRecord.IsEnabled = false;
            //btnTembro.IsEnabled = false;
            Sound(cutmyfile);

        }

        private void btnPlay_MouseMove(object sender, MouseEventArgs e)
        {
            string uri = @"TembroRecord\Button\button-play-hover.png";
            ImgBackPlay.ImageSource = new ImageSourceConverter().ConvertFromString(uri) as ImageSource;
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            Stop();
            string uri1 = @"TembroRecord\Button\button-play-inactive.png";
            ImgBackPlay.ImageSource = new ImageSourceConverter().ConvertFromString(uri1) as ImageSource;
            string uri2 = @"TembroRecord\Button\button-turbo-inactive.png";
            ImgBackTembro.ImageSource = new ImageSourceConverter().ConvertFromString(uri2) as ImageSource;
            btnTembro.IsEnabled = true;
            btnPlay.IsEnabled = true;
            btnRecord.IsEnabled = true;
            ImgBtnPlayClick = 0;
            ImgBtnTembroClick = 0;
            string uri = @"TembroRecord\Button\button-stop-active.png";
            ImgBackStop.ImageSource = new ImageSourceConverter().ConvertFromString(uri) as ImageSource;
        }

        private void btnStop_MouseMove(object sender, MouseEventArgs e)
        {
            string uri = @"TembroRecord\Button\button-stop-hover.png";
            ImgBackStop.ImageSource = new ImageSourceConverter().ConvertFromString(uri) as ImageSource;
        }

        private void btnTembro_Click(object sender, RoutedEventArgs e)
        {
            Stop();
            ImgBtnPlayClick = 0;
            string uri1 = @"TembroRecord\Button\button-play-inactive.png";
            ImgBackPlay.ImageSource = new ImageSourceConverter().ConvertFromString(uri1) as ImageSource;
            ImgBtnTembroClick = 1;
            string uri = @"TembroRecord\Button\button-turbo-active.png";
            ImgBackTembro.ImageSource = new ImageSourceConverter().ConvertFromString(uri) as ImageSource;
            //btnPlay.IsEnabled = false;
            btnRecord.IsEnabled = false;
            btnTembro.IsEnabled = false;
            Sound1(cutmyfile);
        }

        private void btnTembro_MouseMove(object sender, MouseEventArgs e)
        {
            string uri = @"TembroRecord\Button\button-turbo-hover.png";
            ImgBackTembro.ImageSource = new ImageSourceConverter().ConvertFromString(uri) as ImageSource;
        }

        private void btnTembro_MouseLeave(object sender, MouseEventArgs e)
        {
            if (ImgBtnTembroClick == 1)
            {
                string uri = @"TembroRecord\Button\button-turbo-active.png";
                ImgBackTembro.ImageSource = new ImageSourceConverter().ConvertFromString(uri) as ImageSource;
            }
            else
            {
                string uri = @"TembroRecord\Button\button-turbo-inactive.png";
                ImgBackTembro.ImageSource = new ImageSourceConverter().ConvertFromString(uri) as ImageSource;
            }
        }

        private void WinTembroRecord_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                Stop();
                if (File.Exists(fileDeleteRec1))
                {
                    File.Delete(fileDeleteRec1);
                }
                if (File.Exists(fileDeleteCutRec1))
                {
                    File.Delete(fileDeleteCutRec1);
                }
                if (File.Exists(fileDeleteRec2))
                {
                    File.Delete(fileDeleteRec2);
                }
                if (File.Exists(fileDeleteCutRec2))
                {
                    File.Delete(fileDeleteCutRec2);
                }
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                if (langindex == "0")
                {
                    string msg = "Ошибка в SimpleNeurotuner_Closing: \r\n" + ex.Message;
                    LogClass.LogWrite(msg);
                    MessageBox.Show(msg);
                    Debug.WriteLine(msg);
                }
                else
                {
                    string msg = "Error in SimpleNeurotuner_Closing: \r\n" + ex.Message;
                    LogClass.LogWrite(msg);
                    MessageBox.Show(msg);
                    Debug.WriteLine(msg);
                }
            }
        }

        private void btnStop_MouseLeave(object sender, MouseEventArgs e)
        {
            string uri = @"TembroRecord\Button\button-stop-inactive.png";
            ImgBackStop.ImageSource = new ImageSourceConverter().ConvertFromString(uri) as ImageSource;
        }

        private void btnPlay_MouseLeave(object sender, MouseEventArgs e)
        {
            if (ImgBtnPlayClick == 1)
            {
                string uri = @"TembroRecord\Button\button-play-active.png";
                ImgBackPlay.ImageSource = new ImageSourceConverter().ConvertFromString(uri) as ImageSource;
            }
            else
            {
                string uri = @"TembroRecord\Button\button-play-inactive.png";
                ImgBackPlay.ImageSource = new ImageSourceConverter().ConvertFromString(uri) as ImageSource;
            }
        }

        private void SoundOut()
        {
            try
            {

                mSoundOut = new WasapiOut(/*false, AudioClientShareMode.Exclusive, 1*/);
                //Dispatcher.Invoke(() => mSoundOut.Device = mOutputDevices[cmbOutput.SelectedIndex]);
                //mSoundOut.Device = mOutputDevices[cmbOutput.SelectedIndex];


                if (SoundClick == 0)
                {
                    mSoundOut.Initialize(mMp3.ToWaveSource(32).ToMono());
                    mSoundOut.Volume = 5;
                }
                else
                {
                    mSoundOut.Initialize(mMixer.ToWaveSource(32).ToMono());
                    mSoundOut.Volume = 15;
                }


                mSoundOut.Play();


            }
            catch (Exception ex)
            {
                if (langindex == "0")
                {
                    string msg = "Ошибка в SoundOut: \r\n" + ex.Message;
                    LogClass.LogWrite(msg);
                    MessageBox.Show(msg);
                    Debug.WriteLine(msg);

                }
                else
                {
                    string msg = "Error in SoundOut: \r\n" + ex.Message;
                    LogClass.LogWrite(msg);
                    MessageBox.Show(msg);
                    Debug.WriteLine(msg);

                }
            }
        }

        private async void Sound(string FileName)
        {
            try
            {
                SoundClick = 0;
                Mixer();
                mMp3 = CodecFactory.Instance.GetCodec(FileName).ToMono().ToSampleSource();
                mMixer.AddSource(mMp3.ChangeSampleRate(mMp3.WaveFormat.SampleRate).ToWaveSource().Loop().ToSampleSource());
                await Task.Run(() => SoundOut());
            }
            catch (Exception ex)
            {
                if (langindex == "0")
                {
                    string msg = "Ошибка в Sound: \r\n" + ex.Message;
                    LogClass.LogWrite(msg);
                    MessageBox.Show(msg);
                    Debug.WriteLine(msg);
                }
                else
                {
                    string msg = "Error in Sound: \r\n" + ex.Message;
                    LogClass.LogWrite(msg);
                    MessageBox.Show(msg);
                    Debug.WriteLine(msg);
                }
            }
        }

        private async void Sound1(string FileName)
        {
            try
            {
                SoundClick = 1;
                Mixer();
                mMp3 = CodecFactory.Instance.GetCodec(FileName).ToMono().ToSampleSource();
                mDspTurbo = new SampleDSPTurbo(mMp3.ToWaveSource(32).ToSampleSource());
                mMixer.AddSource(mDspTurbo.ChangeSampleRate(mDspTurbo.WaveFormat.SampleRate).ToWaveSource(32).Loop().ToSampleSource());
                await Task.Run(() => SoundOut());
            }
            catch (Exception ex)
            {
                if (langindex == "0")
                {
                    string msg = "Ошибка в Sound: \r\n" + ex.Message;
                    LogClass.LogWrite(msg);
                    MessageBox.Show(msg);
                    Debug.WriteLine(msg);
                }
                else
                {
                    string msg = "Error in Sound: \r\n" + ex.Message;
                    LogClass.LogWrite(msg);
                    MessageBox.Show(msg);
                    Debug.WriteLine(msg);
                }
            }
        }

        private async void Recording1()
        {
            try
            {
                myfile = "MyRecord1.wav";
                cutmyfile = "cutMyRecord1.wav";
                fileDeleteRec1 = myfile;
                fileDeleteCutRec1 = cutmyfile;
                if (File.Exists(myfile))
                {
                    File.Delete(myfile);
                }
                if (File.Exists(cutmyfile))
                {
                    File.Delete(cutmyfile);
                }
                using (mSoundIn = new WasapiCapture())
                {
                    mSoundIn.Device = mInputDevices[cmbInput.SelectedIndex];
                    mSoundIn.Initialize();

                    mSoundIn.Start();
                    pbRecord.Visibility = Visibility.Visible;
                    lbRecordPB.Visibility = Visibility.Visible;
                    using (WaveWriter record = new WaveWriter(cutmyfile, mSoundIn.WaveFormat))
                    {
                        mSoundIn.DataAvailable += (s, data) => record.Write(data.Data, data.Offset, data.ByteCount);
                        for (int i = 0; i < 100; i++)
                        {
                            pbRecord.Value++;
                            await Task.Delay(100);
                            if (pbRecord.Value == 25)
                            {
                                string uri1 = @"TembroRecord\Button\Group 13.png";
                                ImgPBRecordBack.ImageSource = new ImageSourceConverter().ConvertFromString(uri1) as ImageSource;
                            }
                            else if (pbRecord.Value == 50)
                            {
                                string uri2 = @"TembroRecord\Button\Group 12.png";
                                ImgPBRecordBack.ImageSource = new ImageSourceConverter().ConvertFromString(uri2) as ImageSource;
                            }
                            else if (pbRecord.Value == 75)
                            {
                                string uri3 = @"TembroRecord\Button\Group 11.png";
                                ImgPBRecordBack.ImageSource = new ImageSourceConverter().ConvertFromString(uri3) as ImageSource;
                            }
                            else if (pbRecord.Value == 95)
                            {
                                string uri4 = @"TembroRecord\Button\Group 10.png";
                                ImgPBRecordBack.ImageSource = new ImageSourceConverter().ConvertFromString(uri4) as ImageSource;
                            }
                        }
                        //Thread.Sleep(5000);

                        mSoundIn.Stop();
                        lbRecordPB.Visibility = Visibility.Hidden;
                        pbRecord.Value = 0;
                        pbRecord.Visibility = Visibility.Hidden;

                    }
                    Thread.Sleep(1000);
                    string uri = @"TembroRecord\Button\progressbar-backgrnd.png";
                    ImgPBRecordBack.ImageSource = new ImageSourceConverter().ConvertFromString(uri) as ImageSource;
                    btnRecord.IsEnabled = true;
                    btnStop.IsEnabled = true;
                    btnPlay.IsEnabled = true;
                    btnTembro.IsEnabled = true;

                }
                if (langindex == "0")
                {
                    ImgBtnRecordClick = 0;
                    string uri = @"TembroRecord\Button\button-record-inactive.png";
                    ImgBackRecord.ImageSource = new ImageSourceConverter().ConvertFromString(uri) as ImageSource;
                    string msg = "Запись и обработка завершена.";
                    MessageBox.Show(msg);
                    LogClass.LogWrite(msg);
                }
                else
                {
                    ImgBtnRecordClick = 0;
                    string msg = "Recording and processing completed. A graphic representation of your voice will now appear.";
                    LogClass.LogWrite(msg);
                }
            }
            catch (Exception ex)
            {
                if (langindex == "0")
                {
                    string msg = "Ошибка в Recording1: \r\n" + ex.Message;
                    LogClass.LogWrite(msg);
                    MessageBox.Show(msg);
                    Debug.WriteLine(msg);
                }
                else
                {
                    string msg = "Error in Recording1: \r\n" + ex.Message;
                    LogClass.LogWrite(msg);
                    MessageBox.Show(msg);
                    Debug.WriteLine(msg);
                }
            }
        }

        private void WinTembroRecord_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                

                if (SoftCl.IsSoftwareInstalled("Microsoft Visual C++ 2015-2022 Redistributable (x86) - 14.32.31332") == false)
                {
                    Process.Start("VC_redist.x86.exe");
                }

                MMDeviceEnumerator deviceEnum = new MMDeviceEnumerator();
                mInputDevices = deviceEnum.EnumAudioEndpoints(DataFlow.Capture, DeviceState.Active);
                MMDevice activeDevice = deviceEnum.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Multimedia);

                SampleRate = activeDevice.DeviceFormat.SampleRate;

                TembroClass tembro = new TembroClass();
                string PathFile = @"TembroRecord\Pattern\Wide_voice_effect.tmp";
                tembro.Tembro(SampleRate, PathFile);

                foreach (MMDevice device in mInputDevices)
                {
                    cmbInput.Items.Add(device.FriendlyName);
                    if (device.DeviceID == activeDevice.DeviceID) cmbInput.SelectedIndex = cmbInput.Items.Count - 1;
                }


                //Находит устройства для вывода звука и заполняет комбобокс
                activeDevice = deviceEnum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                mOutputDevices = deviceEnum.EnumAudioEndpoints(DataFlow.Render, DeviceState.Active);

                foreach (MMDevice device in mOutputDevices)
                {
                    cmbOutput.Items.Add(device.FriendlyName);
                    if (device.DeviceID == activeDevice.DeviceID) cmbOutput.SelectedIndex = cmbOutput.Items.Count - 1;
                }

                

                string[] filename = File.ReadAllLines(fileInfo1.FullName);
                if (filename.Length == 1)
                {
                    Languages();
                }
                if (!File.Exists("log.tmp"))
                {
                    File.Create("log.tmp").Close();
                }
                else
                {
                    if (File.ReadAllLines("log.tmp").Length > 1000)
                    {
                        File.WriteAllText("log.tmp", " ");
                    }
                }
            }
            catch (Exception ex)
            {
                if (langindex == "0")
                {
                    string msg = "Ошибка в Loaded: \r\n" + ex.Message;
                    LogClass.LogWrite(msg);
                    MessageBox.Show(msg);
                    Debug.WriteLine(msg);
                }
                else
                {
                    string msg = "Error in Loaded: \r\n" + ex.Message;
                    LogClass.LogWrite(msg);
                    MessageBox.Show(msg);
                    Debug.WriteLine(msg);
                }
            }
        }


    }
}
