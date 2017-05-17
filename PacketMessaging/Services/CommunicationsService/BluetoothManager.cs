using MetroLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace PacketMessaging.Services.CommunicationsService
{
	public class BluetoothManager : CommunicationsServiceBase
	{
		private static ILogger log = LogManagerFactory.DefaultLogManager.GetLogger<BluetoothManager>();

		ViewModels.SharedData sharedData = ViewModels.SharedData.SharedDataInstance;
		private static volatile BluetoothManager instance;
		private static readonly Object singletonCreationLock = new Object();

		private CancellationTokenSource _ReadCancellationTokenSource;
		private Object _ReadCancelLock = new Object();

		//private Boolean IsReadTaskPending;
		//private uint ReadBytesCounter = 0;
		DataReader _DataReaderObject = null;
		string _readBytesBuffer = "";

		// Track Write Operation
		private CancellationTokenSource _WriteCancellationTokenSource;
		private Object _WriteCancelLock = new Object();

		//private Boolean IsWriteTaskPending;
		//private uint WriteBytesCounter = 0;
		DataWriter _DataWriteObject = null;

		//bool WriteBytesAvailable = false;

		//SerialDevice _serialDevice = null;

		TNCDevice _tncDevice;
		private ILogger log1;

		private BluetoothManager() : base(log)
		{
			_tncDevice = sharedData.CurrentTNCDevice;

		}

		public static BluetoothManager GetInstance()
		{
			if (instance == null)
			{
				lock (singletonCreationLock)
				{
					if (instance == null)
					{
						instance = new BluetoothManager();
					}
				}
			}
			return instance;
		}

		private async Task<uint> Send(string msg)
		{
			//tbError.Text = string.Empty;

			try
			{
				var writer = new DataWriter(_socket.OutputStream);

				writer.WriteString(msg);

				// Launch an async task to 
				//complete the write operation
				var store = writer.StoreAsync().AsTask();

				return await store;
			}
			catch (Exception ex)
			{
				LogHelper(LogLevel.Error, $"Error writing to Bluetooth: {ex.Message}");

				return 0;
			}
		}

		private async void Listen()
		{
			ReadCancellationTokenSource = new CancellationTokenSource();
			if (_socket.InputStream != null)
			{
				dataReaderObject = new DataReader(_socket.InputStream);
				// keep reading the serial input
				while (true)
				{
					await ReadAsync(ReadCancellationTokenSource.Token);
				}
			}
		}

		DataReader dataReaderObject;
		private CancellationTokenSource ReadCancellationTokenSource;

		private async Task ReadAsync(CancellationToken cancellationToken)
		{
			uint ReadBufferLength = 1024;

			// If task cancellation was requested, comply
			cancellationToken.ThrowIfCancellationRequested();

			// Set InputStreamOptions to complete the asynchronous read operation when one or more bytes is available
			dataReaderObject.InputStreamOptions = InputStreamOptions.Partial;

			// Create a task object to wait for data on the serialPort.InputStream
			Task<UInt32> loadAsyncTask = dataReaderObject.LoadAsync(ReadBufferLength).AsTask(cancellationToken);

			// Launch the task and wait
			UInt32 bytesRead = await loadAsyncTask;
			if (bytesRead > 0)
			{
				string recvdtxt = dataReaderObject.ReadString(bytesRead);
			}
		}

		//private async void Send(string msg)
		//{
		//	Task<UInt32> storeAsyncTask;

		//	DataWriter dataWriteObject = new DataWriter(_socket.OutputStream);
		//	dataWriteObject.WriteString(msg);

		//	// Launch an async task to complete the write operation
		//	storeAsyncTask = dataWriteObject.StoreAsync().AsTask();

		//	UInt32 bytesWritten = await storeAsyncTask;
		//}
	}
}
