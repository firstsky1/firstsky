using System;
using System.IO;
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
using com.claytablet.queue.model;
using com.claytablet.queue.service;
using com.claytablet.queue.service.sqs;
using com.claytablet.storage;
using com.claytablet.storage.service;
using com.claytablet.storage.service.s3;

namespace ClayTablet.CT3.Sample.Net
{
    public class MyProviderReceiver 
    {
        // also connection config file
	    private  ConnectionContext context; 
	    private ProviderAssetTaskTranslationMap assetTaskTranslationMap;  

	    private StorageClientService storageClientService;    	
	    private com.claytablet.service.Event.ProviderSender sender;


        public MyProviderReceiver(ConnectionContext context,
                                  StorageClientService storageClientService,
                                  ProviderSender sender)
        {
            //Pass a ConnectionContext, you need it to retrieve the path where 
            // the translated files will be saved
            this.context = context;

            //pass a StorageClientService, you need it to download file for translation
            this.storageClientService = storageClientService;

            /*ProviderJobMapping is a helper class to help mapping the CT 2.0 Info to your TMS info
             *ProviderAssetTaskTranslationMap is a helper class to handle saving/deleting/searching
             *the ProviderJobMapping objects
             * 
             *You can save the CT 2.0 Info to your TMS system if it supports
             * then you don't need the ProviderJobMapping and ProviderAssetTaskTranslationMap classes
             */
            this.assetTaskTranslationMap = ProviderAssetTaskTranslationMap.Instance;
            
            //Pass a ProducerSender, so you can send Events and Files out if needed
            //Check Sample-Sender for how to send Events and Files.
            this.sender = sender; 
		    
	    }


        public HandleResult ReceiveEvent(IEvent cttEvent)
        {

            if (cttEvent is ApprovedAssetTask)
                return receiveEvent((ApprovedAssetTask)cttEvent);
            else if (cttEvent is CanceledAssetTask)
                return receiveEvent((CanceledAssetTask)cttEvent);
            else if (cttEvent is CanceledSupportAsset)
                return receiveEvent((CanceledSupportAsset)cttEvent);
            else if (cttEvent is ProcessingError)
                return receiveEvent((ProcessingError)cttEvent);
            else if (cttEvent is RejectedAssetTask)
                return receiveEvent((RejectedAssetTask)cttEvent);
            else if (cttEvent is StartAssetTask)
                return receiveEvent((StartAssetTask)cttEvent);
            else if (cttEvent is StartNeedAssetTaskUpdate)
                return receiveEvent((StartNeedAssetTaskUpdate)cttEvent);
            else if (cttEvent is StartSupportAsset)
                return receiveEvent((StartSupportAsset)cttEvent);
            else if (cttEvent is StartUpdateTMAsset)
                return receiveEvent((StartUpdateTMAsset)cttEvent);
            else if (cttEvent is StartNeedTranslationCorrectionAssetTask)
                return receiveEvent((StartNeedTranslationCorrectionAssetTask)cttEvent);
            else
            {
                HandleResult handleResult = new HandleResult();
                handleResult.Success = false;
                handleResult.CanDeleteMessage = true;
                handleResult.ErrorMessage = "Can't identify the Event Type.";

                return handleResult;
            }


        }

        private HandleResult receiveEvent(ApprovedAssetTask curEvent) 
        {
      
            HandleResult handleResult = new HandleResult();
            // TODO - provider integration code goes here.
            // Put your code here to handle ApprovedAssetTask event 

            //delete the assetTask file from storage
            storageClientService.deleteAssetTaskVersions(curEvent.getAssetTaskId());

            return handleResult;

	    }


        private HandleResult receiveEvent(CanceledAssetTask curEvent)
        {


            HandleResult handleResult = new HandleResult(); 
		    try {
    			
			    //The the info from JobMapping
    			
			    ProviderJobMapping mapJob = null;	 
    			 
			    //Find the providerFileId etc info from assetTaskMap
			    foreach (ProviderJobMapping mapping in assetTaskTranslationMap.ListJobs() ) {
    				   
				     if (mapping.AssetTaskId.Equals(curEvent.getAssetTaskId())) 
				     {
					     //Found  
					     mapJob = mapping;   					 
					     break; 
				     }
    				 
			    } 
    			
			    //call TMS to cancel the Job
			    if ( mapJob != null  )
			    {
    				
				    String tmsProjectId = mapJob.TmsProjectGUID;
				    String tmsDocumentId = mapJob.TmsDocumentGUID; 
    				
				    String tms_Url = mapJob.ServerUrl; ;
				    String tms_LoginName = mapJob.LoginName ;
				    String tms_LoginPassword = mapJob.LoginPassword; 
    				
				    //TODO, connect to TMS to cancel ....	

                    handleResult.CanDeleteMessage = true;
                    handleResult.Success = true;

    				 
			    }
		    } catch (Exception e) {

                handleResult.CanDeleteMessage = false;
                handleResult.Success = false;
                handleResult.ErrorMessage = "[CanceledAssetTask] Error.";
                handleResult.Message = e.Message;
		    }

            return handleResult;
	    }


