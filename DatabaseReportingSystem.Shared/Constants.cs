namespace DatabaseReportingSystem.Shared;

public static class Constants
{
    public const string DefaultDatabaseUsername = "drs_viewer";
    public static readonly Guid DefaultUserId = new("d64df337-7bcc-417c-921d-7f1168e05802");

    public static class Strategy
    {
        public const int DefaultRandomSeed = 27;

        public const int DefaultNumberOfExamples = 5;

        public const string BaseSystemPromptMessage =
            "You are a SQL expert. Given an input question and database schema, you will write a SQL query. Only give SQL as a return nothing else. Avoid recursive queries.";

        /// <summary>
        ///     Prompt format of the user message.
        ///     Three values are expected: Question, DBMS, Schema.
        /// </summary>
        public const string UserPromptFormat = "question: {0}, dbms: {1}, schema: {2}";
    }

    public static class Encryption
    {
        public const int PasswordHashSize = 32;

        public const int PasswordSaltSize = 16;

        public const int PasswordIterations = 100_000;

        public const int IvSize = 16;
    }

    public static class Context
    {
        public const int PasswordSize = Encryption.PasswordHashSize * 2 + Encryption.PasswordSaltSize * 2 + 1;

        public const int EmailSize = 64;

        public const int LicenseCodeSize = 8;

        public const int DatabaseManagementSystemSize = 16;

        public const int ModelNameSize = 32;
    }
}
