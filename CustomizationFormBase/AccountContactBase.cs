using System;
using System.Activities;
using System.ServiceModel;
using App.Crm.Plugin;
using App.Custom;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;

namespace CustomizationFormBase
{
    namespace ContactForm
    {
        namespace CreateAndUpdate
        {
            public class CreateAndUpdateForm : Base
            {

                public CreateAndUpdateForm() : base(typeof(CreateAndUpdateForm))
                {                   
                    RegisteredEvents.Add(new Tuple<SdkMessageProcessingStepStage, MessageName, string, Action<LocalPluginContext>>(SdkMessageProcessingStepStage.Prevalidation, MessageName.Create, Contact.EntityName, ValidateBithday));
                    RegisteredEvents.Add(new Tuple<SdkMessageProcessingStepStage, MessageName, string, Action<LocalPluginContext>>(SdkMessageProcessingStepStage.Prevalidation, MessageName.Update, Contact.EntityName, ValidateBithday));
                    RegisteredEvents.Add(new Tuple<SdkMessageProcessingStepStage, MessageName, string, Action<LocalPluginContext>>(SdkMessageProcessingStepStage.Preoperation, MessageName.Create, Contact.EntityName, CalculateAge));
                    RegisteredEvents.Add(new Tuple<SdkMessageProcessingStepStage, MessageName, string, Action<LocalPluginContext>>(SdkMessageProcessingStepStage.Preoperation, MessageName.Update, Contact.EntityName, CalculateAge));
                    RegisteredEvents.Add(new Tuple<SdkMessageProcessingStepStage, MessageName, string, Action<LocalPluginContext>>(SdkMessageProcessingStepStage.Postoperation, MessageName.Create, Contact.EntityName, OnPostCreateAccountAge));
                    RegisteredEvents.Add(new Tuple<SdkMessageProcessingStepStage, MessageName, string, Action<LocalPluginContext>>(SdkMessageProcessingStepStage.Postoperation, MessageName.Update, Contact.EntityName, OnPostUpdateAccountAge));
                }

                protected void ValidateBithday(LocalPluginContext localContext)
                {
                    
                    if (localContext == null) throw new ArgumentNullException(nameof(localContext));
                    var context = localContext.PluginExecutionContext;
                    
                    if (!IsDirty(context, Contact.Birthday)) return;

                    var birthday = GetInputValue<DateTime>(context, Contact.Birthday);
                    // DateTime birthday = DateTime.Parse(entity.Attributes[App.Custom.Contact.Birthday].ToString());
                    if (birthday > DateTime.Now) throw new InvalidPluginExecutionException("Bithday cannot larger than today !!");
                    
                }

                protected void CalculateAge(LocalPluginContext localContext)
                {
                    if (localContext == null) throw new ArgumentNullException(nameof(localContext));
                    var context = localContext.PluginExecutionContext;

                    if (!IsDirty(context, Contact.Birthday)) return;

                    var birthday = GetInputValue<DateTime>(context, Contact.Birthday);
                    var today = DateTime.Today;

                    // Calculate the age.
                    int age = today.Year - birthday.Year;

                    // Go back to the year in which the person was born in case of a leap year
                    if (birthday.Date > today.AddYears(-age)) age--;

                    // Obtain the target entity from the input parameters.  
                    var entity = GetTargetEntity(context);

                    entity.Attributes.Add(Contact.age, age);
                    
                    

                }

                protected void OnPostCreateAccountAge(LocalPluginContext localContext)
                {

                    if (localContext == null) throw new ArgumentNullException(nameof(localContext));
                    var context = localContext.PluginExecutionContext;
                    var service = localContext.OrganizationService;

                    if (!IsDirty(context, Contact.Birthday) && !IsDirty(context, Contact.CompanyName)) return;
                    // var accountlookup = (EntityReference)entity.Attributes[App.Custom.Contact.CompanyName]; 
                    var accountlookup = GetValue<EntityReference>(context, Contact.CompanyName);
                    if (accountlookup.LogicalName == Account.EntityName) UpdateTotalAge(accountlookup.Id, service);
                    
                }

