//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace InstaTransfer.ITResources.Api {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class ApiResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal ApiResources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("InstaTransfer.ITResources.Api.ApiResources", typeof(ApiResources).Assembly);
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
        ///   Looks up a localized string similar to JWTExpirationMinutes.
        /// </summary>
        public static string AppSettingsKeyJWTExpiration {
            get {
                return ResourceManager.GetString("AppSettingsKeyJWTExpiration", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to commercestatus.
        /// </summary>
        public static string CommerceStatusClaim {
            get {
                return ResourceManager.GetString("CommerceStatusClaim", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to cuser.
        /// </summary>
        public static string CUserClaim {
            get {
                return ResourceManager.GetString("CUserClaim", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to cusertestmode.
        /// </summary>
        public static string CUserTestModeClaim {
            get {
                return ResourceManager.GetString("CUserTestModeClaim", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Declaración conciliada con éxito.
        /// </summary>
        public static string DeclarationReconciledSuccessMessage {
            get {
                return ResourceManager.GetString("DeclarationReconciledSuccessMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Declaracion exitosa.
        /// </summary>
        public static string DeclarationSuccessMessage {
            get {
                return ResourceManager.GetString("DeclarationSuccessMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Operacion exitosa.
        /// </summary>
        public static string OperationSuccessMessage {
            get {
                return ResourceManager.GetString("OperationSuccessMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Registro de orden de compra exitosa.
        /// </summary>
        public static string PurchaseOrderSuccessMessage {
            get {
                return ResourceManager.GetString("PurchaseOrderSuccessMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to rif.
        /// </summary>
        public static string RifClaim {
            get {
                return ResourceManager.GetString("RifClaim", resourceCulture);
            }
        }
    }
}
