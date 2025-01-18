using DatabaseReportingSystem.Agency.LanguageModels;
using DatabaseReportingSystem.Agency.Strategies;
using DatabaseReportingSystem.Shared;
using DatabaseReportingSystem.Shared.Models;
using DatabaseReportingSystem.Shared.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using ChatMessage = OpenAI.Chat.ChatMessage;

namespace DatabaseReportingSystem.Agency.Features;

public sealed class ManualGroupChatFeature(IOptions<ApiKeys> apiKeys, ILogger<ManualGroupChatFeature> logger)
{
    private const int MaxNumberOfIterations = 5;
    private const string AnalyzerModelName = "gpt-4o-mini";

    private readonly ApiKeys _apiKeys = apiKeys.Value;
    private readonly ILogger<ManualGroupChatFeature> _logger = logger;

    public async Task<(string, List<ChatMessage>)> RunAsync(
        UserPromptDto userQuestion,
        DatabaseManagementSystem databaseManagementSystem,
        ILanguageModel languageModel,
        IStrategy strategy,
        ConnectionCredentialsDto? credentials)
    {
        var messages = await strategy.GetMessagesAsync();

        string userMessage = Utilities.CreateUserMessage(userQuestion);

        messages.Add(new UserChatMessage(userMessage));

        _logger.LogDebug("Prepared {MessageCount} messages for the chat", messages.Count);

        string lastAnalyzerResponse = string.Empty;
        string lastQueryResponse = string.Empty;

        int i = 0;
        while (i < MaxNumberOfIterations)
        {
            _logger.LogDebug("Iteration {IterationNumber}", i);

            try
            {
                string queryResponse = await languageModel.AskAsync(messages);

                _logger.LogInformation("Received response from language model: {Response}", queryResponse);

                if (string.IsNullOrWhiteSpace(queryResponse))
                {
                    _logger.LogWarning("Received empty response from language model. Skipping iteration");

                    lastAnalyzerResponse = string.Empty;

                    goto iterate;
                }

                messages.Add(new AssistantChatMessage(queryResponse));

                lastQueryResponse = Utilities.TrimSqlString(queryResponse);

                _logger.LogDebug("Trimmed SQL query response: {Query}", lastQueryResponse);

                if (string.IsNullOrWhiteSpace(lastQueryResponse))
                {
                    lastAnalyzerResponse = "INVALID: You did not provide any SQL queries.";
                    goto iterate;
                }

                if (credentials is null)
                {
                    _logger.LogInformation(
                        "User did not provide connection credentials. Skipping database execution and analyzer");

                    break;
                }

                _logger.LogDebug("Initializing analyzer GPT model. Model name: {ModelName}", AnalyzerModelName);

                var analyzerAgent = new GptModel(AnalyzerModelName, _apiKeys.GptApiKey);

                var analyzerMessages = new List<ChatMessage>
                {
                    new SystemChatMessage(
                        """
                        You will analyze a SQL query and determine if it satisfies the requested condition. If it does
                        not satisfy the condition, you should explain what is wrong with the result and what could be 
                        done to fix it. If you are not 100% sure if the SQL query satisfies the condition, you can
                        write "VALID". Otherwise, you must write "INVALID: {reason}".
                        """),
                    new UserChatMessage(
                        $"""
                         dbms: {databaseManagementSystem.ToString().ToLower()}
                         question: {userQuestion.Question}
                         generated_query: ```sql {queryResponse} ```
                         """),
                };

                _logger.LogInformation("Sending messages to analyzer agent");

                string analyzerResponse = await analyzerAgent.AskAsync(analyzerMessages);

                _logger.LogInformation("Received response from analyzer agent: {Response}", analyzerResponse);

                if (analyzerResponse.Equals("VALID", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("Received 'VALID' response from analyzer. Terminating the conversation");

                    break;
                }

                lastAnalyzerResponse = analyzerResponse;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while running the chat");
            }

            iterate:

            if (!string.IsNullOrWhiteSpace(lastAnalyzerResponse))
            {
                messages.Add(new UserChatMessage(lastAnalyzerResponse));
            }

            i++;
        }

        if (i == MaxNumberOfIterations)
        {
            _logger.LogWarning("Reached maximum number of iterations. Terminating the conversation");
        }

        return (lastQueryResponse, messages);
    }
}
