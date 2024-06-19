using FluentValidation;
using ITBrainsBlogAPI.DTOs;

namespace ITBrainsBlogAPI.FluentValidators
{
    public class UpdateUserDTOValidator : AbstractValidator<UpdateUserDTO>
    {
        public UpdateUserDTOValidator()
        {
            RuleFor(x => x.UserId)
                .GreaterThan(0)
                .WithMessage("UserId must be greater than 0.");

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Name is required.")
                .Length(2, 50)
                .WithMessage("Name must be between 2 and 50 characters.");

            RuleFor(x => x.Surname)
                .NotEmpty()
                .WithMessage("Surname is required.")
                .Length(2, 50)
                .WithMessage("Surname must be between 2 and 50 characters.");
        }
    }
}
