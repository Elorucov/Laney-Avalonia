using Avalonia;
using Avalonia.Controls;
using ELOR.Laney.DataModels;

namespace ELOR.Laney.Controls {
    public partial class EntityUI : UserControl {
        public EntityUI() {
            InitializeComponent();
            Loaded += (a, b) => CommandButton.CommandParameter = CommandButton;
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
    }
}