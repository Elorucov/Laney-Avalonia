using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace VKUI {
    public class VKUITheme : Styles {
        private static int position = -1;

        public VKUITheme(IServiceProvider? sp = null) {
            AvaloniaXamlLoader.Load(sp, this);
            position = Application.Current.Styles.Count;
        }

        public static IResourceDictionary Icons { 
            get => (Application.Current.Styles[position] as VKUITheme).Resources.MergedDictionaries[2] as ResourceDictionary;
        }
    }
}