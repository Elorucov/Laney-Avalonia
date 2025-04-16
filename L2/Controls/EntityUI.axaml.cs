using Avalonia;
using Avalonia.Controls;
using ELOR.Laney.DataModels;
using System;

namespace ELOR.Laney.Controls {
    public partial class EntityUI : UserControl {
        public EntityUI() {
            InitializeComponent();
            Loaded += (a, b) => CommandButton.CommandParameter = CommandButton;
            // PropertyChanged += EntityUI_PropertyChanged;
            // Unloaded += (a, b) => PropertyChanged -= EntityUI_PropertyChanged;
        }

        public static readonly StyledProperty<Entity> EntityProperty =
            AvaloniaProperty.Register<EntityUI, Entity>(nameof(Entity));

        public Entity Entity {
            get => GetValue(EntityProperty);
            set {
                SetValue(EntityProperty, value);
                DataContext = value;
            }
        }

        // Binding (EntityUI.axaml#17) is not working idk why...
        //private void EntityUI_PropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e) {
        //    if (e.Property == EntityProperty && Entity != null) Subtitle.IsVisible = !String.IsNullOrWhiteSpace(Entity.Description);
        //}
    }
}