                protected void OnPostUpdateAccountAge(LocalPluginContext localContext)
                {
                    if (localContext == null) throw new ArgumentNullException(nameof(localContext));
                    var context = localContext.PluginExecutionContext;
                    var service = localContext.OrganizationService;

                    // get topic field value before database update perform                   
                   var pretopic = GetPreValue<EntityReference>(context, Contact.CompanyName, "companyName");
                        
                    // get topic field value after database update performed  
                   var posttopic = GetPostValue<EntityReference>(context, Contact.CompanyName, "companyName");
                       
                    if (pretopic.Equals(posttopic) && posttopic.LogicalName == Account.EntityName)
                    {
                        UpdateTotalAge(posttopic.Id, service);
                    }
                    else if (IsPostAttributeFound(context, Contact.CompanyName, "companyName") && !IsPreAttributeFound(context, Contact.CompanyName, "companyName") && posttopic.LogicalName == Account.EntityName)
                    {
                        UpdateTotalAge(posttopic.Id, service);

                    }
                    else if (IsPreAttributeFound(context, Contact.CompanyName, "companyName") && !IsPostAttributeFound(context, Contact.CompanyName, "companyName") && pretopic.LogicalName == Account.EntityName)
                    {
                        UpdateTotalAge(pretopic.Id, service);
                    }
                    else if (IsPreAttributeFound(context, Contact.CompanyName, "companyName") && IsPostAttributeFound(context, Contact.CompanyName, "companyName"))
                    {
                        if (posttopic.LogicalName == Account.EntityName)
                            UpdateTotalAge(posttopic.Id, service);
                        if (pretopic.LogicalName == Account.EntityName)
                            UpdateTotalAge(pretopic.Id, service);
                    }


                }
               
                public void UpdateTotalAge(Guid accountid, IOrganizationService service)
                {
                    int totalAge = 0;
                    QueryExpression query = new QueryExpression(Contact.EntityName)
                    {
                        Distinct = false,
                        ColumnSet = new ColumnSet(Contact.age),
                        LinkEntities =
                    {
                        new LinkEntity(Contact.EntityName, Account.EntityName, Contact.CompanyName, Account.PrimaryKey, JoinOperator.Inner)
                        {
                            LinkCriteria =
                            {
                                Conditions =
                                {
                                    new ConditionExpression(Account.PrimaryKey, ConditionOperator.Equal, accountid)
                                }
                            }

                        }
                    },
                        Criteria =
                    {

                        Conditions =
                        {
                            new ConditionExpression(Contact.age, ConditionOperator.NotNull)

                        }
                    }

                    };

                    DataCollection<Entity> contactCollection = service.RetrieveMultiple(query).Entities;

                    foreach (Entity contact in contactCollection)
                    {
                        totalAge += int.Parse(contact.Attributes[Contact.age].ToString());

                    }

                    //Entity leadObj = service.Retrieve(entityName, accountid, new ColumnSet("app_totalage"));
                    var account = new Entity(Account.EntityName, accountid)
                    {
                        [Account.totalage] = totalAge
                    };

                    service.Update(account);
                }
            }
        }

        namespace Audit
        {
            public class AuditContact : Base
            {
                public AuditContact() : base(typeof(AuditContact))
                {
                    RegisteredEvents.Add(new Tuple<SdkMessageProcessingStepStage, MessageName, string, Action<LocalPluginContext>>(SdkMessageProcessingStepStage.Preoperation, MessageName.Create, Contact.EntityName, IncrementValueOnCreate));
                    RegisteredEvents.Add(new Tuple<SdkMessageProcessingStepStage, MessageName, string, Action<LocalPluginContext>>(SdkMessageProcessingStepStage.Postoperation, MessageName.Create, Contact.EntityName, AuditOnCreate));
                    RegisteredEvents.Add(new Tuple<SdkMessageProcessingStepStage, MessageName, string, Action<LocalPluginContext>>(SdkMessageProcessingStepStage.Postoperation, MessageName.Update, Contact.EntityName, AuditOnUpdate));

                }

                protected void IncrementValueOnCreate(LocalPluginContext localContext)
                {
                    if (localContext == null) throw new ArgumentNullException(nameof(localContext));
                    var context = localContext.PluginExecutionContext;
                   
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        // Obtain the target entity from the input parameters.  
                        Entity entity = (Entity)context.InputParameters["Target"];
                        entity.Attributes[Contact.incrementvalue] = 1;
                    }
                }

