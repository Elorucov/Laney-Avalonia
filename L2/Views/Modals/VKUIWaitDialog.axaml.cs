using Avalonia.Controls;
using System.Threading.Tasks;
using VKUI.Windows;

namespace ELOR.Laney.Views.Modals {
    public class VKUIWaitDialog<T> {
        public async Task<T> ShowAsync(Window owner, Task task) {
            VKUIWaitDialog dialog = new VKUIWaitDialog();
            dialog.Activated += async (a, b) => {
                // Execute task
                try {
                    await Task.Delay(200); // Window open animation
                    await task.ConfigureAwait(true);
                } catch { }

                // Close popup and return result
                dialog.Close();
            };
            await dialog.ShowDialog(owner);

            if (!task.IsFaulted) {
                if (task is Task<T> otask) {
                    return otask.Result;
                } else {
                    return default;
                }
            } else {
                throw task.Exception;
            }
        }
    }

    public partial class VKUIWaitDialog : DialogWindow {
        public VKUIWaitDialog() {
            InitializeComponent();
        }
    }
}
