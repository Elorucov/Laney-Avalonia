using System.Drawing;

namespace ELOR.VKAPILib.Objects {
    public interface IPreview {
        Uri PreviewImageUri { get; }
        Size PreviewImageSize { get; }
    }
}
