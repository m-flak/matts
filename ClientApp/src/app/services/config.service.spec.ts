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
import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { HttpClientModule } from '@angular/common/http';
import { ConfigService } from './config.service';

const configJson = {
  'externalApis': {
    'resumeUploadEndpoint': 'http://localhost:7274/api/resumes/upload',
    'resumeUploadApiKey': 'APIKEY',
  },
};

describe('ConfigService', () => {
  let httpMock: HttpTestingController;
  let configService: ConfigService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [],
      imports: [HttpClientModule, HttpClientTestingModule],
      providers: [{ provide: 'BASE_URL', useValue: '' }, ConfigService],
    });

    configService = TestBed.inject(ConfigService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  it('can populate the configuration after the get', () => {
    const promise = configService.loadConfig();

    const request = httpMock.expectOne('/config/');
    request.flush(configJson);

    httpMock.verify();

    return promise.then(() => {
      expect(configService.config.externalApis.resumeUploadEndpoint).toEqual(
        configJson.externalApis.resumeUploadEndpoint,
      );
      expect(configService.config.externalApis.resumeUploadApiKey).toEqual(configJson.externalApis.resumeUploadApiKey);
    });
  });
});
