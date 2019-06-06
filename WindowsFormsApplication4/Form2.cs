using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace WindowsFormsApplication4
{
    public partial class Form2 : Form
    {
        public static WebBrowser instance;
        public Form2()
        {
            InitializeComponent();
            FormBorderStyle = FormBorderStyle.None;
            Bounds = Screen.PrimaryScreen.Bounds;
            webBrowser1.Bounds = Bounds;
            instance = webBrowser1;
        }

        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);
            unplay();
        }

        public static void play(string file){
            string createText = "source.setAttribute('src', '" + file + "'); video.appendChild(source); video.play();";
            File.WriteAllText("C:/Atom/Tocador/naoaltere.js", createText);
            instance.Url = new Uri("file:///C:/Atom/Tocador/tocador.html");
        }

        public static void unplay() {
            instance.Url = new Uri("file:///C:/Atom/Wallpaper/wallpaper.html");
        }
    }
}
