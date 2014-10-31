using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace HL7SmplLib.Net
{
    
    /*******************************************
     *  Keep Alive logic now:
     *  Check the connection for open and last send time and open/create new connection accordingly.
     *******************************************/
    public class HL7Client
    {
        //Error Handler/Event
        public delegate void ErrorHandler(HL7ErrorEventArgs e);
        public event ErrorHandler ErrorInSend;

        //Response Received Handler/Event
        public delegate void AckReceivedHandler(HL7MessageReceivedEventArgs e);
        public event AckReceivedHandler AckReceived;
        
        
        /****************************************
         * Private variables
         ****************************************/
        private int keepAliveInterval = 600000;
        private DateTime lastSend = DateTime.Now;
        private TcpClient tcpClient;
        private bool listenForAck = true;
        private int readBufferSize = 4096;
        private string host;
        private int port;
        private long messagesSentSinceStart = 0L;

        /****************************************
         * Public properties
         ****************************************/
        
        public int KeepAliveInterval { get { return keepAliveInterval; } set { keepAliveInterval = value; } }
        public bool ListenForAck { get { return listenForAck; } set { listenForAck = value; } }
        public int ReadBufferSize { set { readBufferSize = value; } }
        public bool WrapMLLP { get; set; }
        public bool KeepAlive { get; set; }
        public long MessagesSent { get { return messagesSentSinceStart; } }
        
        /*********************************
         * Constructors
         *********************************/
        public HL7Client(string Host, int Port)
        {
            host = Host;
            port = Port;
            WrapMLLP = true;
        }


        /******************************
         * Public Methods
         ******************************/
        public bool SendFile(string filePath)
        {
            return Send(File.ReadAllText(filePath));
        }

        
        public bool Send(string message)
        {
            NetworkStream stream = null;
            bool success = false;
            messagesSentSinceStart++;
            try
            {
                tcpClient = OpenTcpClient();
                byte[] readBuffer = new byte[readBufferSize];
                stream = tcpClient.GetStream();
                if (WrapMLLP) message = (char)HL7MLLP.VT + message + (char)HL7MLLP.FS + (char)HL7MLLP.CR;
                byte[] bytesToSend = Encoding.ASCII.GetBytes(message);
                stream.Write(new byte[1], 0, 0);
                stream.Write(bytesToSend, 0, bytesToSend.Length);
                stream.Flush();
                int i;
                string response = "";
                if (listenForAck)
                {
                    while ((i = stream.Read(readBuffer, 0, readBuffer.Length)) == readBufferSize)
                    {
                        response += Encoding.ASCII.GetString(readBuffer, 0, i);
                    }
                    response += Encoding.ASCII.GetString(readBuffer, 0, i);
                    response = HL7MLLP.Trim(response);
                    HL7MessageReceivedEventArgs receivedArgs = new HL7MessageReceivedEventArgs();
                    receivedArgs.Message = response;
                    receivedArgs.SourceAddress = host;
                    receivedArgs.SourcePort = port;
                    if (AckReceived != null) AckReceived(receivedArgs);
                    OnAckReceived(receivedArgs);
                    success = GetAckStatus(response);
                }
                else
                {
                    success = true;
                }
            }
            catch (IOException ex)
            {
                HL7ErrorEventArgs errorArgs = new HL7ErrorEventArgs();
                errorArgs.ErrorMessage = "Error sending or opening the network stream. Check your Keep Alive settings.";
                errorArgs.HL7Message = message;
                if (ErrorInSend != null) ErrorInSend(errorArgs);
                OnErrorInSend(errorArgs);
                success = false;
            }
            catch (Exception ex)
            {
                HL7ErrorEventArgs errorArgs = new HL7ErrorEventArgs();
                errorArgs.ErrorMessage = ex.Message;
                errorArgs.HL7Message = message;
                if (ErrorInSend != null) ErrorInSend(errorArgs);
                OnErrorInSend(errorArgs);
                success = false;
            }
            finally
            {
                //if (stream != null) stream.Close();
                if (tcpClient != null && !KeepAlive) tcpClient.Close();
            }
            return success;
        }

        /********************************************
         * Protected Methods -- Overridable
         ********************************************/
        protected virtual void OnAckReceived(HL7MessageReceivedEventArgs receivedArgs)
        {

        }

        protected virtual void OnErrorInSend(HL7ErrorEventArgs errorArgs)
        {

        }

        protected virtual bool GetAckStatus(string ackMessage)
        {
            try
            {
                HL7Message ack = new HL7Message(ackMessage);
                string responseCode = ack["MSA"][1][1][1].Text;
                if (responseCode == "AA")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /*********************************************
         * Private Methods
         *********************************************/
        private TcpClient OpenTcpClient()
        {
            if (KeepAlive)
            {
                if (tcpClient != null && tcpClient.Connected && !ConnectionTimedOut())
                {
                    lastSend = DateTime.Now;
                    return tcpClient;
                }
                else
                {
                    tcpClient = new TcpClient();
                    tcpClient.Connect(host, port);
                    lastSend = DateTime.Now;
                    return tcpClient;
                }
            }
            else
            {
                tcpClient = new TcpClient();
                tcpClient.Connect(host, port);
                lastSend = DateTime.Now;
                return tcpClient;
            }
        }

        private bool ConnectionTimedOut()
        {
            DateTime now = DateTime.Now;
            int elapsedTime = (int)((now - lastSend).TotalMilliseconds);
            return elapsedTime > keepAliveInterval;
        }

        #region OLD CODE
        //public bool StopKeepAlive { get; set; }
        //private object bufferLock = new object();
        //private byte[] buffer = new byte[1];
        //private bool keepAliveActive = false;
        //private ManualResetEvent MRE = new ManualResetEvent(false);

        //private void SendKeepAlive(string message)
        //{
        //    StopKeepAlive = false;
        //    if (!keepAliveActive)
        //    {
        //        tcpClient = new TcpClient();
        //        tcpClient.Connect(host, port);
        //        Thread senderThread = new Thread(new ThreadStart(ProcessMessages));
        //        senderThread.Start();
        //        keepAliveActive = true;
        //    }
        //    lock (bufferLock) buffer = Encoding.ASCII.GetBytes(HL7MLLP.WrapMessage(message));
        //    MRE.Set();
        //}

        //private void ProcessMessages()
        //{
        //    while (!StopKeepAlive)
        //    {
        //        try
        //        {
        //            MRE.Reset();
        //            if (buffer.Length > 1)
        //            {
        //                lock (bufferLock)
        //                {
        //                    NetworkStream stream = tcpClient.GetStream();
        //                    stream.Write(buffer, 0, buffer.Length);

        //                    int bufferSize = 256;
        //                    byte[] readBuffer = new byte[bufferSize];
        //                    int i;
        //                    string response = "";
        //                    if (listenForAck)
        //                    {
        //                        while ((i = stream.Read(readBuffer, 0, readBuffer.Length)) == bufferSize)
        //                        {
        //                            response += Encoding.ASCII.GetString(readBuffer, 0, i);
        //                        }
        //                        response += Encoding.ASCII.GetString(readBuffer, 0, i);
        //                        if (ResponseReceived != null) ResponseReceived(response);
        //                    }

        //                    buffer = new byte[1];
        //                }
        //            }
        //            else
        //            {
        //                NetworkStream stream = tcpClient.GetStream();
        //                stream.Write(buffer, 0, 1);
        //            }
        //            MRE.WaitOne(keepAliveInterval);
        //        }
        //        catch (Exception)
        //        {
        //            StopKeepAlive = true;
        //            keepAliveActive = false;
        //        }
                
        //    }
        //}



       


        //private void SendOnce(string message)
        //{
        //    NetworkStream stream = null;
        //    try
        //    {
        //        tcpClient = new TcpClient();
        //        tcpClient.Connect(host, port);
        //        int bufferSize = 256;
        //        byte[] readBuffer = new byte[bufferSize];
        //        stream = tcpClient.GetStream();
        //        if (WrapMLLP) message = (char)HL7MLLP.VT + message + (char)HL7MLLP.FS + (char)HL7MLLP.CR;
        //        byte[] bytesToSend = Encoding.ASCII.GetBytes(message);
        //        stream.Write(bytesToSend, 0, bytesToSend.Length);
        //        stream.Flush();
        //        int i;
        //        string response = "";
        //        if (listenForAck)
        //        {

        //            while ((i = stream.Read(readBuffer, 0, readBuffer.Length)) == bufferSize)
        //            {
        //                response += Encoding.ASCII.GetString(readBuffer, 0, i);
        //            }
        //            response += Encoding.ASCII.GetString(readBuffer, 0, i);
        //            if (ResponseReceived != null) ResponseReceived(response);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        if (ErrorInSend != null) ErrorInSend(ex.Message);
        //    }
        //    finally
        //    {
        //        if (stream != null) stream.Close();
        //        if (tcpClient != null) tcpClient.Close();
        //    }
        //}


        ////There is an issue here sometimes when sending larger messages to the current hl7 listener tool
        ////These messages are broken up and under the light of hex pad show that there are wrapper characters attached.
        //public void SendWithSocket(string message)
        //{
        //    try
        //    {
        //        int bufferSize = 256;
        //        byte[] readBuffer = new byte[bufferSize];
        //        Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //        sock.Connect(IPAddress.Parse(this.host), this.port);

        //        message = (char)HL7MLLP.VT + message + (char)HL7MLLP.FS + (char)HL7MLLP.CR;
        //        byte[] bytesToSend = Encoding.ASCII.GetBytes(message);
        //        sock.Send(bytesToSend);

        //        int i;
        //        string response = "";
        //        while ((i = sock.Receive(readBuffer, 0, bufferSize, SocketFlags.None)) == bufferSize)
        //        {
        //            response += Encoding.ASCII.GetString(readBuffer, 0, i);
        //        }
        //        response += Encoding.ASCII.GetString(readBuffer, 0, i);
        //        ResponseReceived(response);
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorInSend(ex.Message);
        //    }
        //}
#endregion
    }
}
