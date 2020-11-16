using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;
using System.Net.NetworkInformation;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using Java.Text;
using Java.Util;

namespace Prayer_Times
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class MainActivity : AppCompatActivity
    {
        private static readonly string fileName = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "prayer_times.json");

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            FindViewById<TextView>(Resource.Id.dateTest).Text = GetCurrentDate().Replace("-", "/");

            if (!isFileFound())
            {
                GetData();
            }
            else if (!(CheckData()))
            {
                GetData();
            }
            else
            {
                UpdateTables(ReadData());
            }
        }

        private static bool isFileFound()
        {
            return System.IO.File.Exists(fileName);
        }

        bool CheckData()
        {
            GetCurrentDate();

            bool blnMatched = false;
            JArray jsonArray = JsonConvert.DeserializeObject<JArray>(ReadData());

            foreach (JToken jToken in jsonArray)
            {
                if (GetCurrentDate() == jToken["salah_timing"].Value<string>("date"))
                {
                    blnMatched = true;
                }
            }

            return blnMatched;
        }

        private static string GetCurrentDate()
        {
            Java.Util.Calendar calendar = Java.Util.Calendar.GetInstance(Locale.Default);
            SimpleDateFormat SDFormat = new SimpleDateFormat();
            SDFormat.ApplyPattern("YYYY-MM-dd");
            return SDFormat.Format(calendar.Time);
        }

        void GetData()
        {
            string responseFromServer;

            if (NetworkInterface.GetIsNetworkAvailable())
            {
                WebRequest webRequest = WebRequest.Create("https://www.masjidnow.com/embeds/daily_widget?masjid_id=9679&options[show_adhan]=true&options[show_monthly_info]=true");
                HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
                System.IO.Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                responseFromServer = reader.ReadToEnd();
                reader.Close();
                dataStream.Close();
                response.Close();

                SaveData(ExtractPrayerTimes(responseFromServer));
                UpdateTables(ExtractPrayerTimes(responseFromServer));
            }
        }

        private static string ExtractPrayerTimes(string responseFromServer)
        {
            return Regex.Split((Regex.Split(responseFromServer, "\"salah_timings\":")[1]), ",\"donation_url\"")[0];
        }

        void UpdateTables(string JSONExtracted)
        {
            JArray jsonArray = JsonConvert.DeserializeObject<JArray>(JSONExtracted);

            foreach (JToken jToken in jsonArray)
            {
                if (GetCurrentDate() == jToken["salah_timing"].Value<string>("date"))
                {
                    SetTextData(jToken);
                }
            }
        }

        private void SetTextData(JToken jToken)
        {
            FindViewById<TextView>(Resource.Id.fajr_start).Text = jToken["salah_timing"].Value<string>("fajr_adhan");
            FindViewById<TextView>(Resource.Id.sunrise_start).Text = jToken["salah_timing"].Value<string>("sunrise_adhan");
            FindViewById<TextView>(Resource.Id.zuhr_start).Text = jToken["salah_timing"].Value<string>("dhuhr_adhan");
            FindViewById<TextView>(Resource.Id.asr_start).Text = jToken["salah_timing"].Value<string>("asr_adhan");
            FindViewById<TextView>(Resource.Id.maghrib_start).Text = jToken["salah_timing"].Value<string>("maghrib_adhan");
            FindViewById<TextView>(Resource.Id.isha_start).Text = jToken["salah_timing"].Value<string>("isha_adhan");

            FindViewById<TextView>(Resource.Id.fajr_jamat).Text = jToken["salah_timing"].Value<string>("fajr");
            FindViewById<TextView>(Resource.Id.zuhr_jamat).Text = jToken["salah_timing"].Value<string>("dhuhr");
            FindViewById<TextView>(Resource.Id.asr_jamat).Text = jToken["salah_timing"].Value<string>("asr");
            FindViewById<TextView>(Resource.Id.maghrib_jamat).Text = jToken["salah_timing"].Value<string>("maghrib");
            FindViewById<TextView>(Resource.Id.isha_jamat).Text = jToken["salah_timing"].Value<string>("isha");
        }

        void SaveData(string content)
        {
            using (StreamWriter write = System.IO.File.CreateText(fileName))
            {
                write.Write(content);
            }
        }

        string ReadData()
        {
            string JSONExtracted;
            using (StreamReader read = new StreamReader(fileName, true))
            {
                JSONExtracted = read.ReadToEnd();
            }

            return JSONExtracted;
        }
    }
}