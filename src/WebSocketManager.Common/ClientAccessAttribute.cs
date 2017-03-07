using System;
using System.Collections.Generic;

namespace WebSocketManager.Common
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class ClientAccessAttribute : Attribute
    {
        public ClientAccessAttribute(ClientAccessibilityOptions accessibilityOptions, string[] grantAccessToList = null, string[] denyAccessToList = null)
        {
            AccessibilityOptions = accessibilityOptions;
            GrantAccessToList = grantAccessToList;
            DenyAccessToList = denyAccessToList;
        }

        public ClientAccessibilityOptions AccessibilityOptions { get; set; }

        /// <summary>
        /// Contains a list of users and roles which are cleared for access to this method.
        /// </summary>
        public IEnumerable<string> GrantAccessToList { get; set; }

        /// <summary>
        /// Contains a list of users and roles which are denied access to this method.
        /// </summary>
        public IEnumerable<string> DenyAccessToList { get; set; }
    }

    public enum ClientAccessibilityOptions
    {
        DenyAll,
        GrantAll,
        DenyAnonymous
    }
}