        private HandleResult receiveEvent(CanceledSupportAsset curEvent)
        {

            HandleResult handleResult = new HandleResult();
            // TODO - provider integration code goes here.
            // Put your code here to handle CanceledSupportAsset event  
            
            return handleResult;

	    }


        private HandleResult receiveEvent(ProcessingError curEvent)
        {

            HandleResult handleResult = new HandleResult();
            // TODO - provider integration code goes here.
            // Put your code here to handle ProcessingError event 
            return handleResult;

	    }


        private HandleResult receiveEvent(RejectedAssetTask curEvent)
        {

            HandleResult handleResult = new HandleResult();
            // TODO - provider integration code goes here.
            // Put your code here to handle RejectedAssetTask event 

            //delete the file from storage
            storageClientService.deleteAssetTaskVersions(curEvent.getAssetTaskId());
            return handleResult;

	    }

        private HandleResult receiveEvent(StartNeedAssetTaskUpdate curEvent)
        {
             

            HandleResult handleResult = new HandleResult();
            //  call TMS to translate the AssetTask
            try
            {


                String tms_Url = null;
                String tms_LoginName = null;
                String tms_LoginPassword = null;
                //TODO,  get connection info,  maybe from AppSettinng    			 

                if (tms_Url != null && tms_LoginName != null && tms_LoginPassword != null)
                {

                    try
                    {

                        

                        String tms_SoureLngLCID = null;
                        String tms_TargetLngLCID = null;
                        //TODO, you maybe need to convert CT 2.0 Language Code to TMS language code.

                        //check if same project exists
                        ProviderJobMapping findJob = assetTaskTranslationMap.SearchForBackupJob(curEvent.getAssetTaskId());
                        if (findJob == null)
                        {
                            handleResult.Success = false;
                            handleResult.CanDeleteMessage = true;
                            handleResult.Message = "can't find related translation Project";
                            return handleResult;
                        }
                        else
                        {
                            findJob.EventType = "";
                            assetTaskTranslationMap.BackToLive(findJob); 


                            // send AcceptAssetTask event out to notify CT 2.0 platform 
                            AcceptNeedAssetTaskUpdate acceptEvent = new AcceptNeedAssetTaskUpdate();
                            acceptEvent.setAssetTaskId(curEvent.getAssetTaskId()); 
                            acceptEvent.setEventId(IdGenerator.createId());

                            //call ProviderSender to sent event
                            sender.sendEvent(acceptEvent);

                            //event handling is done, so Message can be removed from SQS queue.
                            handleResult.CanDeleteMessage = true;
                            handleResult.Success = true;
                        }


                    }
                    catch (Exception exp)  // should catch TMS exception here.
                    {
                        handleResult.CanDeleteMessage = false;
                        handleResult.Success = false;
                        handleResult.ErrorMessage = "[StartNeedAssetTaskUpdate] Error.";
                        handleResult.Message = exp.Message;

                    }
                }
                else
                {
                    handleResult.CanDeleteMessage = false;
                    handleResult.Success = false;
                    handleResult.ErrorMessage = "[StartNeedAssetTaskUpdate] Error.";
                    handleResult.Message = "Can't find TMS server config.";
                }

            }
            catch (Exception e)
            {

                handleResult.CanDeleteMessage = false;
                handleResult.Success = false;
                handleResult.ErrorMessage = "[StartNeedAssetTaskUpdate] Error.";
                handleResult.Message = e.Message;
            }

            return handleResult;
             
        }

