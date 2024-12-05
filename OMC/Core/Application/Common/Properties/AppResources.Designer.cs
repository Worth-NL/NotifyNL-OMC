﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Common.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class AppResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal AppResources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Common.Properties.AppResources", typeof(AppResources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to In the settings representing domain unnecessary protocol (http or https) was found: {0}..
        /// </summary>
        public static string Configuration_ERROR_ContainsHttp {
            get {
                return ResourceManager.GetString("Configuration_ERROR_ContainsHttp", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to public error: The key used to retrieve configuration value is null, empty, or whitespace..
        /// </summary>
        public static string Configuration_ERROR_InvalidKey {
            get {
                return ResourceManager.GetString("Configuration_ERROR_InvalidKey", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to In the settings the Template ID is invalid (should be UUID: 00000000-0000-0000-0000-000000000000): {0}..
        /// </summary>
        public static string Configuration_ERROR_InvalidTemplateId {
            get {
                return ResourceManager.GetString("Configuration_ERROR_InvalidTemplateId", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to In the settings the given URI is invalid (e.g., default): {0}..
        /// </summary>
        public static string Configuration_ERROR_InvalidUri {
            get {
                return ResourceManager.GetString("Configuration_ERROR_InvalidUri", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to It was not possible to determine the given data provider. It might be not implemented yet..
        /// </summary>
        public static string Configuration_ERROR_Loader_NotImplemented {
            get {
                return ResourceManager.GetString("Configuration_ERROR_Loader_NotImplemented", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to It was not possible to retrieve any data provider. The loading service might not be set..
        /// </summary>
        public static string Configuration_ERROR_Loader_NotSet {
            get {
                return ResourceManager.GetString("Configuration_ERROR_Loader_NotSet", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The settings do not contain a given value, or the value is empty: {0}..
        /// </summary>
        public static string Configuration_ERROR_ValueNotFoundOrEmpty {
            get {
                return ResourceManager.GetString("Configuration_ERROR_ValueNotFoundOrEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The access to the external Web API service is forbidden..
        /// </summary>
        public static string Operation_ERROR_AccessDenied {
            get {
                return ResourceManager.GetString("Operation_ERROR_AccessDenied", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The notification could not be recognized/deserialized..
        /// </summary>
        public static string Operation_ERROR_Deserialization_Failure {
            get {
                return ResourceManager.GetString("Operation_ERROR_Deserialization_Failure", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The HTTP request wasn&apos;t completed successfully..
        /// </summary>
        public static string Operation_ERROR_HttpRequest_Failure {
            get {
                return ResourceManager.GetString("Operation_ERROR_HttpRequest_Failure", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to public error: An unexpected issue occurred..
        /// </summary>
        public static string Operation_ERROR_Internal {
            get {
                return ResourceManager.GetString("Operation_ERROR_Internal", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This operation is not implemented. Internal server error..
        /// </summary>
        public static string Operation_ERROR_NotImplemented {
            get {
                return ResourceManager.GetString("Operation_ERROR_NotImplemented", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} | Notification: {1}..
        /// </summary>
        public static string Operation_STATUS_Notification {
            get {
                return ResourceManager.GetString("Operation_STATUS_Notification", resourceCulture);
            }
        }
    }
}
