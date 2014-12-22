﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibCommon
{
    public class MySpeech
    {
        private static MySpeech _instance = null;
        private SpeechLib.SpVoiceClass voice = null;
        //private SpeechLib.SpVoice voice = null;
        private System.IO.StreamReader _reader = null;

        private MySpeech()
        {
            Initialize();
        }

        public static MySpeech Instance()
        {
            if (_instance == null)
                _instance = new MySpeech();
            return _instance;
        }

        private void Initialize()
        {
            try
            {
                voice = new SpeechLib.SpVoiceClass();
                voice.EndStream += Voice_EndStream;

                SpeechLib.ISpeechObjectTokens objTokens = voice.GetVoices("", "");
                const string useVoice = "ScanSoft Mei-Ling_Full_22kHz";
                int useIndex = -1;
                for (int i = 0; i < objTokens.Count; i++)
                {
                    SpeechLib.SpObjectToken sot = objTokens.Item(i);
                    if (sot.GetDescription(0) == useVoice)
                    {
                        useIndex = i;
                        break;
                    }
                }
                if (useIndex == -1)
                {
                    useIndex = 0;
                }
                voice.Voice = objTokens.Item(useIndex);
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show("Error:" + e.Message);
            }
        }

        public void Voice_EndStream(int StreamNumber, object StreamPosition)
        {

        }

        public int Volume
        {
            get
            {
                return voice.Volume;
            }
            set
            {
                voice.SetVolume((ushort)(value));
                //voice.Volume = value;
            }
        }

        public int Rate
        {
            get
            {
                return voice.Rate;
            }
            set
            {
                voice.SetRate(value);
            }
        }

        public void Speak()
        {
            try
            {
                _reader = new System.IO.StreamReader
                    (
                    System.Windows.Forms.Application.StartupPath + "\\NoticeTxt.txt",
                    System.Text.Encoding.Default
                    );
                string text = _reader.ReadToEnd();
                _reader.Close();
                voice.Speak(text, SpeechLib.SpeechVoiceSpeakFlags.SVSFlagsAsync);
            }
            catch (Exception err)
            {
                throw (new Exception("an error occurs: " + err.Message));
            }
        }

        public void Stop()
        {
            voice.Speak(string.Empty,
                SpeechLib.SpeechVoiceSpeakFlags.SVSFPurgeBeforeSpeak);
        }

        public void Pause()
        {
            voice.Pause();
        }

        public void Continue()
        {
            voice.Resume();
        }
    }
}
