using Line.Messaging;
using System;

namespace XamlRichMenuMaker
{
    public class RichMenuAction
    {
        public RichMenuAction()
        {
        }

        public TemplateActionType Type { get; set; }
        public string Label { get; set; }
        public string Text { get; set; }
        public string Uri { get; set; }
        public string Data { get; set; }
        public DateTimePickerMode DateTimeMode { get; set; }
        public DateTime? initialDateTime { get; set; }
        public DateTime? minDateTime { get; set; }
        public DateTime? maxDateTime { get; set; }
    }
}