        private HandleResult receiveEvent(StartAssetTask curEvent)
        {

    		HandleResult handleResult = new HandleResult();      		 
		    //  call TMS to translate the AssetTask
		    try {
                 
    			 
			    String tms_Url = null;
                String tms_LoginName = null; 
                String tms_LoginPassword = null ; 
                //TODO,  get connection info,  maybe from AppSettinng    			 
    			
			    if (tms_Url != null && tms_LoginName != null && tms_LoginPassword != null  )
			    { 
    		 
				    try 
				    {

                        // Download the latest asset task revision file  
					    String downloadFilePath = storageClientService
							    .downloadLatestAssetTaskVersion(curEvent.getAssetTaskId(), context
									    .getTargetDirectory());
                         
    					
                        //delete file from staorage
                        storageClientService.deleteAssetTaskVersions(curEvent.getAssetTaskId());

					    String tms_SoureLngLCID = null;
					    String tms_TargetLngLCID = null;
					    //TODO, you maybe need to convert CT 2.0 Language Code to TMS language code.
    					
					    //check if same project exists
					    String tmsProjectGUID = assetTaskTranslationMap.SearchSameTmsProjectGUID(curEvent.getProjectId());
					    if (tmsProjectGUID == null)
					    {
					       // need to create a new TMS project
					       // TODO, create a TMS project 
    					   			
					    }

                        Console.WriteLine("tmsProjectGUID: " + tmsProjectGUID); 
    					
					    String tmpdocumentGUID = null;
					    //TODO,  Add a project document to TMS, put => tmpdocumentGUID
    					 
    				     
					    // Save info to ProviderJobMapping  
					    ProviderJobMapping jobMapping = new ProviderJobMapping();
    		
					    jobMapping.AssetTaskId = curEvent.getAssetTaskId();
					    jobMapping.AssetId = curEvent.getAssetId();
					    jobMapping.ProjectId = curEvent.getProjectId() ;
    					 
					    jobMapping.TmsDocumentGUID= tmpdocumentGUID ;
					    jobMapping.LocalFileGUID= IdGenerator.createId() ;
					    jobMapping.TmsProjectGUID= tmsProjectGUID;
    					
					    jobMapping.FileExt = curEvent.getFileExt() ;
                        jobMapping.FileName= System.IO.Path.GetFileName( downloadFilePath); 
    					
					    jobMapping.LoginName= tms_LoginPassword ;
					    jobMapping.LoginPassword=  tms_LoginName ;
					    jobMapping.ServerUrl=  tms_Url ;
    					 
    					
					    jobMapping.SourceCttLanguageCode=  curEvent.getSourceLanguageCode() ;
					    jobMapping.SourceTmsLanguageCode=  tms_SoureLngLCID ;
					    jobMapping.TargetCttLanguageCode= curEvent.getTargetLanguageCode();
					    jobMapping.TargetTmsLanguageCode=  tms_TargetLngLCID ;

                        jobMapping.EventType = "StartAssetTask";
    				  
                        //Call helper class to handle saving info
					    assetTaskTranslationMap.Add(jobMapping); 
    					
					    // send AcceptAssetTask event out to notify CT 2.0 platform 
					    AcceptAssetTask acceptEvent = new AcceptAssetTask(); 					
					    acceptEvent.setAssetTaskId(curEvent.getAssetTaskId() );
					    acceptEvent.setAssetTaskNativeId( tmpdocumentGUID ); 
					    acceptEvent.setEventId( IdGenerator.createId() );

                        //call ProviderSender to sent event
					    sender.sendEvent( acceptEvent );

                        //event handling is done, so Message can be removed from SQS queue.
                        handleResult.CanDeleteMessage = true;
                        handleResult.Success = true;
    					 
    					
				    }
				    catch (Exception exp)  // should catch TMS exception here.
				    {
                        handleResult.CanDeleteMessage = false;
                        handleResult.Success = false;
                        handleResult.ErrorMessage = "[StartAssetTask] Error.";
                        handleResult.Message = exp.Message; 
    					
				    }
			    }
			    else
			    {
				    handleResult.CanDeleteMessage = false;
                    handleResult.Success = false;
                    handleResult.ErrorMessage = "[StartAssetTask] Error.";
                    handleResult.Message = "Can't find server config in ConnectionContext.xml file."; 
			    }
    			
		    } catch (Exception e) {
                  
			          handleResult.CanDeleteMessage = false;
                      handleResult.Success = false;
                      handleResult.ErrorMessage = "[StartAssetTask] Error.";
                      handleResult.Message = e.Message; 
		    }

            return handleResult;
	    }

        private HandleResult receiveEvent(StartSupportAsset curEvent)
        {

            HandleResult handleResult = new HandleResult(); 

            //Get support asset filoe from storage
            String downloadFilePath = storageClientService
                              .downloadSupportAsset(curEvent.getAssetId(), curEvent.getFileExt(), context
                                      .getTargetDirectory());

            // TODO - provider integration code goes here.
            // Put your code here to handle StartSupportAsset event 

            //delete the file from storage
            storageClientService.deleteSupportAsset(curEvent.getAssetId(), curEvent.getFileExt());

            return handleResult;

	    }

        private HandleResult receiveEvent(StartUpdateTMAsset curEvent)
        {

            HandleResult handleResult = new HandleResult();

            //Get support asset filoe from storage
            String downloadFilePath = storageClientService
                              .downloadUpdateTMAsset(curEvent.getUpdateTMAssetId(), curEvent.getFileExt(), context
                                      .getTargetDirectory());

            // TODO - provider integration code goes here.
            // Put your code here to handle StartSupportAsset event 

            //delete the file from storage
            storageClientService.deleteUpdateTMAsset(curEvent.getUpdateTMAssetId(), curEvent.getFileExt());

            return handleResult;

        }

        private HandleResult receiveEvent(StartNeedTranslationCorrectionAssetTask curEvent)
        {

            HandleResult handleResult = new HandleResult();

            //Get support asset filoe from storage
            String downloadFilePath = storageClientService
                              .downloadLatestAssetTaskVersion(curEvent.getAssetTaskId(), context
                                      .getTargetDirectory());

            // TODO - provider integration code goes here.
            // Put your code here to handle StartNeedTranslationCorrectionAssetTask event 

            //delete the file from storage
            storageClientService.deleteAssetTaskVersions(curEvent.getAssetTaskId());

            return handleResult;

        }


         
    }
}
