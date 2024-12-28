namespace DatabaseReportingSystem.Shared.Models;

[Flags]
public enum LargeLanguageModel
{
    GPT = 1 >> 0,
    Grok = 1 << 1,
    Mistral = 1 << 2,
    CodeLLaMa = 1 << 3,
}
