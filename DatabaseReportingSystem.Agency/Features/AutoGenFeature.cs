using AutoGen.Core;
using AutoGen.OpenAI;
using AutoGen.OpenAI.Extension;
using DatabaseReportingSystem.Agency.LanguageModels;
using DatabaseReportingSystem.Agency.Strategies;
using DatabaseReportingSystem.Shared;
using DatabaseReportingSystem.Shared.Models;
using DatabaseReportingSystem.Shared.Settings;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OpenAI.Chat;
using ChatMessage = OpenAI.Chat.ChatMessage;

namespace DatabaseReportingSystem.Agency.Features;

public class AutoGenFeature(IOptions<ApiKeys> apiKeys)
{
    private const string AnalyzerModelName = "gpt-4o-mini";
    private readonly ApiKeys _apiKeys = apiKeys.Value;

    public async Task<IEnumerable<IMessage>> RunGroupChatAsync(
        UserPromptDto userQuestion,
        DatabaseManagementSystem databaseManagementSystem,
        ILanguageModel languageModel,
        IStrategy strategy,
        ConnectionCredentialsDto? credentials)
    {
        strategy.OnlySystemPrompt = true;

        var messages = await strategy.GetMessagesAsync();

        List<IAgent> agents = [];

        string systemPrompt;

        if (credentials is not null)
        {
            const string systemMessageForQueryWriterWhenAnalyzerAvailable =
                """
                If the analyzer says 'INVALID', please regenerate the SQL query according to it. If it says 
                'VALID', you should write 'TERMINATE' to end the conversation.
                """;

            ChatMessage? firstMessage = messages.FirstOrDefault();

            if (firstMessage is not SystemChatMessage)
            {
                throw new InvalidOperationException("The first message should be a system chat message.");
            }

            systemPrompt = firstMessage.Content[0].Text + "\n\n" + systemMessageForQueryWriterWhenAnalyzerAvailable;

            const string analyzerAgentSystemPrompt =
                """
                You are a query analyzer. Given an input question, run the function for querying database. You will make 
                sure that requested condition is met. Otherwise, you should tell what is wrong with the result, and 
                what could be done. You should not modify the SQL query in any way. You should only say 'VALID' or 
                'INVALID: {reason}'. If the query is valid, you should write 'TERMINATE' to end the conversation.
                """;

            //ConnectionCredentialsDto updatedCredentials = credentials with {Dbms = databaseManagementSystem};

            ConnectionCredentialsDto updatedCredentials = new(
                DatabaseManagementSystem.MySql,
                "213.142.159.62",
                "7441",
                "northwind",
                "northwind",
                "d7mK7TPT6mKRPr5xMrWVt5RWAtYuGllD");

            var function = new AnalyzerFunction(databaseManagementSystem, updatedCredentials);

            var functionMiddleware = new FunctionCallMiddleware(
                [function.RunQueryAsyncFunctionContract],
                new Dictionary<string, Func<string, Task<string>>>
                {
                    [function.RunQueryAsyncFunctionContract.Name] = function.RunQueryAsyncWrapper
                });

            var analyzerAgent = new OpenAIChatAgent(
                    new ChatClient(AnalyzerModelName, _apiKeys.GptApiKey),
                    "query-analyzer",
                    new ChatCompletionOptions { Temperature = 1 },
                    analyzerAgentSystemPrompt)
                .RegisterMessageConnector()
                .RegisterMiddleware(functionMiddleware)
                .RegisterPrintMessage();

            agents.Add(analyzerAgent);
        }
        else
        {
            const string systemMessageForQueryWriterWhenAnalyzerNotAvailable =
                "You should write 'TERMINATE' to end the conversation.";

            ChatMessage? firstMessage = messages.FirstOrDefault();

            if (firstMessage is not SystemChatMessage)
            {
                throw new InvalidOperationException("The first message should be a system chat message.");
            }

            systemPrompt = firstMessage.Content[0].Text + "\n\n" + systemMessageForQueryWriterWhenAnalyzerNotAvailable;
        }

        var queryWriterAgent = languageModel.GetChatAgent("query-writer", systemPrompt)
            .RegisterMessageConnector()
            .RegisterPrintMessage();

        agents.Add(queryWriterAgent);
        agents.Reverse(); // Reverse the order of agents to make the query writer agent the first one

        var groupChat = new RoundRobinGroupChat(agents);

        var groupChatAgent = new GroupChatManager(groupChat);

        string userMessage = Utilities.CreateUserMessage(userQuestion);

        var history = await queryWriterAgent.InitiateChatAsync(
            groupChatAgent,
            userMessage,
            5);

        return history;
    }
}

public partial class AnalyzerFunction(
    DatabaseManagementSystem databaseManagementSystem,
    ConnectionCredentialsDto? credentials)
{
    private readonly ConnectionCredentialsDto? _credentials = credentials;
    private readonly DatabaseManagementSystem _databaseManagementSystem = databaseManagementSystem;

    /// <summary>
    ///     Run a query on the user database.
    /// </summary>
    /// <param name="query">SQL query</param>
    /// <returns>Database execution result in JSON format</returns>
    /// <exception cref="InvalidOperationException">When database connection credentials is not met or there is a failure</exception>
    [Function(nameof(RunQueryAsync),
        "Runs a query on the user database, and returns the execution result in JSON format.")]
    public async Task<string> RunQueryAsync(string query)
    {
        if (_credentials is null) throw new InvalidOperationException("Connection credentials are not provided.");

        string connectionString = Utilities.GenerateConnectionString(_credentials);

        var queryResult = await Utilities.QueryOnUserDatabaseAsync(_databaseManagementSystem, connectionString, query);

        if (queryResult.IsFailure) throw new InvalidOperationException(queryResult.Error);

        return JsonConvert.SerializeObject(queryResult.Value);
    }
}
