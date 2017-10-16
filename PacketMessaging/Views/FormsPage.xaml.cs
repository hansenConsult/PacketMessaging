using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using FormControlBaseClass;
using System.Reflection;
using System.Threading.Tasks;
using System.Diagnostics;
using MetroLog;
using Windows.UI.Popups;
using Windows.UI.Core;
using Windows.Foundation;
using Windows.Graphics.Printing;
using Windows.Graphics.Printing.OptionDetails;
using Windows.UI.Xaml.Printing;
using System.Threading;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage;
using Windows.Graphics.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
using ToggleButtonGroupControl;
using Windows.ApplicationModel.DataTransfer;

namespace PacketMessaging.Views
{
    public class FormControlAttributes
    {
        public string FormControlName
        { get; private set; }

        public string FormControlMenuName
        { get; private set; }

        public FormControlAttribute.FormType FormControlType
        { get; private set; }

        public StorageFile FormControlFileName
        { get; set; }

        public FormControlAttributes(string formControlType, string formControlMenuName, FormControlAttribute.FormType formType, StorageFile formControlFileName)
        {
            FormControlName = formControlType;
            FormControlMenuName = formControlMenuName;
            FormControlType = formType;
            FormControlFileName = formControlFileName;
        }
    }

    public enum PhotoSize : byte
    {
        SizeFullPage,
        Size8x10
    }

    /// <summary>
    /// Photo scaling options
    /// </summary>
    public enum Scaling : byte
    {
        ShrinkToFit
    }

    /// <summary>
    /// Printable page description
    /// </summary>
    public class PageDescription : IEquatable<PageDescription>
    {
        public Size Margin;
        public Size PageSize;
        public Size ViewablePageSize;
        public Size PictureViewSize;
        public bool IsContentCropped;


        public bool Equals(PageDescription other)
        {
            // Detect if PageSize changed
            bool equal = (Math.Abs(PageSize.Width - other.PageSize.Width) < double.Epsilon) &&
                (Math.Abs(PageSize.Height - other.PageSize.Height) < double.Epsilon);

            // Detect if ViewablePageSize changed
            if (equal)
            {
                equal = (Math.Abs(ViewablePageSize.Width - other.ViewablePageSize.Width) < double.Epsilon) &&
                    (Math.Abs(ViewablePageSize.Height - other.ViewablePageSize.Height) < double.Epsilon);
            }

            // Detect if PictureViewSize changed
            if (equal)
            {
                equal = (Math.Abs(PictureViewSize.Width - other.PictureViewSize.Width) < double.Epsilon) &&
                    (Math.Abs(PictureViewSize.Height - other.PictureViewSize.Height) < double.Epsilon);
            }

            // Detect if cropping changed
            if (equal)
            {
                equal = IsContentCropped == other.IsContentCropped;
            }

            return equal;
        }
    }

    class PhotosPrintHelper : PrintHelper
    {
        #region Scenario specific constants.
        /// <summary>
        /// The app's number of photos
        /// </summary>
        private const int NumberOfPhotos = 1;

        /// <summary>
        /// Constant for 96 DPI
        /// </summary>
        private const int DPI96 = 96;

        #endregion

        /// <summary>
        /// Current size settings for the image
        /// </summary>
        private PhotoSize photoSize;

        /// <summary>
        /// Current scale settings for the image
        /// </summary>
        private Scaling photoScale;

        /// <summary>
        /// A map of UIElements used to store the print preview pages.
        /// </summary>
        private Dictionary<int, UIElement> pageCollection = new Dictionary<int, UIElement>();

        /// <summary>
        /// Synchronization object used to sync access to pageCollection and the visual root(PrintingRoot).
        /// </summary>
        private static object printSync = new object();

        /// <summary>
        /// The current printer's page description used to create the content (size, margins, printable area)
        /// </summary>
        private PageDescription currentPageDescription;

        /// <summary>
        /// A request "number" used to describe a Paginate - GetPreviewPage session.
        /// It is used by GetPreviewPage to determine, before calling SetPreviewPage, if the page content is out of date.
        /// Flow:
        /// Paginate will increment the request count and all subsequent GetPreviewPage calls will store a local copy and verify it before calling SetPreviewPage.
        /// If another Paginate event is triggered while some GetPreviewPage workers are still executing asynchronously
        /// their results will be discarded(ignored) because their request number is expired (the photo page description changed).
        /// </summary>
        private long requestCount;

        public PhotosPrintHelper(Page scenarioPage) : base(scenarioPage)
        {
            photoSize = PhotoSize.SizeFullPage;
            photoScale = Scaling.ShrinkToFit;
        }

