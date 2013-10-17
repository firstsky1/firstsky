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
    public class ProviderAssetTaskTranslationMap
    {
        // sample Mapping handling,  used local file to save info
        private String DEFAULT_DATA_DIR = null;
	    private String DEFAULT_DATA_DIR_JOB = null;
        private String DEFAULT_DATA_DIR_JOB_BACKUP = null;
	 
	    public ProviderAssetTaskTranslationMap() {
            
             DEFAULT_DATA_DIR = System.Configuration.ConfigurationManager.AppSettings["CTT2_ConnectionContext_Folder"];
             if (DEFAULT_DATA_DIR.EndsWith("\\"))
             {
                 DEFAULT_DATA_DIR_JOB = DEFAULT_DATA_DIR + "jobs\\";
                 DEFAULT_DATA_DIR_JOB_BACKUP = DEFAULT_DATA_DIR + "jobs_Backup\\";
             }
             else
             {
                 DEFAULT_DATA_DIR_JOB = DEFAULT_DATA_DIR + "\\jobs\\";
                 DEFAULT_DATA_DIR_JOB_BACKUP = DEFAULT_DATA_DIR + "\\jobs_Backup\\";
             }
             
			 if (!System.IO.Directory.Exists(DEFAULT_DATA_DIR_JOB ) )
                        System.IO.Directory.CreateDirectory(DEFAULT_DATA_DIR_JOB);

             if (!System.IO.Directory.Exists(DEFAULT_DATA_DIR_JOB_BACKUP))
                 System.IO.Directory.CreateDirectory(DEFAULT_DATA_DIR_JOB_BACKUP);
			
			 
	    }

        public static ProviderAssetTaskTranslationMap Instance
        {
            get
            {
                return Nested.Instance;
            }
        }

        private class Nested
        {
            private static ProviderAssetTaskTranslationMap instance;

            static Nested()
            {
                instance = new ProviderAssetTaskTranslationMap();
            }

            public static ProviderAssetTaskTranslationMap Instance
            {
                get
                {
                    return instance;
                }
            }

        }
            	 
	    public List<ProviderJobMapping> ListJobs() 
        {

		        // Retrieve the xml data directory where the mappings are stored  
                List<ProviderJobMapping> jobs  = new List<ProviderJobMapping>();	 

                String[] exts = { "xml" };
		        //Console.WriteLine("Load and deserialize all of the JobMapping from " + DEFAULT_DATA_DIR_JOB);
		        List<String> jobFiles = FileUtil.ListFiles(DEFAULT_DATA_DIR_JOB, exts);
                foreach (String file in jobFiles)
                {
			        ProviderJobMapping job = ProviderJobMapping.fromXml(FileUtil.ReadStringFromFile(file));
			        jobs.Add(job);
		        }
    		 
		    return jobs;
    		 
	    }

        public List<ProviderJobMapping> ListBackupJobs()
        {

            // Retrieve the xml data directory where the mappings are stored  
            List<ProviderJobMapping> jobs = new List<ProviderJobMapping>();

            String[] exts = { "xml" };
            //Console.WriteLine("Load and deserialize all of the JobMapping from " + DEFAULT_DATA_DIR_JOB);
            List<String> jobFiles = FileUtil.ListFiles(DEFAULT_DATA_DIR_JOB_BACKUP, exts);
            foreach (String file in jobFiles)
            {
                ProviderJobMapping job = ProviderJobMapping.fromXml(FileUtil.ReadStringFromFile(file));
                jobs.Add(job);
            }

            return jobs;

        }
    	 
	    public String SearchSameTmsProjectGUID(String cttProjectId) 
            //Search if an CTT project had been handled already 
        {
            
		    // Retrieve the xml data directory where the mappings are stored 
            String returnTmsProjectGUID = null;
		    List<ProviderJobMapping> jobs  = ListJobs();
    		  
		    foreach (ProviderJobMapping job in jobs) {			 
			    if ( cttProjectId.Equals(job.ProjectId) )
                {
				    returnTmsProjectGUID =  job.TmsProjectGUID;
                    break;
                }
		    }

            if (returnTmsProjectGUID == null)
            {

                List<ProviderJobMapping> backupJobs = ListBackupJobs();

                foreach (ProviderJobMapping job in backupJobs)
                {
                    if (cttProjectId.Equals(job.ProjectId))
                    {
                        returnTmsProjectGUID = job.TmsProjectGUID;
                        break;
                    }
                }

            }
    		 
		    return returnTmsProjectGUID;
    		 
	    }

        public ProviderJobMapping SearchForBackupJob(String cttAssetTaskId)
        //Search if an CTT project had been handled already 
        {


            ProviderJobMapping findJob = null;
            
            List<ProviderJobMapping> backupJobs = ListBackupJobs();

            foreach (ProviderJobMapping job in backupJobs)
                {
                    if (cttAssetTaskId.Equals(job.AssetTaskId))
                    {
                        findJob = job;
                        break;
                    }
                }
             
            return findJob;

        }
    	 
	    public void Add(ProviderJobMapping job)  {

		    // save to the xml data directory where the JobMapping should stored
            		 
            String guid = IdGenerator.createId();
            job.LocalFileGUID = guid;
            String jobFile = DEFAULT_DATA_DIR_JOB + guid + ".xml";

            FileUtil.WriteStringToFile(jobFile , ProviderJobMapping.toXml(job)); 
            
	    }

    	
	    public void Remove(ProviderJobMapping job)  {

		    // remove a job mapping file	    	     
            String guid = job.LocalFileGUID;
            String jobFile = DEFAULT_DATA_DIR_JOB + guid + ".xml";
            String jobFile_Backup = DEFAULT_DATA_DIR_JOB_BACKUP + guid + ".xml";
            if (System.IO.File.Exists(jobFile))
                System.IO.File.Move(jobFile, jobFile_Backup);
            else if (System.IO.File.Exists(jobFile_Backup))
                System.IO.File.Delete(jobFile_Backup);
	      }

        public void BackToLive(ProviderJobMapping job)
        {

            // remove a job mapping file	    	     
            String guid = job.LocalFileGUID;
            String jobFile = DEFAULT_DATA_DIR_JOB + guid + ".xml";
            String jobFile_Backup = DEFAULT_DATA_DIR_JOB_BACKUP + guid + ".xml";
            if (System.IO.File.Exists(jobFile_Backup))
                System.IO.File.Move(jobFile_Backup, jobFile); 
        }

       
    }
}
