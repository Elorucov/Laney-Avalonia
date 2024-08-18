using Avalonia.Platform.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELOR.Laney.Helpers {
    public enum DroppedFilesType { OnlyPhotos, OnlyVideos, Mixed }

    public static class StorageItemHelper {
        public static DroppedFilesType GetFilesType(this IEnumerable<IStorageItem> items) {
            // var photosExt = FilePickerFileTypes.ImageAll.Patterns;
            var photosExt = new string[] { "png", "jpg", "jpeg", "gif", "bmp", "webp", "heic" };
            var videosExt = new string[] { "mp4", "mpg", "3gp", "avi", "hevc", "webm", "wmv", "mkv" };

            int photosCount = 0;
            int videosCount = 0;
            int othersCount = 0;

            foreach (var item in items) {
                if (item is IStorageFile file) {
                    string ext = file.Name.Split(".").LastOrDefault().ToLower();
                    if (photosExt.Contains(ext)) {
                        photosCount++;
                        continue;
                    } else if (videosExt.Contains(ext)) {
                        videosCount++;
                        continue;
                    } else {
                        othersCount++;
                        continue;
                    }
                } else {
                    throw new FormatException("IStorageItem array contains a non-file item!");
                }
            }

            if (photosCount > 0 && videosCount == 0 && othersCount == 0) return DroppedFilesType.OnlyPhotos;
            if (photosCount == 0 && videosCount > 0 && othersCount == 0) return DroppedFilesType.OnlyVideos;
            return DroppedFilesType.Mixed;
        }
    }
}
