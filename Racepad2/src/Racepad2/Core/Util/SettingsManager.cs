using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Racepad2.Core.Util {
    class SettingsManager {

        private static SettingsManager Instance = null;

        public static SettingsManager GetDefaultSettingsManager() {
            if (Instance == null) {
                Instance = new SettingsManager();
            }
            return Instance;
        }

        public string GetSetting(string key, string def) {

        ApplicationDataContainer container = this.GetStorageContainer();

        if (container.Values[key] == null) {
            container.Values[key] = def;
            return def;
        }

        return container.Values[key].ToString();

    }

        public void PutSetting(string key, string value) {
            ApplicationDataContainer Container = this.GetStorageContainer();
            Container.Values[key] = value;
        }

    private ApplicationDataContainer GetStorageContainer() {

                Windows.Storage.ApplicationData Appdata = Windows.Storage.ApplicationData.Current;
                Windows.Storage.ApplicationDataContainer Settings = Appdata.LocalSettings;
                Windows.Storage.ApplicationDataContainer Container = null;

                if (!Settings.Containers.ContainsKey("appdata")) {
                    Container = Settings.CreateContainer("appdata", Windows.Storage.ApplicationDataCreateDisposition.Always);
                } else {
                    Container = Settings.Containers["appdata"];
                }

                return Container;

            }

        
    }
}


/* 
 

    class SettingsManager {

    private static intance: SettingsManager = null;

    public static getManager(): SettingsManager {

        if (SettingsManager.intance == null) {
            SettingsManager.intance = new SettingsManager();
        }

        return SettingsManager.intance;

    }


    public getSetting(key: string, def: string): string {

        let container = this.getStorageContainer();

        if (container.values[key] == null || container.values[key] == undefined) {
            container.values[key] = def;
            return def;
        }

        return container.values[key];

    }

    public putSetting(key: string, value: string): void {
        let container = this.getStorageContainer();
        container.values[key] = value;
    }

    private getStorageContainer(): Windows.Storage.ApplicationDataContainer {

        let appdata: Windows.Storage.ApplicationData = Windows.Storage.ApplicationData.current;
        let settings: Windows.Storage.ApplicationDataContainer = appdata.localSettings;
        let container: Windows.Storage.ApplicationDataContainer = null;

        if (!settings.containers.hasKey("appdata")) {
            container = settings.createContainer("appdata", Windows.Storage.ApplicationDataCreateDisposition.always);
        } else {
            container = settings.containers.lookup("appdata");
        }

        return container;

    }

}
     
     */
