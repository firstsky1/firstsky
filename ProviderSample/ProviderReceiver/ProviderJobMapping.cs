using System;
using System.Collections.Generic;
using System.Text; 
using Xstream.Core;

namespace ClayTablet.CT3.Sample.Net
{
    public class ProviderJobMapping
    {

        //This is a sample mapping file, you can modify to provide your own mapping (depending on the TMS)
        private String localFileGUID;
	    private String tmsProjectGUID; 
	    private String tmsDocumentGUID; 
	    private String tmsTranslationStatus;
    	
	    private String projectId;
	    private String assetId; 
	    private String assetTaskId; 
	    private String fileExt;
    	 
	    private String filename = null;   
	    private String sourceTmsLanguageCode = null;
	    private String sourceCttLanguageCode = null;	
	    private String targetTmsLanguageCode = null;
	    private String targetCttLanguageCode = null; 
	    private String customReference;	
	    private String serverUrl = null; 
	    private String loginName = null;
	    private String loginPassword = null;
        private String eventType;

	    /**
	     * Empty constructor.
	     * 
	     */
	    public ProviderJobMapping() {
	    }

        public String TmsTranslationStatus
        {
            get { return tmsTranslationStatus; }
            set { tmsTranslationStatus = value; }
        }

        public String EventType
        {
            get { return eventType; }
            set { eventType = value; }
        }

        public String TmsProjectGUID
        {
            get { return tmsProjectGUID; }
            set { tmsProjectGUID = value; }
        }

        public String LocalFileGUID
        {
            get { return localFileGUID; }
            set { localFileGUID = value; }
        }

        public String TmsDocumentGUID
        {
            get { return tmsDocumentGUID; }
            set { tmsDocumentGUID = value; }
        }

        public String AssetTaskId
        {
            get { return assetTaskId; }
            set { assetTaskId = value; }
        }


        public String AssetId
        {
            get { return assetId; }
            set { assetId = value; }
        }

        public String ProjectId
        {
            get { return projectId; }
            set { projectId = value; }
        }

        public String FileExt
        {
            get { return fileExt; }
            set { fileExt = value; }
        }


        public String FileName
        {
            get { return filename; }
            set { filename = value; }
        }

        public String SourceCttLanguageCode
        {
            get { return sourceCttLanguageCode; }
            set { sourceCttLanguageCode = value; }
        }

        public String TargetCttLanguageCode
        {
            get { return targetCttLanguageCode; }
            set { targetCttLanguageCode = value; }
        }

        public String SourceTmsLanguageCode
        {
            get { return sourceTmsLanguageCode; }
            set { sourceTmsLanguageCode = value; }
        }

        public String TargetTmsLanguageCode
        {
            get { return targetTmsLanguageCode; }
            set { targetTmsLanguageCode = value; }
        }

        public String CustomReference
        {
            get { return customReference; }
            set { customReference = value; }
        }

        public String ServerUrl
        {
            get { return serverUrl; }
            set { serverUrl = value; }
        }

        public String LoginName
        {
            get { return loginName; }
            set { loginName = value; }
        }

        public String LoginPassword
        {
            get { return loginPassword; }
            set { loginPassword = value; }
        }
	    
    	
	     
    	
	    public static ProviderJobMapping fromXml(String xml)  {

		    // deserialize the account
		    return (ProviderJobMapping) getXStream().FromXml(xml);
	    }
    	
	    public static String toXml(ProviderJobMapping jobMapping)  {
     
		    // serilize the object to xml and return it
		    return getXStream().ToXml(jobMapping);
	    }
    	
	    private static XStream getXStream() {

		    XStream xstream = new XStream(); 
		    xstream.Alias("mapping", typeof(ProviderJobMapping) );

		    return xstream;
	    } 
    }
}
