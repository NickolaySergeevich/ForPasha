using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace ForPasha
{
    public partial class MainPage : ContentPage
    {
        private class NextWay
        {
            public bool justNext;
            public bool haveOptions;

            public string wayWords;
            public string wayFileName;

            public Dictionary<string[], string> options;

            public NextWay(bool justNext = true, bool haveOptions = false, string wayWords = "", string wayFileName = "")
            {
                this.justNext = justNext;
                this.haveOptions = haveOptions;
                this.wayWords = wayWords;
                this.wayFileName = wayFileName;

                if (haveOptions)
                    options = new Dictionary<string[], string>();
            }
        }

        Plugin.SimpleAudioPlayer.ISimpleAudioPlayer audio = Plugin.SimpleAudioPlayer.CrossSimpleAudioPlayer.Current;

        private Dictionary<string, NextWay[]> ways = new Dictionary<string, NextWay[]>();
        private List<string> whereBe;
        private string playNow = "INTRO-F1";

        public MainPage()
        {
            InitializeComponent();

            string[] lines = Regex.Replace(Resource.Settings, "\r", "").Split('\n');
            whereBe = new List<string>(lines.Length);

            foreach (string line in lines)
            {
                string[] parts = line.Split(':');

                if (parts[1].Count(f => f == ';') == 0)
                {
                    ways.Add(parts[0], new NextWay[] { new NextWay(wayFileName: parts[1]) });
                }
                else
                {
                    string[] waysNext = parts[1].Split(';');
                    ways.Add(parts[0], new NextWay[waysNext.Length]);

                    for (int i = 0; i < waysNext.Length; ++i)
                    {
                        if (waysNext[i].Count(f => f == '(') == 0)
                        {
                            string[] wayWordsWithDest = waysNext[i].Split('_');
                            ways[parts[0]][i] = new NextWay(justNext: false, wayWords: wayWordsWithDest[0], wayFileName: wayWordsWithDest[1]);
                        }
                        else
                        {
                            string[] wayWordsWithDestAndOptions = waysNext[i].Split('_');
                            ways[parts[0]][i] = new NextWay(justNext: false, haveOptions: true, wayWords: wayWordsWithDestAndOptions[0]);

                            for (int j = 1; j < wayWordsWithDestAndOptions.Length - 1; ++j)
                            {
                                string[] wayNameAndOptions = wayWordsWithDestAndOptions[j].Split('(');
                                ways[parts[0]][i].options.Add(wayNameAndOptions[1].Split(','), wayNameAndOptions[0]);
                            }
                            ways[parts[0]][i].wayFileName = wayWordsWithDestAndOptions[wayWordsWithDestAndOptions.Length - 1];
                        }
                    }
                }
            }

            StartPlay();
        }

        private void StartPlay()
        {
            audio.Stop();

            Stream stream = typeof(App).GetTypeInfo().Assembly.GetManifestResourceStream("ForPasha.mp3." + playNow + ".mp3");
            audio.Load(stream);
            audio.Play();

            SetLableText();
        }

        private void SetLableText()
        {
            whereBe.Add(playNow);

            label_main.Text = String.Empty;
            if (ways[playNow][0].justNext)
            {
                label_main.Text = "Next: " + ways[playNow][0].wayFileName;
                return;
            }

            foreach (NextWay nextWay in ways[playNow])
                label_main.Text += nextWay.wayWords + '\n';
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            if (ways[playNow][0].justNext)
            {
                playNow = ways[playNow][0].wayFileName;
                whereBe.Add(playNow);
                StartPlay();
                return;
            }

            string nextWords = entry_main.Text;

            foreach (NextWay way in ways[playNow])
            {
                if (way.wayWords == nextWords)
                {
                    if (!way.haveOptions)
                    {
                        playNow = way.wayFileName;
                        whereBe.Add(playNow);
                        StartPlay();
                        return;
                    }
                    else
                    {
                        foreach (string[] needBe in way.options.Keys)
                        {
                            foreach (string needBeFileName in needBe)
                            {
                                if (whereBe.Contains(needBeFileName))
                                {
                                    playNow = way.options[needBe];
                                    whereBe.Add(playNow);
                                    StartPlay();
                                    return;
                                }
                            }
                        }

                        playNow = way.wayFileName;
                        whereBe.Add(playNow);
                        StartPlay();
                        return;
                    }
                }
            }
        }
    }
}