        /// <summary>
        /// This is the event handler for PrintManager.PrintTaskRequested.
        /// In order to ensure a good user experience, the system requires that the app handle the PrintTaskRequested event within the time specified
        /// by PrintTaskRequestedEventArgs->Request->Deadline.
        /// Therefore, we use this handler to only create the print task.
        /// The print settings customization can be done when the print document source is requested.
        /// </summary>
        /// <param name="sender">The print manager for which a print task request was made.</param>
        /// <param name="e">The print taks request associated arguments.</param>
        protected override void PrintTaskRequested(PrintManager sender, PrintTaskRequestedEventArgs e)
        {
            PrintTask printTask = null;
            printTask = e.Request.CreatePrintTask("Print Packet Form", sourceRequestedArgs =>
            {
                PrintTaskOptionDetails printDetailedOptions = PrintTaskOptionDetails.GetFromPrintTaskOptions(printTask.Options);

                // Choose the printer options to be shown.
                // The order in which the options are appended determines the order in which they appear in the UI
                printDetailedOptions.DisplayedOptions.Clear();
                printDetailedOptions.DisplayedOptions.Add(StandardPrintTaskOptions.MediaSize);
                printDetailedOptions.DisplayedOptions.Add(StandardPrintTaskOptions.Copies);

                // Create a new list option.
                //PrintCustomItemListOptionDetails photoSize = printDetailedOptions.CreateItemListOption("photoSize", "Photo Size");
                //photoSize.AddItem("SizeFullPage", "Full Page");

                // Add the custom option to the option list.
                //printDetailedOptions.DisplayedOptions.Add("photoSize");

                // Set default orientation to portrait.
                //printTask.Options.Orientation = PrintOrientation.Portrait;

                // Register for print task option changed notifications.
                printDetailedOptions.OptionChanged += PrintDetailedOptionsOptionChanged;

                // Register for print task Completed notification.
                // Print Task event handler is invoked when the print job is completed.
                printTask.Completed += async (s, args) =>
                {
                    await scenarioPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        ClearPageCollection();

                        // Reset image options to default values.
                        this.photoScale = Scaling.ShrinkToFit;
                        this.photoSize = PhotoSize.SizeFullPage;

                        // Reset the current page description
                        currentPageDescription = null;

                        // Notify the user when the print operation fails.
                        if (args.Completion == PrintTaskCompletion.Failed)
                        {
                            //MainPage.Current.NotifyUser("Failed to print.", NotifyType.ErrorMessage);
                        }
                    });
                };

                // Set the document source.
                sourceRequestedArgs.SetSource(printDocumentSource);
            });
        }

        /// <summary>
        /// Option change event handler
        /// </summary>
        /// <param name="sender">The print task option details for which an option changed.</param>
        /// <param name="args">The event arguments containing the id of the changed option.</param>
        private async void PrintDetailedOptionsOptionChanged(PrintTaskOptionDetails sender, PrintTaskOptionChangedEventArgs args)
        {
            bool invalidatePreview = false;

            // For this scenario we are interested only when the 2 custom options change (photoSize & scaling) in order to trigger a preview refresh.
            // Default options that change page aspect will trigger preview invalidation (refresh) automatically.
            // It is safe to ignore verifying other options and(or) combinations here because during Paginate event(CreatePrintPreviewPages) we check if the PageDescription changed.
            if (args.OptionId == null)
            {
                return;
            }

            string optionId = args.OptionId.ToString();

            // Invalidate preview if one of the 2 options (photoSize, scaling) changed.
            if (invalidatePreview)
            {
                await scenarioPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, printDocument.InvalidatePreview);
            }
        }

        /// <summary>
        /// This is the event handler for PrintDocument.Paginate.
        /// </summary>
        /// <param name="sender">The document for which pagination occurs.</param>
        /// <param name="e">The pagination event arguments containing the print options.</param>
        protected override void CreatePrintPreviewPages(object sender, Windows.UI.Xaml.Printing.PaginateEventArgs e)
        {

            PrintDocument printDoc = (PrintDocument)sender;

            // A new "session" starts with each paginate event.
            Interlocked.Increment(ref requestCount);

            PageDescription pageDescription = new PageDescription();

            // Get printer's page description.
            //PrintTaskOptionDetails printDetailedOptions = PrintTaskOptionDetails.GetFromPrintTaskOptions(e.PrintTaskOptions);
            PrintPageDescription printPageDescription = e.PrintTaskOptions.GetPageDescription(0);

            // Reset the error state
           // printDetailedOptions.Options["photoSize"].ErrorText = string.Empty;

            // Compute the printing page description (page size & center printable area)
            pageDescription.PageSize = printPageDescription.PageSize;

            pageDescription.Margin.Width = Math.Max(
                printPageDescription.ImageableRect.Left,
                printPageDescription.ImageableRect.Right - printPageDescription.PageSize.Width);

            pageDescription.Margin.Height = Math.Max(
                printPageDescription.ImageableRect.Top,
                printPageDescription.ImageableRect.Bottom - printPageDescription.PageSize.Height);

            pageDescription.ViewablePageSize.Width = printPageDescription.PageSize.Width - pageDescription.Margin.Width * 2;
            pageDescription.ViewablePageSize.Height = printPageDescription.PageSize.Height - pageDescription.Margin.Height * 2;

            // Compute print photo area.
            switch (photoSize)
            {
                case PhotoSize.SizeFullPage:
                    pageDescription.PictureViewSize.Width = pageDescription.ViewablePageSize.Width;
                    pageDescription.PictureViewSize.Height = pageDescription.ViewablePageSize.Height;
                    break;
            }

            // Try to maximize photo-size based on it's aspect-ratio
            if ((pageDescription.ViewablePageSize.Width > pageDescription.ViewablePageSize.Height) && (photoSize != PhotoSize.SizeFullPage))
            {
                var swap = pageDescription.PictureViewSize.Width;
                pageDescription.PictureViewSize.Width = pageDescription.PictureViewSize.Height;
                pageDescription.PictureViewSize.Height = swap;
            }

            pageDescription.IsContentCropped = false;

            // Recreate content only when :
            // - there is no current page description
            // - the current page description doesn't match the new one
            if (currentPageDescription == null || !currentPageDescription.Equals(pageDescription))
            {
                ClearPageCollection();

                if (pageDescription.PictureViewSize.Width > pageDescription.ViewablePageSize.Width ||
                    pageDescription.PictureViewSize.Height > pageDescription.ViewablePageSize.Height)
                {
                    //printDetailedOptions.Options["photoSize"].ErrorText = "Photo doesn’t fit on the selected paper";

                    // Inform preview that it has only 1 page to show.
                    printDoc.SetPreviewPageCount(1, PreviewPageCountType.Intermediate);

                    // Add a custom "preview" unavailable page
                    lock (printSync)
                    {
                        pageCollection[0] = new PreviewUnavailable(pageDescription.PageSize, pageDescription.ViewablePageSize);
                    }
                }
                else
                {
                    // Inform preview that is has #NumberOfPhotos pages to show.
                    printDoc.SetPreviewPageCount(NumberOfPhotos, PreviewPageCountType.Intermediate);
                }

                currentPageDescription = pageDescription;
            }
        }

        /// <summary>
        /// This is the event handler for PrintDocument.GetPrintPreviewPage. It provides a specific print page preview,
        /// in the form of an UIElement, to an instance of PrintDocument.
        /// PrintDocument subsequently converts the UIElement into a page that the Windows print system can deal with.
        /// </summary>
        /// <param name="sender">The print documet.</param>
        /// <param name="e">Arguments containing the requested page preview.</param>
        protected async override void GetPrintPreviewPage(object sender, GetPreviewPageEventArgs e)
        {
            // Store a local copy of the request count to use later to determine if the computed page is out of date.
            // If the page preview is unavailable an async operation will generate the content.
            // When the operation completes there is a chance that a pagination request was already made therefore making this page obsolete.
            // If the page is obsolete throw away the result (don't call SetPreviewPage) since a new GetPrintPreviewPage will server that request.
            long requestNumber = 0;
            Interlocked.Exchange(ref requestNumber, requestCount);
            int pageNumber = e.PageNumber;

            UIElement page;
            bool pageReady = false;

            // Try to get the page if it was previously generated.
            lock (printSync)
            {
                pageReady = pageCollection.TryGetValue(pageNumber - 1, out page);
            }

            if (!pageReady)
            {
                // The page is not available yet.
                page = await GeneratePageAsync(pageNumber, currentPageDescription);

                // If the ticket changed discard the result since the content is outdated.
                if (Interlocked.CompareExchange(ref requestNumber, requestNumber, requestCount) != requestCount)
                {
                    return;
                }

                // Store the page in the list in case an invalidate happens but the content doesn't need to be regenerated.

                lock (printSync)
                {
                    pageCollection[pageNumber - 1] = page;

                    // Add the newly created page to the printing root which is part of the visual tree and force it to go
                    // through layout so that the linked containers correctly distribute the content inside them.
                    PrintCanvas.Children.Add(page);
                    PrintCanvas.InvalidateMeasure();
                    PrintCanvas.UpdateLayout();
                }
            }

            PrintDocument printDoc = (PrintDocument)sender;

            // Send the page to preview.
            printDoc.SetPreviewPage(pageNumber, page);
        }

        /// <summary>
        /// This is the event handler for PrintDocument.AddPages. It provides all pages to be printed, in the form of
        /// UIElements, to an instance of PrintDocument. PrintDocument subsequently converts the UIElements
        /// into a pages that the Windows print system can deal with.
        /// </summary>
        /// <param name="sender">The print document.</param>
        /// <param name="e">Arguments containing the print task options.</param>
        protected override async void AddPrintPages(object sender, AddPagesEventArgs e)
        {
            PrintDocument printDoc = (PrintDocument)sender;

            // Loop over all of the preview pages
            for (int i = 0; i < NumberOfPhotos; i++)
            {
                UIElement page = null;
                bool pageReady = false;

                lock (printSync)
                {
                    pageReady = pageCollection.TryGetValue(i, out page);
                }

                if (!pageReady)
                {
                    // If the page is not ready create a task that will generate its content.
                    page = await GeneratePageAsync(i + 1, currentPageDescription);
                }

                printDoc.AddPage(page);
            }

            // Indicate that all of the print pages have been provided.
            printDoc.AddPagesComplete();

            // Reset the current page description as soon as possible since the PrintTask.Completed event might fire later (long running job)
            currentPageDescription = null;
        }

        /// <summary>
        /// This function creates and adds one print preview page to the internal cache of print preview
        /// pages stored in printPreviewPages.
        /// </summary>
        /// <param name="lastRTBOAdded">Last RichTextBlockOverflow element added in the current content</param>
        /// <param name="printPageDescription">Printer's page description</param>
        //protected virtual RichTextBlockOverflow AddOnePrintPreviewPage(RichTextBlockOverflow lastRTBOAdded, PrintPageDescription printPageDescription)
        //{
        //    // XAML element that is used to represent to "printing page"
        //    FrameworkElement page;

        //    // The link container for text overflowing in this page
        //    RichTextBlockOverflow textLink;

        //    // Check if this is the first page ( no previous RichTextBlockOverflow)
        //    if (lastRTBOAdded == null)
        //    {
        //        // If this is the first page add the specific scenario content
        //        page = firstPage;
        //        //Hide footer since we don't know yet if it will be displayed (this might not be the last page) - wait for layout
        //        StackPanel footer = (StackPanel)page.FindName("Footer");
        //        footer.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        //    }
        //    else
        //    {
        //        // Flow content (text) from previous pages
        //        page = new ContinuationPage(lastRTBOAdded);
        //    }

        //    // Set "paper" width
        //    page.Width = printPageDescription.PageSize.Width;
        //    page.Height = printPageDescription.PageSize.Height;

        //    Grid printableArea = (Grid)page.FindName("PrintableArea");

        //    // Get the margins size
        //    // If the ImageableRect is smaller than the app provided margins use the ImageableRect
        //    double marginWidth = Math.Max(printPageDescription.PageSize.Width - printPageDescription.ImageableRect.Width, printPageDescription.PageSize.Width * ApplicationContentMarginLeft * 2);
        //    double marginHeight = Math.Max(printPageDescription.PageSize.Height - printPageDescription.ImageableRect.Height, printPageDescription.PageSize.Height * ApplicationContentMarginTop * 2);

        //    // Set-up "printable area" on the "paper"
        //    printableArea.Width = firstPage.Width - marginWidth;
        //    printableArea.Height = firstPage.Height - marginHeight;

        //    // Add the (newley created) page to the print canvas which is part of the visual tree and force it to go
        //    // through layout so that the linked containers correctly distribute the content inside them.
        //    PrintCanvas.Children.Add(page);
        //    PrintCanvas.InvalidateMeasure();
        //    PrintCanvas.UpdateLayout();

        //    // Find the last text container and see if the content is overflowing
        //    textLink = (RichTextBlockOverflow)page.FindName("ContinuationPageLinkedContainer");

        //    // Check if this is the last page
        //    if (!textLink.HasOverflowContent && textLink.Visibility == Windows.UI.Xaml.Visibility.Visible)
        //    {
        //        StackPanel footer = (StackPanel)page.FindName("Footer");
        //        footer.Visibility = Windows.UI.Xaml.Visibility.Visible;
        //    }

        //    // Add the page to the page preview collection
        //    printPreviewPages.Add(page);

        //    return textLink;
        //}

        /// <summary>
        /// Helper function that clears the page collection and also the pages attached to the "visual root".
        /// </summary>
        private void ClearPageCollection()
        {
            lock (printSync)
            {
                pageCollection.Clear();
                PrintCanvas.Children.Clear();
            }
        }

        /// <summary>
        /// Generic swap of 2 values
        /// </summary>
        /// <typeparam name="T">typename</typeparam>
        /// <param name="v1">Value 1</param>
        /// <param name="v2">Value 2</param>
        private static void Swap<T>(ref T v1, ref T v2)
        {
            T swap = v1;
            v1 = v2;
            v2 = swap;
        }

        /// <summary>
        /// Generates a page containing a photo.
        /// The image will be rotated if detected that there is a gain from that regarding size (try to maximize photo size).
        /// </summary>
        /// <param name="photoNumber">The photo number.</param>
        /// <param name="pageDescription">The description of the printer page.</param>
        /// <returns>A task that will return the page.</returns>
        private async Task<UIElement> GeneratePageAsync(int photoNumber, PageDescription pageDescription)
        {
            Canvas page = new Canvas
            {
                Width = pageDescription.PageSize.Width,
                Height = pageDescription.PageSize.Height
            };

            Canvas viewablePage = new Canvas()
            {
                Width = pageDescription.ViewablePageSize.Width,
                Height = pageDescription.ViewablePageSize.Height
            };

            viewablePage.SetValue(Canvas.LeftProperty, pageDescription.Margin.Width);
            viewablePage.SetValue(Canvas.TopProperty, pageDescription.Margin.Height);

            // The image "frame" which also acts as a viewport
            Grid photoView = new Grid
            {
                Width = pageDescription.PictureViewSize.Width,
                Height = pageDescription.PictureViewSize.Height
            };

            // Center the frame.
            photoView.SetValue(Canvas.LeftProperty, (viewablePage.Width - photoView.Width) / 2);
            photoView.SetValue(Canvas.TopProperty, (viewablePage.Height - photoView.Height) / 2);

            // Return an async task that will complete when the image is fully loaded.
            //WriteableBitmap bitmap = await LoadBitmapAsync(
            //    new Uri(string.Format("ms-appx:///Assets/photo{0}.jpg", photoNumber)),
            //    pageDescription.PageSize.Width > pageDescription.PageSize.Height);

            //if (bitmap != null)
            //{
            //    Image image = new Image
            //    {
            //        Source = bitmap,
            //        HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center,
            //        VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center
            //    };

            //    // Use the real image size when croping or if the image is smaller then the target area (prevent a scale-up).
            //    if (bitmap.PixelWidth <= pageDescription.PictureViewSize.Width &&
            //        bitmap.PixelHeight <= pageDescription.PictureViewSize.Height)
            //    {
            //        image.Stretch = Stretch.None;
            //        image.Width = bitmap.PixelWidth;
            //        image.Height = bitmap.PixelHeight;
            //    }

            //    // Add the newly created image to the visual root.
            //    photoView.Children.Add(image);
            //    viewablePage.Children.Add(photoView);
            //    page.Children.Add(viewablePage);
            //}

            // Return the page with the image centered.
            return page;
        }

        /// <summary>
        /// Loads an image from an uri source and performs a rotation based on the print target aspect.
        /// </summary>
        /// <param name="source">The location of the image.</param>
        /// <param name="landscape">A flag that indicates if the target (printer page) is in landscape mode.</param>
        /// <returns>A task that will return the loaded bitmap.</returns>
        private async Task<WriteableBitmap> LoadBitmapAsync(Uri source, bool landscape)
        {
            var file = await StorageFile.GetFileFromApplicationUriAsync(source);
            using (var stream = await file.OpenAsync(FileAccessMode.Read))
            {
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);

                BitmapTransform transform = new BitmapTransform();
                transform.Rotation = BitmapRotation.None;
                uint width = decoder.PixelWidth;
                uint height = decoder.PixelHeight;

                if (landscape && width < height)
                {
                    transform.Rotation = BitmapRotation.Clockwise270Degrees;
                    Swap(ref width, ref height);
                }
                else if (!landscape && width > height)
                {
                    transform.Rotation = BitmapRotation.Clockwise90Degrees;
                    Swap(ref width, ref height);
                }

                PixelDataProvider pixelData = await decoder.GetPixelDataAsync(
                    BitmapPixelFormat.Bgra8,    // WriteableBitmap uses BGRA format.
                    BitmapAlphaMode.Straight,
                    transform,
                    ExifOrientationMode.IgnoreExifOrientation,    // This sample ignores Exif orientation.
                    ColorManagementMode.DoNotColorManage);

                WriteableBitmap bitmap = new WriteableBitmap((int)width, (int)height);
                var pixelBuffer = pixelData.DetachPixelData();
                using (var pixelStream = bitmap.PixelBuffer.AsStream())
                {
                    pixelStream.Write(pixelBuffer, 0, (int)pixelStream.Length);
                }

                return bitmap;
            }
        }
    }


    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FormsPage : Page
	{
		private ILogger log = LogManagerFactory.DefaultLogManager.GetLogger<FormsPage>();

        private PhotosPrintHelper printHelper;

        public enum ReturnResult
		{
			Cancel = 0,
			Send = 2,
			Save = 4
		}
		MainPage rootPage = MainPage.Current;


		//bool WriteBytesAvailable = false;

		List<Control> _formFieldsList = new List<Control>();

		Template10.Services.SerializationService.ISerializationService _SerializationService;

		//Compositor _compositor;

		//string _openFilePath;
		//PacForms pacForm;
		PacketMessage _packetMessage;
		bool _loadMessage = false;

		FormControlBase _packetForm;
		SendFormDataControl _packetAddressForm;

		public static Task<List<Assembly>> AssemblyList;
		//bool _forceReadBulletins = false;

		// Serial Device
		//private SuspendingEventHandler appSuspendEventHandler;
		//private EventHandler<Object> appResumeEventHandler;

		//private Collection<DeviceListEntry> listOfDevices;

		//private Dictionary<DeviceWatcher, String> mapDeviceWatchersToDeviceSelector;
		//private Boolean watchersSuspended;
		//private Boolean watchersStarted;

		// Has all the devices enumerated by the device watcher?
		//private Boolean isAllDevicesEnumerated;

		//List<PacketMessage> _packetMessagesReceived = new List<PacketMessage>();
		//List<PacketMessage> _packetMessagesToSend = new List<PacketMessage>();

		public ReturnResult DialogAction { get; private set; }
		//public string MessageSubject { get { return messageSubject.Text; } set { messageSubject.Text = value; } }
		//public string MessageBBS { get { return messageBBS.Text; } set { messageBBS.Text = value; } }
		//public string MessageTNC { get { return messageTNC.Text; } set { messageTNC.Text = value; }  }
		//public string MessageFormName { get; set; }
		//public string MessageFrom { get { return messageFrom.Text; } set { messageFrom.Text = value; } }
		//public string MessageTo { get { return messageTo.Text; } set { messageTo.Text = value; } }
		public string MessageNumber { get { return _packetForm.MessageNo; } set { _packetForm.MessageNo = value; } }

        private List<FormControlAttributes> _formControlAttributeList;

        public FormsPage()
		{
			this.InitializeComponent();

			_SerializationService = Template10.Services.SerializationService.SerializationService.Json;

            _formControlAttributeList = new List<FormControlAttributes>();
            ScanFormAttributes();

            foreach (FormControlAttributes formControlAttribute in _formControlAttributeList)
            {
                PivotItem pivotItem = CreatePivotItem(formControlAttribute);
                //if (pivotItem.Name != "Message")
                //{
                    MyPivot.Items.Add(pivotItem);
                //}
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //DeviceListSource.Source = listOfDevices;
            if (e.Parameter == null)
                return;

            int index = 0;
            var packetMessagePath = (string)_SerializationService.Deserialize((string)e.Parameter);
            _packetMessage = PacketMessage.Open(packetMessagePath);
            _loadMessage = true;
            foreach (PivotItem pivotItem in MyPivot.Items)
            {
                if (pivotItem.Name == _packetMessage.PacFormType || pivotItem.Name == _packetMessage.PacFormName) // If PacFormType is not set
                {
                    MyPivot.SelectedIndex = index;
                    //CreatePacketForm();
                    //FillFormFromPacketMessage();
                    break;
                }
                index++;
            }
        }

        private PivotItem CreatePivotItem(FormControlAttributes formControlAttributes)
        {
            PivotItem pivotItem = new PivotItem();
            pivotItem.Name = formControlAttributes.FormControlName;
            pivotItem.Header = formControlAttributes.FormControlMenuName;

            ScrollViewer scrollViewer = new ScrollViewer();
            scrollViewer.Margin = new Thickness(0, 12, -12, 0);
            scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            scrollViewer.Height = double.NaN;

            StackPanel stackpanel = new StackPanel();
            stackpanel.Name = pivotItem.Name + "Panel";
            scrollViewer.Content = stackpanel;

            pivotItem.Content = scrollViewer;

            return pivotItem;
        }

        private void ScanFormAttributes()
        {
            var files = ViewModels.SharedData.filesInInstalledLocation;
            if (files == null)
                return;

            foreach (StorageFile file in files.Where(file => file.FileType == ".dll" && file.Name.Contains("FormControl.dll")))
            {
                try
                {
                    Assembly assembly = Assembly.Load(new AssemblyName(file.DisplayName));
                    foreach (Type classType in assembly.GetTypes())
                    {
                        var attrib = classType.GetTypeInfo();
                        foreach (CustomAttributeData customAttribute in attrib.CustomAttributes.Where(customAttribute => customAttribute.GetType() == typeof(CustomAttributeData)))
                        {
                            //if (!(customAttribute is FormControlAttribute))
                            //    continue;
                            var namedArguments = customAttribute.NamedArguments;
                            if (namedArguments.Count == 3)
                            {
                                string formControlType = namedArguments[0].TypedValue.Value as string;
                                FormControlAttribute.FormType FormControlType = (FormControlAttribute.FormType)Enum.Parse(typeof(FormControlAttribute.FormType), namedArguments[1].TypedValue.Value.ToString());
                                string formControlMenuName = namedArguments[2].TypedValue.Value as string;
                                FormControlAttributes formControlAttributes = new FormControlAttributes(formControlType, formControlMenuName, FormControlType, file);
                                _formControlAttributeList.Add(formControlAttributes);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
            // Pick latest file version for each type
            for (int i = 0; i < _formControlAttributeList.Count; i++)
            {
                for (int j = i + 1; j < _formControlAttributeList.Count; j++)
                {
                    if (_formControlAttributeList[i].FormControlName == _formControlAttributeList[j].FormControlName)
                    {
                        // Should be version rather than creation date
                        if (_formControlAttributeList[i].FormControlFileName.DateCreated > _formControlAttributeList[j].FormControlFileName.DateCreated)
                        {
                            _formControlAttributeList.Remove(_formControlAttributeList[j]);
                        }
                        else
                        {
                            _formControlAttributeList.Remove(_formControlAttributeList[i]);
                        }
                    }
                }
            }
            List<FormControlAttributes> attributeListTypeNone = new List<FormControlAttributes>();
            List<FormControlAttributes> attributeListTypeCounty = new List<FormControlAttributes>();
            List<FormControlAttributes> attributeListTypeCity = new List<FormControlAttributes>();
            List<FormControlAttributes> attributeListTypeHospital = new List<FormControlAttributes>();
            // Sort by menu type
            foreach (FormControlAttributes formControlAttributes in _formControlAttributeList)
            {
                if (formControlAttributes.FormControlType == FormControlAttribute.FormType.None)
                {
                    attributeListTypeNone.Add(formControlAttributes);
                }
                else if (formControlAttributes.FormControlType == FormControlAttribute.FormType.CountyForm)
                {
                    attributeListTypeCounty.Add(formControlAttributes);
                }
                else if (formControlAttributes.FormControlType == FormControlAttribute.FormType.CityForm)
                {
                    attributeListTypeCity.Add(formControlAttributes);
                }
                else if (formControlAttributes.FormControlType == FormControlAttribute.FormType.HospitalForm)
                {
                    attributeListTypeHospital.Add(formControlAttributes);
                }
            }
            _formControlAttributeList.Clear();
            _formControlAttributeList.AddRange(attributeListTypeNone);
            _formControlAttributeList.AddRange(attributeListTypeCounty);
            _formControlAttributeList.AddRange(attributeListTypeCity);
            _formControlAttributeList.AddRange(attributeListTypeHospital);
        }

        public void ScanControls(DependencyObject panelName)
		{
			var childrenCount = VisualTreeHelper.GetChildrenCount(panelName);

			for (int i = 0; i < childrenCount; i++)
			{
				DependencyObject control = VisualTreeHelper.GetChild(panelName, i);
				if (control is StackPanel || control is Grid || control is Border || control is RelativePanel)
				{
					ScanControls(control);
				}
				else if (control is TextBox || control is ComboBox || control is CheckBox)
				{
					_formFieldsList.Add((Control)control);
				}
			}
		}


		// Create a packetMessage from the filled out form
		private void CreatePacketMessage()
		{
            _packetMessage = new PacketMessage()
            {
                BBSName = _packetAddressForm.MessageBBS,
                TNCName = _packetAddressForm.MessageTNC,
                FormFieldArray = _packetForm.CreateFormFieldsInXML(),
                PacFormName = _packetForm.PacFormName,
                PacFormType = _packetForm.PacFormType,
                MessageFrom = _packetAddressForm.MessageFrom,
                MessageTo = _packetAddressForm.MessageTo,
                MessageNumber = _packetForm.MessageNo
            };
            AddressBook.AddAddressAsync(_packetMessage.MessageTo);
            string subject = _packetForm.CreateSubject();
			// subject is "null" for Simple Message, otherwise use the form generated subject line
			_packetMessage.Subject = (subject ?? _packetAddressForm.MessageSubject );
			//MessageSubject = _packetMessage.MessageSubject;
			_packetMessage.CreateFileName();
		}

		public void FillFormFromPacketMessage()
		{
			_packetAddressForm.MessageBBS = _packetMessage.BBSName;
			_packetAddressForm.MessageTNC = _packetMessage.TNCName;
			_packetForm.FillFormFromFormFields(_packetMessage.FormFieldArray);
			_packetAddressForm.MessageFrom = _packetMessage.MessageFrom;
			_packetAddressForm.MessageTo = _packetMessage.MessageTo;
			MessageNumber = _packetMessage.MessageNumber;
			_packetAddressForm.MessageSubject = _packetMessage.Subject;

            foreach (FormField formField in _packetMessage.FormFieldArray)
            {
                FormControl formControl = _packetForm.FormControlsList.Find(x => x.InputControl.Name == formField.ControlName);
                if (formControl == null)
                    continue;

                Control control = formControl?.InputControl;
                switch (control.Name)
                {
                    case "severity":
                        _packetForm.Severity = ((ToggleButtonGroup)control).CheckedControlName;
                        break;
                    case "handlingOrder":
                        _packetForm.HandlingOrder = ((ToggleButtonGroup)control).CheckedControlName;
                        break;
                    case "msgDate":
                        _packetForm.MsgDate = ((TextBox)control).Text;
                        break;
                    case "msgTime":
                        _packetForm.MsgTime = ((TextBox)control).Text;
                        break;
                    case "operatorCallsign":
                        _packetForm.OperatorCallsign = ((TextBox)control).Text;
                        break;
                    case "operatorName":
                        _packetForm.OperatorName = ((TextBox)control).Text;
                        break;
                    case "operatorDate":
                        _packetForm.OperatorDate = ((TextBox)control).Text;
                        break;
                    case "operatorTime":
                        _packetForm.OperatorTime = ((TextBox)control).Text;
                        break;
                }
            }
        }

		//async void CreatePacketForm()
		//{
		//	_packetForm = CreateFormControlInstance(_packetMessage.PacFormName);
		//	if (_packetForm == null)
		//	{
		//		MessageDialog messageDialog = new MessageDialog($"Form {_packetMessage.PacFormName} not found");
		//		await messageDialog.ShowAsync();
		//		return;
		//	}
		//	_packetAddressForm = new SendFormDataControl();

		//	if (_packetMessage.PacFormName == "SimpleMessage")
		//	{
		//		messageFormPanel.Children.Clear();
		//		messageFormPanel.Children.Insert(0, _packetAddressForm);
		//		messageFormPanel.Children.Insert(1, _packetForm);
		//	}
		//	else
		//	{
		//		Form213Panel.Children.Clear();
		//		Form213Panel.Children.Insert(0, _packetForm);
		//		Form213Panel.Children.Insert(1, _packetAddressForm);

		//		_packetForm.eventSubjectChanged += FormControl_SubjectChange;

		//		//switch (_packetMessage.PacFormName)
		//		//{
		//		//	case "CERT-DA-MTVUniversal-message":
		//		//		//((ICS213MVControl)_packetForm).eventTacticalCallsign += ICS213MVControl_TacticalCallsignChange;
		//		//		((ICS213MVControl)_packetForm).CERTPositions = IdentitySettings.mtvCERTTacticalCallList;
		//		//		((ICS213MVControl)_packetForm).TacticalCallsign = Properties.Settings.Default.TacticalCallsign;
		//		//		break;
		//		//}
		//		DateTime now = DateTime.Now;
		//		_packetForm.OperatorDate = $"{now.Month:d2}/{now.Day:d2}/{now.Year:d2}";
		//		_packetForm.OperatorTime = $"{now.Hour:d2}{now.Minute:d2}";
		//		//packetFormPanel.Children.Add(_packetForm);
		//	}
		//	MessageNumber = ViewModels.SettingsPageViewModel.GetMessageNumberPacket();
		//}

		//public void ProcessFormByName(string formName)
		//{
		//	TNCDevice tncDevice;
		//	if (MainPage._tncTypes.TryGetValue(_currentProfile.TNC, out tncDevice))
		//	{
		//		BBSData bbs;
		//		if (MainPage._bbsTypes.TryGetValue(_currentProfile.BBS, out bbs))
		//		{
		//			PacketMessage packetMessage = new PacketMessage();
		//			packetMessage.PacFormName = formName;
		//			packetMessage.MessageNumber = MainPage.GetMessageNumberPacket();
		//			packetMessage.BBSConnectName = bbs.ConnectName;
		//			packetMessage.TNCName = tncDevice.Name;
		//			packetMessage.MessageFrom = Properties.Settings.Default.UseTacticalCallsign ? Properties.Settings.Default.TacticalCallsign : Properties.Settings.Default.UserCallSign;

		//			var messageWindow = new PacketFormWindow(ref packetMessage);
		//			//messageWindow.DefaultFolderPath = defaultFolderPath;
		//			DateTime now = DateTime.Now;
		//			messageWindow.MessageDate = $"{now.Month:d2}/{now.Day:d2}/{now.Year:d2}";
		//			messageWindow.MessageTime = $"{now.Hour:d2}{now.Minute:d2}";
		//			//messageWindow.MessageTo = _defaultMessageTo;					
		//			messageWindow.MessageTo = _currentProfile.SendTo;
		//			messageWindow.OperatorCallsign = Properties.Settings.Default.UserCallSign;
		//			messageWindow.OperatorName = Properties.Settings.Default.UserName;
		//			if (messageWindow.ShowDialog() == true)
		//			{
		//				// Get the updated packetMessage
		//				ProcessMessageWindow(ref messageWindow);
		//			}
		//			else
		//			{
		//				// Return the unused message number
		//				MainPage.ReturnMessageNumber();
		//			}
		//		}
		//		else
		//		{
		//			//System.Windows.MessageDialog.Show("The BBS could not be found");
		//			//log.Error("The BBS could not be found in Packet Message Window");
		//		}
		//	}
		//	else
		//	{
		//		//System.Windows.MessageDialog.Show($"Could not find the requested TNC ({_currentProfile.TNC})");
		//		//log.Error($"Could not find the requested TNC ({_currentProfile.TNC})");
		//	}
		//}

		public static FormControlBase CreateFormControlInstance(string controlType)
		{
			FormControlBase formControl = null;
			var files = ViewModels.SharedData.filesInInstalledLocation;
			if (files == null)
				return null;

			Type foundType = null;
			foreach (var file in files.Where(file => file.FileType == ".dll" && file.Name.Contains("FormControl.dll")))
			{
				try
				{
					Assembly assembly = Assembly.Load(new AssemblyName(file.DisplayName));
					foreach (Type classType in assembly.GetTypes())
					{
						var attrib = classType.GetTypeInfo();
						foreach (CustomAttributeData customAttribute in attrib.CustomAttributes.Where(customAttribute => customAttribute.GetType() == typeof(CustomAttributeData)))
						{
                            //if (!(customAttribute is FormControlAttribute))
                            //    continue;
                            var namedArguments = customAttribute.NamedArguments;
							if (namedArguments.Count == 3)
							{
								var formControlType = namedArguments[0].TypedValue.Value as string;
                                //var arg1 = Enum.Parse(typeof(FormControlAttribute.FormType), namedArguments[1].TypedValue.Value.ToString());
                                //var arg2 = namedArguments[2].TypedValue.Value;
                                if (formControlType == controlType)
                                {
									foundType = classType;
									break;
								}
							}
						}
						if (foundType != null)
							break;
					}
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex.Message);
				}
				if (foundType != null)
					break;
			}

			if (foundType != null)
			{
				try
				{
					formControl = (FormControlBase)Activator.CreateInstance(foundType);
				}
				catch (Exception e)
				{
					string message = e.Message;
				}
			}

			return formControl;
		}

        public static FormControlBase CreateFormControlInstanceFromFileName(string fileName)
        {
            FormControlBase formControl = null;
            var files = ViewModels.SharedData.filesInInstalledLocation;
            if (files == null)
                return null;

            //foreach (var file in files)   // Test
            //{
            //    string testFileName = file.Name;
            //}
            Type foundType = null;
            foreach (var file in files.Where(file => file.FileType == ".dll" && file.Name.Contains(fileName)))
            {
                try
                {
                    Assembly assembly = Assembly.Load(new AssemblyName(file.DisplayName));
                    foreach (Type classType in assembly.GetTypes())
                    {
                        var attrib = classType.GetTypeInfo();
                        foreach (CustomAttributeData customAttribute in attrib.CustomAttributes.Where(customAttribute => customAttribute.GetType() == typeof(CustomAttributeData)))
                        {
                            //if (!(customAttribute is FormControlAttribute))
                            //    continue;
                            var namedArguments = customAttribute.NamedArguments;
                            if (namedArguments.Count == 3)
                            {
                                var formControlName = namedArguments[0].TypedValue.Value as string;
                                //var arg1 = Enum.Parse(typeof(FormControlAttribute.FormType), namedArguments[1].TypedValue.Value.ToString());
                                //var arg2 = namedArguments[2].TypedValue.Value;
                                //if (formControlName == controlName)
                                //{
                                    foundType = classType;
                                    break;
                                //}
                            }
                        }
                        if (foundType != null)
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
                if (foundType != null)
                    break;
            }

            if (foundType != null)
            {
                try
                {
                    formControl = (FormControlBase)Activator.CreateInstance(foundType);
                }
                catch (Exception e)
                {
                    string message = e.Message;
                }
            }

            return formControl;
        }

        void FormControl_SubjectChange(object sender, FormEventArgs e)
		{
			if (e?.SubjectLine?.Length > 0)
			{
				if (_packetMessage != null)
				{
					_packetMessage.Subject = _packetForm.CreateSubject();
					_packetAddressForm.MessageSubject = _packetMessage.Subject;
				}
			}
		}

		private async void MyPivot_SelectionChangedAsync(object sender, SelectionChangedEventArgs e)
		{
            if (printHelper != null)
            {
                printHelper.UnregisterForPrinting();
            }

            ViewModels.SettingsPageViewModel.ReturnMessageNumber();

			_packetAddressForm = new SendFormDataControl();
            PivotItem pivotItem = (PivotItem)((Pivot)sender).SelectedItem;
            string pivotItemName = pivotItem.Name;
            _packetForm = CreateFormControlInstance(pivotItemName); // Should be PacketFormName, since there may be multiple files with same name
            if (_packetForm == null)
            {
                MessageDialog messageDialog = new MessageDialog(content: "Failed to find packet form.", title: "Packet Messaging Error");
                await messageDialog.ShowAsync();
                return;
            }

            if (!_loadMessage)
            {
                _packetMessage = new PacketMessage();
            }
			_packetForm.MessageNo = ViewModels.SettingsPageViewModel.GetMessageNumberPacket();

            StackPanel stackPanel = ((ScrollViewer)pivotItem.Content).Content as StackPanel;

            stackPanel.Children.Clear();
            if (pivotItemName == "SimpleMessage")
			{
                stackPanel.Children.Insert(0, _packetAddressForm);
                stackPanel.Children.Insert(1, _packetForm);
            }
            //else if (pivotItemName == "Message")
            //{
            //    Form213Panel.Children.Clear();
            //    Form213Panel.Children.Insert(0, _packetForm);
            //    Form213Panel.Children.Insert(1, _packetAddressForm);
            //    _packetAddressForm.MessageSubject = _packetForm.CreateSubject();
            //}
            //else if (pivotItemName != "Message")
            else
            {
                stackPanel.Children.Insert(0, _packetForm);
                stackPanel.Children.Insert(1, _packetAddressForm);
            }

            _packetAddressForm.MessageSubject = _packetForm.CreateSubject();

            if (!_loadMessage)
            {
                _packetForm.EventSubjectChanged += FormControl_SubjectChange;

                DateTime now = DateTime.Now;
                _packetForm.MsgDate = $"{now.Month:d2}/{now.Day:d2}/{now.Year - 2000:d2}";
                _packetForm.MsgTime = $"{now.Hour:d2}{now.Minute:d2}";
                _packetForm.OperatorDate = $"{now.Month:d2}/{now.Day:d2}/{now.Year - 2000:d2}";
                _packetForm.OperatorTime = $"{now.Hour:d2}{now.Minute:d2}";
                _packetForm.OperatorName = ViewModels.SettingsPageViewModel.IdentityPartViewModel.UserName;
                _packetForm.OperatorCallsign = ViewModels.SettingsPageViewModel.IdentityPartViewModel.UserCallsign;
            }

			if (_loadMessage)
			{
				ViewModels.SettingsPageViewModel.ReturnMessageNumber();	// Use original message number

				FillFormFromPacketMessage();
                //_packetForm.MsgTime = ViewModels.FormsPageViewModel.
                _loadMessage = false;
            }
            // Printing-related event handlers will never be called if printing
            // is not supported, but it's okay to register for them anyway.

            // Initalize common helper class and register for printing
            //printHelper = new PrintHelper(this);
            printHelper = new PhotosPrintHelper(this);
            printHelper.RegisterForPrinting();
        }

        private void ICS213Control_Loaded(object sender, RoutedEventArgs e)
		{

		}
#region SendMessage
		private async void AppBarSend_ClickAsync(object sender, RoutedEventArgs e)
		{
            string validationResult = _packetForm.ValidateForm();
            validationResult = _packetAddressForm.ValidateForm(validationResult);
            if (!string.IsNullOrEmpty(validationResult))
            {
                //validationResult = "Please fill out the areas in red." + validationResult;
                validationResult += "\n\nAdd the missing information and press \"Send\" to continue.";
                ContentDialog contentDialog = new ContentDialog
                {
                    Title = "Missing input fields",
                    Content = validationResult,
                    CloseButtonText = "Close"
                };
                ContentDialogResult result = await contentDialog.ShowAsync();
                return;
            }

            CreatePacketMessage();
			DateTime dateTime = DateTime.Now;
			_packetMessage.CreateTime = $"{dateTime.Month:d2}/{dateTime.Day:d2}/{dateTime.Year - 2000:d2} {dateTime.Hour:d2}:{dateTime.Minute:d2}";

			_packetMessage.Save(MainPage.unsentMessagesFolder.Path);

			Services.CommunicationsService.CommunicationsService communicationsService = Services.CommunicationsService.CommunicationsService.CreateInstance();
			communicationsService.BBSConnectAsync();
		}


#endregion SendMessage

		private void AppBarSave_Click(object sender, RoutedEventArgs e)
		{
			CreatePacketMessage();
			DateTime dateTime = DateTime.Now;
			_packetMessage.CreateTime = $"{dateTime.Month:d2}/{dateTime.Day:d2}/{dateTime.Year - 2000:d2} {dateTime.Hour:d2}:{dateTime.Minute:d2}";

			_packetMessage.Save(MainPage._draftMessagesFolder.Path);
			_packetForm.MessageNo = ViewModels.SettingsPageViewModel.GetMessageNumberPacket();
		}

        private async void AppBarPrint_ClickAsync(object sender, RoutedEventArgs e)
        {
            await printHelper.ShowPrintUIAsync();
        }

        private async void AppBarViewOutpostData_ClickAsync(object sender, RoutedEventArgs e)
        {
            CreatePacketMessage();

            if (string.IsNullOrEmpty(_packetMessage.MessageBody))
            {
                _packetMessage.MessageBody = _packetForm.CreateOutpostData(ref _packetMessage);
            }

            //outpostDataDialog.Title = "Outpost Message";
            //outpostDataDialog.Content = _packetMessage.MessageBody;
            //ContentDialogResult result = await outpostDataDialog.ShowAsync();

            //outpostDataDialog contentDialog = new ContentDialog();
            outpostDataDialog.Title = "Outpost Message";
            outpostDataDialog.Content = _packetMessage.MessageBody;
            outpostDataDialog.CloseButtonText = "Cancel";
            outpostDataDialog.IsPrimaryButtonEnabled = true;
            outpostDataDialog.PrimaryButtonText = "Copy";
            ContentDialogResult result = await outpostDataDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                DataPackage dataPackage = new DataPackage();
                dataPackage.RequestedOperation = DataPackageOperation.Copy;
                dataPackage.SetText(_packetMessage.MessageBody);
                Clipboard.SetContent(dataPackage);
            }            
        }

        //private async void appBarOpen_Click(object sender, RoutedEventArgs e)
        //{
        //	StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", MainPage.draftMessagesFolder);

        //	FileOpenPicker fileOpenPicker = new FileOpenPicker();
        //	fileOpenPicker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
        //	fileOpenPicker.FileTypeFilter.Add(".xml");
        //	StorageFile file = await fileOpenPicker.PickSingleFileAsync();
        //	if (file != null)
        //	{
        //		_packetMessage = PacketMessage.Open(MainPage.draftMessagesFolder.Path + @"\" + file.Name);

        //		foreach (PivotItem pivotItem in MyPivot.Items)
        //		{
        //			if (pivotItem.Name == _packetMessage.PacFormName)
        //			{
        //				MyPivot.SelectedItem = pivotItem;
        //				CreatePacketForm();
        //				FillFormFromPacketMessage();
        //				break;
        //			}
        //		}
        //	}
        //}
    }
}