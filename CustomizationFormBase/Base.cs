using Microsoft.Xrm.Sdk;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace App.Crm.Plugin
{
    public enum MessageName
    {
        [EnumMember]
        Assign,

        [EnumMember]
        Associate,

        [EnumMember]
        Create,

        [EnumMember]
        Delete,

        [EnumMember]
        Disassociate,

        [EnumMember]
        Retrieve,

        [EnumMember]
        RetrieveMultiple,

        [EnumMember]
        Update,

        [EnumMember]
        SetStateDynamicEntity,

        [EnumMember]
        app_SMAPGenerateWOCreditNote,

        [EnumMember]
        app_SMAPGenerateEWCreditNote,

        [EnumMember]
        Merge
    }

    public enum SdkMessageProcessingStepStage
    {
        [EnumMember]
        Prevalidation = 10,

        [EnumMember]
        Preoperation = 20,

        [EnumMember]
        Postoperation = 40
    }

    /// <summary>
    /// Base class for all plug-in classes.
    /// </summary>
    public abstract class Base : IPlugin
    {
        private Collection<Tuple<SdkMessageProcessingStepStage, MessageName, string, Action<LocalPluginContext>>> _registeredEvents;

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginBase"/> class.
        /// </summary>
        /// <param name="childClassName">The <see cref="childClassName" cred="Type"/> of the derived class.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "PluginBase")]
        protected Base(Type childClassName) => ChildClassName = childClassName.ToString();

        /// <summary>
        /// Gets or sets the name of the child class.
        /// </summary>
        /// <value>The name of the child class.</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "PluginBase")]
        protected string ChildClassName { get; private set; }

        /// <summary>
        /// Gets the List of events that the plug-in should fire for. Each List
        /// Item is a <see cref="System.Tuple"/> containing the Pipeline Stage, Message and (optionally) the Primary Entity.
        /// In addition, the fourth parameter provide the delegate to invoke on a matching registration.
        /// </summary>
        protected Collection<Tuple<SdkMessageProcessingStepStage, MessageName, string, Action<LocalPluginContext>>> RegisteredEvents => _registeredEvents ?? (_registeredEvents = new Collection<Tuple<SdkMessageProcessingStepStage, MessageName, string, Action<LocalPluginContext>>>());

        /// <summary>
        /// Get attribute value from target input parameters
        /// </summary>
        /// <typeparam name="T">Attribute type</typeparam>
        /// <param name="context">Plugin context</param>
        /// <param name="attributeName">Attribute name</param>
        /// <returns></returns>
        public static T GetInputValue<T>(IPluginExecutionContext context, string attributeName)
        {
            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                var entity = (Entity)context.InputParameters["Target"];
                return entity.GetAttributeValue<T>(attributeName);
            }

            return default;
        }

        /// <summary>
        /// Get post entity image
        /// </summary>
        /// <param name="context">Plugin context</param>
        /// <param name="postEntityImageName"></param>
        /// <returns></returns>
        ///
        public static Entity GetPostEntityImage(IPluginExecutionContext context, string postEntityImageName = null)
        {
            if (string.IsNullOrWhiteSpace(postEntityImageName))
            {
                return context.PostEntityImages.FirstOrDefault().Value;
            }

            return context.PostEntityImages.Contains(postEntityImageName) ? context.PostEntityImages[postEntityImageName] : null;
        }

        /// <summary>
        /// Get attribute value from post entity images
        /// </summary>
        /// <typeparam name="T">Attribute type</typeparam>
        /// <param name="context">Plugin context</param>
        /// <param name="attributeName">Attribute name</param>
        /// <param name="postEntityImageName"></param>
        /// <returns></returns>
        public static T GetPostValue<T>(IPluginExecutionContext context, string attributeName, string postEntityImageName = null)
        {
            if (!context.PostEntityImages.Any())
                return default;

            if (!string.IsNullOrWhiteSpace(postEntityImageName) && context.PostEntityImages.Contains(postEntityImageName))
            {
                var postEntity = context.PostEntityImages[postEntityImageName];
                return postEntity.GetAttributeValue<T>(attributeName);
            }

            var relatedPostEntity = context.PostEntityImages.Values.FirstOrDefault(postEntity => postEntity.Attributes.Contains(attributeName));
            return relatedPostEntity == null ? default : relatedPostEntity.GetAttributeValue<T>(attributeName);
        }

        /// <summary>
        /// Get pre entity image
        /// </summary>
        /// <param name="context">Plugin context</param>
        /// <param name="preEntityImageName"></param>
        /// <returns></returns>
        ///
        public static Entity GetPreEntityImage(IPluginExecutionContext context, string preEntityImageName = null)
        {
            if (string.IsNullOrWhiteSpace(preEntityImageName))
            {
                return context.PreEntityImages.FirstOrDefault().Value;
            }

            return context.PreEntityImages.Contains(preEntityImageName) ? context.PreEntityImages[preEntityImageName] : null;
        }

        /// <summary>
        /// Get attribute value from pre entity images
        /// </summary>
        /// <typeparam name="T">Attribute type</typeparam>
        /// <param name="context">Plugin context</param>
        /// <param name="attributeName">Attribute name</param>
        /// <param name="preEntityImageName"></param>
        /// <returns></returns>
        public static T GetPreValue<T>(IPluginExecutionContext context, string attributeName, string preEntityImageName = null)
        {
            if (!context.PreEntityImages.Any())
                return default;

            if (!string.IsNullOrWhiteSpace(preEntityImageName) && context.PreEntityImages.Contains(preEntityImageName))
            {
                var preEntity = context.PreEntityImages[preEntityImageName];
                return preEntity.GetAttributeValue<T>(attributeName);
            }

            foreach (var preEntity in context.PreEntityImages.Values.Where(preEntity => preEntity.Attributes.Contains(attributeName)))
                return preEntity.GetAttributeValue<T>(attributeName);

            return default;
        }

        /// <summary>
        /// Get relationship entity
        /// </summary>
        /// <param name="context">Plugin context</param>
        /// <returns></returns>
        public static Relationship GetRelationshipEntity(IPluginExecutionContext context) => context.InputParameters.Contains("Relationship") ? context.InputParameters["Relationship"] as Relationship : null;

        /// <summary>
        /// Get target entity
        /// </summary>
        /// <param name="context">Plugin context</param>
        /// <returns></returns>
        public static Entity GetTargetEntity(IPluginExecutionContext context) => context.InputParameters.Contains("Target") ? context.InputParameters["Target"] as Entity : null;

        /// <summary>
        /// Get attribute value from entity
        /// </summary>
        /// <param name="context">Plugin context</param>
        /// <returns></returns>
        public static EntityReference GetTargetEntityReference(IPluginExecutionContext context)
        {
            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is EntityReference entityReference)
                return entityReference;

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity entity)
                return entity.ToEntityReference();

            return null;
        }

        /// <summary>
        /// Get latest attribute value (Prioritize by PostEntityImages = "PostImage"; Target Input Parameters; PreEntityImages = "PreImage")
        /// </summary>
        /// <typeparam name="T">Attribute type</typeparam>
        /// <param name="context">Plugin context</param>
        /// <param name="attributeName">Attribute name</param>
        /// <returns></returns>
        public static T GetValue<T>(IPluginExecutionContext context, string attributeName)
        {
            if (IsPostAttributeFound(context, attributeName))
                return GetPostValue<T>(context, attributeName);

            if (IsDirty(context, attributeName))
                return GetInputValue<T>(context, attributeName);

            return GetPreValue<T>(context, attributeName);
        }

        /// <summary>
        /// Validate changes on target's attribute
        /// </summary>
        /// <param name="context">Plugin context</param>
        /// <param name="attributeName">Attribute name</param>
        /// <returns></returns>
        public static bool IsDirty(IPluginExecutionContext context, string attributeName)
        {
            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                var entity = (Entity)context.InputParameters["Target"];
                return entity.Attributes.Contains(attributeName);
            }

            return false;
        }

        public static bool IsDirty<T>(IPluginExecutionContext context, string attributeName, out T latestValue)
        {
            latestValue = GetValue<T>(context, attributeName);
            return IsDirty(context, attributeName);
        }

        public static bool IsDirty<T>(IPluginExecutionContext context, string attributeName, out T oldValue, out T latestValue)
        {
            oldValue = GetPreValue<T>(context, attributeName);
            latestValue = GetValue<T>(context, attributeName);
            return IsDirty(context, attributeName);
        }

        /// <summary>
        /// Validate attribute value from pre entity images
        /// </summary>
        /// <param name="context">Plugin context</param>
        /// <param name="attributeName">Attribute name</param>
        /// <param name="postEntityImageName"></param>
        /// <returns></returns>
        public static bool IsPostAttributeFound(IPluginExecutionContext context, string attributeName, string postEntityImageName = null)
        {
            if (!context.PostEntityImages.Any())
                return false;

            if (!string.IsNullOrWhiteSpace(postEntityImageName) && (!context.PostEntityImages.Contains(postEntityImageName) || !context.PostEntityImages[postEntityImageName].Contains(attributeName)))
                return false;

            return context.PostEntityImages.Values.Any(postEntity => postEntity.Contains(attributeName));
        }

        /// <summary>
        /// Validate attribute value from pre entity images
        /// </summary>
        /// <param name="context">Plugin context</param>
        /// <param name="attributeName">Attribute name</param>
        /// <param name="preEntityImageName"></param>
        /// <returns></returns>
        public static bool IsPreAttributeFound(IPluginExecutionContext context, string attributeName, string preEntityImageName = null)
        {
            if (!context.PreEntityImages.Any())
                return false;

            if (!string.IsNullOrWhiteSpace(preEntityImageName) && (!context.PreEntityImages.Contains(preEntityImageName) || !context.PreEntityImages[preEntityImageName].Contains(attributeName)))
                return false;

            return context.PreEntityImages.Values.Any(preEntity => preEntity.Contains(attributeName));
        }

        /// <summary>
        /// Main entry point for the business logic that the plug-in is to execute.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <remarks>
        /// For improved performance, Microsoft Dynamics 365 caches plug-in instances.
        /// The plug-in's Execute method should be written to be stateless as the constructor
        /// is not called for every invocation of the plug-in. Also, multiple system threads
        /// could execute the plug-in at the same time. All per invocation state information
        /// is stored in the context. This means that you should not use global variables in plug-ins.
        /// </remarks>
        public void Execute(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
                throw new InvalidPluginExecutionException("serviceProvider");

            //var stopwatch = new Stopwatch();
            //stopwatch.Start();

            // Construct the local plug-in context.
            var localcontext = new LocalPluginContext(serviceProvider);
            localcontext.Trace($"Entered {ChildClassName}.Execute()");

            try
            {
                // Iterate over all of the expected registered events to ensure that the plugin
                // has been invoked by an expected event
                // For any given plug-in event at an instance in time, we would expect at most 1 result to match.
                var entityAction =
                (
                    from registeredEvent in RegisteredEvents
                    where
                        registeredEvent.Item1 == (SdkMessageProcessingStepStage)localcontext.PluginExecutionContext.Stage &&
                        registeredEvent.Item2.ToString() == localcontext.PluginExecutionContext.MessageName &&
                        (
                            string.IsNullOrWhiteSpace(registeredEvent.Item3) ||
                            registeredEvent.Item3 == localcontext.PluginExecutionContext.PrimaryEntityName
                        )
                    select registeredEvent.Item4
                ).FirstOrDefault();

                // now exit - if the derived plug-in has incorrectly registered overlapping event registrations,
                // guard against multiple executions.
                if (entityAction == null)
                    return;

                localcontext.Trace($"{ChildClassName} is firing for Entity: {localcontext.PluginExecutionContext.PrimaryEntityName}({localcontext.PluginExecutionContext.PrimaryEntityId})" +
                    $"\r\nMessage: {localcontext.PluginExecutionContext.MessageName}" +
                    $"\r\nPlugin Stage: {((SdkMessageProcessingStepStage)localcontext.PluginExecutionContext.Stage).ToString()}");

                entityAction.Invoke(localcontext);

                //// Invoke the custom implementation
                //ExecuteCrmPlugin(localcontext);
                //// now exit - if the derived plug-in has incorrectly registered overlapping event registrations,
                //// guard against multiple executions.
            }
            catch (ArgumentException e)
            {
                localcontext.Trace($"Exception: {e}");

                // Handle the exception.
                throw new InvalidPluginExecutionException(e.Message, e);
            }
            catch (FaultException<OrganizationServiceFault> e)
            {
                localcontext.Trace($"Exception: {e}");

                // Handle the exception.
                throw new InvalidPluginExecutionException(e.Message, e);
            }
            //finally
            //{
            //    localcontext.Trace($"Exiting {ChildClassName}.Execute()\r\nElapsed Time: {stopwatch.Elapsed.ConvertToStringRepresentation()}");
            //    stopwatch.Stop();
            //}
        }

        ///// <summary>
        ///// Placeholder for a custom plug-in implementation.
        ///// </summary>
        ///// <param name="localcontext">Context for the current plug-in.</param>
        //protected virtual void ExecuteCrmPlugin(LocalPluginContext localcontext)
        //{
        //    // Do nothing.
        //}

        /// <summary>
        /// Plug-in context object.
        /// </summary>
        protected class LocalPluginContext
        {
            public LocalPluginContext()
            {
            }

            /// <summary>
            /// Helper object that stores the services available in this plug-in.
            /// </summary>
            /// <param name="serviceProvider"></param>
            public LocalPluginContext(IServiceProvider serviceProvider)
            {
                if (serviceProvider == null)
                {
                    throw new InvalidPluginExecutionException("serviceProvider");
                }

                // Obtain the execution context service from the service provider.
                PluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

                // Obtain the tracing service from the service provider.
                TracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

                // Get the notification service from the service provider.
                NotificationService = (IServiceEndpointNotificationService)serviceProvider.GetService(typeof(IServiceEndpointNotificationService));

                // Obtain the Organization Service factory service from the service provider
                Organizationfactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

                // Use the factory to generate the organization service.
                //OrganizationService = Organizationfactory.CreateOrganizationService(PluginExecutionContext.UserId);
                OrganizationService = Organizationfactory.CreateOrganizationService(Guid.Empty);

                // Use the factory to generate the administrator organization service.
                AdministratorOrganizationService = Organizationfactory.CreateOrganizationService(null);
            }

            /// <summary>
            /// The Microsoft Dynamics 365 admintrator organization service.
            /// </summary>
            public IOrganizationService AdministratorOrganizationService { get; private set; }

            /// <summary>
            /// Synchronous registered plug-ins can post the execution context to the Microsoft Azure Service Bus. <br/>
            /// It is through this notification service that synchronous plug-ins can send brokered messages to the Microsoft Azure Service Bus.
            /// </summary>
            public IServiceEndpointNotificationService NotificationService { get; private set; }

            public IOrganizationServiceFactory Organizationfactory { get; private set; }
            /// <summary>
            /// The Microsoft Dynamics 365 organization service.
            /// </summary>
            public IOrganizationService OrganizationService { get; private set; }

            /// <summary>
            /// IPluginExecutionContext contains information that describes the run-time environment in which the plug-in executes, information related to the execution pipeline, and entity business information.
            /// </summary>
            public IPluginExecutionContext PluginExecutionContext { get; private set; }

            public IServiceProvider ServiceProvider { get; private set; }

            /// <summary>
            /// Provides logging run-time trace information for plug-ins.
            /// </summary>
            public ITracingService TracingService { get; private set; }

            /// <summary>
            /// Writes a trace message to the CRM trace log.
            /// </summary>
            /// <param name="message">Message name to trace.</param>
            public void Trace(string message)
            {
                if (string.IsNullOrWhiteSpace(message) || TracingService == null)
                    return;

                if (PluginExecutionContext == null)
                {
                    TracingService.Trace(message);
                }
                else
                {
                    TracingService.Trace(
                        "{0}, Correlation Id: {1}, Initiating User: {2}",
                        message,
                        PluginExecutionContext.CorrelationId,
                        PluginExecutionContext.InitiatingUserId);
                }
            }
        }
    }
}