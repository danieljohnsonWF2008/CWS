using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using WF.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Configuration;

namespace WF.Common.EventLogging
{
    [Serializable]
    public class DatabasePublisher:EventPublisher
    {
        const string SP_SAVE_EVENT_LOG_DETAILS = "cwsaa.event_log";
        
        public DatabasePublisher(ConfigErrorHandler objConfig):base(objConfig)
        {   
            this.Initialize();
        }

        private void Initialize()
        {
            try
            {
                //SetPublisherEventTypes("DatabasePublisherLogEventType");

                //if (base.Configuration.AppSettings.ContainsKey("DatabasePublisherConnectionStringName"))
                //    _DBConnectionStringName = base.Configuration.AppSettings["DatabasePublisherConnectionStringName"];
            }
            catch
            {
                 throw;
            }
        }

        public override bool PublishEvent(EventDetails objEventDetails)
        {
            int result = -1;
            try
            {
                //#region Update Database

                //Database oDB = DatabaseFactory.CreateDatabase(_DBConnectionStringName);

                //DbCommand oCommand = oDB.GetStoredProcCommand(SP_SAVE_EVENT_LOG_DETAILS); 
                //oDB.ExecuteNonQuery(oCommand);

                //int? iReturnValue =-1;

                //result = oDB.ExecuteNonQuery(
                //    SP_SAVE_EVENT_LOG_DETAILS,
                //    CommandType.StoredProcedure,
                //    new Object[]
                //    {
                //        //objEventDetails.EventTime,//?// Event Time
                //        //ConvertToObject(objEventDetails.ServerName), //?// Server Name
                //        ConvertToObject(objEventDetails.EventMessage), //"s_event_message"
                //        ConvertToObject(objEventDetails.EventDetail), //"s_event_detail"
                //        ConvertToObject(objEventDetails.EventTrace), //"s_event_trace"
                //        ConvertToObject(Convert.ToInt32(objEventDetails.EventCategory)), //"i_event_type"
                //        ConvertToObject(objEventDetails.ClassName), //"s_class_name"
                //        ConvertToObject(objEventDetails.MethodName), //"s_method_name"
                //        iReturnValue
                //    });
                //if (iReturnValue != null)
                //    result = Convert.ToInt32(iReturnValue.Value);    

                //#endregion
            }
            catch 
            {
                throw;
            }
            finally
            {
                
            }            
            return (result>0?false:true);
        }

        /// <summary>
        /// Helper function used for Converting generics into objects that are then sent to Stored Proc update methods
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private object ConvertToObject(int? value)
        {
            if (value.HasValue)
            { return (object)value.Value; }
            else
            { return (object)DBNull.Value; }
        }

        /// <summary>
        /// Helper function used for Converting generics into objects that are then sent to Stored Proc update methods
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private object ConvertToObject(string value)
        {
            if (value != null)
            { return (object)value; }
            else
            { return (object)DBNull.Value; }
        }
    }
}
