using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Ghicitori.DataSource
{
    public class GhicitoriDataSource
    {
        private ObservableCollection<Ghicitoare> ToateGhicitorile = new ObservableCollection<Ghicitoare>();
        public bool isDataLoaded;
        public ObservableCollection<Ghicitoare> GhicitoriNerezolvate
        {
            get
            {
                List<Ghicitoare>  ghlist = ToateGhicitorile.Where(gh => gh.isResoleved == false).ToList();
                ObservableCollection<Ghicitoare> ghobs = new ObservableCollection<Ghicitoare>();
                foreach (Ghicitoare gh in ghlist)
                {
                    ghobs.Add(gh);
                }
                return ghobs;

            }
        }
        public GhicitoriDataSource()
        {
            isDataLoaded = false;
        }
        public async Task LoadData()
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            if (localSettings.Values.ContainsKey("ghicitori"))
            {
                string serializedobj = (string)localSettings.Values["ghicitori"];
                ToateGhicitorile = (ObservableCollection<Ghicitoare>)JsonConvert.DeserializeObject(serializedobj);
            }
            else
            {

                Windows.Storage.StorageFolder localFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
                StorageFile file = await localFolder.GetFileAsync("data.json");
                string data = "";
                if (file != null)
                {
                    IBuffer buffer = await FileIO.ReadBufferAsync(file);
                    DataReader reader = DataReader.FromBuffer(buffer);
                    byte[] fileContent = new byte[reader.UnconsumedBufferLength];
                    reader.ReadBytes(fileContent);
                    data = Encoding.UTF8.GetString(fileContent, 0, fileContent.Length);
                }
                //String data = await FileIO.ReadTextAsync(file);
                JArray ghicitori = JArray.Parse(data);
                Random random = new Random();
                List<int> folosit = new List<int>();
         
                foreach (JObject ghicitoare in ghicitori)
                {
                    string content = (string)ghicitoare.GetValue("content");
                    string answer = (string)ghicitoare.GetValue("answer");
                    Ghicitoare gh = new Ghicitoare();
                    gh.Content = content;
                    gh.Answer = answer;
                    int id = random.Next(0, ghicitori.Count);
                    while (folosit.Contains(id))
                    {
                        id = random.Next(0, ghicitori.Count);
                    }
                    gh.Id = id;
                    gh.isResoleved = false;
                    folosit.Add(id);
                    ToateGhicitorile.Add(gh);
                }
            }
            isDataLoaded = true;
        }
    }
    public class Ghicitoare
    {
        public string Content { get; set; }
        public string Answer { get; set; }
        private int _Id { get; set; }
        public int Id
        {
            get
            {
                return _Id+1;
            }
            set
            {
                _Id = value;
            }
        }
        public bool isResoleved { get; set; }
    }
}
