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
using System.Text.Json.Serialization;

namespace matts.AzFunctions.JsonEntities;
public record Task
{
    /// <summary>
    /// The assignee of the task. Either an Employer UUID or 'all'.
    /// </summary>
    [JsonPropertyName("assignee")]
    public required string Assignee { get; set; }

    /// <summary>
    /// The type of task.
    /// </summary>
    [JsonPropertyName("taskType")]
    public required string TaskType { get; set; }

    /// <summary>
    /// The display title of this task.
    /// </summary>
    [JsonPropertyName("title")]
    public required string Title { get; set; }

    /// <summary>
    /// Describes what needs to be done for this task.
    /// </summary>
    [JsonPropertyName("description")]
    public required string Description { get; set; }

    /// <summary>
    /// Creation time of this task. Either a date-time string or a unix epoch.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("timeCreated")]
    public DateTimeOffset? TimeCreated { get; set; }

    /// <summary>
    /// Objects that represent any additional 'what' or 'whom' that are associated with a given
    /// task.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("subjects")]
    public List<Subject>? Subjects { get; set; }
}
