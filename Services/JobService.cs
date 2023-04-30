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
namespace matts.Services;

using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using matts.Interfaces;
using matts.Models;

public partial class JobService : IJobService
{
    private static List<Job>? _jobsDummyData = null;
    private readonly IConfiguration _configuration;
    private readonly IJobRepository _repository;

    private bool _useDummyData;

    public JobService(IConfiguration configuration, IJobRepository repository)
    {
        _configuration = configuration;
        _repository = repository;

        if(_jobsDummyData == null)
        {
            _jobsDummyData = CreateJobs();
        }
        
        ConfigureService();
    }

    public async Task<IEnumerable<Job>> GetJobs()
    {
        if (_useDummyData)
        {
            return _jobsDummyData;
        }

        return await _repository.GetAll();
    }

    public async Task<Job> GetJobDetails(string uuid)
    {
        if (_useDummyData)
        { 
            Job job = _jobsDummyData
            .Where(j => j.Uuid == uuid)
            .First();

            return job;
        }

        return await _repository.GetJobByUuid(uuid);
    }

    private void ConfigureService()
    {
        var useDummyData = _configuration.GetValue<bool>("DummyData:JobService", false);
        _useDummyData = useDummyData;
    }
}
