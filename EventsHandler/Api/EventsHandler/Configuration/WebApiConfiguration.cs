﻿// © 2023, Worth Systems.

using EventsHandler.Extensions;
using EventsHandler.Properties;
using EventsHandler.Services.DataLoading.Enums;
using EventsHandler.Services.DataLoading.Interfaces;
using EventsHandler.Services.DataLoading.Strategy.Interfaces;
using EventsHandler.Services.DataLoading.Strategy.Manager;
using JetBrains.Annotations;

namespace EventsHandler.Configuration
{
    /// <summary>
    /// The object representing all application settings.
    /// <para>
    ///   Different types of settings are supported (to provide setup flexibility):
    ///   <list type="bullet">
    ///     <item>
    ///       <see cref="LoaderTypes.AppSettings"/> - Read from "appsettings.json" file (using <see cref="IConfiguration"/> interface).
    ///     </item>
    ///     <item>
    ///       <see cref="LoaderTypes.Environment"/> - Read from environment variables (using <see cref="Environment.GetEnvironmentVariable(string)"/>).
    ///     </item>
    ///   </list>
    /// </para>
    /// </summary>
    public sealed record WebApiConfiguration
    {
        private readonly IServiceProvider _serviceProvider;

        private AppSettingsComponent? _appSettings;
        private OmcComponent? _omc;
        private UserComponent? _user;

        /// <summary>
        /// Gets the object representing "appsettings[.xxx].json" configuration file with predefined flags and variables.
        /// </summary>
        internal AppSettingsComponent AppSettings
            => GetComponent(ref this._appSettings, LoaderTypes.AppSettings, nameof(AppSettings));

        /// <summary>
        /// Gets the object representing Output Management Component (internal) settings.
        /// </summary>
        internal OmcComponent OMC
            => GetComponent(ref this._omc, LoaderTypes.Environment, nameof(OMC));

        /// <summary>
        /// Gets the object representing (external) settings configured by user for
        /// dependent services ("OpenNotificaties", "OpenZaak", "OpenKlant", "Notify NL").
        /// </summary>
        internal UserComponent User
            => GetComponent(ref this._user, LoaderTypes.Environment, nameof(User));

        /// <summary>
        /// Initializes a new instance of the <see cref="WebApiConfiguration"/> class.
        /// </summary>
        public WebApiConfiguration(IServiceProvider serviceProvider)  // NOTE: The only constructor to be used with Dependency Injection
        {
            this._serviceProvider = serviceProvider;

            // Recreate the structure of settings from "appsettings.json" configuration file or from Environment Variables
            // NOTE: Initialize these components now to spare execution time during real-time (Activator.CreateInstance<T>)
            this._appSettings = AppSettings;
            this._omc = OMC;
            this._user = User;
        }

        #region Settings
        /// <summary>
        /// The "appsettings[.xxx].json" part of the settings.
        /// </summary>
        [UsedImplicitly]
        internal sealed record AppSettingsComponent
        {
            // NOTE: Property "Logging" (from "appsettings.json") is skipped because it's not used anywhere in the code
            
            /// <inheritdoc cref="NetworkComponent"/>
            internal NetworkComponent Network { get; }

            /// <inheritdoc cref="EncryptionComponent"/>
            internal EncryptionComponent Encryption { get; }
            
            /// <inheritdoc cref="FeaturesComponent"/>
            internal FeaturesComponent Features { get; }
            
            /// <inheritdoc cref="VariablesComponent"/>
            internal VariablesComponent Variables { get; }

            // NOTE: Property "AllowedHosts" (from "appsettings.json") is skipped because it's not used anywhere in the code

            /// <summary>
            /// Initializes a new instance of the <see cref="AppSettingsComponent"/> class.
            /// </summary>
            public AppSettingsComponent(ILoadersContext loadersContext, string parentName)
            {
                this.Network = new NetworkComponent(loadersContext, parentName);
                this.Encryption = new EncryptionComponent(loadersContext, parentName);
                this.Features = new FeaturesComponent(loadersContext, parentName);
                this.Variables = new VariablesComponent(loadersContext, parentName);
            }
            
            /// <summary>
            /// The "Network" part of the settings.
            /// </summary>
            internal sealed record NetworkComponent
            {
                private readonly ILoadersContext _loadersContext;
                private readonly string _currentPath;
                
