using System;
using System.Configuration;
using System.Collections.Generic;
using System.Text;
using com.claytablet.model;
using com.claytablet.provider;
using com.claytablet.model.enm;
using com.claytablet.model.Event;
using com.claytablet.model.Event.platform;
using com.claytablet.model.Event.provider;
using com.claytablet.service.Event;
using com.claytablet.util;
using com.claytablet.queue;
using com.claytablet.queue.service;
using com.claytablet.queue.service.sqs;
using com.claytablet.storage;
using com.claytablet.storage.service;
using com.claytablet.storage.service.s3;

namespace ClayTablet.CT3.Sample.Net
{
    public class ReceiverTest
    {
        static void Main(string[] args)
        {

            SourceAccountProvider sap;
            TargetAccountProvider tap;

            QueueSubscriberService queueSubscriberService; 
            QueuePublisherService queuePublisherService;
            StorageClientService storageClientService;
            MyProviderReceiver receiver;

            ConnectionContext context;
            ProviderSender sender;



            String sourceAccountFile;
            if (System.Configuration.ConfigurationManager.AppSettings["CTT2_SourceAccount"] == null)
            {
                Console.WriteLine("Can't location the Source Account XML file [in Configuration.AppSettings].\n\nPlease configure [CTT2_SourceAccount] in AppSetting to point a Account XML file and make sure system have read permission with the Account file.");
                return;
            }
            else
                sourceAccountFile = System.Configuration.ConfigurationManager.AppSettings["CTT2_SourceAccount"].ToString();

            String targetAccountFile;
            if (System.Configuration.ConfigurationManager.AppSettings["CTT2_TargetAccount"] == null)
            {
                Console.WriteLine("Can't location the Target Account XML file [in Configuration.AppSettings].\n\nPlease configure [CTT2_TargetAccount] in AppSetting to point a Account XML file and make sure system have read permission with the Account file.");
                return;
            }
            else
                targetAccountFile = System.Configuration.ConfigurationManager.AppSettings["CTT2_TargetAccount"].ToString();

            String connectionContextFolder;
            if (System.Configuration.ConfigurationManager.AppSettings["CTT2_ConnectionContext_Folder"] == null)
            {
                Console.WriteLine("Can't location the ConnectionContext Folder.\n\nPlease configure [CTT2_ConnectionContext_Folder] in AppSetting to point a folder and make sure system have Full permission with the folder.");
                return;
            }
            else
                connectionContextFolder = System.Configuration.ConfigurationManager.AppSettings["CTT2_ConnectionContext_Folder"].ToString();

            //Load ConnectionContext,  
            context = new ConnectionContext(false);
            context.setConnectionContextPath(connectionContextFolder);
            context.load();

            //Initial a source Account 
            sap = new SourceAccountProvider(sourceAccountFile);

            //Initial a target Account        
            tap = new TargetAccountProvider(targetAccountFile);

            Account sourceAccount = sap.get();

            storageClientService = new StorageClientServiceS3();
            storageClientService.setPublicKey(sourceAccount.getPublicKey());
            storageClientService.setPrivateKey(sourceAccount.getPrivateKey());
            storageClientService.setStorageBucket(sourceAccount.getStorageBucket()); 

            queuePublisherService = new QueuePublisherServiceSQS();
            queuePublisherService.setPublicKey(sourceAccount.getPublicKey());
            queuePublisherService.setPrivateKey(sourceAccount.getPrivateKey());
            queuePublisherService.setEndpoint(sourceAccount.getQueueEndpoint());

            queueSubscriberService = new QueueSubscriberServiceSQS();
            queueSubscriberService.setPublicKey(sourceAccount.getPublicKey());
            queueSubscriberService.setPrivateKey(sourceAccount.getPrivateKey());
            queueSubscriberService.setEndpoint(sourceAccount.getQueueEndpoint());


            //Initial a ProviderSender, receiver may need to send Event back to response.
            sender = new ProviderSender(context,sap,tap, queuePublisherService, storageClientService);
            receiver = new MyProviderReceiver(context, storageClientService, sender);

            Console.WriteLine(string.Format("Check SQS message for Item need translation. Start at {0}\t", System.DateTime.Now.ToString()) );
             

            com.claytablet.queue.model.Message message = queueSubscriberService.receiveMessage();
            
             int msg_Count = 0;
            while (message != null)
            {
                 
                    msg_Count++;
                    Console.WriteLine("Found an new message.");
                    Console.WriteLine(msg_Count.ToString() + ")Message Body:\n" + message.getBody() );

                    try
                    {
                        //deserializing, from xml to AbsEvent,  
                        IEvent curEvent = AbsEvent.fromXml(message.getBody());

                        //call your ProducerReceiver to handle the event
                        HandleResult curHandleResult = receiver.ReceiveEvent(curEvent);

                        if (curHandleResult.CanDeleteMessage)
                        {
                            //Event handled, delete from Queue
                            queueSubscriberService.deleteMessage(message);
                            Console.WriteLine("Message handled, so it can be deleted from queue." );
                        }

                        if (!curHandleResult.Success)
                        {
                            Console.WriteLine("Event handling error.\nError Message:" + curHandleResult.ErrorMessage );
                        }

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message );
                    }

                    message = queueSubscriberService.receiveMessage();
            }


            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
             

        }

        }
    } 
