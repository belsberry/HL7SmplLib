using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

/***********************************
 * TODO Figure out a foolproof way to implement keep alive
 * TODO Implement some logic for handling errors other than printing to console.
 ***********************************/
namespace HL7SmplLib.Net
{    
    public class HL7Listener
    {
        

        /************************************
         * Delegates/Events
         ************************************/
        public delegate void HL7MessageReceivedHandler(HL7MessageReceivedEventArgs hl7Message);
        public event HL7MessageReceivedHandler HL7MessageReceived;

        public delegate void ConnectionAcceptedHandler(ConnectionAcceptedEventArgs args);
        public event ConnectionAcceptedHandler ConnectionAccepted;

        public delegate void ConnectionClosedHandler(ConnectionClosedEventArgs e);
        public event ConnectionClosedHandler ConnectionClosed;

        public delegate void AckSentHandler(HL7MessageSentEventArgs ack);
        public event AckSentHandler AckSent;
        
        /**********************************
         * Private Variables
         **********************************/
        private long messagesReceivedSinceStart = 0L;
        private int port;
        private IPAddress ip;
        private bool shouldStop = false;
        private TcpListener server = null;

        private int readBufferSize = 4096;
        private bool keepAlive = false;
        private int keepAliveTimeout = 600000;
        private IHL7ListenerCommands hl7CommandObj = null;

        /*********************************
         * Public Properties
         *********************************/
        public long MessagesReceived { get { return messagesReceivedSinceStart; } }
        public int ReadBufferSize { set { readBufferSize = value; } }
        public bool KeepAlive { get { return keepAlive; } set { keepAlive = value; } }
        /// <summary>
        /// Set the timeout for keep alive in milliseconds.  Default timeout is 600000ms (10 min)
        /// </summary>
        public int KeepAliveTimeout { get { return keepAliveTimeout; } set { keepAliveTimeout = value; } }
        public string ACK { get; set; }
        public IHL7ListenerCommands HL7Commands { get { return hl7CommandObj; } set { hl7CommandObj = value; } }

        /*******************************
         * Ack Properties
         *******************************/
        private string hl7Version = "2.3";
        public string HL7Version { get { return hl7Version; } set { hl7Version = value; } }

        private string applicationName = "Generic Listener";
        public string ApplicationName { get { return applicationName; } set { applicationName = value; } }

        private string facilityName = "Generic Facility";
        public string FacilityName { get { return facilityName = "Generic Facility"; } set { facilityName = value; } }
        

        /********************************************
         * Constructors
         ********************************************/
        public HL7Listener(int port) : this(null, port) { }
        public HL7Listener(string ip, int port)
        {            
            if (ip == null)
                this.ip = IPAddress.Any;
            else
                this.ip = IPAddress.Parse(ip);
            this.port = port;
        }
        


        protected virtual void OnHL7MessageReceived(HL7MessageReceivedEventArgs e)
        {
            
        }

        protected virtual void OnConnectionAccepted(ConnectionAcceptedEventArgs e)
        {
            
        }
        protected virtual void OnAckSent(HL7MessageSentEventArgs e)
        {
            
        }

        protected virtual void OnConnectionClosed(ConnectionClosedEventArgs e)
        {
            
        }

        public void Listen()
        {
            server = new TcpListener(ip, port);
            server.Start();
            shouldStop = false;

            while (!shouldStop)
            {
                try
                {
                    TcpClient client = server.AcceptTcpClient();
                    ConnectionAcceptedEventArgs connAcceptedArgs = new ConnectionAcceptedEventArgs();
                    
                    IPEndPoint clientEndpoint = (IPEndPoint)client.Client.RemoteEndPoint;
                    connAcceptedArgs.Address = clientEndpoint.Address.ToString();
                    connAcceptedArgs.Port = this.port;
                    OnConnectionAccepted(connAcceptedArgs);
                    if (ConnectionAccepted != null) ConnectionAccepted(connAcceptedArgs);

                    Thread thread;
                    if (keepAlive)
                        thread = new Thread(new ParameterizedThreadStart(ProcessTcpClientKeepAlive));
                    else
                        thread = new Thread(new ParameterizedThreadStart(ProcessTcpClient));
                    thread.Start(client);
                }
                catch (SocketException ex)
                {

                }
                catch (Exception ex)
                {

                }
            }
        }




