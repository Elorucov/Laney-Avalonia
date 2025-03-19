using Avalonia.Controls.Selection;
using ELOR.Laney.Core;
using ELOR.VKAPILib.Objects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ELOR.Laney.ViewModels.Modals {
    public class GroupsPickerViewModel : CommonViewModel {
        private ObservableCollection<Group> _groups = new ObservableCollection<Group>();

        public ObservableCollection<Group> Groups { get { return _groups; } private set { _groups = value; OnPropertyChanged(); } }

        public SelectionModel<Group> SelectedGroups { get; private set; }
        VKSession session;

        public GroupsPickerViewModel(VKSession session) {
            this.session = session;
            SelectedGroups = new SelectionModel<Group>() {
                SingleSelect = false,
                Source = Groups
            };

            GetGroups();
        }

        private void GetGroups() {
            new System.Action(async () => {
                Placeholder = null;
                IsLoading = true;
                try {
                    var response = await session.API.Groups.GetAsync(session.UserId, new List<string> { "can_message" }, new List<string> { "editor" });
                    foreach (var group in response.Items) {
                        if (group.CanMessage == 1) Groups.Add(group);
                    }

                    var selected = VKSession.GetAddedGroupIds();
                    foreach (long gid in selected) {
                        var group = Groups.Where(g => g.Id == gid).FirstOrDefault();
                        if (group == null) continue;
                        int index = Groups.IndexOf(group);
                        SelectedGroups.Select(index);
                    }
                } catch (Exception ex) {
                    Placeholder = PlaceholderViewModel.GetForException(ex, (o) => GetGroups());
                }
                IsLoading = false;
            })();
        }
    }
}