                /// <summary>
                /// Initializes a new instance of the <see cref="NetworkComponent"/> class.
                /// </summary>
                internal NetworkComponent(ILoadersContext loadersContext, string parentPath)
                {
                    this._loadersContext = loadersContext;
                    this._currentPath = loadersContext.GetPathWithNode(parentPath, nameof(Network));
                }

                /// <inheritdoc cref="ILoadingService.GetData{TData}(string)"/>
                internal ushort ConnectionLifetimeInSeconds()
                    => GetValue<ushort>(this._loadersContext, this._currentPath, nameof(ConnectionLifetimeInSeconds));

                /// <inheritdoc cref="ILoadingService.GetData{TData}(string)"/>
                internal ushort HttpRequestTimeoutInSeconds()
                    => GetValue<ushort>(this._loadersContext, this._currentPath, nameof(HttpRequestTimeoutInSeconds));

                /// <inheritdoc cref="ILoadingService.GetData{TData}(string)"/>
                internal ushort HttpRequestsSimultaneousNumber()
                    => GetValue<ushort>(this._loadersContext, this._currentPath, nameof(HttpRequestsSimultaneousNumber));
            }

            /// <summary>
            /// The "Encryption" part of the settings.
            /// </summary>
            internal sealed record EncryptionComponent
            {
                private readonly ILoadersContext _loadersContext;
                private readonly string _currentPath;
                
                /// <summary>
                /// Initializes a new instance of the <see cref="EncryptionComponent"/> class.
                /// </summary>
                internal EncryptionComponent(ILoadersContext loadersContext, string parentPath)
                {
                    this._loadersContext = loadersContext;
                    this._currentPath = loadersContext.GetPathWithNode(parentPath, nameof(Encryption));
                }

                /// <inheritdoc cref="ILoadingService.GetData{TData}(string)"/>
                internal bool IsAsymmetric()
                    => GetValue<bool>(this._loadersContext, this._currentPath, nameof(IsAsymmetric));
            }
            
            /// <summary>
            /// The "Features" part of the settings.
            /// </summary>
            internal sealed record FeaturesComponent
            {
                private readonly ILoadersContext _loadersContext;
                private readonly string _currentPath;
                
                /// <summary>
                /// Initializes a new instance of the <see cref="FeaturesComponent"/> class.
                /// </summary>
                internal FeaturesComponent(ILoadersContext loadersContext, string parentPath)
                {
                    this._loadersContext = loadersContext;
                    this._currentPath = loadersContext.GetPathWithNode(parentPath, nameof(Features));
                }

                /// <inheritdoc cref="ILoadingService.GetData{TData}(string)"/>
                internal byte OmcWorkflowVersion()
                    => GetValue<byte>(this._loadersContext, this._currentPath, nameof(OmcWorkflowVersion));
            }
            
            /// <summary>
            /// The "Variables" part of the settings.
            /// </summary>
            internal sealed record VariablesComponent
            {
                private readonly ILoadersContext _loadersContext;
                private readonly string _currentPath;

                /// <inheritdoc cref="OpenKlantComponent"/>
                internal OpenKlantComponent OpenKlant { get; }

                /// <inheritdoc cref="MessagesComponent"/>
                internal MessagesComponent Messages { get; }
                
                /// <summary>
                /// Initializes a new instance of the <see cref="VariablesComponent"/> class.
                /// </summary>
                internal VariablesComponent(ILoadersContext loadersContext, string parentPath)
                {
                    this._loadersContext = loadersContext;
                    this._currentPath = loadersContext.GetPathWithNode(parentPath, nameof(Variables));

                    this.OpenKlant = new OpenKlantComponent(loadersContext, this._currentPath);
                    this.Messages = new MessagesComponent(loadersContext, this._currentPath);
                }

                /// <inheritdoc cref="ILoadingService.GetData{TData}(string)"/>
                internal string SubjectType()
                    => GetValue(this._loadersContext, this._currentPath, "BetrokkeneType");
                
                /// <inheritdoc cref="ILoadingService.GetData{TData}(string)"/>
                internal string InitiatorRole()
                    => GetValue(this._loadersContext, this._currentPath, "OmschrijvingGeneriek");
                
                /// <inheritdoc cref="ILoadingService.GetData{TData}(string)"/>
                internal string PartyIdentifier()
                    => GetValue(this._loadersContext, this._currentPath, "PartijIdentificator");
                
