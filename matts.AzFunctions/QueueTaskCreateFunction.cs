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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Azure;
using matts.AzFunctions.Utils;
using System.Net.Mime;
using Azure.Storage.Queues.Models;
using System.Text.Json;
using Json.Schema;

namespace matts.AzFunctions;

public class QueueTaskCreateFunction
{
    private readonly ILogger _logger;
    private readonly QueueServiceClient _queueServiceClient;
    private readonly SchemaRegistry _schemaRegistry;

    public QueueTaskCreateFunction(ILoggerFactory loggerFactory, QueueServiceClient queueServiceClient, SchemaRegistry schemaRegistry)
    {
        _logger = loggerFactory.CreateLogger<QueueServiceClient>();
        _queueServiceClient = queueServiceClient;
        _schemaRegistry = schemaRegistry;
    }

    [Function("QueueTaskCreateFunction")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post", "options", Route = "tasks/queue-create")] HttpRequestData req, FunctionContext context)
    {
        if (HttpUtils.HandleCreateOptionsResponse(req, out var optionsResponse))
        {
            return optionsResponse;
        }
        if (!HttpUtils.IsContentType(req, MediaTypeNames.Application.Json))
        {
            const string msg = "Incorrect content type. Content type must be JSON.";
            _logger.LogError(msg);
            return await HttpUtils.CreateMessageResponseAsync(req, HttpStatusCode.BadRequest, msg);
        }

        try
        {
            EvaluationResults results = await ValidateJsonAsync(req.Body, context.CancellationToken);
            if (!results.IsValid)
            {
                const string msg = "Provided JSON is invalid! ";
                string errors = string.Empty;
                if (results.HasErrors)
                {
                    errors = string.Join(' ', results.Errors!.Values);
                }

                _logger.LogError(msg);
                return await HttpUtils.CreateMessageResponseAsync(req, HttpStatusCode.BadRequest, string.Concat(msg, errors));
            }
        }
        catch (JsonSchemaException jse)
        {
            const string msg = "Unable to resolve the task.json schema file!";
            _logger.LogError(msg, jse);
            return await HttpUtils.CreateMessageResponseAsync(req, HttpStatusCode.InternalServerError, msg);
        }

        QueueClient queue = _queueServiceClient.GetQueueClient("tasks");
        if (await queue.CreateIfNotExistsAsync() != null)
        {
            _logger.LogInformation("Tasks Queue did not exist. Creating it...");
        }

        SendReceipt azResponse;
        try
        {
            azResponse = await queue.SendMessageAsync(await req.ReadAsStringAsync());
        }
        catch (RequestFailedException rfe)
        {
            const string msg = "QUEUE CLIENT: Send Message failure!";
            _logger.LogError(msg, rfe);
            return await HttpUtils.CreateMessageResponseAsync(req, HttpStatusCode.ServiceUnavailable, msg);
        }
        catch (Exception e)
        {
            const string msg = "Send Message failure!";
            _logger.LogError(msg, e);
            return await HttpUtils.CreateMessageResponseAsync(req, HttpStatusCode.InternalServerError, msg);
        }

        var response = req.CreateResponse();
        await response.WriteAsJsonAsync(azResponse);
        return response;
    }

    private async Task<EvaluationResults> ValidateJsonAsync(Stream jsonStream, CancellationToken ct)
    {
        var builder = new UriBuilder()
        {
            Path = "task.json"
        };
        if (_schemaRegistry.Get(builder.Uri) is IBaseDocument schema)
        {
            builder = new UriBuilder(schema.BaseUri)
            {
                Path = builder.Path
            };
            JsonSchema taskSchema = new JsonSchemaBuilder()
                .Ref(builder.Uri);

            JsonDocument json = await JsonDocument.ParseAsync(jsonStream, cancellationToken: ct);
            return taskSchema.Evaluate(json);
        }

        throw new JsonSchemaException("Unable to locate the schema file from within the registry.");
    }
}
