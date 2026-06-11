using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TankBattleOnline
{
    public class NetworkManager
    {
        private TcpListener listener;
        private Thread acceptThread;
        private readonly List<TcpClient> clients = new List<TcpClient>();
        private readonly List<StreamWriter> writers = new List<StreamWriter>();
        private readonly List<int> connectionIds = new List<int>();
        private readonly ConcurrentQueue<NetworkMessage> messages = new ConcurrentQueue<NetworkMessage>();
        private readonly Queue<string> outgoingMessages = new Queue<string>();
        private readonly object clientLock = new object();
        private readonly object sendLock = new object();
        private Thread sendThread;
        private string latestFrameMessage;
        private int sessionId = 0;
        private int nextConnectionId = 0;
        private volatile bool running = false;

        public bool IsRunning
        {
            get
            {
                return running;
            }
        }

        public int ClientCount
        {
            get
            {
                lock (clientLock)
                {
                    return clients.Count;
                }
            }
        }

        public void StartHost(int port)
        {
            Stop();
            int session = BeginSession();
            running = true;
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();

            acceptThread = new Thread(() => AcceptLoop(session));
            acceptThread.IsBackground = true;
            acceptThread.Start();
            StartSendLoop(session);
        }

        private void AcceptLoop(int session)
        {
            while (IsSessionActive(session))
            {
                try
                {
                    TcpClient tcpClient = listener.AcceptTcpClient();
                    AddClient(tcpClient, session);
                }
                catch
                {
                }
            }
        }

        public bool ConnectToHost(string ip, int port)
        {
            Stop();

            try
            {
                int session = BeginSession();
                running = true;
                TcpClient tcpClient = new TcpClient();
                tcpClient.Connect(ip, port);
                AddClient(tcpClient, session);
                StartSendLoop(session);
                return true;
            }
            catch
            {
                running = false;
                return false;
            }
        }

        private void AddClient(TcpClient tcpClient, int session)
        {
            if (!IsSessionActive(session))
            {
                tcpClient.Close();
                return;
            }

            tcpClient.NoDelay = true;
            tcpClient.SendTimeout = 250;
            StreamWriter writer = new StreamWriter(tcpClient.GetStream(), Encoding.UTF8);
            writer.AutoFlush = true;
            int connectionId = Interlocked.Increment(ref nextConnectionId);

            lock (clientLock)
            {
                clients.Add(tcpClient);
                writers.Add(writer);
                connectionIds.Add(connectionId);
            }

            Thread receiveThread = new Thread(() => ReceiveLoop(tcpClient, connectionId, session));
            receiveThread.IsBackground = true;
            receiveThread.Start();
        }

        private void ReceiveLoop(TcpClient tcpClient, int connectionId, int session)
        {
            try
            {
                StreamReader reader = new StreamReader(tcpClient.GetStream(), Encoding.UTF8);

                while (IsSessionActive(session) && tcpClient.Connected)
                {
                    string message = reader.ReadLine();

                    if (message == null)
                    {
                        break;
                    }

                    messages.Enqueue(new NetworkMessage(connectionId, message));
                }
            }
            catch
            {
            }

            bool shouldNotify = IsSessionActive(session);
            RemoveClient(tcpClient);

            if (shouldNotify)
            {
                messages.Enqueue(new NetworkMessage(connectionId, NetworkProtocol.DisconnectMessage));
            }
        }

        private void RemoveClient(TcpClient tcpClient)
        {
            lock (clientLock)
            {
                int index = clients.IndexOf(tcpClient);

                if (index >= 0)
                {
                    clients.RemoveAt(index);

                    if (index < connectionIds.Count)
                    {
                        connectionIds.RemoveAt(index);
                    }

                    if (index < writers.Count)
                    {
                        try
                        {
                            writers[index].Close();
                        }
                        catch
                        {
                        }

                        writers.RemoveAt(index);
                    }
                }
            }

            try
            {
                tcpClient.Close();
            }
            catch
            {
            }
        }

        public bool TryGetMessage(out NetworkMessage message)
        {
            return messages.TryDequeue(out message);
        }

        public bool TryGetMessage(out string message)
        {
            NetworkMessage envelope;

            if (TryGetMessage(out envelope))
            {
                message = envelope.Text;
                return true;
            }

            message = null;
            return false;
        }

        public void Stop()
        {
            running = false;
            Interlocked.Increment(ref sessionId);

            try
            {
                if (listener != null)
                {
                    listener.Stop();
                }
            }
            catch
            {
            }

            lock (clientLock)
            {
                foreach (StreamWriter writer in writers)
                {
                    try
                    {
                        writer.Close();
                    }
                    catch
                    {
                    }
                }

                foreach (TcpClient tcpClient in clients)
                {
                    try
                    {
                        tcpClient.Close();
                    }
                    catch
                    {
                    }
                }

                writers.Clear();
                clients.Clear();
                connectionIds.Clear();
            }

            listener = null;

            lock (sendLock)
            {
                outgoingMessages.Clear();
                latestFrameMessage = null;
                Monitor.PulseAll(sendLock);
            }

            NetworkMessage ignored;

            while (messages.TryDequeue(out ignored))
            {
            }
        }

        public void SendLine(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            lock (sendLock)
            {
                if (!running)
                {
                    return;
                }

                if (message.StartsWith(NetworkProtocol.FramePrefix))
                {
                    latestFrameMessage = message;
                }
                else
                {
                    if (latestFrameMessage != null)
                    {
                        outgoingMessages.Enqueue(latestFrameMessage);
                        latestFrameMessage = null;
                    }

                    outgoingMessages.Enqueue(message);
                }

                Monitor.Pulse(sendLock);
            }
        }

        private void StartSendLoop(int session)
        {
            sendThread = new Thread(() => SendLoop(session));
            sendThread.IsBackground = true;
            sendThread.Start();
        }

        private void SendLoop(int session)
        {
            while (true)
            {
                string message = null;

                lock (sendLock)
                {
                    while (IsSessionActive(session) && outgoingMessages.Count == 0 && latestFrameMessage == null)
                    {
                        Monitor.Wait(sendLock);
                    }

                    if (!IsSessionActive(session))
                    {
                        return;
                    }

                    if (outgoingMessages.Count > 0)
                    {
                        message = outgoingMessages.Dequeue();
                    }
                    else
                    {
                        message = latestFrameMessage;
                        latestFrameMessage = null;
                    }
                }

                BroadcastLine(message);
            }
        }

        private void BroadcastLine(string message)
        {
            lock (clientLock)
            {
                for (int i = writers.Count - 1; i >= 0; i--)
                {
                    try
                    {
                        writers[i].WriteLine(message);
                    }
                    catch
                    {
                        if (i < clients.Count)
                        {
                            RemoveClient(clients[i]);
                        }
                    }
                }
            }
        }

        public void SendJoin(int playerId, int tankColorArgb)
        {
            SendJoin(playerId, tankColorArgb, "");
        }

        public void SendJoin(int playerId, int tankColorArgb, string clientToken)
        {
            SendLine(NetworkProtocol.CreateJoinMessage(playerId, tankColorArgb, clientToken));
        }

        public void SendStart()
        {
            SendLine(NetworkProtocol.StartMessage);
        }

        public void SendRoomReturn()
        {
            SendLine(NetworkProtocol.RoomMessage);
        }

        public void SendInput(int playerId, PlayerInput input)
        {
            SendLine(NetworkProtocol.CreateInputMessage(playerId, input));
        }

        public void SendState(GameState state)
        {
            SendLine(NetworkProtocol.CreateFrameStateMessage(state));
        }

        public void SendFullState(GameState state)
        {
            SendLine(NetworkProtocol.CreateStateMessage(state));
        }

        public void SendRoomState(RoomState room)
        {
            SendLine(NetworkProtocol.CreateRoomStateMessage(room));
        }

        public void SendRoomNotice(string notice)
        {
            SendLine(NetworkProtocol.CreateRoomNoticeMessage(notice));
        }

        public void SendSound(string fileName)
        {
            SendLine(NetworkProtocol.CreateSoundMessage(fileName));
        }

        public void SendPing(string clientToken, int pingId, int sentTick)
        {
            SendLine(NetworkProtocol.CreatePingMessage(clientToken, pingId, sentTick));
        }

        public void SendPong(string clientToken, int pingId, int sentTick)
        {
            SendLine(NetworkProtocol.CreatePongMessage(clientToken, pingId, sentTick));
        }

        public void SendLatency(string clientToken, int latencyMs)
        {
            SendLine(NetworkProtocol.CreateLatencyMessage(clientToken, latencyMs));
        }

        public void SendJoinAck(string clientToken, bool accepted, int playerId, string reason)
        {
            SendLine(NetworkProtocol.CreateJoinAckMessage(clientToken, accepted, playerId, reason));
        }

        private int BeginSession()
        {
            return Interlocked.Increment(ref sessionId);
        }

        private bool IsSessionActive(int session)
        {
            return running && session == sessionId;
        }
    }
}
