using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProcessMQTT.WPF.ViewModels
{
    public class SettingsVM : BaseVM
    {

        public SettingsVM() : base()
        {
            hideSettingsCommand = new SimpleCommand(hideSettings);
            defaultSettingsCommand = new SimpleCommand(defaultSettings);
            loadSettings();

        }

        private string defaultMqttBrokerAddress = "gloveboxAE.cloudapp.net";
        private string defaultMqttTopic = "gb/#";

        private void loadSettings()
        {
            if (File.Exists("settings.json"))
            {
                var input = File.ReadAllText("settings.json");
                var settings = JsonConvert.DeserializeObject<Settings>(input);
                if(settings != null)
                {
                    mqttBrokerAddress = settings.MqttBrokerAddress;
                    mqttTopic = settings.MqttTopic;
                }
            }
            else
            {
                // for now, just use default values
                mqttBrokerAddress = defaultMqttBrokerAddress;
                mqttTopic = defaultMqttTopic;
            }
        }
        private void SaveSettings()
        {
            var settings = JsonConvert.SerializeObject(new Settings() { MqttBrokerAddress = this.MqttBrokerAddress, MqttTopic = this.MqttTopic });
            try
            {
                File.WriteAllText("settings.json", settings);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }        
        
        private string mqttBrokerAddress;

        public string MqttBrokerAddress
        {
            get { return mqttBrokerAddress; }
            set { if (mqttBrokerAddress == value) return; mqttBrokerAddress = value; NotifyPropertyChanged(); SaveSettings(); }
        }



        private string mqttTopic;

        public string MqttTopic
        {
            get 
            { 
                return mqttTopic; 
                // return defaultMqttTopic;
            }
            set { if (mqttTopic == value) return; mqttTopic = value; NotifyPropertyChanged(); SaveSettings(); }
        }


        private bool visible;

        public bool Visible
        {
            get { return visible; }
            private set { if (visible == value) return; visible = value; NotifyPropertyChanged(); }
        }

        public void Show()
        { 
            Visible = true; 
        }

        public void Hide()
        {
            Visible = false;
        }

        private SimpleCommand hideSettingsCommand;

        public SimpleCommand HideSettingsCommand
        {
            get { return hideSettingsCommand; }
            set { hideSettingsCommand = value; }
        }

        private void hideSettings()
        {
            Hide();
        }

        private SimpleCommand defaultSettingsCommand;

        public SimpleCommand DefaultSettingsCommand
        {
            get { return defaultSettingsCommand; }
            set { defaultSettingsCommand = value; }
        }
        
        private void defaultSettings()
        {
            MqttBrokerAddress = defaultMqttBrokerAddress;
            MqttTopic = defaultMqttTopic;
            SaveSettings();
        }

    }



    public class Settings
    {
        public string MqttBrokerAddress { get; set; }
        public string MqttTopic { get; set; }
    }

}