                protected void AuditOnCreate(LocalPluginContext localContext)
                {
                    if (localContext == null) throw new ArgumentNullException(nameof(localContext));
                    var context = localContext.PluginExecutionContext;
                    var service = localContext.OrganizationService;
                    
                    var newAudit = new Entity(audit.EntityName);
                    newAudit.Attributes.Add(audit._event, "Create Contact");
                    newAudit.Attributes.Add(audit.PrimaryName, "Create Contact - " + context.PrimaryEntityId.ToString());
                    newAudit.Attributes.Add(audit.auditfrom, new EntityReference(context.PrimaryEntityName, context.PrimaryEntityId));
                       
                    newAudit[audit.fieldname] = "";
                    newAudit[audit.before] = "";
                    newAudit[audit.after] = "";

                    if (IsPostAttributeFound(context, Contact.PrimaryName, "focusField"))
                    {
                        newAudit[audit.fieldname] = "fullname\n";
                        newAudit[audit.before] = "\n";
                        newAudit[audit.after] = GetPostValue<string>(context,Contact.PrimaryName,"focusField") + "\n";

                        if (IsPostAttributeFound(context, Contact.FirstName, "focusField"))
                        {
                            newAudit[audit.fieldname] += "firstname\n";
                            newAudit[audit.before] += "\n";
                            newAudit[audit.after] += GetPostValue<string>(context, Contact.FirstName, "focusField") + "\n";

                        }

                        if (IsPostAttributeFound(context, Contact.LastName, "focusField"))
                        {
                            newAudit[audit.fieldname] += "lastname\n";
                            newAudit[audit.before] += "\n";
                            newAudit[audit.after] += GetPostValue<string>(context, Contact.LastName, "focusField") + "\n";
                        }

                        newAudit[audit.fieldname] += "increment value\n";
                        newAudit[audit.before] += "\n";
                        newAudit[audit.after] += "1\n";
                    }

                    if (IsPostAttributeFound(context, Contact.Birthday, "focusField"))
                    {
                        newAudit[audit.fieldname] += "birthday\n";
                        newAudit[audit.before] += "\n";
                        newAudit[audit.after] += GetPostValue<DateTime>(context, Contact.Birthday, "focusField").ToString().Split(' ')[0] + "\n";

                        newAudit[audit.fieldname] += "age\n";
                        newAudit[audit.before] += "\n";
                        newAudit[audit.after] += getAge(GetPostValue<DateTime>(context, Contact.Birthday, "focusField")) + "\n";

                    }

                    if (IsPostAttributeFound(context, Contact.CompanyName, "focusField"))
                    {
                        newAudit[audit.fieldname] += "company name\n";
                        newAudit[audit.before] += "\n";
                        newAudit[audit.after] += (GetPostValue<EntityReference>(context, Contact.CompanyName, "focusField")).Name + "\n";
                    }

                    service.Create(newAudit);
                    
                }

