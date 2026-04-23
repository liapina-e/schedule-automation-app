using FluentValidation;
using schedule_automation_app_server.Application.DTOs;

namespace schedule_automation_app_server.Application.Validators;

public class CreateSubjectRequestValidator : AbstractValidator<CreateSubjectRequest>
{
    public CreateSubjectRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Название предмета не может быть пустым.")
            .MaximumLength(100).WithMessage("Название не может быть длиннее 100 символов."); // подумать

        RuleFor(x => x.TargetGrade)
            .InclusiveBetween(4, 10).WithMessage("Целевая оценка должна быть от 4 до 10.");

        RuleFor(x => x.Components)
            .NotEmpty().WithMessage("Нужен хотя бы один компонент формулы.");

        RuleFor(x => x.Components)
            .Must(components => Math.Abs(components.Sum(c => c.Weight) - 100) <= 0.01)
            .WithMessage("Сумма весов должна быть равна 100%.")
            .When(x => x.Components != null && x.Components.Count > 0);

        RuleForEach(x => x.Components).ChildRules(component =>
        {
            component.RuleFor(c => c.Name)
                .NotEmpty().WithMessage("Название компонента не может быть пустым.");

            component.RuleFor(c => c.Weight)
                .InclusiveBetween(0, 100).WithMessage("Вес компонента должен быть от 0 до 100.");

            component.RuleFor(c => c.Complexity)
                .InclusiveBetween(1, 10).WithMessage("Сложность должна быть от 1 до 10.");

            component.RuleFor(c => c.CurrentGrade)
                .InclusiveBetween(0, 10).WithMessage("Текущая оценка должна быть от 0 до 10.");
        });
    }
}