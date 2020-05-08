using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace InstaTransfer.BLL.Models.Validation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class RequireNonDefaultAttribute : ValidationAttribute
    {
        public RequireNonDefaultAttribute()
            : base("The {0} field requires a non-default value.")
        {
        }

        /// <summary>
        /// Override of <see cref="ValidationAttribute.IsValid(object)"/>
        /// </summary>
        /// <param name="value">The value to test</param>
        /// <returns><c>false</c> if the <paramref name="value"/> is equal the default value of an instance of its own type.</returns>
        /// <remarks>Is meant for use with primitive types or structs like DateTime, Guid, etc. Specifically ignores null values so that it can be combined with RequiredAttribute. Should not be used with Strings.</remarks>
        /// <example>
        /// //Allows you to effectively mark the field as required with out having to resort to Guid? and [Required] and then having to deal with SomeId.GetValueOrDefault() everywhere (and then test for Guid.Empty)
        /// [RequireNonDefault] 
        /// public Guid SomeId { get; set;}
        /// 
        /// //Enforces validation that requires the field beexplicitly provided, AND not be 0
        /// //Similar to [RequireNonDefault] int, except that using [Required] will add the client side validation (JS) to ensure a value is provided. I don't have client side validation for [RequireNonDefault] yet
        /// [Required][RequireNonDefault] 
        /// public int? SomeId { get; set;}
        /// 
        /// //Lets the field be optional, but if it IS provided, it can't be 0
        /// [RequireNonDefault]
        /// public decimal? Price { get; set;}
        /// 
        /// </example>
        public override bool IsValid(object value)
        {
            return value != null && !Equals(value, Activator.CreateInstance(value.GetType()));
        }
    }
}