        public void Stop()
        {
            if (server != null)
            {
                server.Server.Close();
                shouldStop = true;
            }
        }

        /// <summary>
        /// Override to add custom evaluation of the HL7 message.  If the message will not pass muster then put the reason in the errorMessage and return false.
        /// </summary>
        /// <param name="message">hl7 message</param>
        /// <param name="errorMessage">reason for validation failure</param>
        /// <returns>A member of the HL7AckCode enum</returns>
        
        protected virtual HL7AckCode ValidateHL7Message(string message, out string errorMessage)
        {
            errorMessage = "";
            return HL7AckCode.Accept;
        }

        private void ProcessTcpClient(object clientObj)
        {
            TcpClient client = clientObj as TcpClient;
            
            int bufferSize = readBufferSize;
            byte[] buffer = new byte[bufferSize];
            int read = bufferSize;
            string data = "";
            StringBuilder dataBuilder = new StringBuilder();
            bool hasMessage = true;
            HL7MessageReceivedEventArgs messageArgs = null;
            ConnectionClosedEventArgs connectionClosedArgs = new ConnectionClosedEventArgs() ;
            try
            {

                NetworkStream stream = client.GetStream();                    
                try
                {
                    while ((read = stream.Read(buffer, 0, bufferSize)) > 0)
                    {
                        dataBuilder.Append(Encoding.ASCII.GetString(buffer, 0, read));
                        if (!stream.DataAvailable && (dataBuilder[dataBuilder.Length - 1] == (char)HL7MLLP.FS || dataBuilder[dataBuilder.Length - 2] == (char)HL7MLLP.FS)) break;   
                    }

                    data = dataBuilder.ToString();
                    data.Trim();
                    ParseMessagesSendAcks(data, stream);
                    //while (hasMessage)
                    //{
                    //    string message = "";
                    //    data = ParseMessage(data, ref message);
                    //    hasMessage = GetDataHasMessage(data);
                    //    message = HL7MLLP.Trim(message);

                    //    if (HL7Message.isHL7(message))
                    //    {
                    //        messageArgs = new HL7MessageReceivedEventArgs();
                    //        messageArgs.Message = message;
                    //        messageArgs.HeaderLine = new HL7Message(message)["MSH"].Text;
                    //        OnHL7MessageReceived(messageArgs);
                    //        if (HL7MessageReceived != null) HL7MessageReceived(messageArgs);
                    //        if (hl7CommandObj != null)
                    //        {
                    //            HL7Ack ack = hl7CommandObj.ProcessMessage(message);                              
                    //            SendAck(ack, message, stream);
                    //        }
                    //        else
                    //        {
                    //            string errorMessage;
                    //            HL7AckCode hl7AckCode = ValidateHL7Message(message, out errorMessage);
                    //            SendAck(hl7AckCode, message, stream, errorMessage);
                    //            //TODO look at this section and determine how to handle the invalid ack.                               
                                
                    //        }
                            
                    //    }
                            
                    //}
                    connectionClosedArgs.Reason = "Closed connection normally";
                }
                catch (IOException)
                {
                    throw;
                }

                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                finally
                {
                    if (stream != null) stream.Close();
                    if (client != null) client.Close();
                }
            }
            catch (Exception ex)
            {
                connectionClosedArgs.Reason = String.Format("Closed connection due to exception: {0}", ex.Message);
            }
            OnConnectionClosed(connectionClosedArgs);
            if (ConnectionClosed != null) ConnectionClosed(connectionClosedArgs);
        }