                protected void AuditOnUpdate(LocalPluginContext localContext)
                {
                    if (localContext == null) throw new ArgumentNullException(nameof(localContext));
                    var context = localContext.PluginExecutionContext;
                    var service = localContext.OrganizationService;

                    Entity newAudit = new Entity(audit.EntityName);
                    newAudit.Attributes.Add(audit._event, "Update Contact");
                    newAudit.Attributes.Add(audit.PrimaryName, "Update Contact - " + context.PrimaryEntityId.ToString());
                    newAudit.Attributes.Add(audit.auditfrom, new EntityReference(context.PrimaryEntityName, context.PrimaryEntityId));

                        

                    newAudit[audit.fieldname] = "";
                    newAudit[audit.before] = "";
                    newAudit[audit.after] = "";

                    if (IsPreAttributeFound(context, Contact.PrimaryName, "NameImage") || IsPostAttributeFound(context, Contact.PrimaryName, "NameImage"))
                    {

                        if (!GetPreValue<string>(context, Contact.PrimaryName, "NameImage").Equals(GetPostValue<string>(context, Contact.PrimaryName, "NameImage")))
                        {
                            newAudit[audit.fieldname] = "fullname\n";
                            newAudit[audit.before] =   GetPreValue<string>(context,Contact.PrimaryName, "NameImage")+"\n";
                            newAudit[audit.after] = GetPostValue<string>(context, Contact.PrimaryName, "NameImage") + "\n";

                            if (IsPreAttributeFound(context, Contact.FirstName, "NameImage") || IsPostAttributeFound(context, Contact.FirstName, "NameImage"))
                            {

                                if (IsPreAttributeFound(context, Contact.FirstName, "NameImage") && IsPostAttributeFound(context, Contact.FirstName, "NameImage"))
                                {
                                    if (!GetPreValue<string>(context, Contact.FirstName, "NameImage").Equals(GetPostValue<string>(context, Contact.FirstName, "NameImage")))
                                    {
                                        newAudit[audit.fieldname] += "firstname\n";
                                        newAudit[audit.before] += GetPreValue<string>(context, Contact.FirstName, "NameImage") + "\n";
                                        newAudit[audit.after] += GetPostValue<string>(context, Contact.FirstName, "NameImage") + "\n";
                                    }
                                }
                                else
                                {
                                    newAudit[audit.fieldname] += "firstname\n";

                                    if (IsPreAttributeFound(context, Contact.FirstName, "NameImage"))
                                        newAudit[audit.before] += GetPreValue<string>(context, Contact.FirstName, "NameImage") + "\n";
                                    else
                                        newAudit[audit.before] += "\n";

                                    if (IsPostAttributeFound(context, Contact.FirstName, "NameImage"))
                                        newAudit[audit.after] += GetPostValue<string>(context, Contact.FirstName, "NameImage") + "\n";
                                    else
                                        newAudit[audit.after] += "\n";
                                }
                            }

                            if (IsPreAttributeFound(context, Contact.LastName, "NameImage") || IsPostAttributeFound(context, Contact.LastName, "NameImage"))
                            {

                                if (IsPreAttributeFound(context, Contact.LastName, "NameImage") && IsPostAttributeFound(context, Contact.LastName, "NameImage"))
                                {
                                    if (!GetPreValue<string>(context, Contact.LastName, "NameImage").Equals(GetPostValue<string>(context, Contact.LastName, "NameImage")))
                                    {
                                        newAudit[audit.fieldname] += "lastname\n";
                                        newAudit[audit.before] += GetPreValue<string>(context, Contact.LastName, "NameImage") + "\n";
                                        newAudit[audit.after] += GetPostValue<string>(context, Contact.LastName, "NameImage") + "\n";
                                    }
                                }
                                else
                                {
                                    newAudit[audit.fieldname] += "lastname\n";

                                    if (IsPreAttributeFound(context, Contact.LastName, "NameImage"))
                                        newAudit[audit.before] += GetPreValue<string>(context, Contact.LastName, "NameImage") + "\n";
                                    else
                                        newAudit[audit.before] += "\n";

                                    if (IsPostAttributeFound(context, Contact.LastName, "NameImage"))
                                        newAudit[audit.after] += GetPostValue<string>(context, Contact.LastName, "NameImage") + "\n";
                                    else
                                        newAudit[audit.after] += "\n";
                                }
                            }

                            newAudit[audit.fieldname] += "increment value\n";
                               
                            Entity contactObj = service.Retrieve(context.PrimaryEntityName, context.PrimaryEntityId, new ColumnSet(Contact.incrementvalue));

                            //contactObj["app_prefullname"] = preMessageImage.Attributes["fullname"];
                            newAudit[audit.before] += contactObj[Contact.incrementvalue].ToString() + "\n";

                            contactObj[Contact.incrementvalue] = int.Parse(contactObj[Contact.incrementvalue].ToString()) + 1;
                            newAudit[audit.after] += contactObj[Contact.incrementvalue] + "\n";

                            service.Update(contactObj);

                        }
                    }

                    if (IsPreAttributeFound(context, Contact.Birthday, "NameImage") || IsPostAttributeFound(context, Contact.Birthday, "NameImage"))
                    {
                        if (IsPreAttributeFound(context, Contact.Birthday, "NameImage") && IsPostAttributeFound(context, Contact.Birthday, "NameImage"))
                        {
                            if (!GetPreValue<DateTime>(context, Contact.Birthday, "NameImage").ToString().Equals(GetPostValue<DateTime>(context, Contact.Birthday, "NameImage").ToString()))
                            {
                                newAudit[App.Custom.audit.fieldname] += "birthday\n";
                                newAudit[App.Custom.audit.before] += GetPreValue<DateTime>(context, Contact.Birthday, "NameImage").ToString().Split(' ')[0] + "\n";
                                newAudit[App.Custom.audit.after] += GetPostValue<DateTime>(context, Contact.Birthday, "NameImage").ToString().Split(' ')[0] + "\n";
                            }

                            int preAge = getAge(GetPreValue<DateTime>(context, Contact.Birthday, "NameImage"));
                            int postAge = getAge(GetPostValue<DateTime>(context, Contact.Birthday, "NameImage"));

                            if (preAge != postAge)
                            {
                                newAudit[audit.fieldname] += "age\n";
                                newAudit[audit.before] += preAge + "\n";
                                newAudit[audit.after] += postAge + "\n";
                            }
                        }
                        else
                        {
                            newAudit[audit.fieldname] += "birthday\n";
                            newAudit[audit.fieldname] += "age\n";

                            if (IsPreAttributeFound(context, Contact.Birthday, "NameImage"))
                            {
                                newAudit[audit.before] += GetPreValue<DateTime>(context, Contact.Birthday, "NameImage").ToString().Split(' ')[0] + "\n";

                                int preAge = getAge(GetPreValue<DateTime>(context, Contact.Birthday, "NameImage"));

                                newAudit[audit.before] += preAge + "\n";

                            }
                            else
                                newAudit[audit.before] += "\n";

                            if (IsPostAttributeFound(context, Contact.Birthday, "NameImage"))
                            {
                                newAudit[audit.after] += GetPostValue<DateTime>(context, Contact.Birthday, "NameImage").ToString().Split(' ')[0] + "\n";

                                int postAge = getAge(GetPostValue<DateTime>(context, Contact.Birthday, "NameImage"));


                                newAudit[audit.after] += postAge + "\n";
                            }
                            else
                                newAudit[audit.after] += "\n";
                        }
                    }

                    if (IsPreAttributeFound(context, Contact.CompanyName, "NameImage") || IsPostAttributeFound(context, Contact.CompanyName, "NameImage"))
                    {

                        if (IsPreAttributeFound(context, Contact.CompanyName, "NameImage") && IsPostAttributeFound(context, Contact.CompanyName, "NameImage"))
                        {
                            if (!GetPreValue<EntityReference>(context, Contact.CompanyName, "NameImage").Equals(GetPostValue<EntityReference>(context, Contact.CompanyName, "NameImage")))
                            {
                                newAudit[App.Custom.audit.fieldname] += "company name\n";
                                newAudit[App.Custom.audit.before] += GetPreValue<EntityReference>(context, Contact.CompanyName, "NameImage").Name + "\n";
                                newAudit[App.Custom.audit.after] += GetPostValue<EntityReference>(context, Contact.CompanyName, "NameImage").Name + "\n";
                            }

                        }
                        else
                        {
                            newAudit[App.Custom.audit.fieldname] += "company name\n";

                            if (IsPreAttributeFound(context, Contact.CompanyName, "NameImage"))
                                newAudit[App.Custom.audit.before] += GetPreValue<EntityReference>(context, Contact.CompanyName, "NameImage").Name + "\n";
                            else
                                newAudit[App.Custom.audit.before] += "\n";

                            if (IsPostAttributeFound(context, Contact.CompanyName, "NameImage"))
                                newAudit[App.Custom.audit.after] += GetPostValue<EntityReference>(context, Contact.CompanyName, "NameImage").Name + "\n";
                            else
                                newAudit[App.Custom.audit.after] += "\n";
                        }
                    }

                    service.Create(newAudit);
                    
                }

