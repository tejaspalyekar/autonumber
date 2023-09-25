/*using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace autonumber
{
    public class CaseAutonumber : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            try
            {
                IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                    IOrganizationService service = factory.CreateOrganizationService(context.UserId);
                    ITracingService tracing = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

                    if (context.MessageName.ToLower() != "create" && context.Stage != 20)
                    {
                        return;
                    }

                    Entity targetEntity = context.InputParameters["Target"] as Entity;
                    Entity updateAutoNumberConfig = new Entity("hotel_hotel");
                    StringBuilder autoNumber = new StringBuilder();
                    string current, year, month;
                    DateTime today = DateTime.Now;
                    month = today.Month.ToString("00");
                    year = today.Year.ToString();

                    QueryExpression qeAutoNumberConfig = new QueryExpression { EntityName = "hotel_hotel", ColumnSet = new ColumnSet("hotel_city") };

                    EntityCollection ecAutoNumber = service.RetrieveMultiple(qeAutoNumberConfig);
                    if (ecAutoNumber.Entities.Count == 0)
                    {
                        return;
                    }

                    foreach (Entity entity in ecAutoNumber.Entities)
                    {
                        if (ecAutoNumber.Entities.Count > 0)
                        {
                            // suffix = entity.GetAttributeValue<string>("cai_suffixnumber");
                            current = entity.GetAttributeValue<string>("hotel_city");
                            
                            int tempCurrent = int.Parse(current);
                            
                            tempCurrent++;
                            current = tempCurrent.ToString();
                            updateAutoNumberConfig.Id = entity.Id;
                            updateAutoNumberConfig["hotel_city"] = current;
                            service.Update(updateAutoNumberConfig);
                            autoNumber.Append(current);
                            break;
                        }
                    }
                    targetEntity["ticketnumber"] = autoNumber.ToString();
                }
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("An error occured in Autonumber Plugin: " + ex.Message.ToString(), ex);
            }
        }
    }
}*/
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace autonumber
{
    public class CaseAutonumber : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            try
            {
                IPluginExecutionContext context = (IPluginExecutionContext)
             serviceProvider.GetService(typeof(IPluginExecutionContext));

                IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {

                    Entity entity = (Entity)context.InputParameters["Target"];

                    //get config table row
                    /*var fetch1 = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='soft_autocountertable'>
                                    <attribute name='soft_autocountertableid' />
                                    <attribute name='soft_currentnumber' />
                                    <attribute name='createdon' />
                                    <order attribute='soft_currentnumber' descending='false' />
                                    <filter type='and'>
                                      <condition attribute='soft_rule' operator='eq' value='AUTONUMBER' />
                                    </filter>
                                  </entity>
                                </fetch>";*/

                    var fetch = @"<fetch version ='1.0' mapping='logical' distinct='false'>
                               <entity name='hotel_autocountertable'>
                                 <attribute name='hotel_autocountertableid' />
                                 <attribute name='hotel_currentnumber' />
                                 <attribute name='createdon' />
                                 <order attribute='hotel_currentnumber' descending='false' />
                                 <filter type='and'>
                                   <condition attribute='hotel_rule' operator='eq' value='AUTONUMBER' />
                                 </filter>
                               </entity>
                             </fetch>";

                    EntityCollection ecAuto = service.RetrieveMultiple(new FetchExpression(fetch));
                    Entity entAuto = ecAuto[0];
                    var autoNumberRecordId = entAuto.Id;

                    //initiate a UPDATE LOCK on counter entity
                    Entity couterTable = new Entity("hotel_autocountertable");
                    couterTable.Attributes["hotel_note"] = "lock " + DateTime.Now;
                    couterTable.Id = autoNumberRecordId;
                    service.Update(couterTable);

                    Entity AutoPost = service.Retrieve("hotel_autocountertable", autoNumberRecordId, new ColumnSet(true));
                    var currentrecordcounternumber = AutoPost.GetAttributeValue<String>("hotel_currentnumber");


                    //initialize counter
                    var newCounterValue = Convert.ToInt32(currentrecordcounternumber) + 1;


                    //update the counter in revenue
                    Entity newudpate = new Entity();
                    newudpate.LogicalName = entity.LogicalName;
                    newudpate.Id = entity.Id;
                    newudpate["hotel_revenueid"] = newCounterValue.ToString();
                    service.Update(newudpate);

                    //update the config
                    Entity newudpateconfig = new Entity();
                    newudpateconfig.LogicalName = "hotel_autocountertable";
                    newudpateconfig.Id = autoNumberRecordId;
                    newudpateconfig["hotel_currentnumber"] = newCounterValue.ToString();
                    service.Update(newudpateconfig);

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}

