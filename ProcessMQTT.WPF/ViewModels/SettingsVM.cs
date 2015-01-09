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
            loadSettings();

        }

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
                mqttBrokerAddress = "gloveboxAE.cloudapp.net";
                mqttTopic = "gb/#";
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
            get { return mqttTopic; }
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

    }



    public class Settings
    {
        public string MqttBrokerAddress { get; set; }
        public string MqttTopic { get; set; }
    }

}
