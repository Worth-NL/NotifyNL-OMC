// © 2023, Worth Systems.

using Common.Extensions;
using Common.Settings.Attributes;
using Common.Settings.Enums;
using Common.Settings.Extensions;
using Common.Settings.Interfaces;
using Common.Settings.Strategy.Interfaces;
using Common.Settings.Strategy.Manager;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;
using System.Reflection;

namespace Common.Settings.Configuration
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
    public sealed record OmcConfiguration : IDisposable
    {
        #region Dictionaries (cached values)
        private static readonly ConcurrentDictionary<
            string /* Unique final path */,
            string /* Setting value */> s_cachedStrings = new();

        private static readonly ConcurrentDictionary<
            string /* Unique final path */,
            Guid /* Setting value */> s_cachedGuids = new();

        private static readonly ConcurrentDictionary<
            string /* Unique final path */,
            HashSet<Guid> /* Setting value */> s_cachedArrayGuids = new();

        private static readonly ConcurrentDictionary<
            string /* Unique final path */,
            Uri /* Setting value */> s_cachedUris = new();

        private static readonly ConcurrentDictionary<
            string /* Unique final path */,
            string[] /* Setting value */> s_cachedArrays = new();
        #endregion

        #region Properties (root components)
        /// <summary>
        /// Gets the object representing "appsettings[.xxx].json" configuration file with predefined flags and variables.
        /// </summary>
        [Config]
        public AppSettingsComponent AppSettings { get; }

        /// <summary>
        /// Gets the settings used by public service OMC (Output Management Component).
        /// </summary>
        [Config]
        public OmcComponent OMC { get; }

        /// <summary>
        /// Gets the settings used by external services that belongs to the group of:
        /// ZGW (Zaakgericht Werken) / "Open Services" - such as OpenNotificatie, OpenZaak, OpenKlant, etc...
        /// </summary>
        [Config]
        public ZgwComponent ZGW { get; }

        /// <summary>
        /// Gets the settings used by external service Notify NL.
        /// </summary>
        [Config]
        public NotifyComponent Notify { get; }
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="OmcConfiguration"/> class.
        /// </summary>
        public OmcConfiguration(IServiceProvider serviceProvider)  // NOTE: The only constructor to be used with Dependency Injection
        {
            // Recreate the structure of settings from "appsettings.json" configuration file or from Environment Variables
            this.AppSettings = new AppSettingsComponent(serviceProvider, nameof(AppSettings));
            this.OMC = new OmcComponent(serviceProvider, nameof(OMC));
            this.ZGW = new ZgwComponent(serviceProvider, nameof(ZGW), this);
            this.Notify = new NotifyComponent(serviceProvider, nameof(Notify));
        }

        #region AppSettings.json
        /// <summary>
        /// The wrapper responsible for handling "fallback" scenarios (switching between different <see cref="ILoadersContext"/>s if necessary).
        /// </summary>
        public struct FallbackContextWrapper
        {
            /// <summary>
            /// The primary <see cref="ILoadersContext"/> (used to load configurations with preferred loading strategy).
            /// </summary>
            public ILoadersContext PrimaryLoaderContext { get; }
            
            /// <summary>
            /// The fallback <see cref="ILoadersContext"/> (used to load configurations when the first context one fails).
            /// </summary>
            public ILoadersContext FallbackLoaderContext { get; }

            /// <summary>
            /// The primary configuration (key) path (to read the configuration value when the primary <see cref="ILoadersContext"/> strategy is used).
            /// </summary>
            public string PrimaryCurrentPath { get; private set; }

            /// <summary>
            /// The fallback configuration (key) path (to read the configuration value when the fallback <see cref="ILoadersContext"/> strategy is used).
            /// </summary>
            public string FallbackCurrentPath { get; private set; }
            
            /// <summary>
            /// Initializes a new instance of the <see cref="FallbackContextWrapper"/> struct.
            /// </summary>
            public FallbackContextWrapper(ILoadersContext firstLoaderContext, ILoadersContext secondLoaderContext, string parentPath, string currentNode)
            {
                this.PrimaryLoaderContext = firstLoaderContext;
                this.FallbackLoaderContext = secondLoaderContext;

                this.PrimaryCurrentPath = this.PrimaryLoaderContext.GetPathWithNode(parentPath, currentNode);
                this.FallbackCurrentPath = this.FallbackLoaderContext.GetPathWithNode(parentPath, currentNode);
            }

            /// <summary>
            /// Updates both paths using the given node.
            /// </summary>
            public FallbackContextWrapper Update(string currentNode)
            {
                this.PrimaryCurrentPath = this.PrimaryLoaderContext.GetPathWithNode(this.PrimaryCurrentPath, currentNode);
                this.FallbackCurrentPath = this.FallbackLoaderContext.GetPathWithNode(this.FallbackCurrentPath, currentNode);

                return this;
            }
        }

        /// <summary>
        /// The "appsettings[.xxx].json" part of the settings (not changing frequently).
        /// </summary>
        [UsedImplicitly]
        public sealed record AppSettingsComponent
        {
            // NOTE: Property "Logging" (from "appsettings.json") is skipped because it's not used anywhere in the code

            /// <inheritdoc cref="NetworkComponent"/>
            [Config]
            public NetworkComponent Network { get; }

            /// <inheritdoc cref="EncryptionComponent"/>
            [Config]
            public EncryptionComponent Encryption { get; }

            /// <inheritdoc cref="VariablesComponent"/>
            [Config]
            public VariablesComponent Variables { get; }

            // NOTE: Property "AllowedHosts" (from "appsettings.json") is skipped because it's not used anywhere in the code

            /// <summary>
            /// Initializes a new instance of the <see cref="AppSettingsComponent"/> class.
            /// </summary>
            public AppSettingsComponent(IServiceProvider serviceProvider, string parentName)
            {
                ILoadersContext firstLoadersContext = GetLoader(serviceProvider, LoaderTypes.Environment);  // NOTE: Prefer Environment Variables first to make "overriding" AppSettings possible
                ILoadersContext secondLoadersContext = GetLoader(serviceProvider, LoaderTypes.AppSettings);

                this.Network = new NetworkComponent(new FallbackContextWrapper(firstLoadersContext, secondLoadersContext, parentName, nameof(Network)));
                this.Encryption = new EncryptionComponent(new FallbackContextWrapper(firstLoadersContext, secondLoadersContext, parentName, nameof(Encryption)));
                this.Variables = new VariablesComponent(new FallbackContextWrapper(firstLoadersContext, secondLoadersContext, parentName, nameof(Variables)));
            }

            /// <summary>
            /// The "Network" part of the settings.
            /// </summary>
            public sealed record NetworkComponent
            {
                private readonly FallbackContextWrapper _fallbackContextWrapper;

                /// <summary>
                /// Initializes a new instance of the <see cref="NetworkComponent"/> class.
                /// </summary>
                public NetworkComponent(FallbackContextWrapper contextWrapper)
                {
                    this._fallbackContextWrapper = contextWrapper;
                }

                // TODO: paths should be also path of some object, to handle them easier and more consistently
                /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                [Config]
                public ushort ConnectionLifetimeInSeconds()
                    => GetCachedValue<ushort>(this._fallbackContextWrapper, nameof(ConnectionLifetimeInSeconds));

                /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                [Config]
                public ushort HttpRequestTimeoutInSeconds()
                    => GetCachedValue<ushort>(this._fallbackContextWrapper, nameof(HttpRequestTimeoutInSeconds));

                /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                [Config]
                public ushort HttpRequestsSimultaneousNumber()
                    => GetCachedValue<ushort>(this._fallbackContextWrapper, nameof(HttpRequestsSimultaneousNumber));
            }

            /// <summary>
            /// The "Encryption" part of the settings.
            /// </summary>
            public sealed record EncryptionComponent
            {
                private readonly FallbackContextWrapper _fallbackContextWrapper;

                /// <summary>
                /// Initializes a new instance of the <see cref="EncryptionComponent"/> class.
                /// </summary>
                public EncryptionComponent(FallbackContextWrapper contextWrapper)
                {
                    this._fallbackContextWrapper = contextWrapper;
                }

                /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                [Config]
                public bool IsAsymmetric()
                    => GetCachedValue<bool>(this._fallbackContextWrapper, nameof(IsAsymmetric));
            }

            /// <summary>
            /// The "Variables" part of the settings.
            /// </summary>
            public sealed record VariablesComponent
            {
                private readonly FallbackContextWrapper _fallbackContextWrapper;

                /// <inheritdoc cref="OpenKlantComponent"/>
                [Config]
                public OpenKlantComponent OpenKlant { get; }

                /// <inheritdoc cref="UxMessagesComponent"/>
                [Config]
                public UxMessagesComponent UxMessages { get; }

                /// <summary>
                /// Initializes a new instance of the <see cref="VariablesComponent"/> class.
                /// </summary>
                public VariablesComponent(FallbackContextWrapper contextWrapper)
                {
                    this._fallbackContextWrapper = contextWrapper;

                    this.OpenKlant = new OpenKlantComponent(this._fallbackContextWrapper);
                    this.UxMessages = new UxMessagesComponent(this._fallbackContextWrapper);
                }

                /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                [Config]
                public string SubjectType()
                    => GetCachedValue(this._fallbackContextWrapper, "BetrokkeneType");

                /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                [Config]
                public string InitiatorRole()
                    => GetCachedValue(this._fallbackContextWrapper, "OmschrijvingGeneriek");

                /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                [Config]
                public string PartyIdentifier()
                    => GetCachedValue(this._fallbackContextWrapper, "PartijIdentificator");

                /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                [Config]
                public string EmailGenericDescription()
                    => GetCachedValue(this._fallbackContextWrapper, "EmailOmschrijvingGeneriek");

                /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                [Config]
                public string PhoneGenericDescription()
                    => GetCachedValue(this._fallbackContextWrapper, "TelefoonOmschrijvingGeneriek");

                /// <summary>
                /// The "OpenKlant" part of the settings.
                /// </summary>
                public sealed class OpenKlantComponent
                {
                    private readonly FallbackContextWrapper _fallbackContextWrapper;

                    /// <summary>
                    /// Initializes a new instance of the <see cref="OpenKlantComponent"/> class.
                    /// </summary>
                    public OpenKlantComponent(FallbackContextWrapper contextWrapper)
                    {
                        this._fallbackContextWrapper = contextWrapper.Update(nameof(OpenKlant));
                    }

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    [Config]
                    public string CodeObjectType()
                        => GetCachedValue(this._fallbackContextWrapper, nameof(CodeObjectType));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    [Config]
                    public string CodeRegister()
                        => GetCachedValue(this._fallbackContextWrapper, nameof(CodeRegister));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    [Config]
                    public string CodeObjectTypeId()
                        => GetCachedValue(this._fallbackContextWrapper, nameof(CodeObjectTypeId));
                }

                /// <summary>
                /// The "UX Messages" part of the settings.
                /// </summary>
                public sealed class UxMessagesComponent
                {
                    private readonly FallbackContextWrapper _fallbackContextWrapper;

                    /// <summary>
                    /// Initializes a new instance of the <see cref="UxMessagesComponent"/> class.
                    /// </summary>
                    public UxMessagesComponent(FallbackContextWrapper contextWrapper)
                    {
                        this._fallbackContextWrapper = contextWrapper.Update(nameof(UxMessages));
                    }

                    #region SMS
                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    [Config]
                    public string SMS_Success_Subject()
                        => GetCachedValue(this._fallbackContextWrapper, nameof(SMS_Success_Subject));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    [Config]
                    public string SMS_Success_Body()
                        => GetCachedValue(this._fallbackContextWrapper, nameof(SMS_Success_Body));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    [Config]
                    public string SMS_Failure_Subject()
                        => GetCachedValue(this._fallbackContextWrapper, nameof(SMS_Failure_Subject));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    [Config]
                    public string SMS_Failure_Body()
                        => GetCachedValue(this._fallbackContextWrapper, nameof(SMS_Failure_Body));
                    #endregion

                    #region E-mail
                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    [Config]
                    public string Email_Success_Subject()
                        => GetCachedValue(this._fallbackContextWrapper, nameof(Email_Success_Subject));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    [Config]
                    public string Email_Success_Body()
                        => GetCachedValue(this._fallbackContextWrapper, nameof(Email_Success_Body));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    [Config]
                    public string Email_Failure_Subject()
                        => GetCachedValue(this._fallbackContextWrapper, nameof(Email_Failure_Subject));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    [Config]
                    public string Email_Failure_Body()
                        => GetCachedValue(this._fallbackContextWrapper, nameof(Email_Failure_Body));
                    #endregion
                }
            }
        }
        #endregion

        #region Environment Variables
        // NOTE: Environment variable "ASPNETCORE_ENVIRONMENT" is skipped because it is optional one and not used by the business logic

        /// <summary>
        /// The "OMC" part of the settings.
        /// </summary>
        [UsedImplicitly]
        public sealed record OmcComponent
        {
            /// <inheritdoc cref="AuthenticationComponent"/>
            [Config]
            public AuthenticationComponent Auth { get; }

            /// <inheritdoc cref="FeatureComponent"/>
            [Config]
            public FeatureComponent Feature { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="OmcComponent"/> class.
            /// </summary>
            public OmcComponent(IServiceProvider serviceProvider, string parentName)
            {
                ILoadersContext loadersContext = GetLoader(serviceProvider, LoaderTypes.Environment);

                this.Auth = new AuthenticationComponent(loadersContext, parentName);
                this.Feature = new FeatureComponent(loadersContext, parentName);
            }

            /// <summary>
            /// The "Authentication" part of the settings.
            /// </summary>
            public sealed record AuthenticationComponent
            {
                /// <inheritdoc cref="JwtComponent"/>
                [Config]
                public JwtComponent JWT { get; }

                /// <summary>
                /// Initializes a new instance of the <see cref="AuthenticationComponent"/> class.
                /// </summary>
                public AuthenticationComponent(ILoadersContext loadersContext, string parentPath)
                {
                    string currentPath = loadersContext.GetPathWithNode(parentPath, nameof(Auth));

                    this.JWT = new JwtComponent(loadersContext, currentPath);
                }

                /// <summary>
                /// The "JWT" part of the settings.
                /// </summary>
                public sealed record JwtComponent
                {
                    private readonly ILoadersContext _loadersContext;
                    private readonly string _currentPath;

                    /// <summary>
                    /// Initializes a new instance of the <see cref="JwtComponent"/> class.
                    /// </summary>
                    public JwtComponent(ILoadersContext loadersContext, string parentPath)
                    {
                        this._loadersContext = loadersContext;
                        this._currentPath = loadersContext.GetPathWithNode(parentPath, nameof(JWT));
                    }

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    [Config]
                    public string Secret()
                        => GetCachedValue(this._loadersContext, this._currentPath, nameof(Secret));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    [Config]
                    public string Issuer()
                        => GetCachedValue(this._loadersContext, this._currentPath, nameof(Issuer));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    [Config]
                    public string Audience()
                        => GetCachedValue(this._loadersContext, this._currentPath, nameof(Audience), disableValidation: true);

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    [Config, UsedImplicitly]
                    public ushort ExpiresInMin()
                        => GetCachedValue<ushort>(this._loadersContext, this._currentPath, nameof(ExpiresInMin));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    [Config, UsedImplicitly]
                    public string UserId()
                        => GetCachedValue(this._loadersContext, this._currentPath, nameof(UserId));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    [Config, UsedImplicitly]
                    public string UserName()
                        => GetCachedValue(this._loadersContext, this._currentPath, nameof(UserName));
                }
            }

            /// <summary>
            /// The "Feature" part of the settings.
            /// </summary>
            public sealed record FeatureComponent
            {
                private readonly ILoadersContext _loadersContext;
                private readonly string _currentPath;

                /// <summary>
                /// Initializes a new instance of the <see cref="FeatureComponent"/> class.
                /// </summary>
                public FeatureComponent(ILoadersContext loadersContext, string parentPath)
                {
                    this._loadersContext = loadersContext;
                    this._currentPath = loadersContext.GetPathWithNode(parentPath, nameof(Feature));
                }

                /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                [Config]
                public byte Workflow_Version()
                    => GetCachedValue<byte>(this._loadersContext, this._currentPath, nameof(Workflow_Version));
            }
        }

        /// <summary>
        /// The "ZGW" part of the settings.
        /// </summary>
        [UsedImplicitly]
        public sealed record ZgwComponent
        {
            /// <inheritdoc cref="AuthenticationComponent"/>
            [Config]
            public AuthenticationComponent Auth { get; }

            /// <inheritdoc cref="EndpointComponent"/>
            [Config]
            public EndpointComponent Endpoint { get; }

            /// <inheritdoc cref="WhitelistComponent"/>
            [Config]
            public WhitelistComponent Whitelist { get; }

            /// <inheritdoc cref="VariableComponent"/>
            [Config]
            public VariableComponent Variable { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="ZgwComponent"/> class.
            /// </summary>
            public ZgwComponent(IServiceProvider serviceProvider, string parentName, OmcConfiguration configuration)
            {
                ILoadersContext loadersContext = GetLoader(serviceProvider, LoaderTypes.Environment);

                this.Auth = new AuthenticationComponent(loadersContext, parentName, configuration);
                this.Endpoint = new EndpointComponent(loadersContext, parentName);
                this.Whitelist = new WhitelistComponent(loadersContext, parentName);
                this.Variable = new VariableComponent(loadersContext, parentName);
            }

            /// <summary>
            /// The "Authentication" part of the settings.
            /// </summary>
            public sealed record AuthenticationComponent
            {
                /// <inheritdoc cref="JwtComponent"/>
                [Config]
                public JwtComponent JWT { get; }

                /// <inheritdoc cref="KeyComponent"/>
                [Config]
                public KeyComponent Key { get; }

                /// <summary>
                /// Initializes a new instance of the <see cref="AuthenticationComponent"/> class.
                /// </summary>
                public AuthenticationComponent(ILoadersContext loadersContext, string parentPath, OmcConfiguration configuration)
                {
                    string currentPath = loadersContext.GetPathWithNode(parentPath, nameof(Auth));

                    this.JWT = new JwtComponent(loadersContext, currentPath);
                    this.Key = new KeyComponent(loadersContext, currentPath, configuration);
                }

                /// <summary>
                /// The "JWT" part of the settings.
                /// </summary>
                public sealed record JwtComponent
                {
                    private readonly ILoadersContext _loadersContext;
                    private readonly string _currentPath;

                    /// <summary>
                    /// Initializes a new instance of the <see cref="JwtComponent"/> class.
                    /// </summary>
                    public JwtComponent(ILoadersContext loadersContext, string parentPath)
                    {
                        this._loadersContext = loadersContext;
                        this._currentPath = loadersContext.GetPathWithNode(parentPath, nameof(JWT));
                    }

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    [Config]
                    public string Secret()
                        => GetCachedValue(this._loadersContext, this._currentPath, nameof(Secret));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    [Config]
                    public string Issuer()
                        => GetCachedValue(this._loadersContext, this._currentPath, nameof(Issuer));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    [Config]
                    public string Audience()
                        => GetCachedValue(this._loadersContext, this._currentPath, nameof(Audience), disableValidation: true);

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    [Config]
                    public ushort ExpiresInMin()
                        => GetCachedValue<ushort>(this._loadersContext, this._currentPath, nameof(ExpiresInMin));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    [Config]
                    public string UserId()
                        => GetCachedValue(this._loadersContext, this._currentPath, nameof(UserId));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    [Config]
                    public string UserName()
                        => GetCachedValue(this._loadersContext, this._currentPath, nameof(UserName));
                }

                /// <summary>
                /// The "Key" part of the settings.
                /// </summary>
                public sealed record KeyComponent
                {
                    private readonly ILoadersContext _loadersContext;
                    private readonly string _currentPath;
                    private readonly OmcConfiguration _configuration;

                    /// <summary>
                    /// Initializes a new instance of the <see cref="KeyComponent"/> class.
                    /// </summary>
                    public KeyComponent(ILoadersContext loadersContext, string parentPath, OmcConfiguration configuration)
                    {
                        this._loadersContext = loadersContext;
                        this._currentPath = loadersContext.GetPathWithNode(parentPath, nameof(Key));
                        this._configuration = configuration;
                    }

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    [Config]
                    public string OpenKlant()
                        => GetCachedValue(this._loadersContext, this._currentPath, nameof(OpenKlant),
                           disableValidation: this._configuration.OMC.Feature.Workflow_Version() == 1);  // NOTE: OMC Workflow v1 is not using API Key for OpenKlant

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    [Config]
                    public string Objecten()
                        => GetCachedValue(this._loadersContext, this._currentPath, nameof(Objecten));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    [Config]
                    public string ObjectTypen()
                        => GetCachedValue(this._loadersContext, this._currentPath, nameof(ObjectTypen));
                }
            }

            /// <summary>
            /// The "Endpoint" part of the settings.
            /// </summary>
            public sealed record EndpointComponent
            {
                private readonly ILoadersContext _loadersContext;
                private readonly string _currentPath;

                /// <summary>
                /// Initializes a new instance of the <see cref="EndpointComponent"/> class.
                /// </summary>
                public EndpointComponent(ILoadersContext loadersContext, string parentPath)
                {
                    this._loadersContext = loadersContext;
                    this._currentPath = loadersContext.GetPathWithNode(parentPath, nameof(Endpoint));
                }

                /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                [Config]
                public string OpenNotificaties()
                    => GetCachedEndpointValue(this._loadersContext, this._currentPath, nameof(OpenNotificaties));

                /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                [Config]
                public string OpenZaak()
                    => GetCachedEndpointValue(this._loadersContext, this._currentPath, nameof(OpenZaak));

                /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                [Config]
                public string OpenKlant()
                    => GetCachedEndpointValue(this._loadersContext, this._currentPath, nameof(OpenKlant));

                /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                [Config]
                public string Besluiten()
                    => GetCachedEndpointValue(this._loadersContext, this._currentPath, nameof(Besluiten));

                /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                [Config]
                public string Objecten()
                    => GetCachedEndpointValue(this._loadersContext, this._currentPath, nameof(Objecten));

                /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                [Config]
                public string ObjectTypen()
                    => GetCachedEndpointValue(this._loadersContext, this._currentPath, nameof(ObjectTypen));

                /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                [Config]
                public string ContactMomenten()
                    => GetCachedEndpointValue(this._loadersContext, this._currentPath, nameof(ContactMomenten));
            }

            /// <summary>
            /// The "Whitelist" part of the settings.
            /// </summary>
            public sealed record WhitelistComponent : IDisposable
            {
                private static readonly object s_whitelistLock = new();
                private static readonly HashSet<string> s_allWhitelistedCaseTypeIds = [];  // All whitelisted Case type IDs from different scenarios
                private static readonly ConcurrentDictionary<string /* Node name */, IDs> s_cachedIDs = [];  // Cached IDs nested models

                private readonly ILoadersContext _loadersContext;
                private readonly string _currentPath;

                /// <summary>
                /// Initializes a new instance of the <see cref="WhitelistComponent"/> class.
                /// </summary>
                public WhitelistComponent(ILoadersContext loadersContext, string parentPath)
                {
                    this._loadersContext = loadersContext;
                    this._currentPath = loadersContext.GetPathWithNode(parentPath, nameof(Whitelist));
                }

                // ----------------------------
                // Allowed Case Identifications
                // ----------------------------

                /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                [Config]
                public IDs ZaakCreate_IDs()
                    => GetIDs(this._loadersContext, this._currentPath, nameof(ZaakCreate_IDs));

                /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                [Config]
                public IDs ZaakUpdate_IDs()
                    => GetIDs(this._loadersContext, this._currentPath, nameof(ZaakUpdate_IDs));

                /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                [Config]
                public IDs ZaakClose_IDs()
                    => GetIDs(this._loadersContext, this._currentPath, nameof(ZaakClose_IDs));

                /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                [Config]
                public IDs TaskAssigned_IDs()
                    => GetIDs(this._loadersContext, this._currentPath, nameof(TaskAssigned_IDs));

                /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                [Config]
                public IDs DecisionMade_IDs()
                    => GetIDs(this._loadersContext, this._currentPath, nameof(DecisionMade_IDs));

                // --------------
                // Flags (simple)
                // --------------

                /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                [Config]
                public bool Message_Allowed()
                    => GetCachedValue<bool>(this._loadersContext, this._currentPath, nameof(Message_Allowed));

                #region Helper methods
                /// <summary>
                /// Returns cached <see cref="IDs"/> or creates a new one.
                /// </summary>
                private static IDs GetIDs(ILoadingService loadersContext, string currentPath, string nodeName)
                {
                    return s_cachedIDs.GetOrAdd(
                        key: nodeName,
                        value: new IDs(loadersContext, currentPath, nodeName));
                }
                #endregion

                // ReSharper disable once InconsistentNaming
                /// <summary>
                /// A helper class encapsulating vulnerable hashed IDs and common operations on them.
                /// </summary>
                public sealed record IDs
                {
                    private const string Wildcard = "*";

                    private readonly string _finalPath;
                    private readonly bool _isEverythingAllowed;

                    /// <summary>
                    /// The count of the whitelisted IDs.
                    /// </summary>
                    /// <remarks>
                    /// NOTE: To keep the data integrity do not expose the cached whitelist IDs directly.
                    /// </remarks>
                    public int Count { get; }

                    /// <summary>
                    /// Initializes a new instance of the <see cref="IDs"/> class.
                    /// </summary>
                    public IDs(ILoadingService loadersContext, string currentPath, string nodeName)
                    {
                        // Get values
                        this._finalPath = loadersContext.GetPathWithNode(currentPath, nodeName);
                        string[] caseTypeIds = GetCachedValues(loadersContext, this._finalPath, disableValidation: true);

                        // Construct the IDs object
                        this.Count = caseTypeIds.Length;

                        // Cache the current IDs among all scenario-specific IDs
                        lock (s_whitelistLock)
                        {
                            if (caseTypeIds.Contains(Wildcard))
                            {
                                this._isEverythingAllowed = true;

                                return;  // NOTE: Initializing collection of Case Type IDs is not necessary, because everything is allowed anyway
                            }

                            foreach (string id in caseTypeIds)
                            {
                                s_allWhitelistedCaseTypeIds.Add(ComposeID(this._finalPath, id));
                            }
                        }
                    }

                    /// <summary>
                    /// Determines whether the specified identifier is whitelisted.
                    /// </summary>
                    public bool IsAllowed(string caseTypeId)
                    {
                        lock (s_whitelistLock)
                        {
                            if (this._isEverythingAllowed)
                            {
                                return true;  // NOTE: No need to check anything else
                            }

                            return !string.IsNullOrWhiteSpace(caseTypeId) &&
                                   s_allWhitelistedCaseTypeIds.Count != 0 &&
                                   s_allWhitelistedCaseTypeIds.Contains(ComposeID(this._finalPath, caseTypeId));
                        }
                    }

                    private static string ComposeID(string finalPath, string caseTypeId)
                    {
                        return $"{finalPath}:{caseTypeId}";
                    }

                    /// <inheritdoc cref="object.ToString()"/>
                    public override string ToString() => this._finalPath;
                }

                #region Disposing
                /// <inheritdoc cref="IDisposable.Dispose()"/>
                public void Dispose()
                {
                    lock (s_whitelistLock)
                    {
                        s_allWhitelistedCaseTypeIds.Clear();
                    }

                    s_cachedIDs.Clear();
                }
                #endregion
            }

            /// <summary>
            /// The "Variable" part of the settings.
            /// </summary>
            public sealed record VariableComponent
            {
                /// <inheritdoc cref="ObjectTypeComponent"/>
                [Config]
                public ObjectTypeComponent ObjectType { get; }

                /// <summary>
                /// Initializes a new instance of the <see cref="VariableComponent"/> class.
                /// </summary>
                public VariableComponent(ILoadersContext loadersContext, string parentPath)
                {
                    string currentPath = loadersContext.GetPathWithNode(parentPath, nameof(Variable));

                    this.ObjectType = new ObjectTypeComponent(loadersContext, currentPath);
                }

                /// <summary>
                /// The "Objecten" part of the settings.
                /// </summary>
                public sealed class ObjectTypeComponent
                {
                    private readonly ILoadersContext _loadersContext;
                    private readonly string _currentPath;

                    /// <summary>
                    /// Initializes a new instance of the <see cref="ObjectTypeComponent"/> class.
                    /// </summary>
                    public ObjectTypeComponent(ILoadersContext loadersContext, string parentPath)
                    {
                        this._loadersContext = loadersContext;
                        this._currentPath = loadersContext.GetPathWithNode(parentPath, nameof(ObjectType));
                    }

                    // ---------------------------
                    // Allowed types (UUID / GUID)
                    // ---------------------------

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    [Config]
                    public Guid TaskObjectType_Uuid()
                        => GetCachedUuidValue(this._loadersContext, this._currentPath, nameof(TaskObjectType_Uuid));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    [Config]
                    public Guid MessageObjectType_Uuid()
                        => GetCachedUuidValue(this._loadersContext, this._currentPath, nameof(MessageObjectType_Uuid));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    [Config]
                    public ushort MessageObjectType_Version()
                        => GetCachedValue<ushort>(this._loadersContext, this._currentPath, nameof(MessageObjectType_Version));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    [Config]
                    public HashSet<Guid> DecisionInfoObjectType_Uuids()
                        => GetCachedUuidsValue(this._loadersContext, this._currentPath, nameof(DecisionInfoObjectType_Uuids));
                }
            }
        }

        /// <summary>
        /// The "Notify" part of the settings.
        /// </summary>
        [UsedImplicitly]
        public sealed record NotifyComponent
        {
            /// <inheritdoc cref="ApiComponent"/>
            [Config]
            public ApiComponent API { get; }

            /// <inheritdoc cref="TemplateIdComponent"/>
            [Config]
            public TemplateIdComponent TemplateId { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="NotifyComponent"/> class.
            /// </summary>
            public NotifyComponent(IServiceProvider serviceProvider, string parentName)
            {
                ILoadersContext loadersContext = GetLoader(serviceProvider, LoaderTypes.Environment);

                this.API = new ApiComponent(loadersContext, parentName);
                this.TemplateId = new TemplateIdComponent(loadersContext, parentName);
            }

            /// <summary>
            /// The "API" part of the settings.
            /// </summary>
            public sealed record ApiComponent
            {
                private readonly ILoadersContext _loadersContext;
                private readonly string _currentPath;

                /// <summary>
                /// Initializes a new instance of the <see cref="ApiComponent"/> class.
                /// </summary>
                public ApiComponent(ILoadersContext loadersContext, string parentPath)
                {
                    this._loadersContext = loadersContext;
                    this._currentPath = loadersContext.GetPathWithNode(parentPath, nameof(API));
                }

                /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                [Config]
                public Uri BaseUrl()
                    => GetCachedUri(this._loadersContext, this._currentPath, nameof(BaseUrl));

                /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                [Config]
                public string Key()
                    => GetCachedValue(this._loadersContext, this._currentPath, nameof(Key));
            }

            /// <summary>
            /// The "TemplateIds" part of the settings.
            /// </summary>
            public sealed record TemplateIdComponent
            {
                private readonly ILoadersContext _loadersContext;
                private readonly string _currentPath;

                /// <inheritdoc cref="EmailComponent"/>
                [Config]
                public EmailComponent Email { get; }

                /// <inheritdoc cref="SmsComponent"/>
                [Config]
                public SmsComponent Sms { get; }

                /// <summary>
                /// Initializes a new instance of the <see cref="TemplateIdComponent"/> class.
                /// </summary>
                public TemplateIdComponent(ILoadersContext loadersContext, string parentPath)
                {
                    this._loadersContext = loadersContext;
                    this._currentPath = loadersContext.GetPathWithNode(parentPath, nameof(TemplateId));

                    this.Email = new EmailComponent(this._loadersContext, this._currentPath);
                    this.Sms = new SmsComponent(this._loadersContext, this._currentPath);
                }

                /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                [Config]
                public Guid DecisionMade()
                    => GetCachedUuidValue(this._loadersContext, this._currentPath, nameof(DecisionMade));

                /// <summary>
                /// The "Email" part of the settings.
                /// </summary>
                public sealed record EmailComponent
                {
                    private readonly ILoadersContext _loadersContext;
                    private readonly string _currentPath;

                    /// <summary>
                    /// Initializes a new instance of the <see cref="EmailComponent"/> class.
                    /// </summary>
                    public EmailComponent(ILoadersContext loadersContext, string parentPath)
                    {
                        this._loadersContext = loadersContext;
                        this._currentPath = loadersContext.GetPathWithNode(parentPath, nameof(Email));
                    }

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    [Config]
                    public Guid ZaakCreate()
                        => GetCachedUuidValue(this._loadersContext, this._currentPath, nameof(ZaakCreate));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    [Config]
                    public Guid ZaakUpdate()
                        => GetCachedUuidValue(this._loadersContext, this._currentPath, nameof(ZaakUpdate));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    [Config]
                    public Guid ZaakClose()
                        => GetCachedUuidValue(this._loadersContext, this._currentPath, nameof(ZaakClose));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    [Config]
                    public Guid TaskAssigned()
                        => GetCachedUuidValue(this._loadersContext, this._currentPath, nameof(TaskAssigned));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    [Config]
                    public Guid MessageReceived()
                        => GetCachedUuidValue(this._loadersContext, this._currentPath, nameof(MessageReceived));
                }

                /// <summary>
                /// The "SMS" part of the settings.
                /// </summary>
                public sealed record SmsComponent
                {
                    private readonly ILoadersContext _loadersContext;
                    private readonly string _currentPath;

                    /// <summary>
                    /// Initializes a new instance of the <see cref="SmsComponent"/> class.
                    /// </summary>
                    public SmsComponent(ILoadersContext loadersContext, string parentPath)
                    {
                        this._loadersContext = loadersContext;
                        this._currentPath = loadersContext.GetPathWithNode(parentPath, nameof(Sms));
                    }

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    [Config]
                    public Guid ZaakCreate()
                        => GetCachedUuidValue(this._loadersContext, this._currentPath, nameof(ZaakCreate));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    [Config]
                    public Guid ZaakUpdate()
                        => GetCachedUuidValue(this._loadersContext, this._currentPath, nameof(ZaakUpdate));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    [Config]
                    public Guid ZaakClose()
                        => GetCachedUuidValue(this._loadersContext, this._currentPath, nameof(ZaakClose));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    [Config]
                    public Guid TaskAssigned()
                        => GetCachedUuidValue(this._loadersContext, this._currentPath, nameof(TaskAssigned));

                    /// <inheritdoc cref="ILoadingService.GetData{TData}(string, bool)"/>
                    [Config]
                    public Guid MessageReceived()
                        => GetCachedUuidValue(this._loadersContext, this._currentPath, nameof(MessageReceived));
                }
            }
        }

        // NOTE: Environment variables "SENTRY_DSN" and "SENTRY_ENVIRONMENT" are skipped because they are dependent on third-party (assured and validated)
        #endregion

        #region Loading
        /// <summary>
        /// Initializes a specific type of <see cref="ILoadersContext"/> with predefined <see cref="ILoadingService"/>.
        /// </summary>
        private static ILoadersContext GetLoader(IServiceProvider serviceProvider, LoaderTypes loaderType)
        {
            ILoadersContext loaderContext = new LoadersContext(serviceProvider);
            loaderContext.SetLoader(loaderType);

            return loaderContext;
        }
        #endregion

        #region Caching
        private const char Separator = ',';

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
        /// Retrieves cached <see langword="string"/> value (with optional validation).
        /// </summary>
        /// <remarks>
        /// A shortcut to not use GetValue&lt;<see langword="string"/>&gt; method invocation for the most common settings value type.
        /// <para>
        /// Validation: optional
        /// </para>
        /// </remarks>
        private static string GetCachedValue(FallbackContextWrapper contextWrapper, string nodeName, bool disableValidation = false)
        {
            // Check if settings values are already cached
            if (s_cachedStrings.TryGetValue(contextWrapper.PrimaryCurrentPath + nodeName, out string? value))
            {
                // First attempt
                return value;
            }

            if (s_cachedStrings.TryGetValue(contextWrapper.FallbackCurrentPath + nodeName, out value))
            {
                // Second attempt
                return value;
            }

            // Try to load settings values and then cache them
            try
            {
                // First attempt
                return GetAndCache(contextWrapper.PrimaryLoaderContext, contextWrapper.PrimaryCurrentPath, nodeName, disableValidation);
            }
            catch
            {
                // Second attempt
                return GetAndCache(contextWrapper.FallbackLoaderContext, contextWrapper.FallbackCurrentPath, nodeName, disableValidation);
            }
            
            // NOTE: Shorthand to not use the most popular <string> type in most cases
            static string GetAndCache(ILoadersContext loadersContext, string currentPath, string nodeName, bool disableValidation)
            {
                // Validation happens once during initial loading, before caching the value
                string value = GetValue<string>(loadersContext, currentPath, nodeName, disableValidation);  // Validate not empty (if validation is enabled)

                return s_cachedStrings.TryAdd(currentPath + nodeName, value) ? value : string.Empty;
            }
        }

        /// <summary>
        /// Retrieves cached domain value.
        /// </summary>
        /// <remarks>
        /// Validation: enabled
        /// </remarks>
        private static string GetCachedEndpointValue(ILoadingService loadersContext, string currentPath, string nodeName)
        {
            return s_cachedStrings.GetOrAdd(
                currentPath + nodeName,
                // Validation happens once during initial loading, before caching the value
                GetValue<string>(loadersContext, currentPath, nodeName, disableValidation: false)  // Validate not empty (if validation is enabled)
                    .GetWithoutProtocol());
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
                                                                                                            // Handles the cases: "1,2,3" and "1, 2, 3", or " 1, 2,  3, "
                        .Split(Separator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .ToArray();

                    // Validation #2: Checking if the comma-separated string was properly split into array
                    return disableValidation
                        ? values
                        : values.GetNotEmpty(finalPath);  // Handles the case: "," => RemoveEmptyEntries => { }
                });
        }

        /// <summary>
        /// Retrieves cached GUID value (in correct format).
        /// </summary>
        /// <remarks>
        /// Validation: enabled
        /// </remarks>
        private static Guid GetCachedUuidValue(ILoadingService loadersContext, string currentPath, string nodeName)
        {
            return s_cachedGuids.GetOrAdd(
                currentPath + nodeName,
                // Validation happens once during initial loading, before caching the value
                GetValue<string>(loadersContext, currentPath, nodeName, disableValidation: false)  // Validate not empty (if validation is enabled)
                    .GetValidGuid());
        }

        /// <summary>
        /// Retrieves cached GUID value (in correct format).
        /// </summary>
        /// <remarks>
        /// Validation: enabled
        /// </remarks>
        private static HashSet<Guid> GetCachedUuidsValue(ILoadingService loadersContext, string currentPath, string nodeName)
        {
            return s_cachedArrayGuids.GetOrAdd(
                currentPath + nodeName,
                // Validation happens once during initial loading, before caching the value
                GetValue<string>(loadersContext, currentPath, nodeName, disableValidation: false)  // Validate not empty (if validation is enabled)
                                                                                                   // Works with "A,B,C" and "A, B, C", or "  A, B, C, " => { "A", "B", "C" }
                    .Split(Separator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    // Convert each string into GUID
                    .Select(value => value.GetValidGuid())
                    // Combine them into a fast look-up oriented data structure
                    .ToHashSet());
        }

        /// <summary>
        /// Retrieves cached <see cref="Uri"/> value (in correct format).
        /// </summary>
        /// <remarks>
        /// Validation: enabled
        /// </remarks>
        private static Uri GetCachedUri(ILoadingService loadersContext, string currentPath, string nodeName)
        {
            return s_cachedUris.GetOrAdd(
                currentPath + nodeName,
                // Validation happens once during initial loading, before caching the value
                GetValue<string>(loadersContext, currentPath, nodeName, disableValidation: false)  // Validate not empty (if validation is enabled)
                    .GetValidUri());
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
            return s_cachedStrings.GetOrAdd(
                currentPath + nodeName,
                // Validation happens once during initial loading, before caching the value
                $"{GetValue<TData>(loadersContext, currentPath, nodeName, disableValidation)}")  // Validate not empty (if validation is enabled)
                    .ChangeType<TData>();
        }

        /// <summary>
        /// Retrieves cached <typeparamref name="TData"/> value.
        /// </summary>
        /// <remarks>
        /// Validation: optional
        /// </remarks>
        private static TData GetCachedValue<TData>(FallbackContextWrapper contextWrapper, string nodeName, bool disableValidation = false)
            where TData : notnull
        {
            return GetCachedValue(contextWrapper, nodeName, disableValidation)
                .ChangeType<TData>();
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

        #region Testing
        #pragma warning disable IDE0008  // Using "explicit types" wouldn't help with readability of the code
        // ReSharper disable SuggestVarOrType_SimpleTypes

        /// <summary>
        /// Test if all "appsettings.json" configurations are present.
        /// </summary>
        public static string TestAppSettingsConfigs(OmcConfiguration omcConfiguration)
        {
            int counter = 0;
            List<string> methodNames = [];

            var appSettings = omcConfiguration.AppSettings;

            // AppSettings | Network
            TryGetConfigurations(ref counter, methodNames, appSettings.Network);

            // AppSettings | Encryption
            TryGetConfigurations(ref counter, methodNames, appSettings.Encryption);

            var variablesSettings = omcConfiguration.AppSettings.Variables;

            // AppSettings | Variables
            TryGetConfigurations(ref counter, methodNames, variablesSettings, isRecursionEnabled: false);  // NOTE: Variables contains properties and other nodes. Disabling recursion will display only those separate properties without nods

            // AppSettings | Variables | OpenKlant
            TryGetConfigurations(ref counter, methodNames, variablesSettings.OpenKlant);

            // AppSettings | Variables | UX Messages
            TryGetConfigurations(ref counter, methodNames, variablesSettings.UxMessages);

            return $"Tested appsettings.json values: {counter}" +
                   $"{Environment.NewLine}" +
                   $"{Environment.NewLine}" +
                   $"Methods:" +
                     $"{Environment.NewLine}" +
                     $"{methodNames.Join(Environment.NewLine)}";
        }

        /// <summary>
        /// Test if all environment variables configurations are present.
        /// </summary>
        public static string TestEnvVariablesConfigs(OmcConfiguration configuration)
        {
            int counter = 0;
            List<string> methodNames = [];
            
            var omcConfiguration = configuration.OMC;

            // OMC | Authorization | JWT
            TryGetConfigurations(ref counter, methodNames, omcConfiguration.Auth.JWT);

            // OMC | Features
            TryGetConfigurations(ref counter, methodNames, omcConfiguration.Feature);
                
            var zgwConfiguration = configuration.ZGW;

            // ZGW | Authorization | JWT
            TryGetConfigurations(ref counter, methodNames, zgwConfiguration.Auth.JWT);

            // ZGW | Key
            TryGetConfigurations(ref counter, methodNames, zgwConfiguration.Auth.Key);

            // ZGW | Domain
            TryGetConfigurations(ref counter, methodNames, zgwConfiguration.Endpoint);

            // ZGW | Whitelist
            TryGetConfigurations(ref counter, methodNames, zgwConfiguration.Whitelist);

            // ZGW | Variables | Objecten
            TryGetConfigurations(ref counter, methodNames, zgwConfiguration.Variable.ObjectType);
                
            var notifyConfiguration = configuration.Notify;

            // Notify | API
            TryGetConfigurations(ref counter, methodNames, notifyConfiguration.API);

            // Notify | Templates (Email + SMS)
            TryGetConfigurations(ref counter, methodNames, notifyConfiguration.TemplateId.Email);
            TryGetConfigurations(ref counter, methodNames, notifyConfiguration.TemplateId.Sms);

            return $"Tested environment variables: {counter}" +
                   $"{Environment.NewLine}" +
                   $"{Environment.NewLine}" +
                   $"Methods:" +
                     $"{Environment.NewLine}" +
                     $"{methodNames.Join(Environment.NewLine)}";
        }

        private static void TryGetConfigurations(ref int counter, List<string> methodNames, object instance, bool isRecursionEnabled = true)
        {
            foreach (MethodInfo method in GetConfigMethods(instance.GetType(), isRecursionEnabled).ToArray())
            {
                object? result = method.Invoke(instance, null);

                counter++;
                methodNames.Add($"  {method.Name}() => \"{result}\"");
            }
        }

        private static IEnumerable<MethodInfo> GetConfigMethods(Type currentType, bool isRecursionEnabled)
        {
            IEnumerable<MemberInfo> members = currentType
                .GetMembers(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(member => member.GetCustomAttribute<ConfigAttribute>() != null);

            foreach (MemberInfo member in members)
            {
                // Returning method
                if (member is MethodInfo method)
                {
                    yield return method;
                }

                if (isRecursionEnabled && member is PropertyInfo property)
                {
                    // Traversing recursively to get methods from property
                    foreach (MethodInfo nestedMethod in GetConfigMethods(property.PropertyType, isRecursionEnabled))
                    {
                        yield return nestedMethod;
                    }
                }
            }
        }
        #pragma warning restore IDE00008
        #endregion

        #region Disposing
        /// <inheritdoc cref="IDisposable.Dispose()"/>
        public void Dispose()
        {
            s_cachedStrings.Clear();
            s_cachedGuids.Clear();
            s_cachedUris.Clear();
            s_cachedArrays.Clear();

            this.ZGW.Whitelist.Dispose();
        }
        #endregion
    }
}