                /// <inheritdoc cref="ILoadingService.GetData{TData}(string)"/>
                internal string EmailGenericDescription()
                    => GetValue(this._loadersContext, this._currentPath, "EmailOmschrijvingGeneriek");

                /// <inheritdoc cref="ILoadingService.GetData{TData}(string)"/>
                internal string PhoneGenericDescription()
                    => GetValue(this._loadersContext, this._currentPath, "TelefoonOmschrijvingGeneriek");
                
                /// <summary>
                /// The "OpenKlant" part of the settings.
                /// </summary>
                internal sealed class OpenKlantComponent
                {
                    private readonly ILoadersContext _loadersContext;
                    private readonly string _currentPath;

                    /// <summary>
                    /// Initializes a new instance of the <see cref="OpenKlantComponent"/> class.
                    /// </summary>
                    internal OpenKlantComponent(ILoadersContext loadersContext, string parentPath)
                    {
                        this._loadersContext = loadersContext;
                        this._currentPath = loadersContext.GetPathWithNode(parentPath, nameof(OpenKlant));
                    }

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string)"/>
                    internal string CodeObjectType()
                        => GetValue(this._loadersContext, this._currentPath, nameof(CodeObjectType));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string)"/>
                    internal string CodeRegister()
                        => GetValue(this._loadersContext, this._currentPath, nameof(CodeRegister));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string)"/>
                    internal string CodeObjectTypeId()
                        => GetValue(this._loadersContext, this._currentPath, nameof(CodeObjectTypeId));
                }

                /// <summary>
                /// The "Messages" part of the settings.
                /// </summary>
                internal sealed class MessagesComponent
                {
                    private readonly ILoadersContext _loadersContext;
                    private readonly string _currentPath;

                    /// <summary>
                    /// Initializes a new instance of the <see cref="MessagesComponent"/> class.
                    /// </summary>
                    internal MessagesComponent(ILoadersContext loadersContext, string parentPath)
                    {
                        this._loadersContext = loadersContext;
                        this._currentPath = loadersContext.GetPathWithNode(parentPath, nameof(Messages));
                    }

