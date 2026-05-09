using FluentValidation.Results;
using schedule_automation_app_server.Application.DTOs;
using schedule_automation_app_server.Application.Validators;

namespace schedule_automation_app_server.Tests;

public class ValidatorTests
{
    private readonly CreateSubjectRequestValidator _createValidator = new();
    private readonly UpdateSubjectRequestValidator _updateValidator = new();

    private static ComponentDto ValidComponent(
        string name = "Экзамен",
        double weight = 100,
        int complexity = 5,
        double currentGrade = 5,
        bool isAutoGrade = false,
        double autoGradeMinScore = 0) => new ComponentDto
    {
        Name = name,
        Weight = weight,
        Complexity = complexity,
        CurrentGrade = currentGrade,
        IsAutoGrade = isAutoGrade,
        AutoGradeMinScore = autoGradeMinScore
    };

    private static CreateSubjectRequest ValidCreateRequest(
        string name = "Математика",
        int targetGrade = 7,
        List<ComponentDto>? components = null) => new CreateSubjectRequest
    {
        Name = name,
        TargetGrade = targetGrade,
        Components = components ?? new List<ComponentDto> { ValidComponent() }
    };

    [Fact]
    public void CreateValidator_ValidRequest_PassesValidation()
    {
        ValidationResult result = _createValidator.Validate(ValidCreateRequest());
        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreateValidator_EmptyName_FailsValidation()
    {
        ValidationResult result = _createValidator.Validate(ValidCreateRequest(name: ""));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public void CreateValidator_NameOver100Chars_FailsValidation()
    {
        string longName = new string('А', 101);
        ValidationResult result = _createValidator.Validate(ValidCreateRequest(name: longName));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public void CreateValidator_TargetGradeBelow4_FailsValidation()
    {
        ValidationResult result = _createValidator.Validate(ValidCreateRequest(targetGrade: 3));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "TargetGrade");
    }

    [Fact]
    public void CreateValidator_TargetGradeAbove10_FailsValidation()
    {
        ValidationResult result = _createValidator.Validate(ValidCreateRequest(targetGrade: 11));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "TargetGrade");
    }

    [Fact]
    public void CreateValidator_TargetGrade4_PassesValidation()
    {
        ValidationResult result = _createValidator.Validate(ValidCreateRequest(targetGrade: 4));
        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreateValidator_TargetGrade10_PassesValidation()
    {
        ValidationResult result = _createValidator.Validate(ValidCreateRequest(targetGrade: 10));
        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreateValidator_EmptyComponents_FailsValidation()
    {
        ValidationResult result = _createValidator.Validate(ValidCreateRequest(components: new List<ComponentDto>()));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Components");
    }

    [Fact]
    public void CreateValidator_ComponentsWeightNot100_FailsValidation()
    {
        List<ComponentDto> components = new List<ComponentDto>
        {
            ValidComponent(weight: 60),
            ValidComponent(name: "ДЗ", weight: 30)
        };
        ValidationResult result = _createValidator.Validate(ValidCreateRequest(components: components));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Components");
    }

    [Fact]
    public void CreateValidator_ComponentWeightAbove100_FailsValidation()
    {
        List<ComponentDto> components = new List<ComponentDto>
        {
            ValidComponent(weight: 101)
        };
        ValidationResult result = _createValidator.Validate(ValidCreateRequest(components: components));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void CreateValidator_ComponentComplexityBelow1_FailsValidation()
    {
        List<ComponentDto> components = new List<ComponentDto>
        {
            ValidComponent(complexity: 0)
        };
        ValidationResult result = _createValidator.Validate(ValidCreateRequest(components: components));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void CreateValidator_ComponentComplexityAbove10_FailsValidation()
    {
        List<ComponentDto> components = new List<ComponentDto>
        {
            ValidComponent(complexity: 11)
        };
        ValidationResult result = _createValidator.Validate(ValidCreateRequest(components: components));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void CreateValidator_ComponentGradeAbove10_FailsValidation()
    {
        List<ComponentDto> components = new List<ComponentDto>
        {
            ValidComponent(currentGrade: 11)
        };
        ValidationResult result = _createValidator.Validate(ValidCreateRequest(components: components));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void CreateValidator_ComponentGradeBelow0_FailsValidation()
    {
        List<ComponentDto> components = new List<ComponentDto>
        {
            ValidComponent(currentGrade: -1)
        };
        ValidationResult result = _createValidator.Validate(ValidCreateRequest(components: components));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void CreateValidator_AutoGradeMinScoreAbove10_FailsValidation()
    {
        List<ComponentDto> components = new List<ComponentDto>
        {
            ValidComponent(isAutoGrade: true, autoGradeMinScore: 11)
        };
        ValidationResult result = _createValidator.Validate(ValidCreateRequest(components: components));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void CreateValidator_AutoGradeMinScoreBelow0_FailsValidation()
    {
        List<ComponentDto> components = new List<ComponentDto>
        {
            ValidComponent(isAutoGrade: true, autoGradeMinScore: -1)
        };
        ValidationResult result = _createValidator.Validate(ValidCreateRequest(components: components));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void CreateValidator_AutoGradeMinScoreValid_PassesValidation()
    {
        List<ComponentDto> components = new List<ComponentDto>
        {
            ValidComponent(isAutoGrade: true, autoGradeMinScore: 7)
        };
        ValidationResult result = _createValidator.Validate(ValidCreateRequest(components: components));
        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreateValidator_ComponentEmptyName_FailsValidation()
    {
        List<ComponentDto> components = new List<ComponentDto>
        {
            ValidComponent(name: "")
        };
        ValidationResult result = _createValidator.Validate(ValidCreateRequest(components: components));
        Assert.False(result.IsValid);
    }
    
    [Fact]
    public void UpdateValidator_ValidRequest_PassesValidation()
    {
        UpdateSubjectRequest request = new UpdateSubjectRequest
        {
            Name = "Физика",
            TargetGrade = 8,
            Components = new List<ComponentDto> { ValidComponent() }
        };
        ValidationResult result = _updateValidator.Validate(request);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void UpdateValidator_EmptyName_FailsValidation()
    {
        UpdateSubjectRequest request = new UpdateSubjectRequest
        {
            Name = "",
            TargetGrade = 7,
            Components = new List<ComponentDto> { ValidComponent() }
        };
        ValidationResult result = _updateValidator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public void UpdateValidator_TargetGradeBelow4_FailsValidation()
    {
        UpdateSubjectRequest request = new UpdateSubjectRequest
        {
            Name = "Физика",
            TargetGrade = 3,
            Components = new List<ComponentDto> { ValidComponent() }
        };
        ValidationResult result = _updateValidator.Validate(request);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void UpdateValidator_ComponentsWeightNot100_FailsValidation()
    {
        UpdateSubjectRequest request = new UpdateSubjectRequest
        {
            Name = "Физика",
            TargetGrade = 7,
            Components = new List<ComponentDto>
            {
                ValidComponent(weight: 60),
                ValidComponent(name: "ДЗ", weight: 30)
            }
        };
        ValidationResult result = _updateValidator.Validate(request);
        Assert.False(result.IsValid);
    }
}