using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Text.RegularExpressions;
using System.IO;
using System.Net;

namespace GuessAPicture
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static Image image = new Image();
        static bool isImageWasAdded = false;
        //static Dictionary<string, string> paintersAndUrls = new Dictionary<string, string>();
        //static Dictionary<string, string> paintersAndPaintingsUrls = new Dictionary<string, string>();
        static List<Painter> painters = new List<Painter>();

        static int correctGuessesCount = 0;
        static int wrongGuessesCount = 0;

        static string[] currentQuestion;

        public MainWindow()
        {
            InitializeComponent();

            wrapPanel.Width = image.Width;
            wrapPanel.Height = image.Height;

            InitializePaintersList();

            currentQuestion = GetQuestion();

            UpdateQuestionUI();

            UpdateScoreLabel();

            //FillPaiterUrlsOfPaintings(0);

            //var t = GetImgUrlFromPictureUrl(painters[0].PaintingsUrls[0]);

            //var foo = GetQuestion();

            //ChangeImage("http://sr.gallerix.ru/73167723/B/1699319714/");
            //ChangeImage("http://sr.gallerix.ru/73167723/B/1391119563/");     
        }

        void ChangeImage(string imgUrl)
        {
            if (isImageWasAdded)
                wrapPanel.Children.RemoveAt(wrapPanel.Children.Count - 1);

            //http://gallerix.ru/	
            var fullFilePath = imgUrl;

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(fullFilePath, UriKind.Absolute);
            bitmap.EndInit();

            image.Source = bitmap;
            wrapPanel.Children.Add(image);

            isImageWasAdded = true;
        }

        //static string GetHtmlByURL(string url)
        //{
        //    var html = "";

        //    using (var webClient = new WebClient())
        //    {
        //        webClient.UseDefaultCredentials = true;

        //        Stream data = webClient.OpenRead(url);
        //        StreamReader reader = new StreamReader(data);
        //        html = reader.ReadToEnd();
        //    }

        //    return html;
        //}

        static string DownloadHtml(string uri)
        {
            HttpWebRequest request = WebRequest.Create(uri) as HttpWebRequest;
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.3; rv:41.0) Gecko/20100101 Firefox/41.0";
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*;q=0.8";
            request.Headers.Set("Accept-Language", "en-US;q=0.8,en-US;q=0.5,en;q=0.3");

            request.Headers.Set("DNT", "1");
            request.CookieContainer = new CookieContainer();

            request.KeepAlive = true;

            try
            {
                var response = request.GetResponse() as HttpWebResponse;

                StreamReader sr = new StreamReader(response.GetResponseStream());
                string html = sr.ReadToEnd();
                return html;
            }
            catch
            {
                return "";
            }
        }

        void InitializePaintersList()
        {
            var html = DownloadHtml("http://gallerix.ru/");

            var regex = "valign=\"top\"><a href='(?<URL>.+?)'.+?alt=\"Художник: (?<Painter>.+?)\"";

            var matches = Regex.Matches(html, regex, RegexOptions.Singleline);

            foreach (Match m in matches)
            {
                var painter = m.Groups["Painter"].Value;
                var url = m.Groups["URL"].Value;

                //paintersAndUrls[painter] = url;
                painters.Add(new Painter { Name = painter, URL = url });
            }
        }

        static void FillPaiterUrlsOfPaintings(int painterIndex)
        {
            var paintingsUrls = new List<string>();

            var html = DownloadHtml(painters[painterIndex].URL);

            var regex = "<div class=\"a_shad1\">.+?'top'>.+?<a href=\"(?<URL>.+?)\"";

            var matches = Regex.Matches(html, regex, RegexOptions.Singleline);

            foreach (Match m in matches)
            {
                var url = m.Groups["URL"].Value;

                paintingsUrls.Add(url);
            }

            painters[painterIndex].PaintingsUrls = paintingsUrls.ToArray();
        }

        static string GetImgUrlFromPictureUrl(string pictureUrl)
        {
            // http://sr.gallerix.ru/442966767/_EX/735147328/
            var regex = "<meta itemprop='image' content='(?<URL>.+?)'";

            var html = DownloadHtml("http://gallerix.ru" + pictureUrl);

            var url = Regex.Match(html, regex).Groups["URL"].Value;

            return url;
        }

        static Random rnd = new Random();
        static string[] GetQuestion()
        {
            // 0 - img url
            // 1 - correct answer
            try
            {
                var question = new List<string>();

                var rndPainterId = rnd.Next(painters.Count);
                var rndPainterName = painters[rndPainterId].Name;

                if (painters[rndPainterId].PaintingsUrls == null)
                    FillPaiterUrlsOfPaintings(rndPainterId);

                var rndPaintingUrlId = rnd.Next(painters[rndPainterId].PaintingsUrls.Count());
                var rndPaintingUrl = painters[rndPainterId].PaintingsUrls[rndPaintingUrlId];

                var pictureUrl = GetImgUrlFromPictureUrl(rndPaintingUrl);

                question.Add(pictureUrl); // 0

                question.Add(rndPainterName); // 1

                while (question.Count != 5)
                {
                    var wrongAnswer = painters[rnd.Next(painters.Count)].Name;

                    if (!question.Contains(wrongAnswer))
                        question.Add(wrongAnswer);
                }

                return question.ToArray();
            }
            catch
            {
                return GetQuestion();
            }            
        }

        void UpdateScoreLabel()
        {
            var score = "";

            score += string.Format("Правильно: {0}", correctGuessesCount) + Environment.NewLine;
            score += string.Format("Ошибок: {0}", wrongGuessesCount);

            Score_label.Content = score;
        }

        void UpdateQuestionUI()
        {
            ChangeImage(currentQuestion[0]);

            var rndCorrectRadioButton = rnd.Next(4);

            switch(rndCorrectRadioButton)
            {
                case 0: Variant1_radioButton.Content = currentQuestion[1]; break;
                case 1: Variant2_radioButton.Content = currentQuestion[1]; break;
                case 2: Variant3_radioButton.Content = currentQuestion[1]; break;
                case 3: Variant4_radioButton.Content = currentQuestion[1]; break;
            }

            int currWrongAnswerIndex = 2;

            for(int i = 0; i < 4; i++)
            {
                if (i == rndCorrectRadioButton) continue;

                switch (i)
                {
                    case 0: Variant1_radioButton.Content = currentQuestion[currWrongAnswerIndex]; break;
                    case 1: Variant2_radioButton.Content = currentQuestion[currWrongAnswerIndex]; break;
                    case 2: Variant3_radioButton.Content = currentQuestion[currWrongAnswerIndex]; break;
                    case 3: Variant4_radioButton.Content = currentQuestion[currWrongAnswerIndex]; break;
                }

                currWrongAnswerIndex++;
            }

            Variant1_radioButton.IsChecked = false;
            Variant2_radioButton.IsChecked = false;
            Variant3_radioButton.IsChecked = false;
            Variant4_radioButton.IsChecked = false;
        }

        class Painter
        {
            public string Name { get; set; }
            public string URL { get; set; }
            public string[] PaintingsUrls { get; set; }
        }

        private void Answer_button_Click(object sender, RoutedEventArgs e)
        {
            var userVariant = "";

            if ((bool)Variant1_radioButton.IsChecked)
                userVariant = Variant1_radioButton.Content.ToString();
            else if ((bool)Variant2_radioButton.IsChecked)
                userVariant = Variant2_radioButton.Content.ToString();
            else if ((bool)Variant3_radioButton.IsChecked)
                userVariant = Variant3_radioButton.Content.ToString();
            else if ((bool)Variant4_radioButton.IsChecked)
                userVariant = Variant4_radioButton.Content.ToString();
            else
                return;

            if (userVariant == currentQuestion[1])
                correctGuessesCount++;
            else
            {
                wrongGuessesCount++;

                MessageBox.Show(string.Format("Верный ответ: {0}", currentQuestion[1]), "Не врено", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            UpdateScoreLabel();

            currentQuestion = GetQuestion();

            UpdateQuestionUI();
        }
    }
}
