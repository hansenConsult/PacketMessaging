﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using FormControlBaseClass;
using Windows.Storage;
using MetroLog;
using Windows.Storage.Search;
using Windows.Data.Pdf;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Media.Ocr;
using Windows.Graphics.Imaging;
using Windows.Globalization;
using System.Text;
using PacketMessaging.Models;

namespace PacketMessaging.Models
{
	public class TacticalCallsignData
    {
        private string fileNameField;

        private string areaNameField;

        private string startStringField;

		private string stopStringField;

		private string rawDataFileNameField;

        private string bulletinFileNameField;

        private TacticalCallsigns tacticalCallsignsField;

        private bool tacticalCallsignsChangedField = false;

        public string FileName
        {
            get { return fileNameField; }
            set { fileNameField = value; }
        }

        public string AreaName
        {
            get { return areaNameField; }
            set { areaNameField = value; }
        }

        public string StartString
        {
            get { return startStringField; }
            set { startStringField = value; }
        }

		public string StopString
		{
			get { return stopStringField; }
			set { stopStringField = value; }
		}

		public string RawDataFileName
		{
			get => rawDataFileNameField;
			set => rawDataFileNameField = value;
		}

		public string BulletinFileName
        {
            get { return bulletinFileNameField; }
            set { bulletinFileNameField = value; }
        }

        public TacticalCallsigns TacticalCallsigns
        {
            get { return tacticalCallsignsField; }
            set { tacticalCallsignsField = value; }
        }

        public bool TacticalCallsignsChanged
        {
            get { return tacticalCallsignsChangedField; }
            set { tacticalCallsignsChangedField = value; }
        }
    }

    // 
    // This source code was auto-generated by xsd, Version=4.0.30319.33440.
    // With modifications
    //


    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    //[System.SerializableAttribute()]
    //[System.Diagnostics.DebuggerStepThroughAttribute()]
    //[System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class TacticalCallsigns
    {
        //private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static ILogger log = LogManagerFactory.DefaultLogManager.GetLogger<TacticalCallsigns>();

		private static string TactiCallsMasterFileName = "Tactical_Calls.txt";


		private TacticalCall[] tacticalCallsignsArrayField;

        private string areaField;

        private DateTime bulletinCreationTimeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("TacticalCalls", IsNullable = false)]
        public TacticalCall[] TacticalCallsignsArray
        {
            get
            {
                return this.tacticalCallsignsArrayField;
            }
            set
            {
                this.tacticalCallsignsArrayField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Area
        {
            get
            {
                return this.areaField;
            }
            set
            {
                this.areaField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime BulletinCreationTime
        {
            get
            {
                return this.bulletinCreationTimeField;
            }
            set
            {
                this.bulletinCreationTimeField = value;
            }
        }


        public static async Task<TacticalCallsigns> OpenAsync(string fileName)
        {
            TacticalCallsigns tacticalCallsigns = null;
            StorageFile file = null;
            try
            {
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                var storageItem = await localFolder.TryGetItemAsync(fileName);
				FileInfo fileInfo = null;
				if (storageItem != null)
				{
					fileInfo = new FileInfo(storageItem.Path);
				}
				if (storageItem == null || fileInfo?.Length == 0)
                {
                    TacticalCallsignData tacticalCallsignData = App._tacticalCallsignDataDictionary[fileName];
					if (tacticalCallsignData.RawDataFileName == "Tactical_Calls.txt")
					{
						storageItem = await localFolder.TryGetItemAsync(TactiCallsMasterFileName);
						if (storageItem == null)
						{
							// Copy from install folder
							var folder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Assets");
							var storageFile = await folder.TryGetItemAsync(TactiCallsMasterFileName);
							if (storageFile != null)
							{
								await ((StorageFile)storageFile).CopyAsync(localFolder, TactiCallsMasterFileName, Windows.Storage.NameCollisionOption.ReplaceExisting);
							}
							else
							{
								return null; // Reinstall
							}
						}

						tacticalCallsigns = await ParseTactiCallsMasterList(tacticalCallsignData);
					}
					else
					{
						tacticalCallsigns = null;// await CreateFromBulletinAsync(tacticalCallsignData); Exception here
					}
                    if (tacticalCallsigns == null)
                    {
                        // Copy from data folder
                        storageItem = await localFolder.TryGetItemAsync(tacticalCallsignData.FileName);
                        if (storageItem == null)
                        {
                            // Copy the file from the install folder to the local folder
                            var folder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Assets");
                            var storageFile = await folder.TryGetItemAsync(tacticalCallsignData.FileName);
                            if (storageFile != null)
                            {
                                await ((StorageFile)storageFile).CopyAsync(localFolder, tacticalCallsignData.FileName, Windows.Storage.NameCollisionOption.FailIfExists);
                            }
                            else
                            {

                            }
                        }
                    }
                    else
                    {
                        tacticalCallsigns.SaveAsync(fileName);
                    }
                }

                file = await localFolder.GetFileAsync(fileName);

                using (FileStream stream = new FileStream(file.Path, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(TacticalCallsigns));
                        tacticalCallsigns = (TacticalCallsigns)serializer.Deserialize(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Warn($"Failed to open {fileName}. {ex.Message}");
            }

            return tacticalCallsigns;
        }

        public async void SaveAsync(string fileName)
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            try
            {
                StorageFile file = await localFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                using (StreamWriter writer = new StreamWriter(new FileStream(file.Path, FileMode.OpenOrCreate)))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(TacticalCallsigns));
                    serializer.Serialize(writer, this);
                }
            }
            catch (Exception e)
            {
                log.Error($"Failed to save {fileName}", e);
            }
        }

        public static async void SaveAsync(TacticalCallsigns tacticalCallsigns, string fileName)
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            try
            {
                StorageFile file = await localFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                using (StreamWriter writer = new StreamWriter(new FileStream(file.Path, FileMode.OpenOrCreate)))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(TacticalCallsigns));
                    serializer.Serialize(writer, tacticalCallsigns);
                }
            }
            catch (Exception e)
            {
                log.Error($"Failed to save {fileName}", e);
            }
        }

