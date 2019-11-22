using Line.Messaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Prism.Commands;
using Prism.Mvvm;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace XamlRichMenuMaker
{
    class MainWindowViewModel : BindableBase
    {
        private static readonly string OutputDirectory = "RichMenuImages";
        private static LineMessagingClient _line;
        private IList<ResponseRichMenu> _richMenus;
        private ResponseRichMenu _selectedRichMenu;
        private BitmapSource _previewImage;
        private string _userId;
        private string _selectedRichMenuJson;
        private JsonSerializerSettings _jsonSerializerSettings;
        private IList<ResponseRichMenu> _newRichMenus;

        public IList<ResponseRichMenu> RichMenus
        {
            get => _richMenus;
            set
            {
                if (_richMenus == value) { return; }
                _richMenus = value;
                RaisePropertyChanged(nameof(RichMenus));
            }
        }

        public ResponseRichMenu SelectedRichMenu
        {
            get => _selectedRichMenu;
            set
            {
                SetProperty(ref _selectedRichMenu, value);
                SelectedRichMenuJson = JsonConvert.SerializeObject(SelectedRichMenu, _jsonSerializerSettings);
                RaiseCanExecuteChanged();
            }
        }

        public BitmapSource PreviewImage
        {
            get => _previewImage;
            set => SetProperty(ref _previewImage, value);
        }

        public string SelectedRichMenuJson
        {
            get => _selectedRichMenuJson;
            set => SetProperty(ref _selectedRichMenuJson, value);
        }

        public DelegateCommand CreateRichMenuCommand { get; }
        public DelegateCommand DeleteRichMenuCommand { get; }
        public DelegateCommand LinkToUserCommand { get; }
        public DelegateCommand UnlinkFromUserCommand { get; }
        public DelegateCommand SetDefaultRitchMenuCommand { get; }

        public MainWindowViewModel(string channelAccessToken, string userId)
        {
            if (_line == null) { _line = new LineMessagingClient(channelAccessToken); }
            _userId = userId;

            Directory.CreateDirectory(OutputDirectory);

            _jsonSerializerSettings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore
            };
            _jsonSerializerSettings.Converters.Add(new StringEnumConverter { NamingStrategy= new CamelCaseNamingStrategy() });

            RichMenus = new List<ResponseRichMenu>();
            CreateRichMenuCommand = new DelegateCommand(async () => await CreateRichMenuAsync(), () => IsNewItemSelected());
            DeleteRichMenuCommand = new DelegateCommand(async () => await DeleteRichMenuAsync(), () => !IsNewItemSelected());
            LinkToUserCommand = new DelegateCommand(async () => await LinkToUserAsync(), () => !IsNewItemSelected());
            UnlinkFromUserCommand = new DelegateCommand(async () => await UnlinkFromUserAsync(), () => !IsNewItemSelected());
            SetDefaultRitchMenuCommand = new DelegateCommand(async () => await SetDefaultRichMenuAsync(), () => !IsNewItemSelected());
        }

        private async Task SetDefaultRichMenuAsync() => await _line.SetDefaultRichMenuAsync(SelectedRichMenu.RichMenuId);

        public async Task GetRichMenuListAsync(IList<ResponseRichMenu> richMenuList, string newRichMenuId)
        {
            if (richMenuList != null)
            {
                _newRichMenus = richMenuList;
            }
            var menus = await _line.GetRichMenuListAsync();

            RichMenus = _newRichMenus.Concat(menus).ToList();
            SelectedRichMenu = RichMenus.FirstOrDefault(m => m.RichMenuId == newRichMenuId) ?? RichMenus[0];
        }

        private async Task<Stream> GetRichMenuImageStream()
        {
            if (File.Exists(Path.Combine(OutputDirectory, SelectedRichMenu.RichMenuId + ".png")))
            {
                using (var file = new FileStream(Path.Combine(OutputDirectory, SelectedRichMenu.RichMenuId + ".png"), FileMode.Open, FileAccess.Read))
                {
                    var memoryStream = new MemoryStream();
                    await file.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;
                    return memoryStream;
                }
            }
            else
            {
                var stream = await _line.DownloadRichMenuImageAsync(SelectedRichMenu.RichMenuId);
                using (var file = new FileStream(Path.Combine(OutputDirectory, SelectedRichMenu.RichMenuId + ".png"), FileMode.Create, FileAccess.Write))
                {
                    await stream.CopyToAsync(file);
                    stream.Position = 0;
                }

                return stream;
            }

        }

        public async Task LoadSelectedMenuImage()
        {
            if (SelectedRichMenu == null) { return; }

            try
            {
                var stream = await GetRichMenuImageStream();
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.DecodePixelWidth = SelectedRichMenu.Size.Width / 4;
                bitmap.DecodePixelHeight = SelectedRichMenu.Size.Height / 4;
                bitmap.EndInit();
                bitmap.Freeze();
                PreviewImage = bitmap;

            }
            catch (LineResponseException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    PreviewImage = null;
                }
                else { throw; }
            }
        }

        public void SaveRichMenuImage(RenderTargetBitmap bitmap, string fileName)
        {
            using (var stream = new FileStream(Path.Combine(OutputDirectory, fileName + ".png"), FileMode.Create, FileAccess.Write))
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                encoder.Save(stream);
            }
        }

        private bool IsNewItemSelected()
        {
            if ((SelectedRichMenu == null) || (RichMenus == null) || (_newRichMenus == null))
            {
                return false;
            }
            foreach (var menu in _newRichMenus)
            {
                if (SelectedRichMenu == menu) { return true; }
            }
            return false;
        }

        private async Task CreateRichMenuAsync()
        {
            var menu = SelectedRichMenu ?? RichMenus[0];
            var jsonString = JsonConvert.SerializeObject(menu, _jsonSerializerSettings);

            var menuId = await _line.CreateRichMenuAsync(menu);

            using (var stream = new FileStream(Path.Combine(OutputDirectory, menu.RichMenuId + ".png"), FileMode.Open, FileAccess.Read))
            {
                await _line.UploadRichMenuPngImageAsync(stream, menuId);
            }

            await GetRichMenuListAsync(null, menuId);
        }

        private async Task DeleteRichMenuAsync()
        {
            if (SelectedRichMenu == null) { return; }

            await _line.DeleteRichMenuAsync(SelectedRichMenu.RichMenuId);

            await GetRichMenuListAsync(null, null);
        }

        private Task LinkToUserAsync() => _line.LinkRichMenuToUserAsync(_userId, SelectedRichMenu.RichMenuId);

        private Task UnlinkFromUserAsync() => _line.UnLinkRichMenuFromUserAsync(_userId);

        private void RaiseCanExecuteChanged()
        {
            CreateRichMenuCommand.RaiseCanExecuteChanged();
            DeleteRichMenuCommand.RaiseCanExecuteChanged();
            LinkToUserCommand.RaiseCanExecuteChanged();
            UnlinkFromUserCommand.RaiseCanExecuteChanged();
            SetDefaultRitchMenuCommand.RaiseCanExecuteChanged();
        }

    }
}
