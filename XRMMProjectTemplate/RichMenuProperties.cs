using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace XamlRichMenuMaker
{
    public static class RichMenuProperties
    {

        public static RichMenuSettings GetSettings(DependencyObject obj)
        {
            return (RichMenuSettings)obj.GetValue(SettingsProperty);
        }

        public static void SetSettings(DependencyObject obj, RichMenuSettings value)
        {
            obj.SetValue(SettingsProperty, value);
        }

        // Using a DependencyProperty as the backing store for RichMenuDetails.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SettingsProperty =
            DependencyProperty.RegisterAttached("Settings", typeof(RichMenuSettings), typeof(RichMenuProperties), new PropertyMetadata(null));


        public static RichMenuAction GetAction(DependencyObject obj)
        {
            return (RichMenuAction)obj.GetValue(ActionProperty);
        }

        public static void SetAction(DependencyObject obj, RichMenuAction value)
        {
            obj.SetValue(ActionProperty, value);
        }

        // Using a DependencyProperty as the backing store for RichMenuAction.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ActionProperty =
            DependencyProperty.RegisterAttached("Action", typeof(RichMenuAction), typeof(RichMenuProperties), new PropertyMetadata(null));


    }
}
