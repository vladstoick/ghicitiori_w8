﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Ghicitori.DataSource
{
    public class GhicitoriDataSource
    {
        public ObservableCollection<Ghicitoare> ToateGhicitorile = new ObservableCollection<Ghicitoare>();
        public bool isDataLoaded;
        public ObservableCollection<Ghicitoare> GhicitoriNerezolvate
        {
            get
            {
                List<Ghicitoare>  ghlist = ToateGhicitorile.Where(gh => gh.IsResolved == false).OrderBy(gh=> gh.Id).ToList();
                ObservableCollection<Ghicitoare> ghobs = new ObservableCollection<Ghicitoare>();
                foreach (Ghicitoare gh in ghlist)
                {
                    ghobs.Add(gh);
                }
                return ghobs;

            }
        }
        public List<int> _ResolvedGhicitori = new List<int>();
        public List<int> ResolvedGhictiori
        {
            get
            {
                return _ResolvedGhicitori;
            }
            set
            {
                _ResolvedGhicitori = value;
            }
        }
        public ObservableCollection<Ghicitoare> GhicitoriRezolvate
        {
            get
            {
                List<Ghicitoare> ghlist = ToateGhicitorile.Where(gh => gh.IsResolved == true).OrderBy(gh => gh.Id).ToList();
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
                ResolvedGhictiori = (List<int>)JsonConvert.DeserializeObject<List<int>>(serializedobj);
            }
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
            int id = 0;        
            foreach (JObject ghicitoare in ghicitori)
            {
                string content = (string)ghicitoare.GetValue("content");
                string answer = (string)ghicitoare.GetValue("answer");
                Ghicitoare gh = new Ghicitoare();
                gh.Content = content;
                gh.Answer = answer;
                gh.Id = id;
                gh.IsResolved = ResolvedGhictiori.Contains(id);
                id++;
                ToateGhicitorile.Add(gh);
            }
            localSettings.Values["ghicitori"] = JsonConvert.SerializeObject(ResolvedGhictiori);
            
            isDataLoaded = true;
        }
    }
    public class Ghicitoare
    {
        public string Content { get; set; }
        public string Answer { get; set; }
        private int _Id { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
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
        private bool _IsResoleved { get; set; }
        public bool IsResolved
        {
            get
            {
                return _IsResoleved;
            }
            set
            {
                _IsResoleved = value;

                OnPropertyChanged("IsResolved");
            }
        }
        public char TransformIntoEnglishCar(char a)
        {
            if ( a == 'ă' || a == 'â' || a == 'Ă' || a == 'Â')
            {
                return 'a';
            }
            if ( a == 'ț' ||  a == 'Ț')
            {
                return 't';
            }
            if (a == 'ș' ||  a == 'Ș' || a == 'ş')
            {
                return 's';
            }
            if (a == 'î' ||  a == 'Î')
            {
                return 'i';
            }
            return char.ToLower(a);

        }
        public bool CheckIfSolution(string text){
            //VERIFICARE LUNGIME
            if (text.Length != Answer.Length)
            {
                return false;
            }
            for(int i=0; i < text.Length ; i++ )
            {
                if(TransformIntoEnglishCar(text[i])!=TransformIntoEnglishCar(Answer[i]))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
