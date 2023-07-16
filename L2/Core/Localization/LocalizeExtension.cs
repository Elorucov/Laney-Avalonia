using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using System;

namespace ELOR.Laney.Core.Localization {
    public class LocalizeExtension : MarkupExtension {
        public LocalizeExtension(string key) {
            this.Key = key;
        }

        public string Key { get; set; }
        public string Context { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider) {
            //var keyToUse = Key;
            //if (!string.IsNullOrWhiteSpace(Context))
            //    keyToUse = $"{Context}/{Key}";

            //var binding = new ReflectionBindingExtension($"[{keyToUse}]") {
            //    Mode = BindingMode.OneWay,
            //    Source = Localizer.Instance,
            //};

            //return binding.ProvideValue(serviceProvider);

            return Localizer.Instance[Key];
        }
    }
}