using ELOR.Laney.Helpers;
using System;

namespace ELOR.Laney.DataModels {
    public class Command {
        public string IconId { get; private set; }
        public string Label { get; private set; }
        public bool IsDestructive { get; private set; }
        public RelayCommand Action { get; private set; }

        public Command(string iconId, string label, bool isDestructive, Action<object> action) {
            IconId = iconId;
            Label = label;
            IsDestructive = isDestructive;
            Action = new RelayCommand(action);
        }

        public bool CanAction() {
            return true;
        }
    }
}