        static bool IsAllCaps(string capitalString)
        {
            string lowercaseString = capitalString.ToLower();
            for (int i = 0; i < capitalString.Length; i++)
            {
                if (capitalString[i] != ' ' && capitalString[i] != '\'')
                {
                    if (lowercaseString[i] == capitalString[i])
                        return false;
                }
            }
            return true;
        }

       static string AdjustCaptilization(string capitalString)
        {
            // Only capitalize the first letter in each word
            string resultStringLower = capitalString.ToLower();
            char[] resultStringArray = resultStringLower.ToCharArray();
            for (int i = 0; i < capitalString.Length; i++)
            {
                if (i == 0 || capitalString[i - 1] == ' ' || capitalString[i - 1] == '\'')
                {
                    resultStringArray[i] = capitalString[i];
                }
            }
            return new string(resultStringArray);
        }

        /*"Santa Clara County Hospital Net Script HOSPITAL ROLL CALL Revised April 2015 DATE Hospital MEDICAL HEALTH OPERATIONS CENTER (Packet HOSDOC) STANFORD HOSPITAL (Packet: HOSSUH) PALO ALTO VETERANS HOSPITAL (Packet HOSPAV) EL CAMINO HOSPITAL MOUNTAIN VIEW (Packet HOSECM) KAISER SANTA CLARA (Packet HOSKSC) EL CAMINO HOSPITAL LOS GATOS (Packet HOSECL) GOOD SAMARITAN HOSPITAL (Packet HOSGSH) O'CONNOR HOSPITAL (Packet HOSOCH) REGIONAL SAN JOSE (Packet HOSRSJ) VALLEY MEDICAL CENTER (Packet HOSVMC) KAISER SAN JOSE MEDICAL CENTER (Packet HOSKSJ) Call Sign Name Call Sign Name Traffic Hos Packet Eq (Net Control: SLH is using a cross band repeater. After calling them there will be 4 second delay before they respond.) SAINT LOUISE REGIONAL (Packet HOSSLH) [Go back and call again for any hospital(s) not responding the first time]"*/
        private static void ParseHospitals(ref TacticalCallsigns tacticalCallsigns, string hospitalString)
        {
            string hospitalData = hospitalString;
            hospitalData = hospitalData.Replace("Call Sign Name Call Sign Name Traffic Hos Packet Eq (Net Control: SLH is using a cross band repeater. After calling them there will be 4 second delay", "");
            hospitalData = hospitalData.Replace("before they respond.)", "");

            List<TacticalCall> tacticalList = new List<TacticalCall>();

            var lines = hospitalData.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            int startIndex;
            int i = 0;

            string searchString = "Revised ";
            while ((startIndex = lines[i++].IndexOf(searchString)) < 0 && i < lines.Length);
            if (startIndex < 0)
            {
                log.Error($"Failed to parse {tacticalCallsigns.Area} at {searchString}");
                return;
            }
            string resultString = lines[i-1].Substring(startIndex + searchString.Length);

            DateTime revisionDate = DateTime.Parse(resultString);
            tacticalCallsigns.BulletinCreationTime = revisionDate;

            searchString = "HOSDOC";
            while ((startIndex = lines[i++].IndexOf(searchString)) < 0 && i < lines.Length);
            if (startIndex < 0)
            {
                log.Error($"Failed to parse {tacticalCallsigns.Area} at {searchString}");
                return;
            }

            (string primaryBBS, string secondaryBBS) = FindCountyPrimSecBBS("HOSDOC");

            for (; i < lines.Length; i++)
            {
                if (!IsAllCaps(lines[i]))
                    continue;

                TacticalCall tacticalCall = new TacticalCall();

                // Only capitalize the first letter in each word                
                tacticalCall.AgencyName = AdjustCaptilization(lines[i]);
                if (!lines[i+1].Contains("Packet"))
                {
                    i++;
                    tacticalCall.AgencyName += (" " + AdjustCaptilization(lines[i]));
                }
                // Tactical callsign
                i++;
                searchString = "(Packet: ";
                startIndex = lines[i].IndexOf(searchString);
                resultString = lines[i].Substring(startIndex + searchString.Length, 6);
                tacticalCall.TacticalCallsign = resultString;
                tacticalCall.Prefix = resultString.Substring(3, 3);

                tacticalCall.PrimaryBBS = primaryBBS;
                tacticalCall.PrimaryBBSActive = true;
                tacticalCall.SecondaryBBS = secondaryBBS;
                //tacticalCall.SecondaryBBSActive = false;
                tacticalList.Add(tacticalCall);
            }            
            tacticalCallsigns.Area = "County Hospitals";
            tacticalCallsigns.TacticalCallsignsArray = tacticalList.ToArray();
        }

