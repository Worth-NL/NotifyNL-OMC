// © 2023, Worth Systems.

using EventsHandler.Extensions;
using EventsHandler.Properties;
using EventsHandler.Services.Settings.Enums;
using EventsHandler.Services.Settings.Interfaces;
using EventsHandler.Services.Settings.Strategy.Interfaces;
using EventsHandler.Services.Settings.Strategy.Manager;
using JetBrains.Annotations;
using System.Collections.Concurrent;

namespace EventsHandler.Services.Settings.Configuration
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
    public sealed record WebApiConfiguration : IDisposable
    {
        #region Dictionaries (cached values)
        private static readonly ConcurrentDictionary<
            string /* Unique final path */,
            string /* Setting value */> s_cachedStrings = new();

        private static readonly ConcurrentDictionary<
            string /* Unique final path */,
            string[] /* Setting value */> s_cachedArrays = new();
        #endregion

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

                /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                internal ushort ConnectionLifetimeInSeconds()
                    => GetCachedValue<ushort>(this._loadersContext, this._currentPath, nameof(ConnectionLifetimeInSeconds));

                /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                internal ushort HttpRequestTimeoutInSeconds()
                    => GetCachedValue<ushort>(this._loadersContext, this._currentPath, nameof(HttpRequestTimeoutInSeconds));

                /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                internal ushort HttpRequestsSimultaneousNumber()
                    => GetCachedValue<ushort>(this._loadersContext, this._currentPath, nameof(HttpRequestsSimultaneousNumber));
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

                /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                internal bool IsAsymmetric()
                    => GetCachedValue<bool>(this._loadersContext, this._currentPath, nameof(IsAsymmetric));
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

                /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                internal byte OmcWorkflowVersion()
                    => GetCachedValue<byte>(this._loadersContext, this._currentPath, nameof(OmcWorkflowVersion));
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

                /// <inheritdoc cref="ObjectenComponent"/>
                internal ObjectenComponent Objecten { get; }

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
                    this.Objecten = new ObjectenComponent(loadersContext, this._currentPath);
                    this.Messages = new MessagesComponent(loadersContext, this._currentPath);
                }

                /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                internal string SubjectType()
                    => GetCachedValue(this._loadersContext, this._currentPath, "BetrokkeneType");

                /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                internal string InitiatorRole()
                    => GetCachedValue(this._loadersContext, this._currentPath, "OmschrijvingGeneriek");

                /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                internal string PartyIdentifier()
                    => GetCachedValue(this._loadersContext, this._currentPath, "PartijIdentificator");

                /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                internal string EmailGenericDescription()
                    => GetCachedValue(this._loadersContext, this._currentPath, "EmailOmschrijvingGeneriek");

                /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                internal string PhoneGenericDescription()
                    => GetCachedValue(this._loadersContext, this._currentPath, "TelefoonOmschrijvingGeneriek");

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

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    internal string CodeObjectType()
                        => GetCachedValue(this._loadersContext, this._currentPath, nameof(CodeObjectType));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    internal string CodeRegister()
                        => GetCachedValue(this._loadersContext, this._currentPath, nameof(CodeRegister));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    internal string CodeObjectTypeId()
                        => GetCachedValue(this._loadersContext, this._currentPath, nameof(CodeObjectTypeId));
                }

                /// <summary>
                /// The "Objecten" part of the settings.
                /// </summary>
                internal sealed class ObjectenComponent
                {
                    private readonly ILoadersContext _loadersContext;
                    private readonly string _currentPath;

                    /// <summary>
                    /// Initializes a new instance of the <see cref="ObjectenComponent"/> class.
                    /// </summary>
                    internal ObjectenComponent(ILoadersContext loadersContext, string parentPath)
                    {
                        this._loadersContext = loadersContext;
                        this._currentPath = loadersContext.GetPathWithNode(parentPath, nameof(Objecten));
                    }

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    internal Guid TaskTypeGuid()
                        => GetCachedValue<Guid>(this._loadersContext, this._currentPath, nameof(TaskTypeGuid));
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
                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    internal string SMS_Success_Subject()
                        => GetCachedValue(this._loadersContext, this._currentPath, nameof(SMS_Success_Subject));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    internal string SMS_Success_Body()
                        => GetCachedValue(this._loadersContext, this._currentPath, nameof(SMS_Success_Body));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    internal string SMS_Failure_Subject()
                        => GetCachedValue(this._loadersContext, this._currentPath, nameof(SMS_Failure_Subject));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    internal string SMS_Failure_Body()
                        => GetCachedValue(this._loadersContext, this._currentPath, nameof(SMS_Failure_Body));
                    #endregion

                    #region E-mail
                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    internal string Email_Success_Subject()
                        => GetCachedValue(this._loadersContext, this._currentPath, nameof(Email_Success_Subject));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    internal string Email_Success_Body()
                        => GetCachedValue(this._loadersContext, this._currentPath, nameof(Email_Success_Body));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    internal string Email_Failure_Subject()
                        => GetCachedValue(this._loadersContext, this._currentPath, nameof(Email_Failure_Subject));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    internal string Email_Failure_Body()
                        => GetCachedValue(this._loadersContext, this._currentPath, nameof(Email_Failure_Body));
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

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    internal string Secret()
                        => GetCachedValue(this._loadersContext, this._currentPath, nameof(Secret));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    internal string Issuer()
                        => GetCachedValue(this._loadersContext, this._currentPath, nameof(Issuer));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    internal string Audience()
                        => GetCachedValue(this._loadersContext, this._currentPath, nameof(Audience), disableValidation: true);

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    internal ushort ExpiresInMin()
                        => GetCachedValue<ushort>(this._loadersContext, this._currentPath, nameof(ExpiresInMin));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    internal string UserId()
                        => GetCachedValue(this._loadersContext, this._currentPath, nameof(UserId));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    internal string UserName()
                        => GetCachedValue(this._loadersContext, this._currentPath, nameof(UserName));
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

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    internal Uri NotifyNL()
                        => GetCachedUri(this._loadersContext, this._currentPath, nameof(NotifyNL));
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

            /// <inheritdoc cref="WhitelistComponent"/>
            internal WhitelistComponent Whitelist { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="UserComponent"/> class.
            /// </summary>
            public UserComponent(ILoadersContext loadersContext, string parentName)
                : base(loadersContext, parentName)
            {
                this.API = new ApiComponent(loadersContext, parentName);
                this.Domain = new DomainComponent(loadersContext, parentName);
                this.TemplateIds = new TemplateIdsComponent(loadersContext, parentName);
                this.Whitelist = new WhitelistComponent(loadersContext, parentName);
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

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    internal string OpenKlant_2()
                        => GetCachedValue(this._loadersContext, this._currentPath, nameof(OpenKlant_2));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    internal string Objecten()
                        => GetCachedValue(this._loadersContext, this._currentPath, nameof(Objecten));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    internal string ObjectTypen()
                        => GetCachedValue(this._loadersContext, this._currentPath, nameof(ObjectTypen));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    internal string NotifyNL()
                        => GetCachedValue(this._loadersContext, this._currentPath, nameof(NotifyNL));
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

                /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                internal string OpenNotificaties()
                    => GetCachedDomainValue(this._loadersContext, this._currentPath, nameof(OpenNotificaties));

                /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                internal string OpenZaak()
                    => GetCachedDomainValue(this._loadersContext, this._currentPath, nameof(OpenZaak));

                /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                internal string OpenKlant()
                    => GetCachedDomainValue(this._loadersContext, this._currentPath, nameof(OpenKlant));

                /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                internal string Objecten()
                    => GetCachedDomainValue(this._loadersContext, this._currentPath, nameof(Objecten));

                /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                internal string ObjectTypen()
                    => GetCachedDomainValue(this._loadersContext, this._currentPath, nameof(ObjectTypen));
            }

            /// <summary>
            /// The "TemplateIds" part of the settings.
            /// </summary>
            internal sealed record TemplateIdsComponent
            {
                /// <inheritdoc cref="EmailComponent"/>
                internal EmailComponent Email { get; }

                /// <inheritdoc cref="SmsComponent"/>
                internal SmsComponent Sms { get; }

                /// <summary>
                /// Initializes a new instance of the <see cref="TemplateIdsComponent"/> class.
                /// </summary>
                internal TemplateIdsComponent(ILoadersContext loadersContext, string parentPath)
                {
                    string currentPath = loadersContext.GetPathWithNode(parentPath, nameof(TemplateIds));

                    this.Email = new EmailComponent(loadersContext, currentPath);
                    this.Sms = new SmsComponent(loadersContext, currentPath);
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

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    internal string ZaakCreate()
                        => GetCachedTemplateIdValue(this._loadersContext, this._currentPath, nameof(ZaakCreate));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    internal string ZaakUpdate()
                        => GetCachedTemplateIdValue(this._loadersContext, this._currentPath, nameof(ZaakUpdate));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    internal string ZaakClose()
                        => GetCachedTemplateIdValue(this._loadersContext, this._currentPath, nameof(ZaakClose));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    internal string TaskAssigned()
                        => GetCachedTemplateIdValue(this._loadersContext, this._currentPath, nameof(TaskAssigned));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    internal string DecisionMade()
                        => GetCachedTemplateIdValue(this._loadersContext, this._currentPath, nameof(DecisionMade));
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

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    internal string ZaakCreate()
                        => GetCachedTemplateIdValue(this._loadersContext, this._currentPath, nameof(ZaakCreate));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    internal string ZaakUpdate()
                        => GetCachedTemplateIdValue(this._loadersContext, this._currentPath, nameof(ZaakUpdate));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    internal string ZaakClose()
                        => GetCachedTemplateIdValue(this._loadersContext, this._currentPath, nameof(ZaakClose));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    internal string TaskAssigned()
                        => GetCachedTemplateIdValue(this._loadersContext, this._currentPath, nameof(TaskAssigned));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    internal string DecisionMade()
                        => GetCachedTemplateIdValue(this._loadersContext, this._currentPath, nameof(DecisionMade));
                }
            }

            /// <summary>
            /// The "Whitelist" part of the settings.
            /// </summary>
            internal sealed record WhitelistComponent : IDisposable
            {
                private static readonly object s_whitelistLock = new();
                private static readonly HashSet<string> s_allWhitelistedCaseIds = new();  // All whitelisted Case IDs from different scenarios
                private static readonly ConcurrentDictionary<string /* Node name */, IDs> s_cachedIDs = new();  // Cached IDs nested models

                private readonly ILoadersContext _loadersContext;
                private readonly string _currentPath;

                /// <summary>
                /// Initializes a new instance of the <see cref="WhitelistComponent"/> class.
                /// </summary>
                internal WhitelistComponent(ILoadersContext loadersContext, string parentPath)
                {
                    this._loadersContext = loadersContext;
                    this._currentPath = loadersContext.GetPathWithNode(parentPath, nameof(Whitelist));
                }

                /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                internal IDs ZaakCreate_IDs()
                    => GetIDs(this._loadersContext, this._currentPath, nameof(ZaakCreate_IDs));

                /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                internal IDs ZaakUpdate_IDs()
                    => GetIDs(this._loadersContext, this._currentPath, nameof(ZaakUpdate_IDs));

                /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                internal IDs ZaakClose_IDs()
                    => GetIDs(this._loadersContext, this._currentPath, nameof(ZaakClose_IDs));

                /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                internal IDs TaskAssigned_IDs()
                    => GetIDs(this._loadersContext, this._currentPath, nameof(TaskAssigned_IDs));

                /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                internal IDs DecisionMade_IDs()
                    => GetIDs(this._loadersContext, this._currentPath, nameof(DecisionMade_IDs));

                /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                internal bool Message_Allowed()
                    => GetCachedValue<bool>(this._loadersContext, this._currentPath, nameof(Message_Allowed));

                /// <summary>
                /// Returns cached <see cref="IDs"/> or creates a new one.
                /// </summary>
                private static IDs GetIDs(ILoadingService loadersContext, string currentPath, string nodeName)
                {
                    return s_cachedIDs.GetOrAdd(
                        key: nodeName,
                        value: new IDs(loadersContext, currentPath, nodeName));
                }

                // ReSharper disable once InconsistentNaming
                /// <summary>
                /// A helper class encapsulating vulnerable hashed IDs and common operations on them.
                /// </summary>
                internal sealed record IDs
                {
                    private readonly string _finalPath;

                    /// <summary>
                    /// Gets the count of the whitelisted IDs.
                    /// </summary>
                    /// <remarks>
                    /// NOTE: To keep the data integrity do not expose the cached whitelist IDs directly.
                    /// </remarks>
                    internal int Count { get; }

                    /// <summary>
                    /// Initializes a new instance of the <see cref="IDs"/> class.
                    /// </summary>
                    internal IDs(ILoadingService loadersContext, string currentPath, string nodeName)
                    {
                        // Get values
                        this._finalPath = loadersContext.GetPathWithNode(currentPath, nodeName);
                        string[] caseIds = GetCachedValues(loadersContext, this._finalPath, disableValidation: true);

                        // Construct the IDs object
                        this.Count = caseIds.Length;

                        if (this.Count == 0)
                        {
                            return;
                        }

                        // Cache the current IDs among all scenario-specific IDs
                        lock (s_whitelistLock)
                        {
                            foreach (string value in caseIds)
                            {
                                s_allWhitelistedCaseIds.Add(ComposeID(this._finalPath, value));
                            }
                        }
                    }

                    /// <summary>
                    /// Determines whether the specified case identifier is whitelisted.
                    /// </summary>
                    internal bool IsAllowed(string caseId)
                    {
                        lock (s_whitelistLock)
                        {
                            return !string.IsNullOrWhiteSpace(caseId) &&
                                   s_allWhitelistedCaseIds.Count != 0 &&
                                   s_allWhitelistedCaseIds.Contains(ComposeID(this._finalPath, caseId));
                        }
                    }

                    private static string ComposeID(string finalPath, string caseId)
                    {
                        return $"{finalPath}:{caseId}";
                    }

                    public override string ToString() => this._finalPath;
                }

                /// <inheritdoc cref="IDisposable.Dispose()"/>
                public void Dispose()
                {
                    lock (s_whitelistLock)
                    {
                        s_allWhitelistedCaseIds.Clear();
                    }

                    s_cachedIDs.Clear();
                }
            }
        }
        #endregion

        #region Caching
        /// <summary>
        /// Retrieves cached <see langword="string"/> value (with optional validation).
        /// </summary>
        /// <remarks>
        /// A shortcut to not use GetValue&lt;<see langword="string"/>&gt; method invocation for the most common settings value type.
        /// <para>
        /// Validation: optional
        /// </para>
        /// </remarks>
        private static string GetCachedValue(ILoadingService loadersContext, string currentPath, string nodeName, bool disableValidation = false)
        {
            // NOTE: Shorthand to not use the most popular <string> type in most cases
            return s_cachedStrings.GetOrAdd(
                currentPath + nodeName,
                // Validation happens once during initial loading, before caching the value
                GetValue<string>(loadersContext, currentPath, nodeName, disableValidation));  // Validate not empty (if validation is enabled)
        }

        /// <summary>
        /// Retrieves cached domain value (without http/s and API endpoint).
        /// </summary>
        /// <remarks>
        /// Validation: enabled
        /// </remarks>
        private static string GetCachedDomainValue(ILoadingService loadersContext, string currentPath, string nodeName)
        {
            return s_cachedStrings.GetOrAdd(
                currentPath + nodeName,
                // Validation happens once during initial loading, before caching the value
                GetValue<string>(loadersContext, currentPath, nodeName, disableValidation: false)  // Validate not empty (if validation is enabled)
                    .ValidateNoHttp()
                    .ValidateNoEndpoint());
        }

        /// <summary>
        /// Retrieves cached template ID value (in correct UUID format).
        /// </summary>
        /// <remarks>
        /// Validation: enabled
        /// </remarks>
        private static string GetCachedTemplateIdValue(ILoadingService loadersContext, string currentPath, string nodeName)
        {
            return s_cachedStrings.GetOrAdd(
                currentPath + nodeName,
                // Validation happens once during initial loading, before caching the value
                GetValue<string>(loadersContext, currentPath, nodeName, disableValidation: false)  // Validate not empty (if validation is enabled)
                    .ValidateTemplateId());
        }
        
        /// <summary>
        /// Retrieves cached multiple <see langword="string"/> values.
        /// </summary>
        /// <remarks>
        /// Validation: optional
        /// </remarks>
        private static string[] GetCachedValues(ILoadingService loadersContext, string finalPath, bool disableValidation = false)
        {
            return s_cachedArrays.GetOrAdd(
                finalPath,
                // Validation happens once during initial loading, before caching the value
                _ =>
                {
                    // Validation #1: Checking if the string value is not null or empty
                    string[] values = GetValue<string>(loadersContext, finalPath, disableValidation: true)  // Allow empty values
                        .Split(",", StringSplitOptions.RemoveEmptyEntries)  // Handles the case: "1,2,3"
                        .Select(value => value.TrimStart())  // Handles the case: "1, 2, 3"
                        .ToArray();

                    // Validation #2: Checking if the comma-separated string was properly split into array
                    return disableValidation
                        ? values
                        : values.ValidateNotEmpty(finalPath);  // Handles the case: "," => RemoveEmptyEntries => { }
                });
        }
        
        /// <summary>
        /// Retrieves cached <see cref="Uri"/> value (in correct format).
        /// </summary>
        /// <remarks>
        /// Validation: enabled
        /// </remarks>
        private static Uri GetCachedUri(ILoadingService loadersContext, string currentPath, string nodeName)
        {
            return new Uri(s_cachedStrings.GetOrAdd(
                currentPath + nodeName,
                // Validation happens once during initial loading, before caching the value
                $"{GetValue<Uri>(loadersContext, currentPath, nodeName, disableValidation: false)  // Validate not empty (if validation is enabled)
                    .ValidateUri()}"));
        }
        
        /// <summary>
        /// Retrieves cached <typeparamref name="TData"/> value.
        /// </summary>
        /// <remarks>
        /// Validation: optional
        /// </remarks>
        private static TData GetCachedValue<TData>(ILoadingService loadersContext, string currentPath, string nodeName, bool disableValidation = false)
            where TData : notnull
        {
            string value = s_cachedStrings.GetOrAdd(
                currentPath + nodeName,
                // Validation happens once during initial loading, before caching the value
                // Save as string (to not maintain type-specific or <string, object> dictionary requiring unboxing overhead, since most values are strings)
                $"{GetValue<TData>(loadersContext, currentPath, nodeName, disableValidation)}");  // Validate not empty (if validation is enabled)

            return value.ChangeType<TData>();
        }
        #endregion

        #region Reading
        /// <summary>
        /// Retrieves settings <typeparamref name="TData"/> value (with optional validation).
        /// </summary>
        private static TData GetValue<TData>(ILoadingService loadersContext, string currentPath, string nodeName, bool disableValidation)
            where TData : notnull
        {
            // NOTE: Shorthand to combine the settings path and node name in one place
            string finalPath = loadersContext.GetPathWithNode(currentPath, nodeName);

            return GetValue<TData>(loadersContext, finalPath, disableValidation);  // Validate not empty (if validation is enabled)
        }

        /// <summary>
        /// Retrieves settings <typeparamref name="TData"/> value using final path
        /// instead of path + node concatenation (with optional validation).
        /// </summary>
        private static TData GetValue<TData>(ILoadingService loadersContext, string finalPath, bool disableValidation)
            where TData : notnull
        {
            return loadersContext.GetData<TData>(finalPath, disableValidation);
        }
        #endregion

        /// <inheritdoc cref="IDisposable.Dispose()"/>
        public void Dispose()
        {
            s_cachedStrings.Clear();
            s_cachedArrays.Clear();
            this.User.Whitelist.Dispose();
        }
    }
}