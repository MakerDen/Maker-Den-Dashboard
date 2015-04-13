using MQTTProcessor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace ProcessMQTT.WPF.ViewModels {
    public class MainVM : BaseVM {


        // const string SensorNamespace = "gb/#";
        const string DeviceCapabilitiesNameSpace = "gbdevice/#";

        private MQTTManager mgr;
        private readonly factVM temp;
        private readonly factVM humidity;
        private readonly factVM light;
        private readonly factVM sound;

        // How long (in minutes) should a sensor remain on the dashboard after its last update
        private double sensorTTL = 2d;
        private DispatcherTimer cleanupTimer;
        private DispatcherTimer statsTimer;

        private ObservableCollection<factVM> sensors;
        public ObservableCollection<factVM> Sensors { get { sensors = sensors ?? new ObservableCollection<factVM>(); return sensors; } }
        public factVM Temp { get { return temp; } }
        public factVM Humidity { get { return humidity; } }
        public factVM Light { get { return light; } }
        public factVM Sound { get { return sound; } }

        private SettingsVM settingsVM;

        public SettingsVM SettingsVM {
            get { settingsVM = settingsVM ?? new SettingsVM(); return settingsVM; }
        }




        public MainVM() {
            Application.Current.MainWindow.Closing += new CancelEventHandler(MainWindow_Closing);

            showSettingsPaneCommand = new SimpleCommand(showSettingsPane);

            restartMqttManager();

            temp = new factVM();
            humidity = new factVM();
            sound = new factVM();
            light = new factVM();

            SettingsVM.PropertyChanged += SettingsVM_PropertyChanged;

            cleanupTimer = new DispatcherTimer();
            cleanupTimer.Tick += cleanupTimer_Tick;
            cleanupTimer.Interval = TimeSpan.FromSeconds(10);
            cleanupTimer.Start();

            statsTimer = new DispatcherTimer();
            statsTimer.Tick += statsTimer_Tick;
            statsTimer.Interval = TimeSpan.FromSeconds(10);
            statsTimer.Start();

            getStats();
        }

        private SimpleCommand showSettingsPaneCommand;

        public SimpleCommand ShowSettingsPaneCommand {
            get { return showSettingsPaneCommand; }
            set { showSettingsPaneCommand = value; }
        }

        private void showSettingsPane() {
            SettingsVM.Show();
        }


        void SettingsVM_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == "MqttBrokerAddress" || e.PropertyName == "MqttTopic") {
                restartMqttManager();
            }
        }

        private void restartMqttManager() {
            if (mgr != null) {
                mgr.OnDataEvent -= mgr_OnDataEvent;
                mgr.Disconnect();
                mgr = null;
            }

            if (Sensors != null) {
                Sensors.Clear();
            }
            try {
                mgr = new MQTTManager(SettingsVM.MqttBrokerAddress, SettingsVM.MqttTopic, true);
                mgr.OnDataEvent += mgr_OnDataEvent;
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }

        void MainWindow_Closing(object sender, CancelEventArgs e) {
            mgr.Disconnect();

            mgr.OnDataEvent -= mgr_OnDataEvent;
            cleanupTimer.Tick -= cleanupTimer_Tick;
            statsTimer.Tick -= statsTimer_Tick;
            SettingsVM.PropertyChanged -= SettingsVM_PropertyChanged;

            cleanupTimer.Stop();
            statsTimer.Stop();
        }

        private async void getStats() {
            //if you lose connectivity then this crashes
            try {
                var lightTask = httpClient.GetStringAsync(string.Format(countServiceUri, "light"));
                var tempTask = httpClient.GetStringAsync(string.Format(countServiceUri, "temp"));
                var soundTask = httpClient.GetStringAsync(string.Format(countServiceUri, "sound"));
                Task.WaitAll(new Task[] { lightTask, tempTask, soundTask });
                LightCount = JsonConvert.DeserializeObject<int>(lightTask.Result);
                TempCount = JsonConvert.DeserializeObject<int>(tempTask.Result);
                SoundCount = JsonConvert.DeserializeObject<int>(soundTask.Result);
            }
            catch (Exception ex) {
                Debug.WriteLine(ex.Message);
            }
        }

        private HttpClient httpClient = new HttpClient();
        private string countServiceUri = "http://OrleansSensorCounter.cloudapp.net/api/sensor/{0}";

        void statsTimer_Tick(object sender, EventArgs e) {
            getStats();
        }

        private int lightCount;
        public int LightCount {
            get { return lightCount; }
            set { if (lightCount == value) return; lightCount = value; NotifyPropertyChanged(); }
        }
        private int tempCount;
        public int TempCount {
            get { return tempCount; }
            set { if (tempCount == value) return; tempCount = value; NotifyPropertyChanged(); }
        }
        private int soundCount;
        public int SoundCount {
            get { return soundCount; }
            set { if (soundCount == value) return; soundCount = value; NotifyPropertyChanged(); }
        }

        private string selectedCommand;
        public string SelectedCommand {
            get { return selectedCommand; }
            set { if (selectedCommand == value) return; selectedCommand = value; NotifyPropertyChanged(); }
        }

        private string deviceID;
        public string DeviceID {
            get { return deviceID; }
            set { if (deviceID == value) return; deviceID = value; NotifyPropertyChanged(); }
        }

        private ObservableCollection<string> commands;
        public ObservableCollection<string> Commands {
            get {
                commands = commands ?? getCommands();
                return commands;
            }
        }

        private ObservableCollection<string> getCommands() {
            // put any commands you want prepopulated in the combo box into the list here
            return new ObservableCollection<string>()
            {
               "on/relay01", "off/relay01", "hello"
            };
        }

        void cleanupTimer_Tick(object sender, EventArgs e) {
            // get a list of the Sensors to be cleaned up
            if (Sensors.Count(s => s.LastUpdated.AddMinutes(sensorTTL) < DateTime.UtcNow) > 0) {
                var oldSensors = Sensors.Where(s => s.LastUpdated.AddMinutes(sensorTTL) < DateTime.UtcNow).ToList();
                foreach (var sensor in oldSensors) {
                    Sensors.Remove(sensor);
                }
            }
        }

        void mgr_OnDataEvent(object sender, EventArgs e) {

            // make sure we're on the despatcher thread
            Application.Current.Dispatcher.Invoke(
                () => {
                    var fact = ((MQTTManager.DataEventArgs)e).fact;

                    switch (fact.Type) {
                        case "light":
                            Light.Add(fact);
                            break;
                        case "temp":
                            Temp.Add(fact);
                            break;
                        case "humd":
                            humidity.Add(fact);
                            break;
                        case "sound":
                            Sound.Add(fact);
                            break;
                        case "mem":
                            fact.Val[0] = fact.Val[0] / 1000;
                            fact.Topic = fact.Topic + "(kb)";
                            break;
                        default:
                            break;
                    }

                    // now look up the factVM to add this to
                    // var matchingSensors = Sensors.Count(s => s.Topic == fact.Topic);
                    var thisSensor = Sensors.Where(s => s.Topic == fact.Topic).FirstOrDefault();
                    if (thisSensor == null) {
                        thisSensor = new factVM();
                        // work out where to add the sensor (sorted by topic)
                        int addAt = 0;
                        foreach (var sensor in Sensors) {
                            if (fact.Topic.CompareTo(sensor.Topic) < 0)
                                break;
                            addAt++;
                        }
                        Sensors.Insert(addAt, thisSensor);
                    }
                    if (thisSensor != null)
                        thisSensor.Add(fact);
                },
                DispatcherPriority.Normal
            );
        }


        internal void PostCommand() {
            string actionItem = string.Empty;
            string parameters = string.Empty;
            // this is where to do the MQTT Command Magic
            // You have access to SelectedCommand and DeviceID
            // if the command was valid you can also get the system to remember it
            if (string.IsNullOrEmpty(selectedCommand)) { return; }

            int index = selectedCommand.IndexOf(' ');
            if (index == -1) { actionItem = selectedCommand; }
            else {
                actionItem = selectedCommand.Substring(0, index);
                parameters = selectedCommand.Substring(index + 1);
            }

            mgr.Publish(CreateTopic(DeviceID, actionItem), parameters);

            RememberCommand();
        }


        private string CreateTopic(string deviceId, string actionItem) {
            string result = string.Empty;
            if (string.IsNullOrEmpty(deviceId)) { result = "gbcmd/all/" + actionItem; }
            else { result = "gbcmd/dev/" + deviceId + "/" + actionItem; }
            return result;
        }

        private void RememberCommand() {
            if (SelectedCommand == null)
                return;

            var existingCommand = Commands.Where(c => c.ToLowerInvariant() == SelectedCommand.ToLowerInvariant()).FirstOrDefault();
            if (existingCommand == null) {
                Commands.Reverse();
                Commands.Add(SelectedCommand);
                Commands.Reverse();
            }
            else {
                Commands.Remove(existingCommand);
                Commands.Reverse();
                Commands.Add(existingCommand);
                Commands.Reverse();
            }
        }

        internal void SetDefaults()
        {
            
        }
    }

    public class factVM : BaseVM {

        // TTL for facts before they're cleaned out of the list
        private double factTTL = 5;
        private DispatcherTimer cleanupTimer;
        private bool doingCleanup = false;


        public factVM() {
            facts = new ObservableCollection<Fact>();

            facts.CollectionChanged += facts_CollectionChanged;

            cleanupTimer = new DispatcherTimer();
            cleanupTimer.Tick += doCleanup;
            cleanupTimer.Interval = TimeSpan.FromSeconds(10);
            cleanupTimer.Start();
        }

        private void doCleanup(object sender, EventArgs e) {
            doingCleanup = true;

            try {
                // get a list of the objects to be cleaned up
                var oldFacts = facts.Where(f => f.Utc.AddMinutes(factTTL) < DateTime.UtcNow).ToList();
                foreach (var fact in oldFacts) {
                    facts.Remove(fact);
                }
            }
            catch (Exception ex) {

            }
            finally {
                doingCleanup = false;
            }

        }


        void facts_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            if (doingCleanup)
                return;
            NotifyPropertyChanged("Topic");
            NotifyPropertyChanged("Type");
            NotifyPropertyChanged("Heading");
            NotifyPropertyChanged("Val");
            NotifyPropertyChanged("Max");
            NotifyPropertyChanged("Min");
            NotifyPropertyChanged("Avg");
            NotifyPropertyChanged("LastUpdated");
        }

        private readonly ObservableCollection<Fact> facts;
        public ObservableCollection<Fact> Facts {
            get {
                return facts;
            }
        }

        public double Val { get { return Facts.Count == 0 ? 0 : Facts.LastOrDefault().Val[0]; } }


        public double Max {
            get { return Facts.Count == 0 ? 0 : Facts.Max(f => f.Val[0]); }
        }

        public double Min {
            get { return Facts.Count == 0 ? 0 : Facts.Min(f => f.Val[0]); }
        }

        public double Avg {
            get { return Facts.Count == 0 ? 0 : Facts.Average(f => f.Val[0]); }
        }

        public string Topic {
            get { return Facts.Count == 0 ? string.Empty : Facts.LastOrDefault().Topic; }
        }

        public string Type {
            get { return Facts.Count == 0 ? string.Empty : Facts.LastOrDefault().Type; }
        }

        public DateTime LastUpdated {
            get {
                return Facts.Count == 0 ? DateTime.MinValue : Facts.Max(f => f.Utc);
            }
        }

        public string Heading {
            get {
                string heading;

                switch (Type.ToLowerInvariant()) {
                    case "light":
                        heading = "Light";
                        break;
                    case "temp":
                        heading = "Temperature";
                        break;
                    case "sound":
                        heading = "Sound";
                        break;
                    default:
                        heading = "Unknown";
                        break;
                }

                return heading;
            }
        }

        public void Add(Fact f) {
            facts.Add(f);
        }
    }

}
