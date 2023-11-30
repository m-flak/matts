/* matts
 * "Matthew's ATS" - Portfolio Project
 * Copyright (C) 2023  Matthew E. Kehrer <matthew@kehrer.dev>
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 * 
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
**/
using Azure;
using System.Security.Cryptography;
using System.Text.Json;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace matts.AzFunctions;

public class CreateTaskFunction
{
    private readonly ILogger _logger;
    private readonly RandomNumberGenerator _rng;
    private readonly TableServiceClient _tableServiceClient;

    public CreateTaskFunction(ILoggerFactory loggerFactory, TableServiceClient tableServiceClient)
    {
        _logger = loggerFactory.CreateLogger<CreateTaskFunction>();
        _rng = RandomNumberGenerator.Create();
        _tableServiceClient = tableServiceClient;
    }

    [Function("CreateTaskFunction")]
    [TableOutput("tasks", Connection = "AzureWebJobsStorage")]
    public async Task<TableEntities.Task> Run([QueueTrigger("tasks", Connection = "AzureWebJobsStorage")] string taskJson, FunctionContext context)
    {
        bool hasSubjects = false;
        JsonEntities.Task parsedTask;
        try
        {
            parsedTask = JsonSerializer.Deserialize<JsonEntities.Task>(taskJson)!;
            ArgumentNullException.ThrowIfNull(parsedTask);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deserializing {J}", nameof(taskJson));
            throw new RequestFailedException("InvalidInput", ex);
        }

        await _tableServiceClient.CreateTableIfNotExistsAsync("tasks", context.CancellationToken);
        string rowKey = MakeRowKey();

        // Add subject entities (if any) first
        TableClient tableClient = _tableServiceClient.GetTableClient("tasks");
        if (parsedTask.Subjects is List<JsonEntities.Subject> subjects)
        {
            hasSubjects = subjects.Any();

            foreach (var subject in subjects)
            {
                var entity = new TableEntities.Subject()
                {
                    PartitionKey = rowKey,  // The rowKey of the parent task
                    RowKey = subject.Id.ToString(),
                    Id = subject.Id,
                    SubjectType = subject.SubjectType,
                    Name = subject.Name,
                    RefUuid = subject.RefUuid
                };

                await tableClient.AddEntityAsync(entity, context.CancellationToken);
            }
        }

        return new TableEntities.Task()
        {
            PartitionKey = parsedTask.Assignee,
            RowKey = rowKey,
            Assignee = parsedTask.Assignee,
            TaskType = parsedTask.TaskType,
            Title = parsedTask.Title,
            Description = parsedTask.Description,
            TimeCreated = DateTime.UtcNow,
            HasSubjects = hasSubjects
        };
    }

    private string MakeRowKey()
    {
        var bytes = new byte[sizeof(int)];
        _rng.GetBytes(bytes);

        return string.Concat(
            DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString(),
            '_',
            BitConverter.ToString(bytes).Replace("-", string.Empty)
            );
    }
}