                    #region SMS
                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string)"/>
                    internal string SMS_Success_Subject()
                        => GetValue(this._loadersContext, this._currentPath, nameof(SMS_Success_Subject));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string)"/>
                    internal string SMS_Success_Body()
                        => GetValue(this._loadersContext, this._currentPath, nameof(SMS_Success_Body));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string)"/>
                    internal string SMS_Failure_Subject()
                        => GetValue(this._loadersContext, this._currentPath, nameof(SMS_Failure_Subject));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string)"/>
                    internal string SMS_Failure_Body()
                        => GetValue(this._loadersContext, this._currentPath, nameof(SMS_Failure_Body));
                    #endregion

                    #region E-mail
                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string)"/>
                    internal string Email_Success_Subject()
                        => GetValue(this._loadersContext, this._currentPath, nameof(Email_Success_Subject));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string)"/>
                    internal string Email_Success_Body()
                        => GetValue(this._loadersContext, this._currentPath, nameof(Email_Success_Body));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string)"/>
                    internal string Email_Failure_Subject()
                        => GetValue(this._loadersContext, this._currentPath, nameof(Email_Failure_Subject));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string)"/>
                    internal string Email_Failure_Body()
                        => GetValue(this._loadersContext, this._currentPath, nameof(Email_Failure_Body));
                    #endregion
                }
            }
        }

        /// <summary>
        /// The common base for <see cref="OmcComponent"/> and <see cref="UserComponent"/>.
        /// </summary>
        internal abstract record BaseComponent
        {
            /// <inheritdoc cref="AuthorizationComponent"/>
            internal AuthorizationComponent Authorization { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="BaseComponent"/> class.
            /// </summary>
            protected BaseComponent(ILoadersContext loadersContext, string parentName)
            {
                this.Authorization = new AuthorizationComponent(loadersContext, parentName);
            }

            /// <summary>
            /// The "Authorization" part of the settings.
            /// </summary>
            internal sealed record AuthorizationComponent
            {
                /// <inheritdoc cref="JwtComponent"/>
                internal JwtComponent JWT { get; }

                /// <summary>
                /// Initializes a new instance of the <see cref="AuthorizationComponent"/> class.
                /// </summary>
                internal AuthorizationComponent(ILoadersContext loadersContext, string parentPath)
                {
                    string currentPath = loadersContext.GetPathWithNode(parentPath, nameof(Authorization));

                    this.JWT = new JwtComponent(loadersContext, currentPath);
                }

                /// <summary>
                /// The "JWT" part of the settings.
                /// </summary>
                internal sealed record JwtComponent
                {
                    private readonly ILoadersContext _loadersContext;
                    private readonly string _currentPath;

                    /// <summary>
                    /// Initializes a new instance of the <see cref="JwtComponent"/> class.
                    /// </summary>
                    internal JwtComponent(ILoadersContext loadersContext, string parentPath)
                    {
                        this._loadersContext = loadersContext;
                        this._currentPath = loadersContext.GetPathWithNode(parentPath, nameof(JWT));
                    }

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string)"/>
                    internal string Secret()
                        => GetValue(this._loadersContext, this._currentPath, nameof(Secret));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string)"/>
                    internal string Issuer()
                        => GetValue(this._loadersContext, this._currentPath, nameof(Issuer));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string)"/>
                    internal string Audience()
                        => GetValue(this._loadersContext, this._currentPath, nameof(Audience));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string)"/>
                    internal ushort ExpiresInMin()
                        => GetValue<ushort>(this._loadersContext, this._currentPath, nameof(ExpiresInMin));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string)"/>
                    internal string UserId()
                        => GetValue(this._loadersContext, this._currentPath, nameof(UserId));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string)"/>
                    internal string UserName()
                        => GetValue(this._loadersContext, this._currentPath, nameof(UserName));
                }
            }
        }

        /// <summary>
        /// The "OMC" part of the settings.
        /// </summary>
        [UsedImplicitly]
        internal sealed record OmcComponent : BaseComponent
        {
            /// <inheritdoc cref="ApiComponent"/>
            internal ApiComponent API { get; }
            
            /// <summary>
            /// Initializes a new instance of the <see cref="OmcComponent"/> class.
            /// </summary>
            public OmcComponent(ILoadersContext loadersContext, string parentName)
                : base(loadersContext, parentName)
            {
                this.API = new ApiComponent(loadersContext, parentName);
            }

            /// <summary>
            /// The "API" part of the settings.
            /// </summary>
            internal sealed record ApiComponent
            {
                /// <inheritdoc cref="BaseUrlComponent"/>
                internal BaseUrlComponent BaseUrl { get; }

                /// <summary>
                /// Initializes a new instance of the <see cref="ApiComponent"/> class.
                /// </summary>
                internal ApiComponent(ILoadersContext loadersContext, string parentPath)
                {
                    string currentPath = loadersContext.GetPathWithNode(parentPath, nameof(API));

                    this.BaseUrl = new BaseUrlComponent(loadersContext, currentPath);
                }
                
                /// <summary>
                /// The "Base URL" part of the settings.
                /// </summary>
                internal sealed record BaseUrlComponent
                {
                    private readonly ILoadersContext _loadersContext;
                    private readonly string _currentPath;

                    /// <summary>
                    /// Initializes a new instance of the <see cref="BaseUrlComponent"/> class.
                    /// </summary>
                    public BaseUrlComponent(ILoadersContext loadersContext, string parentPath)
                    {
                        this._loadersContext = loadersContext;
                        this._currentPath = loadersContext.GetPathWithNode(parentPath, nameof(BaseUrl));
                    }

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string)"/>
                    internal string NotifyNL()
                        => GetValue(this._loadersContext, this._currentPath, nameof(NotifyNL));
                }
            }
        }

        /// <summary>
        /// The "User" part of the settings.
        /// </summary>
        [UsedImplicitly]
        internal sealed record UserComponent : BaseComponent
        {
            /// <inheritdoc cref="ApiComponent"/>
            internal ApiComponent API { get; }

            /// <inheritdoc cref="DomainComponent"/>
            internal DomainComponent Domain { get; }

            /// <inheritdoc cref="TemplateIdsComponent"/>
            internal TemplateIdsComponent TemplateIds { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="UserComponent"/> class.
            /// </summary>
            public UserComponent(ILoadersContext loadersContext, string parentName)
                : base(loadersContext, parentName)
            {
                this.API = new ApiComponent(loadersContext, parentName);
                this.Domain = new DomainComponent(loadersContext, parentName);
                this.TemplateIds = new TemplateIdsComponent(loadersContext, parentName);
            }

            /// <summary>
            /// The "API" part of the settings.
            /// </summary>
            internal sealed record ApiComponent
            {
                /// <inheritdoc cref="KeyComponent"/>
                internal KeyComponent Key { get; }

                /// <summary>
                /// Initializes a new instance of the <see cref="ApiComponent"/> class.
                /// </summary>
                internal ApiComponent(ILoadersContext loadersContext, string parentPath)
                {
                    string currentPath = loadersContext.GetPathWithNode(parentPath, nameof(API));

                    this.Key = new KeyComponent(loadersContext, currentPath);
                }

                /// <summary>
                /// The "Key" part of the settings.
                /// </summary>
                internal sealed record KeyComponent
                {
                    private readonly ILoadersContext _loadersContext;
                    private readonly string _currentPath;

                    /// <summary>
                    /// Initializes a new instance of the <see cref="KeyComponent"/> class.
                    /// </summary>
                    internal KeyComponent(ILoadersContext loadersContext, string parentPath)
                    {
                        this._loadersContext = loadersContext;
                        this._currentPath = loadersContext.GetPathWithNode(parentPath, nameof(Key));
                    }

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string)"/>
                    internal string NotifyNL()
                        => GetValue(this._loadersContext, this._currentPath, nameof(NotifyNL));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string)"/>
                    internal string OpenKlant_2()
                        => GetValue(this._loadersContext, this._currentPath, nameof(OpenKlant_2));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string)"/>
                    internal string Objecten()
                        => GetValue(this._loadersContext, this._currentPath, nameof(Objecten));
                }
            }

            /// <summary>
            /// The "Domain" part of the settings.
            /// </summary>
            internal sealed record DomainComponent
            {
                private readonly ILoadersContext _loadersContext;
                private readonly string _currentPath;

                /// <summary>
                /// Initializes a new instance of the <see cref="DomainComponent"/> class.
                /// </summary>
                internal DomainComponent(ILoadersContext loadersContext, string parentPath)
                {
                    this._loadersContext = loadersContext;
                    this._currentPath = loadersContext.GetPathWithNode(parentPath, nameof(Domain));
                }

                /// <inheritdoc cref="ILoadingService.GetData{TData}(string)"/>
                internal string OpenNotificaties()
                    => GetDomainValue(this._loadersContext, this._currentPath, nameof(OpenNotificaties));
                
                /// <inheritdoc cref="ILoadingService.GetData{TData}(string)"/>
                internal string OpenZaak()
                    => GetDomainValue(this._loadersContext, this._currentPath, nameof(OpenZaak));
                    
                /// <inheritdoc cref="ILoadingService.GetData{TData}(string)"/>
                internal string OpenKlant()
                    => GetDomainValue(this._loadersContext, this._currentPath, nameof(OpenKlant));

                /// <inheritdoc cref="ILoadingService.GetData{TData}(string)"/>
                internal string Objecten()
                    => GetDomainValue(this._loadersContext, this._currentPath, nameof(Objecten));

                /// <inheritdoc cref="ILoadingService.GetData{TData}(string)"/>
                internal string ObjectTypen()
                    => GetDomainValue(this._loadersContext, this._currentPath, nameof(ObjectTypen));
            }

            /// <summary>
            /// The "TemplateIds" part of the settings.
            /// </summary>
            internal sealed record TemplateIdsComponent
            {
                /// <inheritdoc cref="SmsComponent"/>
                internal SmsComponent Sms { get; }

                /// <inheritdoc cref="EmailComponent"/>
                internal EmailComponent Email { get; }

                /// <summary>
                /// Initializes a new instance of the <see cref="TemplateIdsComponent"/> class.
                /// </summary>
                internal TemplateIdsComponent(ILoadersContext loadersContext, string parentPath)
                {
                    string currentPath = loadersContext.GetPathWithNode(parentPath, nameof(TemplateIds));

                    this.Sms = new SmsComponent(loadersContext, currentPath);
                    this.Email = new EmailComponent(loadersContext, currentPath);
                }

                /// <summary>
                /// The "Sms" part of the settings.
                /// </summary>
                internal sealed record SmsComponent
                {
                    private readonly ILoadersContext _loadersContext;
                    private readonly string _currentPath;

                    /// <summary>
                    /// Initializes a new instance of the <see cref="SmsComponent"/> class.
                    /// </summary>
                    internal SmsComponent(ILoadersContext loadersContext, string parentPath)
                    {
                        this._loadersContext = loadersContext;
                        this._currentPath = loadersContext.GetPathWithNode(parentPath, nameof(Sms));
                    }

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string)"/>
                    internal string ZaakCreate()
                        => GetTemplateIdValue(this._loadersContext, this._currentPath, nameof(ZaakCreate));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string)"/>
                    internal string ZaakUpdate()
                        => GetTemplateIdValue(this._loadersContext, this._currentPath, nameof(ZaakUpdate));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string)"/>
                    internal string ZaakClose()
                        => GetTemplateIdValue(this._loadersContext, this._currentPath, nameof(ZaakClose));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string)"/>
                    internal string DecisionMade()
                        => GetTemplateIdValue(this._loadersContext, this._currentPath, nameof(DecisionMade));
                }

                /// <summary>
                /// The "Email" part of the settings.
                /// </summary>
                internal sealed record EmailComponent
                {
                    private readonly ILoadersContext _loadersContext;
                    private readonly string _currentPath;

                    /// <summary>
                    /// Initializes a new instance of the <see cref="EmailComponent"/> class.
                    /// </summary>
                    internal EmailComponent(ILoadersContext loadersContext, string parentPath)
                    {
                        this._loadersContext = loadersContext;
                        this._currentPath = loadersContext.GetPathWithNode(parentPath, nameof(Email));
                    }

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string)"/>
                    internal string ZaakCreate()
                        => GetTemplateIdValue(this._loadersContext, this._currentPath, nameof(ZaakCreate));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string)"/>
                    internal string ZaakUpdate()
                        => GetTemplateIdValue(this._loadersContext, this._currentPath, nameof(ZaakUpdate));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string)"/>
                    internal string ZaakClose()
                        => GetTemplateIdValue(this._loadersContext, this._currentPath, nameof(ZaakClose));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string)"/>
                    internal string DecisionMade()
                        => GetTemplateIdValue(this._loadersContext, this._currentPath, nameof(DecisionMade));
                }
            }
        }
        #endregion

        #region Helper methods
        /// <summary>
        /// Initializes a specific type of settings component with its name and <see cref="ILoadingService"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException"/>
        private TComponent GetComponent<TComponent>(ref TComponent? component, LoaderTypes loaderType,
            string name)
            where TComponent : class
        {
            if (component == null)
            {
                // Initialize new loaders context
                ILoadersContext loaderContext = new LoadersContext(this._serviceProvider);

                // Set what type of loading service is going to be used
                loaderContext.SetLoader(loaderType);

                // Set value to reference
                component = (TComponent?)Activator.CreateInstance(typeof(TComponent), loaderContext, name)
                            ?? throw new InvalidOperationException(Resources.Configuration_ERROR_CannotInitializeSettings);
            }
            
            // Return reference
            return component;
        }

        /// <summary>
        /// Retrieves cached settings value.
        /// </summary>
        private static string GetValue(ILoadingService loadersContext, string currentPath, string nodeName, bool disableValidation = false)
        {
            string finalPath = loadersContext.GetPathWithNode(currentPath, nodeName);

            return disableValidation
                ? loadersContext.GetData<string>(finalPath)
                : loadersContext.GetData<string>(finalPath).NotEmpty(finalPath);
        }

        /// <summary>
        /// Retrieves cached settings value.
        /// </summary>
        private static TData GetValue<TData>(ILoadingService loadersContext, string currentPath, string nodeName, bool disableValidation = false)
        {
            string finalPath = loadersContext.GetPathWithNode(currentPath, nodeName);

            return disableValidation
                ? loadersContext.GetData<TData>(finalPath)
                : loadersContext.GetData<TData>(finalPath).NotEmpty(finalPath);
        }

        /// <summary>
        /// Retrieves cached settings value, ensuring it will be a domain (without http/s and API endpoint).
        /// </summary>
        private static string GetDomainValue(ILoadingService loadersContext, string currentPath, string nodeName)
        {
            return GetValue<string>(loadersContext, currentPath, nodeName)
                .WithoutHttp()
                .WithoutEndpoint();
        }

        /// <summary>
        /// Retrieves cached settings value, ensuring it will be a valid Template Id.
        /// </summary>
        private static string GetTemplateIdValue(ILoadingService loadersContext, string currentPath, string nodeName)
        {
            return GetValue<string>(loadersContext, currentPath, nodeName)
                .ValidTemplateId();
        }
        #endregion
    }
}