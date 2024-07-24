using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace VKUI {
    public class VKUITheme : Styles {
        private static VKUITheme _instance;

        public VKUITheme(IServiceProvider? sp = null) {
            AvaloniaXamlLoader.Load(sp, this);
            _instance = this;

            var fonts = _instance.Resources.MergedDictionaries[0] as ResourceDictionary;
            int i = 0;
            
        }

        public static IResourceDictionary Icons { 
            get => _instance.Resources.MergedDictionaries[3] as ResourceDictionary;
        }
    }
}