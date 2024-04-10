// © 2023, Worth Systems.

using EventsHandler.Extensions;
using EventsHandler.Services.DataLoading.Interfaces;
using EventsHandler.Services.DataLoading.Strategy.Interfaces;

namespace EventsHandler.Configuration
{
    /// <summary>
    /// The object to encapsulate <see cref="WebApplication"/> configurations
    /// from "appsettings.json" (public) and "secrets.json" (private).
    /// </summary>
    public sealed record WebApiConfiguration
    {
        /// <summary>
        /// Gets the configuration for OMC (internal) system.
        /// </summary>
        internal OmcComponent OMC { get; }

        /// <summary>
        /// Gets the configuration for the user (external) system.
        /// </summary>
        internal UserComponent User { get; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="WebApiConfiguration"/> class.
        /// </summary>
        /// <param name="loaderContext">The strategy context using a specific data provider configuration loader.</param>
        public WebApiConfiguration(ILoadersContext loaderContext)  // NOTE: The only constructor to be used with Dependency Injection
        {
            // Recreating structure of "appsettings.json" or "secrets.json" files to use them later as objects
            this.OMC = new OmcComponent(loaderContext, nameof(this.OMC));
            this.User = new UserComponent(loaderContext, nameof(this.User));
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
            /// The "Authorization" part of the configuration.
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
                /// The "JWT" part of the configuration.
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
        /// The "OMC" part of the configuration.
        /// </summary>
        internal record OmcComponent : BaseComponent
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
            /// The "API" part of the configuration.
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
                /// The "Base URL" part of the configuration.
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
        /// The "Parent" part of the configuration.
        /// </summary>
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
            /// The "API" part of the configuration.
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
                /// The "Key" part of the configuration.
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
                    internal string Objecten()
                        => GetValue(this._loadersContext, this._currentPath, nameof(Objecten));
                }
            }

            /// <summary>
            /// The "Domain" part of the configuration.
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
            /// The "TemplateIds" part of the configuration.
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
                /// The "Sms" part of the configuration.
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
                }

                /// <summary>
                /// The "Email" part of the configuration.
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
                }
            }
        }

        #region Helper methods
        /// <summary>
        /// Retrieves cached configuration value.
        /// </summary>
        private static string GetValue(ILoadingService loadersContext, string currentPath, string nodeName)
        {
            string finalPath = loadersContext.GetPathWithNode(currentPath, nodeName);

            return loadersContext.GetData<string>(finalPath)
                .NotEmpty(finalPath);
        }

        /// <summary>
        /// Retrieves cached configuration value.
        /// </summary>
        private static TData GetValue<TData>(ILoadingService loadersContext, string currentPath, string nodeName)
        {
            string finalPath = loadersContext.GetPathWithNode(currentPath, nodeName);

            return loadersContext.GetData<TData>(finalPath)
                .NotEmpty(finalPath);
        }

        /// <summary>
        /// Retrieves cached configuration value, ensuring it will be a domain (without http/s and API endpoint).
        /// </summary>
        private static string GetDomainValue(ILoadingService loadersContext, string currentPath, string nodeName)
        {
            return GetValue<string>(loadersContext, currentPath, nodeName)
                .WithoutHttp()
                .WithoutEndpoint();
        }

        /// <summary>
        /// Retrieves cached configuration value, ensuring it will be a valid Template Id.
        /// </summary>
        private static string GetTemplateIdValue(ILoadingService loadersContext, string currentPath, string nodeName)
        {
            return GetValue<string>(loadersContext, currentPath, nodeName)
                .ValidTemplateId();
        }
        #endregion
    }
}