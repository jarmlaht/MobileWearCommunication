using Android.Content;
using Android.Gms.Common.Apis;
using Android.Gms.Wearable;
using Java.Interop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Communication
{
    public class Communicator : Java.Lang.Object, IMessageApiMessageListener, IDataApiDataListener
    {
        readonly GoogleApiClient client;
        const string path = "/communicator";

        public Communicator(Context context)
        {
            client = new GoogleApiClient.Builder(context)
                .AddApi(WearableClass.API)
                .Build();
        }

        public void Resume()
        {
            if (!client.IsConnected)
            {
                client.Connect();
                WearableClass.MessageApi.AddListener(client, this);
                WearableClass.DataApi.AddListener(client, this);
            }
        }

        public void Pause()
        {
            if (client != null && client.IsConnected)
            {
                client.Disconnect();
                WearableClass.MessageApi.RemoveListener(client, this);
                WearableClass.DataApi.RemoveListener(client, this);
            }
        }

        public void SendMessage(string message)
        {
            Task.Run(() => {
                foreach (var node in Nodes())
                {
                    var bytes = Encoding.Default.GetBytes(message);
                    var result = WearableClass.MessageApi.SendMessage(client, node.Id, path, bytes).Await();
                    var success = result.JavaCast<IMessageApiSendMessageResult>().Status.IsSuccess ? "Ok." : "Failed!";
                    Console.WriteLine(string.Format("Communicator: Sending message {0}... {1}", message, success));
                }
            });
        }

        public void SendData(DataMap dataMap)
        {
            Task.Run(() => {
                var request = PutDataMapRequest.Create(path);
                request.DataMap.PutAll(dataMap);
                var result = WearableClass.DataApi.PutDataItem(client, request.AsPutDataRequest()).Await();
                var success = result.JavaCast<IDataApiDataItemResult>().Status.IsSuccess ? "Ok." : "Failed!";
                Console.WriteLine(string.Format("Communicator: Sending data map {0}... {1}", dataMap, success));
            });
        }

        public void OnMessageReceived(IMessageEvent messageEvent)
        {
            var message = Encoding.Default.GetString(messageEvent.GetData());
            Console.WriteLine(string.Format("Communicator: Message received \"{0}\"", message));
            MessageReceived(message);
        }

        public void OnDataChanged(DataEventBuffer p0)
        {
            Console.WriteLine(string.Format("Communicator: Data changed ({0} data events)", p0.Count));
            for (var i = 0; i < p0.Count; i++)
            {
                var dataEvent = p0.Get(i).JavaCast<IDataEvent>();
                if (dataEvent.Type == DataEvent.TypeChanged && dataEvent.DataItem.Uri.Path == path)
                    DataReceived(DataMapItem.FromDataItem(dataEvent.DataItem).DataMap);
            }
        }

        public event Action<string> MessageReceived = delegate { };

        public event Action<DataMap> DataReceived = delegate { };

        IList<INode> Nodes()
        {
            var result = WearableClass.NodeApi.GetConnectedNodes(client).Await();
            return result.JavaCast<INodeApiGetConnectedNodesResult>().Nodes;
        }
    }
}