                private int getAge(DateTime birthday)
                {
                    DateTime today = DateTime.Today;

                    // Calculate the age.
                    int age = today.Year - birthday.Year;
                    // Go back to the year in which the person was born in case of a leap year
                    if (birthday.Date > today.AddYears(-age)) age--;

                    return age;
                }
            }
        }
    }

    namespace AccountFormUpdateAge
    {
        public class AccountUpdateAge : CodeActivity
        {
            [RequiredArgument]
            [Input("EntityReference input")]
            [ReferenceTarget(Account.EntityName)]
            public InArgument<EntityReference> AccountReference { get; set; }

            protected override void Execute(CodeActivityContext context)
            {
                try
                {
                    // var testing = new ContactForm.CreateAndUpdate.CreateAndUpdateForm();
                    var accountAgeUpdate = new ContactForm.CreateAndUpdate.CreateAndUpdateForm();

                    var workflowcontext = context.GetExtension<IWorkflowContext>();

                    //Create an Organization Service
                    var serviceFactory = context.GetExtension<IOrganizationServiceFactory>();
                    var service = serviceFactory.CreateOrganizationService(workflowcontext.InitiatingUserId);

                    //Retrieve the contact id
                    var accountRef = AccountReference.Get<EntityReference>(context);

                    //  testing.UpdateTotalAge(accountRef.Id, service);
                    accountAgeUpdate.UpdateTotalAge(accountRef.Id, service);


                }
                catch (Exception ex)
                {
                    throw new NotImplementedException(ex.Message);
                }

                //totalAgeOutput.Set(executionContext, totalAge.ToString());


            }
        }
    }
}
