using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Net;
using Leaf.xNet;
using Youtube_Viewers.Helpers;
using System.IO;

namespace Youtube_Viewer_Form
{
    public partial class Form1 : Form
    {
        static string id = "";
        static int threadsCount = 1000;

        static ProxyQueue scraper;
        static ProxyType proxyType = ProxyType.HTTP;

        static int botted = 0;
        static int errors = 0;

        static string viewers = "Parsing...";
        static string title = "Parsing...";

        public Form1()
        {
            InitializeComponent();
        }

        static string buildUrl(Dictionary<string, string> args)
        {
            var url = "https://s.youtube.com/api/stats/watchtime?";
            foreach (var arg in args)
            {
                url += $"{arg.Key}={arg.Value}&";
            }
            return url;
        }

        private void Log()
        {
            while (true)
            {
                label4.Text = botted.ToString();
                label5.Text = errors.ToString();
                label7.Text = viewers.ToString();
                label9.Text = title;
                Thread.Sleep(250);
            }
        }


        private void Worker()
        {
            while (true)
            {
                try
                {
                    using (var req = new HttpRequest()
                    {
                        Proxy = scraper.Next(),
                        Cookies = new CookieStorage()
                    })
                    {
                        req.UserAgentRandomize();
                        req.Cookies.Container.Add(new Uri("https://www.youtube.com"), new Cookie("CONSENT", "YES+cb.20210629-13-p0.en+FX+407"));

                        var sres = req.Get($"https://www.youtube.com/watch?v={textBox1.Text}").ToString();
                        var viewersTemp = string.Join("", RegularExpressions.Viewers.Match(sres).Groups[1].Value.Where(char.IsDigit));

                        if (!string.IsNullOrEmpty(viewersTemp))
                            viewers = viewersTemp;

                        title = RegularExpressions.Title.Match(sres).Groups[1].Value;

                        var url = RegularExpressions.ViewUrl.Match(sres).Groups[1].Value;
                        url = url.Replace(@"\u0026", "&").Replace("%2C", ",").Replace(@"\/", "/");

                        var query = System.Web.HttpUtility.ParseQueryString(url);

                        var cl = query.Get(query.AllKeys[0]);
                        var ei = query.Get("ei");
                        var of = query.Get("of");
                        var vm = query.Get("vm");
                        var cpn = GetCPN();

                        var start = DateTime.UtcNow;

                        var st = random.Next(1000, 10000);
                        var et = GetCmt(start);
                        var lio = GetLio(start);

                        var rt = random.Next(10, 200);

                        var lact = random.Next(1000, 8000);
                        var rtn = rt + 300;

                        var args = new Dictionary<string, string>
                        {
                            ["ns"] = "yt",
                            ["el"] = "detailpage",
                            ["cpn"] = cpn,
                            ["docid"] = textBox1.Text,
                            ["ver"] = "2",
                            ["cmt"] = et.ToString(),
                            ["ei"] = ei,
                            ["fmt"] = "243",
                            ["fs"] = "0",
                            ["rt"] = rt.ToString(),
                            ["of"] = of,
                            ["euri"] = "",
                            ["lact"] = lact.ToString(),
                            ["live"] = "dvr",
                            ["cl"] = cl,
                            ["state"] = "playing",
                            ["vm"] = vm,
                            ["volume"] = "100",
                            ["cbr"] = "Firefox",
                            ["cbrver"] = "83.0",
                            ["c"] = "WEB",
                            ["cplayer"] = "UNIPLAYER",
                            ["cver"] = "2.20201210.01.00",
                            ["cos"] = "Windows",
                            ["cosver"] = "10.0",
                            ["cplatform"] = "DESKTOP",
                            ["delay"] = "5",
                            ["hl"] = "en_US",
                            ["rtn"] = rtn.ToString(),
                            ["aftm"] = "140",
                            ["rti"] = rt.ToString(),
                            ["muted"] = "0",
                            ["st"] = st.ToString(),
                            ["et"] = et.ToString(),
                            ["lio"] = lio.ToString()
                        };

                        string urlToGet = buildUrl(args);
                        req.AcceptEncoding = "gzip, deflate";
                        req.AddHeader("Host", "www.youtube.com");
                        req.Get(urlToGet.Replace("watchtime", "playback"));

                        req.AcceptEncoding = "gzip, deflate";
                        req.AddHeader("Host", "www.youtube.com");
                        req.Get(urlToGet);

                        Interlocked.Increment(ref botted);

                    }
                }
                catch
                {
                    Interlocked.Increment(ref errors);
                }

                Thread.Sleep(1);
            }
        }

        public static double GetCmt(DateTime date)
        {
            var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var start = date.ToUniversalTime() - origin;
            var now = DateTime.UtcNow.ToUniversalTime() - origin;
            var value = (now.TotalSeconds - start.TotalSeconds).ToString("#.000");
            return double.Parse(value);
        }

        public static double GetLio(DateTime date)
        {
            var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var start = date.ToUniversalTime() - origin;
            var value = start.TotalSeconds.ToString("#.000");
            return double.Parse(value);
        }

        private static Random random = new Random();
        public static string GetCPN()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";
            return new string(Enumerable.Repeat(chars, 16)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private void button1_Click(object sender, EventArgs e)
        {

            for (int i = 0; i < 1500; i++)
            {
                List<Thread> threads = new List<Thread>();

                Thread t = new Thread(Worker);
                t.Start();
                threads.Add(t);
            }

            List<Thread> Logthread = new List<Thread>();

            Thread logWorker = new Thread(Log);
            logWorker.Start();
            Logthread.Add(logWorker);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Proxy list (*.txt)|*.txt";

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            scraper = new ProxyQueue(File.ReadAllText(dialog.FileName), proxyType);

            label12.Text = scraper.Length.ToString();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Environment.Exit(1);
        }
    }
}
