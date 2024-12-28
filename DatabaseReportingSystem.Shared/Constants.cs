namespace DatabaseReportingSystem.Shared;

public static class Constants
{
    public class Strategy
    {
        public const int DefaultRandomSeed = 27;

        public const int DefaultNumberOfExamples = 5;

        public const string BaseSystemPromptMessage =
            "You are a SQL expert. Given an input question and database schema, you will write a SQL query. Only give SQL as a return nothing else. Avoid recursive queries.";

        /// <summary>
        /// Prompt format of the user message.
        /// Three values are expected: Question, DBMS, Schema.
        /// </summary>
        public const string UserPromptFormat = "question: {0}, dbms: {1}, schema: {2}";
    }
}
