using System.ComponentModel.DataAnnotations;

namespace TareasBlazor.Validation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class FutureDateAttribute : ValidationAttribute
    {
        /// <summary>
        /// If true, uses UTC time for comparison; otherwise uses local time.
        /// </summary>
        public bool UseUtc { get; set; } = false;

        public FutureDateAttribute()
        {
            ErrorMessage = "The date must be in the future.";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                // Let [Required] handle null checks
                return ValidationResult.Success;
            }

            var now = UseUtc ? DateTime.UtcNow : DateTime.Now;

            if (value is DateTime dateTimeValue)
            {
                if (dateTimeValue >= now)
                    return ValidationResult.Success;
            }
            else if (value is DateOnly dateOnlyValue)
            {
#if NET6_0_OR_GREATER
                if (dateOnlyValue >= DateOnly.FromDateTime(now))
                    return ValidationResult.Success;
#endif
            }
            else
            {
                return new ValidationResult("Invalid date type.");
            }

            return new ValidationResult(ErrorMessage, new[] { validationContext.MemberName! });
        }
    }
}