        private void ProcessTcpClientKeepAlive(object clientObj)
        {
            /*********************************
             * TODO work on the idea of Keep Alive and see if there are more guidelines for this 
             * type of interaction.
             *********************************/
            TcpClient client = clientObj as TcpClient;

            int bufferSize = readBufferSize;
            byte[] buffer = new byte[bufferSize];
            int read = 0;
            string data = "";
            //StringBuilder dataBuilder = new StringBuilder();
            //int fsLocation = 0;
            //bool hasMessage = true;
            
            //HL7MessageReceivedEventArgs messageArgs = null;
            try
            {
                NetworkStream stream = client.GetStream();
                stream.ReadTimeout = keepAliveTimeout;
                while (true)
                {
                    read = stream.Read(buffer, 0, bufferSize);
                    data += Encoding.ASCII.GetString(buffer, 0, read);
                    //data = data.Trim('\0', (char)0x16);
                    if (data.Contains((char)HL7MLLP.FS))
                    {
                        data = ParseMessagesSendAcks(data, stream);
                    }
                }
            }
            catch (IOException)
            {
                Console.WriteLine("Connection closed due to inactivity");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            
        }

        private bool GetTcpClientConnected(TcpClient client)
        {
            //client.Client.Poll(0, SelectMode.SelectRead);
            byte[] buffer = new byte[1];
            client.Client.Receive(buffer, SocketFlags.Peek);
            return client.Client.Available == 0;
            //if (client.Client.Poll(0, SelectMode.SelectRead))
            //{
            //    byte[] buff = new byte[1];
            //    if (client.Client.Receive(buff, SocketFlags.Peek) == 0)
            //    {
            //        return false;
            //    }
            //    else
            //    {
            //        return false;
            //    }
            //}
            //else
            //{
            //    return false;
            //}
        }

        private bool GetDataHasMessage(string data)
        {
            return data.Contains((char)HL7MLLP.VT);
        }

        private void SendAck(HL7Ack ack, string message, NetworkStream stream)
        {
            string myAck = "";
            if (ack.AckCode != HL7AckCode.Custom)
            {
                myAck = GetAck(message, ack.AckCode, ack.ErrorAckMessage);
            }
            else if(ack.AckCode == HL7AckCode.Custom)
            {
                myAck = ack.CustomAckHL7Message;
            }
            string wrappedAck = HL7MLLP.WrapMessage(myAck);
            byte[] ackBytes = Encoding.ASCII.GetBytes(wrappedAck);
            stream.Write(ackBytes, 0, ackBytes.Length);
            HL7MessageSentEventArgs ackArgs = new HL7MessageSentEventArgs();
            ackArgs.Message = myAck;
            try
            {
                HL7Message ackMessage = new HL7Message(myAck);
                ackArgs.HeaderLine = ackMessage["MSH"] != null ? ackMessage["MSH"].Text : "";
            }
            catch (Exception) { }
            OnAckSent(ackArgs);
            if (AckSent != null) AckSent(ackArgs);
            //stream.Flush();
        }

        private void SendAck(HL7AckCode code, string message, NetworkStream stream, string errorMessage)
        {
            string ackTemp = GetAck(message, code, errorMessage);
            string ack = HL7MLLP.WrapMessage(ackTemp);
            byte[] ackBytes = Encoding.ASCII.GetBytes(ack);
            stream.Write(ackBytes, 0, ackBytes.Length);
            HL7MessageSentEventArgs ackArgs = new HL7MessageSentEventArgs();
            ackArgs.Message = ackTemp;
            HL7Message ackMessage = new HL7Message(ackTemp);
            ackArgs.HeaderLine = ackMessage["MSH"] != null ? ackMessage["MSH"].Text : "";
            OnAckSent(ackArgs);
            if (AckSent != null) AckSent(ackArgs);
            //stream.Flush();
        }

        protected virtual string GetAck(string myMessage, HL7AckCode code, string errorMessage)
        {

            string ackBase = "MSH|^~\\&|Listener|System|Hosp|Generic Listener|timestamp||ACK^A01|controlID|P|2.3|\r";
            ackBase += "MSA|ackCode|senderControlID|text|";

            string controlId = "";
            string sendingApp = "";
            string sendingFacility = "";
            string MSH = myMessage;
            try
            {

                do
                {
                    MSH = MSH.Trim();
                    MSH = MSH.Substring(0, MSH.IndexOf('\r'));
                } while (!MSH.StartsWith("MSH"));

                if (!String.IsNullOrEmpty(MSH))
                {
                    HL7Segment mshSeg = new HL7Segment(MSH);
                    sendingApp = mshSeg[3][1][1].Text;
                    sendingFacility = mshSeg[4][1][1].Text;
                    controlId = mshSeg[10][1][1].Text;
                }
            }
            catch (Exception)
            {
            }

            string ackCode = "";
            string ackMessage = "";
            switch (code)
            {
                case HL7AckCode.Accept:
                    ackCode = "AA"; 
                    ackMessage = "Message Successfully Received";
                    break;
                case HL7AckCode.Error:
                    ackCode = "AE";
                    ackMessage = errorMessage;
                    break;
                case HL7AckCode.Reject:
                    ackCode = "AR";
                    ackMessage = errorMessage;
                    break;
            }

            HL7Message hl7Message = new HL7Message(ackBase);
            DateTime now = DateTime.Now;
            hl7Message["MSH"][3][1][1].Text = this.applicationName;
            hl7Message["MSH"][4][1][1].Text = this.facilityName;
            hl7Message["MSH"][5][1][1].Text = sendingApp;
            hl7Message["MSH"][6][1][1].Text = sendingFacility; //Set receiving facility to the same value as sending facility
            hl7Message["MSH"][7][1][1].Text = now.ToString("yyyyMMddHHmmss");
            hl7Message["MSH"][10][1][1].Text = now.ToString("yyyyMMddHHmmssfff");
            hl7Message["MSH"][12][1][1].Text = HL7Version;
            hl7Message["MSA"][1][1][1].Text = ackCode;
            hl7Message["MSA"][2][1][1].Text = controlId;
            hl7Message["MSA"][3][1][1].Text = ackMessage;
            return hl7Message.Text;
        }

        private string ParseMessage(string data, ref string message)
        {
            int fsIndex = data.IndexOf((char)HL7MLLP.FS);
            string dataToReturn = "";
            if (fsIndex != -1) 
            {
                message = HL7MLLP.Trim(data.Substring(0, fsIndex));
                dataToReturn = HL7MLLP.Trim(data.Substring(fsIndex + 1).TrimStart());
            }
            else message = data;
            
            return dataToReturn;
        }

        private string ParseMessagesSendAcks(string data, NetworkStream stream)
        {
            bool hasMessage = true;
            string message = "";
            HL7MessageReceivedEventArgs messageArgs;
            while (hasMessage)
            {
                data = ParseMessage(data, ref message);
                hasMessage = GetDataHasMessage(data);
                message = HL7MLLP.Trim(message);
                if (HL7Message.IsHL7(message))
                {
                    messagesReceivedSinceStart++;
                    messageArgs = new HL7MessageReceivedEventArgs();
                    messageArgs.Message = message;
                    HL7Message hl7Message = new HL7Message(message);
                    messageArgs.HeaderLine = hl7Message["MSH"] != null ? hl7Message["MSH"].Text : "";
                    if (HL7MessageReceived != null) HL7MessageReceived(messageArgs);
                    OnHL7MessageReceived(messageArgs);
                    if (hl7CommandObj != null)
                    {
                        HL7Ack ack = hl7CommandObj.ProcessMessage(message);
                        SendAck(ack, message, stream);
                    }
                    else
                    {
                        string errorMessage;
                        HL7AckCode ackCode = ValidateHL7Message(message, out errorMessage);
                        SendAck(ackCode, message, stream, "");
                    }
                }                
            }
            return data;
        }
    }
}
