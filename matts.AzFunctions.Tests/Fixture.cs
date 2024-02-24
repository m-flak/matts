namespace matts.AzFunctions.Tests;
public sealed class Fixture
{
    public static string NewTaskRequestBody
        => """
           {
                "assignee": "all",
                "taskType": "TEST_TASK",
                "title": "Test Task",
                "description": "Test Task"
           }
           """;

    public static string NewTaskWithSubjects
        => """
           {
                "assignee": "all",
                "taskType": "TEST_TASK",
                "title": "Test Task",
                "description": "Test Task",
                "subjects": [{
                    "id": 1,
                    "subjectType": "TARGET",
                    "name": "Test Me!"
                }]
           }
           """;

    private Fixture() { }
}