        private static void ParseMountainView(ref TacticalCallsigns tacticalCallsigns, string mountainViewString)
        {
            string mtvString = mountainViewString;

            List<TacticalCall> tacticalList = new List<TacticalCall>();

			(string primaryBBS, string secondaryBBS) = FindCountyPrimSecBBS("MTVEOC");

            var callsigns = mtvString.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in callsigns)
            {
                int index = line.IndexOf("MTV");
                int index2 = line.IndexOf(@"\t");
                if (index < 0 || index2 < 0)
                    continue;

				tacticalList.Add(new TacticalCall()
				{
					TacticalCallsign = line.Substring(0, 6),
					Prefix = line.Substring(3, 3),
					AgencyName = line.Substring(8),
					PrimaryBBS = primaryBBS,
					PrimaryBBSActive = true,
					SecondaryBBS = secondaryBBS,
					//SecondaryBBSActive = false
				});
            }
            string searchString = "emergency_plan_";
            // Find revision time
            foreach (var line in callsigns)
            {
                if (line.Contains(searchString))
                {
                    int startIndex = line.IndexOf(searchString) + searchString.Length;
                    int endIndex = line.IndexOf(@"\t");
                    string revisionTime = line.Substring(startIndex, endIndex - startIndex);
                    DateTime revisionDate = DateTime.Parse(revisionTime);
                    tacticalCallsigns.BulletinCreationTime = revisionDate;
                    break;
                }
            }
            tacticalCallsigns.Area = "Local Mountain View";
            tacticalCallsigns.TacticalCallsignsArray = tacticalList.ToArray();
        }

		public static async Task<TacticalCallsigns> ParseTactiCallsMasterList(TacticalCallsignData tacticalCallsignData)
		{
			StorageFolder localFolder = ApplicationData.Current.LocalFolder;
			StorageFile file = await localFolder.GetFileAsync(TactiCallsMasterFileName);
			string tacticalCalls = await Windows.Storage.FileIO.ReadTextAsync(file);

			TacticalCallsigns tacticalCallsigns = new TacticalCallsigns();
			List<TacticalCall> tacticalList = new List<TacticalCall>();

			switch (tacticalCallsignData.AreaName)
			{
				case "County Hospitals":			
					(string primaryBBS, string secondaryBBS) = FindCountyPrimSecBBS("HOSDOC");

					string startString = "HOSDOC	SCCo Hospitals DEOC";
					int startIndex = tacticalCalls.IndexOf(startString);
					startIndex += startString.Length;
					int stopIndex = tacticalCalls.IndexOf("# HOS001 - HOS010");
					string tacticalLinesPlus = tacticalCalls.Substring(startIndex, stopIndex - startIndex);
					var tacticallLines = tacticalLinesPlus.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
					foreach (string line in tacticallLines)
					{
						if (!line.StartsWith("#"))
						{
							string callsign = line.Substring(0, 6);
							string agencyName = line.Substring(6);
							// Remove tab characters, if any
							var parts = agencyName.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
							TacticalCall tacticalCall = new TacticalCall()
							{
								AgencyName = parts[0],
								TacticalCallsign = callsign,
								Prefix = callsign.Substring(3, 3),
								PrimaryBBS = primaryBBS,
								PrimaryBBSActive = true,
								SecondaryBBS = secondaryBBS,
							};
							tacticalList.Add(tacticalCall);
						}
					}
					tacticalCallsigns.Area = "County Hospitals";
					tacticalCallsigns.TacticalCallsignsArray = tacticalList.ToArray();
					break;
				case "Local Mountain View":
					(primaryBBS, secondaryBBS) = FindCountyPrimSecBBS("MTVEOC");

					startString = "MTVEOC	Mtn. View Emergency Operations Ctr";
					startIndex = tacticalCalls.IndexOf(startString);
					startIndex += startString.Length;
					stopIndex = tacticalCalls.IndexOf("#MTV001 thru MTV010 also permissible");
					tacticalLinesPlus = tacticalCalls.Substring(startIndex, stopIndex - startIndex);
					tacticallLines = tacticalLinesPlus.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
					foreach (string line in tacticallLines)
					{
						if (!line.StartsWith("#"))
						{
							string callsign = line.Substring(0, 6);
							string agencyName = line.Substring(6);
							// Remove tab characters, if any
							var parts = agencyName.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
							TacticalCall tacticalCall = new TacticalCall()
							{
								AgencyName = parts[0],
								TacticalCallsign = callsign,
								Prefix = callsign.Substring(3, 3),
								PrimaryBBS = primaryBBS,
								PrimaryBBSActive = true,
								SecondaryBBS = secondaryBBS,
							};
							tacticalList.Add(tacticalCall);
						}
					}
					tacticalCallsigns.Area = "Local Mountain View";
					tacticalCallsigns.TacticalCallsignsArray = tacticalList.ToArray();
					break;
				case "Local Cupertino":
					(primaryBBS, secondaryBBS) = FindCountyPrimSecBBS("CUPEOC");

					startString = "# Cupertino OES";
					startIndex = tacticalCalls.IndexOf(startString);
					startIndex += startString.Length;
					stopIndex = tacticalCalls.IndexOf("# City of Palo Alto");
					tacticalLinesPlus = tacticalCalls.Substring(startIndex, stopIndex - startIndex);
					tacticallLines = tacticalLinesPlus.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
					foreach (string line in tacticallLines)
					{
						if (!line.StartsWith("#"))
						{
							string callsign = line.Substring(0, 6);
							string agencyName = line.Substring(6);
							// Remove tab characters, if any
							var parts = agencyName.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
							TacticalCall tacticalCall = new TacticalCall()
							{
								AgencyName = parts[0],
								TacticalCallsign = callsign,
								Prefix = callsign.Substring(3, 3),
								PrimaryBBS = primaryBBS,
								PrimaryBBSActive = true,
								SecondaryBBS = secondaryBBS,
							};
							tacticalList.Add(tacticalCall);
						}
					}
					tacticalCallsigns.Area = tacticalCallsignData.AreaName;
					tacticalCallsigns.TacticalCallsignsArray = tacticalList.ToArray();
					break;
			}
			return tacticalCallsigns;

			// Local Function
			TacticalCall CreateTactical(string line, string primaryBBS, string secondaryBBS)
			{
				TacticalCall tacticalCall = null;
				if (!line.StartsWith("#"))
				{
					string callsign = line.Substring(0, 6);
					string agencyName = line.Substring(6);
					// Remove tab characters, if any
					var parts = agencyName.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
					tacticalCall = new TacticalCall()
					{
						AgencyName = parts[0],
						TacticalCallsign = callsign,
						Prefix = callsign.Substring(3, 3),
						PrimaryBBS = primaryBBS,
						PrimaryBBSActive = true,
						SecondaryBBS = secondaryBBS
					};
				}
				return tacticalCall;
			}
		}


		public class LineInfo
        {
            double lineTop;
            int lineNumber = -1;
            double columnStart;
            double columnWidth;
            double columnHeight;
            int columnNumber = -1;
            string columnString;

            public double LineTop { get => lineTop;  set => lineTop = value;  }
            public int LineNumber { get => lineNumber;  set => lineNumber = value;  }
            public double ColumnStart { get => columnStart;  set => columnStart = value;  }
            public double ColumnWidth { get => columnWidth;  set => columnWidth = value;  }
            public double ColumnHeight { get => columnHeight;  set => columnHeight = value;  }
            public int ColumnNumber { get => columnNumber;  set => columnNumber = value;  }
            public string ColumnString { get => columnString;  set => columnString = value; }
        }


        private static async Task<string> ConvertPDFToTextAsync(PdfDocument pdfDocument, uint startPage, uint endPage)
        {
            string recognizedText = "";

            for (uint p = startPage; p <= endPage; p++)
            {
                using (PdfPage page = pdfDocument.GetPage(p))
                {
                    var stream = new InMemoryRandomAccessStream();
                    var options = new PdfPageRenderOptions()
                    {
                        DestinationHeight = (uint)page.Size.Height * 2,
                        DestinationWidth = (uint)page.Size.Width * 2
                    };

                    await page.RenderToStreamAsync(stream, options);

                    //BitmapImage src = new BitmapImage();
                    //await src.SetSourceAsync(stream);

                    var decoder = await BitmapDecoder.CreateAsync(stream);
                    //SoftwareBitmap bitmap = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                    SoftwareBitmap bitmap = await decoder.GetSoftwareBitmapAsync();

                    var imgSource = new WriteableBitmap(bitmap.PixelWidth, bitmap.PixelHeight);
                    bitmap.CopyToBuffer(imgSource.PixelBuffer);


                    // Check if OcrEngine supports image resoulution.
                    if (bitmap.PixelWidth > OcrEngine.MaxImageDimension || bitmap.PixelHeight > OcrEngine.MaxImageDimension)
                    {
                        log.Error(String.Format($"Bitmap dimensions ({bitmap.PixelWidth}x{bitmap.PixelHeight}) are too big for OCR.") +
                            "Max image dimension is " + OcrEngine.MaxImageDimension + ".");

                        return "";
                    }
                    OcrEngine ocrEngine = OcrEngine.TryCreateFromLanguage(new Language("en"));

                    if (ocrEngine != null)
                    {
                        Dictionary<double, string> tacticalDefinitions = new Dictionary<double, string>();
                        List<LineInfo> lineInfoList = new List<LineInfo>();
                        List<LineInfo> lineInfoByLineTop = new List<LineInfo>();
                        
                        // Recognize text from image.
                        var ocrResult = await ocrEngine.RecognizeAsync(bitmap);
                        foreach (var line in ocrResult.Lines)
                        {
                            LineInfo lineInfo = new LineInfo()
                            {
                                LineTop = line.Words[0].BoundingRect.Top,
                                ColumnStart = line.Words[0].BoundingRect.Left,
                                ColumnWidth = line.Words[line.Words.Count - 1].BoundingRect.Right - line.Words[0].BoundingRect.Left,
                                ColumnHeight = line.Words[0].BoundingRect.Height,
                                ColumnString = line.Text
                            };
                            lineInfoList.Add(lineInfo);
                        }

                        // Sort by line top
                        for (int i = 0; i < lineInfoList.Count; i++)
                        {
                            lineInfoByLineTop.Add(lineInfoList[i]);
                            for (int j = i + 1; j < lineInfoList.Count; j++)
                            {
                                if (lineInfoList[i].LineTop < lineInfoList[j].LineTop + 5 && lineInfoList[i].LineTop > lineInfoList[j].LineTop - 5)
                                {
                                    if (lineInfoList[j].ColumnStart - (lineInfoList[i].ColumnStart + lineInfoList[i].ColumnWidth) > 10)
                                    {
                                        lineInfoList[i].ColumnString += (@"\t" + lineInfoList[j].ColumnString);

                                        lineInfoList.Remove(lineInfoList[j]);
                                    }
                                }
                            }
                        }
                        StringBuilder stringBuilder = new StringBuilder();
                        // Create text string
                        for (int i = 0; i < lineInfoList.Count; i++)
                        {
                            stringBuilder.AppendLine(lineInfoList[i].ColumnString);
                            if (i < lineInfoList.Count - 1 && lineInfoList[i + 1].LineTop - lineInfoList[i].LineTop > lineInfoList[i].ColumnHeight * 2)
                            {
                                stringBuilder.AppendLine();
                            }
                        }
                        return stringBuilder.ToString();
                    }
                }
            }
            return recognizedText;
        }

        public static async Task<PacketMessage> FindLatestBulletinAsync(TacticalCallsignData tacticalCallsignData)  //.BulletinFileName, string bulletin
        {
			DateTime lastRevisionTime = DateTime.MinValue;
			PacketMessage bulletinPacketMessage = null;

            StorageFolder archivedMessagesFolder = Views.MainPage._archivedMessagesFolder;

            // Set options for file type and sort order.
            List<string> fileTypeFilter = new List<string>();
            fileTypeFilter.Add(".xml");
            fileTypeFilter.Add(".pdf");
			fileTypeFilter.Add(".txt");
			QueryOptions queryOptions = new QueryOptions(CommonFileQuery.DefaultQuery, fileTypeFilter);

            // Get the files in the user's archive folder
            StorageFileQueryResult results = archivedMessagesFolder.CreateFileQueryWithOptions(queryOptions);
            // Iterate over the results
            IReadOnlyList<StorageFile> files = await results.GetFilesAsync();
            bool latestFound = false;
            foreach (StorageFile file in files)
            {
                if (file.FileType == ".pdf")
                {
                    TacticalCallsigns tacticalCallsigns = new TacticalCallsigns();
                    PdfDocument pdfDocument = await PdfDocument.LoadFromFileAsync(file);

                    // Local Mountain View
                    if (tacticalCallsignData.AreaName == "Local Mountain View" && tacticalCallsignData.BulletinFileName.ToLower().Contains(file.Name.ToLower()))
                    {
                        string pdfText = await ConvertPDFToTextAsync(pdfDocument, 15, 15);

                        ParseMountainView(ref tacticalCallsigns, pdfText);
                        latestFound = true;
                    }
                    else if (tacticalCallsignData.AreaName == "County Hospitals" && tacticalCallsignData.BulletinFileName.ToLower().Contains(file.Name.ToLower()))    // Hospitals
                    {
                        // Page index starts at 0
                        string pdfText = await ConvertPDFToTextAsync(pdfDocument, 1, 1);

						ParseHospitals(ref tacticalCallsigns, pdfText);
						StorageFolder localFolder = ApplicationData.Current.LocalFolder;
						var storageItem = await localFolder.TryGetItemAsync(TactiCallsMasterFileName);
						if (storageItem == null)
						{

							StorageFile tacticalfile = await localFolder.GetFileAsync(TactiCallsMasterFileName);
							string tacticalCalls = await Windows.Storage.FileIO.ReadTextAsync(file);

							//ParseTactiCallsMasterList(ref tacticalCallsigns, tacticalCalls);
						}

						latestFound = true;
                    }
                    if (latestFound)
                    {
                        tacticalCallsignData.TacticalCallsigns = tacticalCallsigns;
						//SaveAsync(tacticalCallsigns, tacticalCallsignData.FileName);
						tacticalCallsigns.SaveAsync(tacticalCallsignData.FileName);
						return null;
                    }
                }
                else if (file.FileType == ".xml")
                {
                    PacketMessage packetMessage = PacketMessage.Open(file);
                }
            }


            //DirectoryInfo diFolderPath = new DirectoryInfo(Views.MainPage._archivedMessagesFolder.Path);
            //foreach (FileInfo fi in diFolderPath.GetFiles("*.xml"))
            fileTypeFilter = new List<string>();
            fileTypeFilter.Add(".xml");
            queryOptions = new QueryOptions(CommonFileQuery.DefaultQuery, fileTypeFilter);

            // Get the files in the user's archive folder
            results = archivedMessagesFolder.CreateFileQueryWithOptions(queryOptions);
            // Iterate over the results
            files = await results.GetFilesAsync();
            foreach (StorageFile file in files)

            {
                PacketMessage packetMessage = PacketMessage.Open(file);

				var lines = packetMessage.MessageBody?.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
				for (int i = 0; i < lines?.Length; i++)
				{
					if (lines[i].Contains(tacticalCallsignData.BulletinFileName))
					{
						for (; i < lines.Length; i++)
						{
							int start = 0;
							int end;
							string startSearchString = "revised:";
							if ((start = lines[i].IndexOf(startSearchString)) >= 0)
							{
								start += startSearchString.Length;
								end = lines[i].IndexOf("by");
								string revTime = "";
								if (end >= 0)
								{
									revTime = lines[i].Substring(start, end - 1 - start).Trim();
								}
								else
								{
									revTime = lines[i].Substring(start, lines[i].Length - start).Trim();
								}
								revTime = revTime.Replace("at", "");
								DateTime revisionTime = DateTime.Parse(revTime);
								if (revisionTime > lastRevisionTime)
								{
									lastRevisionTime = revisionTime;
									packetMessage.CreateTime = $"{lastRevisionTime.Month:d2}/{lastRevisionTime.Day:d2}/{lastRevisionTime.Year - 2000:d2} {lastRevisionTime.Hour:d2}:{lastRevisionTime.Minute:d2}";
									bulletinPacketMessage = packetMessage;
								}
								break;
							}
						}
					}
				}
			}
			return bulletinPacketMessage;
		}

		public static async Task<TacticalCallsigns> CreateFromBulletinAsync(TacticalCallsignData tacticalCallsignData)
		{
			//Read the bulletin file
			PacketMessage tacticalCallsBulletin = await FindLatestBulletinAsync(tacticalCallsignData);
			if (tacticalCallsBulletin == null)
			{
                // If none are found download pdf file  BulletinFileName = "http://www.scc-ares-races.org/netscripts/HospitalNetScriptApr2015.pdf"
                if (tacticalCallsignData.BulletinFileName.StartsWith("http"))
                {
                    BackgroundTransfer backgroundTransfer = BackgroundTransfer.CreateBackgroundTransfer();
                    // The downloaded file is in the archived folder. The name is derived from the URI
                    backgroundTransfer.StartDownloadAsync(tacticalCallsignData.BulletinFileName);
                }
                return null;
			}

			string bulletin = tacticalCallsBulletin?.MessageBody;

			if (bulletin == null)
				return null;

			string primaryBBS = "";
			string secondaryBBS = "";

			string agency = tacticalCallsignData.AreaName;
			int start = bulletin.IndexOf(tacticalCallsignData.StartString);

			bulletin = bulletin.Substring(start);

			start = bulletin.IndexOf("#-----");
			string agencyData = bulletin.Substring(start);
			start = agencyData.IndexOf("\n");
			agencyData = agencyData.Substring(start + 1);
			int end = agencyData.IndexOf('#');
			string sccoAgencies = agencyData.Substring(0, end - 1);

			var lines = sccoAgencies.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
			TacticalCall[] tacticalCalls = new TacticalCall[lines.Length];
            for (int i = 0; i < lines.Length; i++)
            {
                var callsign = lines[i].Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);

                TacticalCall tacticalCall = new TacticalCall
                {
                    TacticalCallsign = callsign[0],
                    SecondaryBBS = "",
                    AgencyName = callsign[1]
                };
				if (callsign.Length > 2)
				{
					tacticalCall.Prefix = callsign[2];
				}
				else
				{
					tacticalCall.Prefix = callsign[0].Substring(3, 3);
				}
				if (callsign.Length > 3)
				{
					tacticalCall.PrimaryBBS = callsign[3];
				}
				else
				{
					tacticalCall.PrimaryBBS = primaryBBS;
				}
				if (callsign.Length > 4)
				{
					tacticalCall.SecondaryBBS = callsign[4];
				}
				else
				{
					tacticalCall.SecondaryBBS = secondaryBBS;
				}
				tacticalCall.Selected = false;

				tacticalCalls[i] = tacticalCall;
			}
			tacticalCalls[0].Selected = true;
            TacticalCallsigns tacticalCallsigns = new TacticalCallsigns()
            {
                BulletinCreationTime = tacticalCallsBulletin.JNOSDate,
                Area = agency,
                TacticalCallsignsArray = tacticalCalls
            };
            return tacticalCallsigns;
		}

        private static (string primaryBBS, string secondaryBBS) FindCountyPrimSecBBS(string callsign)
        {
            TacticalCallsignData tacticalCallsignData = App._tacticalCallsignDataDictionary["CountyTacticalCallsigns.xml"];

            string primBBS = "", secBBS = "";
            foreach (var callsignData in tacticalCallsignData.TacticalCallsigns.TacticalCallsignsArray)
            {
                if (callsignData.TacticalCallsign == callsign)
                {
                    primBBS = callsignData.PrimaryBBS;
                    secBBS = callsignData.SecondaryBBS;
                    break;
                }
            }
            return (primBBS, secBBS);
        }
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
	//[System.SerializableAttribute()]
	//[System.Diagnostics.DebuggerStepThroughAttribute()]
	//[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	public partial class TacticalCall
	{

		private string tacticalCallsignField;

		private string agencyNameField;

		private string prefixField;

		private string primaryBBSField;

		private bool primaryActiveField;

		private string secondaryBBSField = "";

		private bool secondaryActiveField;

		private bool selectedField;

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string TacticalCallsign
		{
			get
			{
				return this.tacticalCallsignField;
			}
			set
			{
				this.tacticalCallsignField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string AgencyName
		{
			get
			{
				return this.agencyNameField;
			}
			set
			{
				this.agencyNameField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string Prefix
		{
			get
			{
				return this.prefixField;
			}
			set
			{
				this.prefixField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string PrimaryBBS
		{
			get
			{
				return this.primaryBBSField;
			}
			set
			{
				this.primaryBBSField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public bool PrimaryBBSActive
		{
			get
			{
				return this.primaryActiveField;
			}
			set
			{
				this.primaryActiveField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string SecondaryBBS
		{
			get
			{
				return this.secondaryBBSField;
			}
			set
			{
				this.secondaryBBSField = value;
			}
		}

		/// <remarks/>
		//[System.Xml.Serialization.XmlAttributeAttribute()]
		//public bool SecondaryBBSActive
		//{
		//	get
		//	{
		//		return this.secondaryActiveField;
		//	}
		//	set
		//	{
		//		this.secondaryActiveField = value;
		//	}
		//}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public bool Selected
		{
			get
			{
				return this.selectedField;
			}
			set
			{
				this.selectedField = value;
			}
		}
	}
}
