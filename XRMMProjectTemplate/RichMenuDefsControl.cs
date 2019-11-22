using Line.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace XamlRichMenuMaker
{
    /// <summary>
    /// RichMenuDefsControl.xaml の相互作用ロジック
    /// </summary>
    public class RichMenuDefsControl : UserControl
    {
        public static readonly double RichMenuWidth = 2500;
        public static readonly double RichMenuShortHeight = 843;
        public static readonly double RichMenuNormalHeight = 1686;

        public (RichMenu newRichMenu, RenderTargetBitmap bitmap) RenderingRichMenu()
        {
            var body = this.FindName("menu_body") as UIElement;
            if (body == null) { return (null, null); }

            var areas = new List<ActionArea>();
            foreach (var area in Enumerable.Range(1, 20).Select(n => "area_" + n))
            {
                var areaObj = this.FindName(area) as UIElement;
                if (areaObj == null) { continue; }

                var menuAction = RichMenuProperties.GetAction(areaObj);
                ITemplateAction action = null;
                switch (menuAction.Type)
                {
                    case TemplateActionType.Message:
                        action = new MessageTemplateAction(menuAction.Label, menuAction.Text);
                        break;
                    case TemplateActionType.Postback:
                        action = new PostbackTemplateAction(menuAction.Label, menuAction.Data, menuAction.Text);
                        break;
                    case TemplateActionType.Uri:
                        action = new UriTemplateAction(menuAction.Label, menuAction.Uri);
                        break;
                    case TemplateActionType.Datetimepicker:
                        action = new DateTimePickerTemplateAction(menuAction.Label, menuAction.Data, menuAction.DateTimeMode,
                            menuAction.initialDateTime, menuAction.minDateTime, menuAction.maxDateTime);
                        break;
                }
                if (action == null) { continue; };

                var topLeft = areaObj.TransformToVisual(body).Transform(new Point(0, 0));
                areas.Add(new ActionArea()
                {
                    Bounds = new ImagemapArea(
                        (int)topLeft.X,
                        (int)topLeft.Y,
                        (int)areaObj.RenderSize.Width,
                        (int)areaObj.RenderSize.Height),
                    Action = action
                });
            }

            var details = RichMenuProperties.GetSettings(body);
            var menu = new RichMenu()
            {
                Name = details.Name,
                ChatBarText = details.ChatBarText,
                Selected = details.Selected,
                Size = new ImagemapSize((int)body.RenderSize.Width, (int)body.RenderSize.Height),
                Areas = areas
            };

            var bmp = new RenderTargetBitmap(menu.Size.Width, menu.Size.Height, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(body);

            return (menu, bmp);
        }

    }
}
