using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Speech.Synthesis;
using Microsoft.Speech.Recognition;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading;
using System.IO;
using Newtonsoft.Json;

namespace WindowsFormsApplication4 {
    public partial class Form1 : Form {

        // ATIVAR MOTO DESENVOLVIDOR
        public bool dev = false;

        public Form1() {
            InitializeComponent();

            // Fullscreen
            //FormBorderStyle = FormBorderStyle.None;
            //Bounds = Screen.PrimaryScreen.Bounds;

            // Click-thru
            //Opacity = 0.9999999f;
            //int ini = (int)GetWindowLong(this.Handle, -20);
            //SetWindowLong(this.Handle, -20, ini | 0x80000 | 0x20);
            Visible = true;
        }
        
        /** - - - - - IMPORTAR FUNÇÕES/SIGNATURES DO S.O. - - - - - **/
        [Flags]
        enum SendMessageTimeoutFlags : uint {
            SMTO_NORMAL = 0x0,
            SMTO_BLOCK = 0x1,
            SMTO_ABORTIFHUNG = 0x2,
            SMTO_NOTIMEOUTIFNOTHUNG = 0x8,
            SMTO_ERRORONEXIT = 0x20
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint Msg, UIntPtr wParam, IntPtr lParam, uint fuFlags, uint uTimeout, out UIntPtr lpdwResult);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessageTimeout(IntPtr windowHandle, uint Msg, IntPtr wParam, IntPtr lParam, uint flags, uint timeout, out IntPtr result);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow( [MarshalAs(UnmanagedType.LPTStr)] string lpClassName, [MarshalAs(UnmanagedType.LPTStr)] string lpWindowName);

        [DllImport("user32.dll")]
        static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, IntPtr windowTitle);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);


        /** - - - - - APP - - - - - **/
        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);
            this.Invalidate();

            paintToDesktop();
        }

        /** - - - - - TEXT TO SPEECH | SPEECH TO TEXT - - - - - **/
        SpeechSynthesizer tts = new SpeechSynthesizer();
        SpeechRecognitionEngine stt;
        private void Form1_Load(object sender, EventArgs e) {
            // TTS
            tts.SetOutputToDefaultAudioDevice();
            tts.Volume = 100;  // 0...100
            tts.Rate = -2;     // -10...10
            tts.SelectVoice("IVONA 2 Ricardo OEM");
            tts.SpeakAsync("Olá mestre! Estou a seu dispôr.");
            
            // STT
            dicionario();
            stt.RecognizeAsync(RecognizeMode.Multiple);
            stt.SpeechRecognized += stt_SpeechRecognized;
        }

        Fala[] falas;
        void dicionario() {
            stt = new SpeechRecognitionEngine(Thread.CurrentThread.CurrentCulture);
            listRegocnizers();
            listVoices(tts);
            stt.SetInputToDefaultAudioDevice();
            falas = JsonConvert.DeserializeObject<Fala[]>(File.ReadAllText("C:/Atom/Falas/falas.json"));
            foreach (Fala f in falas) 
                obtemFalas(f, null);

            falasTotais.ForEach(e => {
                text.Text += "\n" + e;
            });

            Choices dict = new Choices(falasTotais.ToArray());

            GrammarBuilder gb = new GrammarBuilder();
            gb.Culture = Thread.CurrentThread.CurrentCulture;
            gb.Append(dict);
            Grammar grammar = new Grammar(gb);
            stt.LoadGrammar(grammar);
        }

        List<String> falasTotais = new List<String>();
        public void obtemFalas(Fala fala, Fala pai) {
            if (fala.fala.IndexOf("|") != -1) {
                string[] strs = fala.fala.Split('|');
                foreach (string str in strs) {
                    falasTotais.Add(str);
                }
            } else
                falasTotais.Add(fala.fala);
            fala.pai = pai;
            if (fala.libera == null) return;
            foreach (Fala f in fala.libera) {
                obtemFalas(f, fala);
            }
        }

        void listVoices(SpeechSynthesizer stt) {
            text.Text += "\n\nVozes:";
            ReadOnlyCollection<InstalledVoice> c = stt.GetInstalledVoices();
            for (int i = 0; i < c.Count; i++) {
                text.Text += "\n" + c[i].VoiceInfo.Name;
            }
        }

        void listRegocnizers() {
            text.Text += "\n\nReconhecedores:";
            ReadOnlyCollection<Microsoft.Speech.Recognition.RecognizerInfo> c = Microsoft.Speech.Recognition.SpeechRecognitionEngine.InstalledRecognizers();
            for (int i = 0; i < c.Count; i++) {
                text.Text += "\n" + c[i].Name + " - " + c[i].Culture.ToString();
            }
        }

        Fala ultima = null;
        void stt_SpeechRecognized(object sender, SpeechRecognizedEventArgs e) {
            text.Text += ("\n" + e.Result.Text + " (" + (e.Result.Confidence*100) + "%)"); 
            if (e.Result.Confidence < .70)
                return;

            string resultado = e.Result.Text;
            bool feito = false;
            if (ultima!=null&&ultima.libera!=null)
                foreach (Fala ff in ultima.libera)
                    if (match(ff, resultado)) {
                        aceita(ff);
                        ultima = ff;
                        feito = true;
                        return;
                    }
            if (!feito)
                foreach (Fala ff in falas)
                    if (match(ff, resultado)) {
                        aceita(ff);
                        ultima = ff;
                        return;
                    }
        }

        bool match(Fala fala, string resultado) {
            if (fala.fala.IndexOf("|") != -1) {
                string[] strs = fala.fala.Split('|');
                foreach (string str in strs)
                    if (str.Equals(resultado))
                        return true;
                return false;
            }
            else
                return fala.fala.Equals(resultado);
        }

        void aceita(Fala fala) {
            tts.SpeakAsync(fala.resposta);
            if (fala.libera == null) ultima = null;
            if (fala.cmd == null) return;
            string[] cmd = fala.cmd.Split(' ');
            
            // COMANDOS
            if (cmd[0].Equals("atlz")) {
                Form2.instance.Refresh();
            } else if (cmd[0].Equals("play")) {
                Form2.play(fala.cmd.Substring(5));
            } else if (cmd[0].Equals("unplay")) {
                Form2.unplay();
            }
        }

        /** - - - - - PAINT TO DESKTOP - - - - - **/
        Form2 wallpaper;
        void paintToDesktop() {
            IntPtr progman = FindWindow("Progman", null);
            IntPtr result = IntPtr.Zero;
            SendMessageTimeout(progman, 0x052C, new IntPtr(0), IntPtr.Zero, (uint) SendMessageTimeoutFlags.SMTO_NORMAL, 1000, out result);
            IntPtr workerw = IntPtr.Zero;
            
            EnumWindows(new EnumWindowsProc((tophandle, topparamhandle) => {
                IntPtr p = FindWindowEx(tophandle, IntPtr.Zero, "SHELLDLL_DefView", IntPtr.Zero);
                // Gets the WorkerW Window after the current one.
                if (p != IntPtr.Zero) 
                    workerw = FindWindowEx(IntPtr.Zero, tophandle, "WorkerW", IntPtr.Zero);
                return true;
            }), IntPtr.Zero);
            
            wallpaper = new Form2();
            wallpaper.Text = "Wallpaper";
            wallpaper.Load += new EventHandler((s, e) => { SetParent(wallpaper.Handle, workerw); });
            if (!dev)
                Application.Run(wallpaper);
        }
    }
}
