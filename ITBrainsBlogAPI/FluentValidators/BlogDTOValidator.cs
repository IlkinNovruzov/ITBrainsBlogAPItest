using FluentValidation;
using ITBrainsBlogAPI.DTOs;
using ITBrainsBlogAPI.Services;
namespace ITBrainsBlogAPI.FluentValidators
{
    public class BlogDTOValidator : AbstractValidator<BlogDTO>
    {
        public BlogDTOValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(100).WithMessage("Title must not exceed 100 characters.");

            RuleFor(x => x.Body)
                .NotEmpty().WithMessage("Body is required.");

            //RuleForEach(x => x.ImgFiles)
            //    .Must(BeAValidImage).WithMessage("All files must be valid image files.");
        }

        private bool BeAValidImage(IFormFile file)
        {
            // Burada dosyanın bir geçerli resim dosyası olup olmadığını kontrol edebilirsiniz.
            // Örneğin, dosya uzantısını kontrol edebilir veya MIME türünü doğrulayabilirsiniz.
            // Gerekirse, FileExtensions.IsImage gibi bir yardımcı yöntem kullanabilirsiniz.
            return FileExtensions.IsImage(file);
        }
    }

}
