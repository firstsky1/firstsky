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
    public class ProviderStatePollerTest
    {
        static void Main(string[] args)
        {
            SourceAccountProvider sap;
            TargetAccountProvider tap;

            QueueSubscriberService queueSubscriberService;
            QueuePublisherService queuePublisherService;
            StorageClientService storageClientService; 

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


            //Initial a ProviderSender, may need to send Event back.
            sender = new ProviderSender(context, sap, tap, queuePublisherService, storageClientService);

            ProviderAssetTaskTranslationMap assetTaskTranslationMap = ProviderAssetTaskTranslationMap.Instance;
            Console.WriteLine("Polling TMS for asset task state changes.");

            foreach (ProviderJobMapping mapping in assetTaskTranslationMap.ListJobs())
            { 
    			 
			    String sourceLanguageCode = mapping.SourceCttLanguageCode;
			    String targetLanguageCode = mapping.TargetCttLanguageCode;
    			
			    String tmsSourceLCID = mapping.SourceTmsLanguageCode;
			    String tmsTargetLCID = mapping.TargetTmsLanguageCode ;
    		 
			    String tmsProjectGUID = mapping.TmsProjectGUID;
			    String tmsDocumentGUID = mapping.TmsDocumentGUID;
			    String localFileGUID = mapping.LocalFileGUID;
    			 
			    String tms_Url = mapping.ServerUrl; 
			    String tms_LoginName = mapping.LoginName ;
			    String tms_LoginPassword = mapping.LoginPassword ;
    			 
			     if ( tmsDocumentGUID != null && tmsProjectGUID != null )
				    {
                        Console.WriteLine("Check document Status:" + tmsDocumentGUID);
					    try
					    {
						    //TODO, your code to check the translation status	 
						    Boolean  translationFinished = false;
    						
						    if ( translationFinished )
						    {
                                //if a file translation is done, send it back with submitAssetTask events
                                 String downloadFileFolder = context.getTargetDirectory();
                                 String downloadFilePath = null;
                                 //TODO, download translated file from TMS to local folder: downloadFileFolder    
                                 // the full file path goes to downloadFilePath

                                 // send event out, notify CT2 platform a file translation 
                                 // is completd.

								    SubmitAssetTask submitAssetTask = new SubmitAssetTask(); 
								    submitAssetTask.setEventId( IdGenerator.createId() );

                                    //set original CT2 AssetTaskId
								    submitAssetTask.setAssetTaskId(mapping.AssetTaskId );
								    submitAssetTask.setNativeState("Completed"); 
								    submitAssetTask.setFileExt(downloadFilePath.Substring(downloadFilePath
										    .LastIndexOf(".") + 1, downloadFilePath.Length ) ); 
								    sender.sendEvent(submitAssetTask, downloadFilePath );
                                    Console.WriteLine("Send out translated file: " + downloadFilePath);
    								
								    //job is down, remove this job from Mapping
								    assetTaskTranslationMap.Remove(mapping); 	
    								
								    // You may also add code to delete the job or delete project file from TMS if needed.
						    } 
						    else
						    {
                                 
							    //check the  translation status, if status changed,  send a UpdateAssetTaskState event
							    String lastStatus = mapping.TmsTranslationStatus;
							    String tmsCurrentStatus = null;
							    //TODO,  get the status from TMS, put => tmsCurrentStatus
							    if (tmsCurrentStatus != null && lastStatus != null )
								    if (! tmsCurrentStatus.Equals(lastStatus))
								    {
                                        //status changed
									    UpdateAssetTaskState updateAssetTaskState = new UpdateAssetTaskState();
									    updateAssetTaskState.setAssetTaskId(mapping.AssetTaskId);
									    updateAssetTaskState.setEventId(IdGenerator.createId());
									    updateAssetTaskState.setNativeState(tmsCurrentStatus);
									    /* set the percentage of translation
									     * Since CT2.0 can't map all the native state from every TMS system
									     * to a state in CT2.0,  basically Translation Percentage provides very
									     * useful information to Producer (CMS) side, even the native state from TMS 
									     * is not very user friendly.
									     * 
									     * To fire a UpdateAssetTaskState event, Provider have to setup the TranslationPercentage
									     * to avoid the default 0 percent to be send back to Producer side.
    									 								
									     event.setTranslationPercentage(50);
    									
									    */
                                        sender.sendEvent(updateAssetTaskState);									
								    }
    							
    							
						    }
    						
					    }
                        catch (Exception e) // should also catch tms exception here
					    {
						    //do something here.
					    }
				    }


			    }

                Console.WriteLine("Press any key to exit...");
                Console.ReadLine();
        }
    }
}
