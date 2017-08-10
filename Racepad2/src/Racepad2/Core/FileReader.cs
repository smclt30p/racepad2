using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Racepad2.Core {
    class FileReader {

        public async static Task<string> ReadFile(IStorageItem item) {
            if (item.IsOfType(StorageItemTypes.File)) {
                StorageFile file = item as StorageFile;
                return await FileIO.ReadTextAsync(file);
            }
            return null;
        }

    }
}
